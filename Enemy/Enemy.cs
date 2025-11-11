using Godot;
using System;

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
    private Player player;

    //===探测器相关===
    private RayCast2D playerDetector; //玩家检测器
    private RayCast2D wallDetector; //墙壁检测器
    private RayCast2D cliffDetector; //悬崖检测器


    public override void _Ready()
	{
		currentState = initialState; //设置当前状态为初始状态
		rng.Randomize(); //初始化随机数生成器

        player = GetTree().GetNodesInGroup("Player")[0] as Player;
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        playerDetector = GetNode<RayCast2D>("PlayerDetector");
        wallDetector = GetNode<RayCast2D>("PlayerDetector/WallDetector");
        cliffDetector = GetNode<RayCast2D>("PlayerDetector/CliffDetector");

        EnterState(currentState);
    }


	public override void _PhysicsProcess(double delta)
	{
		stateTimer += delta; //更新状态计时器
        UpdateState(currentState, delta);

        // 基础的物理移动（所有状态通用）
        MoveAndSlide();
    }

	protected virtual void ChangeState(EnemyState newState) //切换状态方法,参数为要切换到的新状态
    {
		if (currentState == newState) return; //如果新状态与当前状态相同,则不进行状态切换

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
        //如果看到玩家，切换到追逐状态
        if (CanSeePlayer())
        {
            ChangeState(EnemyState.run);
            return;
        }

        if (stateTimer > 2.0) // 2秒后开始巡逻
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
    }

    protected virtual void UpdateWalk(double delta)
    {
        // 如果看到玩家，切换到追逐状态
        if (CanSeePlayer())
        {
            ChangeState(EnemyState.run);
            return;
        }

        // 基础巡逻移动
        Velocity = new Vector2(50 * (isfacingright ? 1 : -1), Velocity.Y);

        // 遇到墙壁或悬崖就转身
        if (wallDetector.IsColliding() || !cliffDetector.IsColliding())
        {
            isfacingright = !isfacingright;
            Filp(isfacingright);
            FilpDetector(isfacingright);
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
        Velocity = new Vector2(direction.X * 100, Velocity.Y); //设置水平速度为100，垂直速度保持不变

        //更新方向
        if (Mathf.Abs(direction.X) > 0.1f) //玩家在敌人左右移动时更新面向方向
        {
            bool facingright = direction.X > 0; // >0表示玩家在右侧，<0表示在左侧
            Filp(facingright);
            FilpDetector(facingright);
        }

        if (GlobalPosition.DistanceTo(player.GlobalPosition) < 30f) //接近玩家时切换到攻击状态
        {
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
    protected virtual bool CanSeePlayer() //检测是否看到玩家,返回bool值
    {
        return playerDetector.IsColliding() && playerDetector.GetCollider() is Player;
    }

    public virtual void Filp(bool facingright)
	{ 
		if (isfacingright == facingright) return;
		isfacingright = facingright;

		animatedSprite.FlipH = !facingright;
    }

	public virtual void FilpDetector(bool facingright)
	{
        if (isfacingright == facingright) return;
        isfacingright = facingright;

		var scale = playerDetector.Scale;
		scale.X = facingright ? 1 : -1;

		playerDetector.Scale = scale;
		wallDetector.Scale = scale;
		cliffDetector.Scale = scale;
    }
}
