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
    private float patrolRadius = 250f; //巡逻半径
    private bool isPatrollingRight = true; //是否向右巡逻

    //===重力相关===
    private bool isOnFloor = false; //是否在地面上

    //===转身冷却相关===
    private double turnAroundCooldown = 0f; //转身冷却计时器
    private const float turnAroundCooldownDuration = 0.5f; // 转身冷却时间

    //===信号===


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
        //如果看到玩家,切换到追逐状态
        if (CanSeePlayer() && !ShouldTurnAround() && !HeightDifference())
        {
            Filp(PlayerPosition());
            FilpDetector(PlayerPosition());
            ChangeState(EnemyState.run);
            return;
        }

        if (stateTimer > 2.0 && !CanSeePlayer()) // 2秒后开始巡逻
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

        Filp(isPatrollingRight);
        FilpDetector(isPatrollingRight);
    }

    protected virtual void UpdateWalk(double delta)
    {
        // 如果看到玩家,切换到追逐状态
        if (CanSeePlayer() && !ShouldTurnAround() && !HeightDifference())
        {
            Filp(PlayerPosition());
            FilpDetector(PlayerPosition());
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
            GD.Print("[Walk] 超出巡逻范围，转身");
            TurnAround();
            return;
        }

        // 遇到墙壁或悬崖或斜坡就转身
        if (ShouldTurnAround() && turnAroundCooldown <= 0)
        {
            GD.Print("[Walk] 检测到障碍物，转身");
            TurnAround();
            return;
        }
        else if (ShouldTurnAround() && turnAroundCooldown > 0)
        {
            GD.Print($"[Walk] 检测到障碍物，但在冷却中 ({turnAroundCooldown:F2}秒)");
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
        if (!CanSeePlayer())
        { 
            ChangeState(EnemyState.idle);
            return;
        }

        //追逐逻辑
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized(); //计算方向向量并归一化
       
        if (ShouldTurnAround())
        {
            // 如果前方有障碍物,停止追逐
            if (HeightDifference()) //检测高度差
            {
                ChangeState(EnemyState.idle);
                return;
            }
            ChangeState(EnemyState.idle);
            return;
        }

        Velocity = new Vector2(direction.X * attributes.MoveSpeed * 2f, Velocity.Y); //设置水平速度为100，垂直速度保持不变

        //更新方向
        if (Mathf.Abs(direction.X) > 0.1f) //玩家在敌人左右移动时更新面向方向
        {           
            Filp(PlayerPosition());
            FilpDetector(PlayerPosition());
        }

        if (GlobalPosition.DistanceTo(player.GlobalPosition) < 30f) //接近玩家时切换到攻击状态
        {
            Filp(PlayerPosition());
            FilpDetector(PlayerPosition());
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
            if (CanSeePlayer())
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
    protected virtual bool ShouldTurnAround() //检测是否需要转身(综合检测墙壁、悬崖和斜坡),返回bool值
    {
        if (wallDetector.IsColliding())
        {
            return true; // 遇到墙壁，转身
        }
        if (!cliffDetector.IsColliding())
        {
            return true; // 遇到悬崖，转身
        }
        if (slopeDetector.IsColliding())
        { 
            Vector2 collisionNormal = slopeDetector.GetCollisionNormal(); //获取碰撞法线
            float slopeAngle = Mathf.Abs(Mathf.RadToDeg(Mathf.Abs(collisionNormal.Dot(Vector2.Up)))); //计算斜坡角度

            if (slopeAngle > 10f)
            {
                GD.Print($"斜坡角度过大: {slopeAngle:F2}度");
                return true; // 斜坡角度过大，转身
            }
        }
        return false;
    }

    protected virtual bool CanSeePlayer() //检测是否看到玩家,返回bool值
    {
        bool rightDetected = playerDetectorRight.IsColliding() && playerDetectorRight.GetCollider() is Player;
        bool leftDetected = playerDetectorLeft.IsColliding() && playerDetectorLeft.GetCollider() is Player;

        return rightDetected || leftDetected;
    }

    public virtual bool PlayerPosition() //检测玩家在敌人左侧还是右侧,返回bool值,true表示在右侧
    {
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
        return direction.X > 0;
    }

    public virtual bool HeightDifference() //检测与玩家高度差,返回bool值
    {
        float verticalDistance = Mathf.Abs(player.GlobalPosition.Y - GlobalPosition.Y);

        if (verticalDistance > 15f)
        {
            GD.Print($"高度差过大: 垂直={verticalDistance:F0}");
            return true; // 高度差过大
        }
        else
        {
            return false; // 高度差在可接受范围内
        }
    }

    public virtual void Filp(bool facingright)
	{ 
		isfacingright = facingright;

		animatedSprite.FlipH = !facingright;
    }

	public virtual void FilpDetector(bool facingright)
	{       
		playerDetectorRight.Enabled = facingright; //朝右侧检测器启动
        playerDetectorLeft.Enabled = !facingright; //朝左侧检测器启动

        var scale = playerDetectorRight.Scale; // 使用右侧检测器作为基准
        scale.X = facingright ? 1 : -1;

		wallDetector.Scale = scale;
		cliffDetector.Scale = scale;
        slopeDetector.Scale = scale;
    }

    private void TurnAround() //统一转身方法
    { 
        isfacingright = !isfacingright;
        Filp(isfacingright);
        FilpDetector(isfacingright);
        patrolStartPosition = GlobalPosition; // 重置巡逻起始位置
        turnAroundCooldown = turnAroundCooldownDuration; // 启动冷却
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
    private void OnAttackAreaBodyEntered(Node boby)
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
