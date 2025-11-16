using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Enemy : CharacterBody2D //怪物基类
{   
	//===状态枚举===
	public enum EnemyState
	{
		idle,
        attack,
        death,
        hurt,
        walk,
        run
    }

	protected EnemyState initialState = EnemyState.idle; //初始状态,protected允许子类访问和修改该成员变量
	protected EnemyState currentState; //当前状态
	protected EnemyState previousState; //上一个状态

	protected double stateTimer = 0.0; //状态计时器
	protected RandomNumberGenerator rng = new RandomNumberGenerator(); //随机数生成器

    private bool isfacingright = true; //是否面向右侧

    private AnimatedSprite2D animatedSprite;
    private Area2D area2d;
    private Player player;
    private EnemyAttributes attributes;

    //===探测器相关===
    private RayCast2D playerDetectorRight; //玩家右检测器
    private RayCast2D playerDetectorLeft;//玩家左检测器
    private RayCast2D wallDetector; //墙壁检测器
    private RayCast2D cliffDetector; //悬崖检测器
    private RayCast2D slopeDetector; //斜坡检测器

    //===巡逻逻辑相关===
    private Vector2 patrolStartPosition; //巡逻起始位置
    protected float patrolRadius = 250f; //巡逻半径
    private bool isPatrollingRight = true; //是否向右巡逻

    //===重力相关===
    private bool isOnFloor = false; //是否在地面上

    //===转身冷却相关===
    private double turnAroundCooldown = 0f; //转身冷却计时器
    private const float turnAroundCooldownDuration = 0.5f; // 转身冷却时间

    //===方向决策结果===
    private DirectionCheckResult directionCheck;


    public override void _Ready()
	{
		currentState = initialState; //设置当前状态为初始状态
		rng.Randomize(); //初始化随机数生成器

        player = GetTree().GetNodesInGroup("Player")[0] as Player;
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        area2d = GetNode<Area2D>("Area2D");
        
        attributes = new EnemyAttributes(); //初始化属性组件

        playerDetectorRight = GetNode<RayCast2D>("PlayerDetectorRight");
        playerDetectorLeft = GetNode<RayCast2D>("PlayerDetectorRight/PlayerDetectorLeft");
        wallDetector = GetNode<RayCast2D>("PlayerDetectorRight/WallDetector");
        cliffDetector = GetNode<RayCast2D>("PlayerDetectorRight/CliffDetector");
        slopeDetector = GetNode<RayCast2D>("PlayerDetectorRight/SlopeDetector");

        EnterState(currentState);

        area2d.BodyEntered += OnAttackAreaBodyEntered; //攻击范围信号
        attributes.IsDeathChanged += EnterDeath; //死亡信号
        attributes.IsHurtChanged += EnterHurt;   //受伤信号 
    }


	public override void _PhysicsProcess(double delta)
	{
		stateTimer += delta; //更新状态计时器

        if (turnAroundCooldown > 0)
        {
            turnAroundCooldown -= delta;
        }

        directionCheck = CheckDirection(); //更新方向决策结果

        if (currentState != EnemyState.death)
        {
            ApplyGravity(delta);
        }

        UpdateState(currentState, delta);

        // 基础的物理移动（所有状态通用）
        MoveAndSlide();
    }

	protected virtual void ChangeState(EnemyState newState) //切换状态方法,参数为要切换到的新状态
    {
		if (currentState == newState) return; //如果新状态与当前状态相同,则不进行状态切换

        GD.Print($"切换状态: {currentState} -> {newState}");
        ExitState(currentState); //调用当前状态的退出方法

		previousState = currentState; //保存当前状态为上一个状态
		currentState = newState; //更新当前状态为新状态
		stateTimer = 0.0; //重置状态计时器

        EnterState(newState); //调用新状态的进入方法
    }

    //===状态分发器===
    protected virtual void EnterState(EnemyState state)
	{
        switch (state)
        {
            case EnemyState.idle: EnterIdle(); break;
            case EnemyState.attack: EnterAttack(); break;
            case EnemyState.death: EnterDeath(); break;
            case EnemyState.hurt: EnterHurt(); break;
            case EnemyState.walk: EnterWalk(); break;
            case EnemyState.run: EnterRun(); break;
        }
    }

    public virtual void UpdateState(EnemyState state, double delta)
	{
        switch (state)
        {
            case EnemyState.idle: UpdateIdle(delta); break;
            case EnemyState.attack: UpdateAttack(delta); break;
            case EnemyState.death: UpdateDeath(delta); break;
            case EnemyState.hurt: UpdateHurt(delta); break;
            case EnemyState.walk: UpdateWalk(delta); break;
            case EnemyState.run: UpdateRun(delta); break;
        }
    }


    public virtual void ExitState(EnemyState state)
	{
        switch (state)
        {
            case EnemyState.idle: ExitIdle(); break;
            case EnemyState.attack: ExitAttack(); break;
            case EnemyState.death: ExitDeath(); break;
            case EnemyState.hurt: ExitHurt(); break;
            case EnemyState.walk: ExitWalk(); break;
            case EnemyState.run: ExitRun(); break;
        }
    }

    // === 默认状态方法（子类可以重写）===

    //===待机状态===
    protected virtual void EnterIdle() //进入Idle状态
	{
		Velocity = Vector2.Zero;
		animatedSprite.Play("idle");
    }

    protected virtual void UpdateIdle(double delta) //更新Idle状态
    {
        if (directionCheck.PlayerNearby && !directionCheck.CanSeePlayer)
        {
            // 玩家在附近但在背后
            if (turnAroundCooldown <= 0)
            {
                GD.Print($"[Idle] 感知到背后有玩家，转身");
                SetFacingDirection(directionCheck.PlayerOnRight);
                turnAroundCooldown = 0.2f;
            }
        }

        //如果看到玩家,切换到追逐状态
        if (directionCheck.CanChasePlayer)
        {
            FacePlayer();
            ChangeState(EnemyState.run);
            return;
        }

        if (stateTimer > 2.0 && !directionCheck.CanSeePlayer) // 2秒后开始巡逻
        {
            ChangeState(EnemyState.walk);
        }
    }

    protected virtual void ExitIdle()
    {
        
    }

    // === 行走状态(巡逻) ===  
    protected virtual void EnterWalk()
    {
        animatedSprite.Play("walk");    // 播放行走动画
        patrolStartPosition = GlobalPosition; // 记录巡逻起始位置
        isPatrollingRight = rng.RandiRange(0,1) == 1; // 随机选择初始巡逻方向,1为右,0为左

        SetFacingDirection(isPatrollingRight);
    }

    protected virtual void UpdateWalk(double delta)
    {
        if (directionCheck.PlayerNearby && !directionCheck.CanSeePlayer)
        {          
            if (turnAroundCooldown <= 0)
            {
                GD.Print($"[Walk] 感知到背后有玩家（距离:{directionCheck.PlayerDistance:F0}），转身");
                SetFacingDirection(directionCheck.PlayerOnRight);
                turnAroundCooldown = 0.2f; // 短冷却，避免抖动
            }
        }
        // 如果看到玩家,切换到追逐状态
        if (directionCheck.CanSeePlayer)
        {
            FacePlayer();
            ChangeState(EnemyState.run);
            return;
        }

        if (stateTimer > 5)
        {
            ChangeState(EnemyState.idle);
            return;
        }

        // 基础巡逻移动
        Velocity = new Vector2(attributes.MoveSpeed * (isfacingright ? 1 : -1), Velocity.Y);

        // 超出巡逻范围则转身
        float distanceFromStart = Mathf.Abs(GlobalPosition.X - patrolStartPosition.X);
        if (distanceFromStart >= patrolRadius)
        {
            TurnAround();
            return;
        }

        // 遇到墙壁或悬崖或斜坡就转身
        if (directionCheck.ShouldTurnAround) //前方有障碍物且冷却结束
        {
            TurnAround();
        }
        else if (directionCheck.HasObstacle && turnAroundCooldown > 0)
        {
            // 停止移动，等待冷却
            Velocity = Vector2.Zero;
        }
    }

    protected virtual void ExitWalk() { }

    //=== 奔跑状态（追逐）===
    protected virtual void EnterRun()
    {
        animatedSprite.Play("run");
    }
    protected virtual void UpdateRun(double delta)
    {
        if (!directionCheck.CanSeePlayer)
        { 
            ChangeState(EnemyState.idle);
            return;
        }
             
        if (directionCheck.HasObstacle || directionCheck.HeightDifference > 15f)
        {
            // 如果前方有障碍物或高度差,停止追逐            
            ChangeState(EnemyState.idle);
            return;
        }

        //追逐逻辑
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized(); //计算方向向量并归一化
        Velocity = new Vector2(direction.X * attributes.MoveSpeed * 2f, Velocity.Y); //设置水平速度为100，垂直速度保持不变

        //更新方向
        if (Mathf.Abs(direction.X) > 0.1f) //玩家在敌人左右移动时更新面向方向
        {
            FacePlayer();
        }

        if (directionCheck.PlayerDistance < 30f) //接近玩家时切换到攻击状态
        {
            FacePlayer();
            ChangeState(EnemyState.attack);
            return;
        }
    }
    protected virtual void ExitRun() { }

    //=== 攻击状态 ===
    protected virtual void EnterAttack() 
    {
        Velocity = Vector2.Zero;
        animatedSprite.Play("attack");
    }
    protected virtual void UpdateAttack(double delta) 
    {
        if (stateTimer > 0.5f) //攻击动作持续0.5秒
        {
            if (directionCheck.CanSeePlayer)
            {
                ChangeState(EnemyState.run);
            }
            else
            {
                ChangeState(EnemyState.idle);
            }
            
        }
    }
    protected virtual void ExitAttack() { }

    //=== 受伤状态 ===
    protected virtual void EnterHurt() 
    {
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();

        if (Mathf.Abs(direction.X) > 0.1f) //玩家在敌人左右移动时更新面向方向
        {
            bool facingright = direction.X > 0;
            float knockbackForce = 200f; //击退力大小

            if (facingright)
            {
                Velocity = new Vector2(-knockbackForce, -knockbackForce / 2); //向左击退并略微向上
            }
            else
            {
                Velocity = new Vector2(knockbackForce, -knockbackForce / 2); //向右击退并略微向上
            }
        }
            
        animatedSprite.Play("hurt");
    }
    protected virtual void UpdateHurt(double delta)
    {
        if (stateTimer > 0.5f)
        {
            ChangeState(previousState);
        }
    }
    protected virtual void ExitHurt() { }

    //=== 死亡状态 ===
    protected virtual void EnterDeath()
    {
        Velocity = Vector2.Zero;
        animatedSprite.Play("death");
    }
    protected virtual void UpdateDeath(double delta) 
    {
        if (stateTimer > 2f)
        {
            QueueFree(); //删除敌人节点
        }
    }
    protected void ExitDeath() { }

    // === 工具方法 ===
    protected struct DirectionCheckResult //方向决策结果结构体,struct用于封装多个相关变量,(储存检查结果)
    {
        public bool CanSeePlayer;       //是否看到玩家
        public bool PlayerOnRight;      //玩家是否在右侧
        public float PlayerDistance;    //与玩家的距离
        public float HeightDifference;  //与玩家的高度差

        public bool HasObstacle;        //前方是否有障碍物
        public bool HasWall;            //前方是否有墙壁
        public bool HasCliff;           //前方是否有悬崖
        public bool HasSteepSlope;      //前方是否有陡峭斜坡
        public float SlopeAngle;        //斜坡角度

        public bool CanChasePlayer;     // 是否可以追逐玩家（综合判断）
        public bool ShouldTurnAround;   // 是否应该转身（巡逻时）

        public bool PlayerNearby;       // 玩家是否在附近（不考虑方向）
        public float AlertRange;        // 警戒范围
    }

    protected virtual DirectionCheckResult CheckDirection() //(统一的方向检查方法),返回DirectionCheckResult结构体
    {
        var result = new DirectionCheckResult();
        result.AlertRange = 200f; //设置警戒范围

        //=== 玩家相关检测 ===
        //射线检测（单向视野）
        bool rightDetected = playerDetectorRight.IsColliding() && playerDetectorRight.GetCollider() is Player;
        bool leftDetected = playerDetectorLeft.IsColliding() && playerDetectorLeft.GetCollider() is Player;
        result.CanSeePlayer = rightDetected || leftDetected;

        //距离检测（360度感知）
        result.PlayerDistance = GlobalPosition.DistanceTo(player.GlobalPosition);
        result.PlayerNearby = result.PlayerDistance < result.AlertRange;

        if (result.CanSeePlayer || result.PlayerNearby)
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
            result.PlayerOnRight = direction.X > 0;
            result.HeightDifference = Mathf.Abs(player.GlobalPosition.Y - GlobalPosition.Y);
        }

        //=== 障碍相关检测 ===
        result.HasWall = wallDetector.IsColliding();
        result.HasCliff = !cliffDetector.IsColliding();
        result.HasSteepSlope = slopeDetector.IsColliding();

        if (result.HasSteepSlope)
        {
            Vector2 collisionNormal = slopeDetector.GetCollisionNormal(); //获取碰撞法线
            result.SlopeAngle = Mathf.Abs(Mathf.RadToDeg(Mathf.Acos(collisionNormal.Dot(Vector2.Up)))); //计算斜坡角度
            result.HasSteepSlope = result.SlopeAngle > 10f; //超过10度视为陡峭斜坡
        }

        result.HasObstacle = result.HasWall || result.HasCliff || result.HasSteepSlope;

        //=== 综合判断 ===
        //能追逐玩家的条件：看到玩家 + 前方无障碍物 + 高度差合适
        result.CanChasePlayer = result.CanSeePlayer && !result.HasObstacle && result.HeightDifference < 15f;

        //巡逻时是否应该转身的条件: 前方有障碍物 + 转身冷却结束
        result.ShouldTurnAround = result.HasObstacle && turnAroundCooldown <= 0;

        return result;
    }

    protected void SetFacingDirection(bool faceRight) //统一设置面向方向方法
    {
        if (isfacingright == faceRight) return; // 方向相同则跳过
        isfacingright = faceRight;

        // 翻转动画
        animatedSprite.FlipH = !faceRight;

        // 翻转检测器
        playerDetectorRight.Enabled = faceRight;
        playerDetectorLeft.Enabled = !faceRight;

        var scale = playerDetectorRight.Scale;
        scale.X = faceRight ? 1 : -1;
        wallDetector.Scale = scale;
        cliffDetector.Scale = scale;
        slopeDetector.Scale = scale;
    }
         
    protected void TurnAround() //统一转身方法（巡逻时使用）
    {
        SetFacingDirection(!isfacingright);       
        patrolStartPosition = GlobalPosition; // 重置巡逻起始位置
        turnAroundCooldown = turnAroundCooldownDuration; // 启动冷却
    }

    protected void FacePlayer() //面向玩家方法
    {
        if (directionCheck.CanSeePlayer)
        {
            SetFacingDirection(directionCheck.PlayerOnRight);
        }
    }

    protected virtual void ApplyGravity(double delta) //应用重力方法
    {
        if (!IsOnFloor())
        {
            Velocity += new Vector2(Velocity.X, attributes.BasicGravity * (float)delta);
        }
        else
        {
            Velocity = new Vector2(Velocity.X, 0);
        }
    }

    //===信号方法===
    protected void OnAttackAreaBodyEntered(Node boby)
    {
        if(currentState == EnemyState.attack)
        {
            if (boby is Player targetPlayer)
            {
                targetPlayer.Attributes.TakeDamage(attributes.AttackPower);
            }
        }
    }
}
