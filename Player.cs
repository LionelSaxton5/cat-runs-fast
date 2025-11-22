using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    [Export] public float speed = 150; //移动速度
    [Export] public float JumpVelocity = -600f; //初始跳跃速度
    [Export] public float BasicGravity = 2000f; //基础重力
    [Export] public float VariableGravity = 3000f; //可变重力
    [Export] public CharacterAttributes Attributes { get; private set; } //角色属性节点

    public bool isfacingright = true; //是否面向右侧

    //检测器节点相关
    public RayCast2D wallDetector; //墙壁检测器
    public RayCast2D cliffDetector; //悬崖检测器

    //计时器相关
    public Timer idletimer;

    //动画播放相关
    private AnimatedSprite2D person; //人物动画节点
    public AnimatedSprite2D cat; //猫咪动画节点
    private string Currentanimation = ""; //当前动画名称    

    //状态及相关
    private Attack1State attackState1; //普通攻击状态节点
    private SlideState slideState; //滑墙状态节点
    private SprintState sprintState; //冲刺状态节点

    //速度相关
    public float currentspeed = 0f; //当前速度
    public float acceleration = 0f; //加速度
    public float MaxFallSpeed = 600f; //最大下落速度

    //跳跃相关
    private bool isJumping = false; //是否在跳跃   
    private float jumpBufferTime = 0.0f; //跳跃缓冲时间，“容错”机制，允许玩家在“快要落地”时提前按下跳跃键，角色落地瞬间会自动跳起
    private const float JumpBufferDuration = 0.1f; //当前剩余的跳跃缓冲时间
    private bool jumpKeyReleased = false; // 跳跃键是否已释放

    //攻击相关
    private Area2D attackArea; //攻击范围节点

    public override void _Ready()
    {
        //初始化节点
        person = GetNode<AnimatedSprite2D>("Person");
        cat = GetNode<AnimatedSprite2D>("Cat");
        idletimer = GetNode<Timer>("IdleTimer");
        wallDetector = GetNode<RayCast2D>("WallDetector");
        cliffDetector = GetNode<RayCast2D>("WallDetector/CliffDetector");
        attackArea = GetNode<Area2D>("AttackArea2D");
        //cat.Visible = false; //初始不可见

        //获取状态机子节点
        attackState1 = GetNode<Attack1State>("StateMachine/Attack1State"); //获取普通攻击状态节点
        slideState = GetNode<SlideState>("StateMachine/SlideState"); //获取滑墙状态节点
        sprintState = GetNode<SprintState>("StateMachine/SprintState"); //获取冲刺状态节点

        if (CharacterUi.Instance != null)
        {
            CharacterUi.Instance.Visible = true; //显示角色UI
        }       
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaF = (float)delta;
        var velocity = Velocity; //初始化移动向量

        if (jumpBufferTime > 0) jumpBufferTime -= deltaF; //减少跳跃缓冲时间

        if (!slideState.isSliding && !sprintState.isSprinting) //滑墙或冲刺状态
        {
            HorizontalMovement(ref velocity, deltaF);
        }
        HandleVerticalMovement(ref velocity);
        ApplyVariableGravity(ref velocity, deltaF);

        Velocity = velocity;

        MoveAndSlide(); //内置方法移动
    }  

    public void AnimationPlayback(string animation) //动画播放方法
    {
        if (Currentanimation == animation) //动画相同则返回
            return;
        Currentanimation = animation;

        if (person.Visible && person != null)
        {
            person.Play(animation);
        }
        else if (cat.Visible && cat != null) 
        {
            cat.Play(animation);
        }
        else
        {
            GD.PrintErr($"AnimationPlayback: 无法播放 {animation}，person 和 cat 都不可见或为 null");
        }
    }

    private void HorizontalMovement(ref Vector2 velocity, float delta) //水平移动方法
    {
        var direction = Input.GetAxis("left", "right"); //获取水平输入

        var effectiveSpeed = Attributes.GetEffectiveMoveSpeed(speed); //获取有效移动速度
        var targetspeed = direction * effectiveSpeed; //计算目标速度

        if (IsOnFloor()) //在地面上
        {
            if (attackState1.isAttack)
            {
                acceleration = 900f; //攻击时加速度增加
                targetspeed = direction * 30f; //攻击时目标速度为0
            }
            acceleration = direction != 0 ? 600f : 400f; //如有输入则加速，否则减速
        }
        else if(!IsOnFloor()) //在空中,加速度稍微减小
        {
            acceleration = (direction != 0) ? 300f : 100f; //如有输入则加速，否则减速
        }

        currentspeed = Mathf.MoveToward(currentspeed, targetspeed, acceleration * delta); //平滑过渡到目标速度
        currentspeed = Mathf.Clamp(currentspeed, -speed ,speed); //限制速度
        velocity.X = currentspeed; //设置速度

        if (direction != 0)
        {
            if (direction > 0) //向右移动
            {
                Flip(true);
                InversionDetector(true);
            }
            if (direction < 0)
            {
                Flip(false);
                InversionDetector(false);
            }
        }       
    }

    private void HandleVerticalMovement(ref Vector2 velocity) // 处理垂直移动
    {  
         if (Input.IsActionJustPressed("jump")) //按下触发跳跃
         {
            jumpBufferTime = JumpBufferDuration; //重置跳跃缓冲时间
            jumpKeyReleased = false; // 按下时重置，确保上升阶段采用按住的重力
         }
         if (jumpBufferTime > 0 && IsOnFloor()) //跳跃缓冲时间大于0且在地面上
         {
            StartJump(ref velocity);
            jumpBufferTime = 0; //重置缓冲时间
         }
         if (Input.IsActionJustReleased("jump")) //提前松开跳跃键
         {
             jumpKeyReleased = true; //释放   
         }        
    }

    private void StartJump(ref Vector2 velocity) //开始跳跃
    {
        var effectiveJumpPower = Attributes.GetEffectiveJumpPower(JumpVelocity); //获取有效跳跃速度
        velocity.Y = effectiveJumpPower; //初始速度

        if (Mathf.Abs(currentspeed) > 0)
        {
            velocity.X = currentspeed * 1.6f; //跳跃时有水平加速
            velocity.X = Mathf.Clamp(velocity.X, -speed * 1.6f, speed * 1.6f); //限制水平速度
        }
        else
        {
            velocity.X = currentspeed; //保持原速
        }       

        isJumping = true; //设置跳跃状态
        jumpKeyReleased = false; //重置跳跃键释放状态
    }

    private void ApplyVariableGravity(ref Vector2 velocity, float delta) //可变重力曲线
    {
        if (slideState.isSliding)
        { 
            velocity.Y += 100f * delta; //滑墙时固定重力,缓慢下滑
            return;
        }

        bool groundedThisFrame = IsOnFloor() && velocity.Y >= 0; //在地面

        if (!groundedThisFrame)
        {
            if (isJumping) //在跳跃过程中
            {               
                if (jumpKeyReleased) //跳跃键已释放
                {
                    velocity.Y += VariableGravity * 3.0f * delta; //增加重力,可变重力
                }
                else
                {
                    velocity.Y += BasicGravity * delta; //基础重力
                }
                if (velocity.Y >= 0) //达到跳跃顶点
                {
                    isJumping = false; //结束跳跃
                }                            
            }
            else //下降阶段,跳跃顶点检查
            {
                velocity.Y += VariableGravity * delta; //增加重力

                if (velocity.Y > MaxFallSpeed) //限制最大下落速度
                {
                    velocity.Y = MaxFallSpeed;
                }
            }
        }
        else
        {
            //在地面上，重置跳跃状态和跳跃键释放状态
            isJumping = false;
            jumpKeyReleased = false;
            if (velocity.Y > 0) velocity.Y = 0; // 避免在地面残余向下速度
        }
    }
    
    private void Flip(bool facingright) //反转人物
    {
        if (isfacingright == facingright) return; //如果方向没有变化则返回

        isfacingright = facingright; //更新方向
        person.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);
        cat.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);

        if (attackArea != null)
        {
            attackArea.Position = new Vector2(facingright ? Mathf.Abs(attackArea.Position.X) : -Mathf.Abs(attackArea.Position.X), attackArea.Position.Y);
        }
    }

    private void InversionDetector(bool facingright) //反转检测器
    {
        if (isfacingright == facingright) return;

        isfacingright = facingright; //更新方向
        wallDetector.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);
        cliffDetector.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);
    }
}
