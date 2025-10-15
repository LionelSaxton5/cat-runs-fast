using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float speed = 150; //移动速度
    [Export] public float JumpVelocity = -1000f; //初始跳跃速度
    [Export] public float BasicGravity = 2000f; //基础重力
    [Export] public float VariableGravity = 3000f; //可变重力

    //动画播放相关
    private AnimatedSprite2D person; //人物动画节点
    private AnimatedSprite2D cat; //猫咪动画节点
    private string Currentanimation = ""; //当前动画名称

    private bool isfacingright = true; //是否面向右侧

    //速度相关
    private float currentspeed = 0f; //当前速度
    private float acceleration = 0f; //加速度
    private float MaxFallSpeed = 600f; //最大下落速度

    //跳跃相关
    private bool isJumping = false; //是否在跳跃   
    private float jumpBufferTime = 0.0f; //跳跃缓冲时间，“容错”机制，允许玩家在“快要落地”时提前按下跳跃键，角色落地瞬间会自动跳起
    private const float JumpBufferDuration = 0.1f; //当前剩余的跳跃缓冲时间
    private bool jumpKeyReleased = false; // 跳跃键是否已释放

    public override void _Ready()
    {
        person = GetNode<AnimatedSprite2D>("Person");
        cat = GetNode<AnimatedSprite2D>("Cat");

        cat.Visible = false; //初始不可见
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaF = (float)delta;
        var velocity = Velocity; //初始化移动向量

        HorizontalMovement(ref velocity, deltaF);
        HandleVerticalMovement(ref velocity);
        ApplyVariableGravity(ref velocity, deltaF);


        Velocity = velocity;

        MoveAndSlide(); //内置方法移动
    }

    private enum CharacterStatus //人物状态枚举
    {
        idle,
        walk
    }

    private enum CatStatus //猫咪状态枚举
    {
        attack1, attack2, climb, death, eat, hurt, idle, jump, lick, run, runjump, scare, sleep, slide, waking, walk
    }

    private void StateSelection() //状态选择方法
    {
        CharacterStatus character = new CharacterStatus();
        CatStatus catStatus = new CatStatus();
        if (person.Visible)
        {
            switch (character)
            {
                case CharacterStatus.idle:
                    break;
                case CharacterStatus.walk:
                    break;
            }
        }
        if (cat.Visible)
        {
            switch (catStatus)
            {
                case CatStatus.idle: //待机
                    break;
                case CatStatus.walk: //走路
                    break;
                case CatStatus.jump: //跳跃
                    break;
                case CatStatus.run: //奔跑
                    break;
                case CatStatus.runjump: //助跑跳
                    break;
                case CatStatus.attack1: //攻击1
                    break;
                case CatStatus.attack2: //攻击2
                    break;
                case CatStatus.climb: //攀爬
                    break;
                case CatStatus.death: //死亡
                    break;
                case CatStatus.eat: //吃东西
                    break;
                case CatStatus.hurt: //受伤
                    break;
                case CatStatus.scare: //吓唬
                    break;
                case CatStatus.sleep: //睡觉
                    break;
                case CatStatus.slide: //滑行
                    break;
                case CatStatus.waking: //醒来
                    break;
                case CatStatus.lick: //舔
                    break;
                default:
                    break;
            }
        }
    }

    private void AnimationPlayback(string animation) //动画播放方法
    {
        if (Currentanimation == animation) //动画相同则返回
            return;
        Currentanimation = animation;

        if (person.Visible)
        {
            person.Play(animation);
        }
        else if (cat.Visible)
        {
            cat.Play(animation);
        }
    }

    private void HorizontalMovement(ref Vector2 velocity, float delta) //水平移动方法
    {
        var direction = Input.GetAxis("left", "right"); //获取水平输入
        var targetspeed = direction * speed; //计算目标速度

        if (IsOnFloor()) //在地面上
        {
            acceleration = direction != 0 ? 1200 : 600; //如有输入则加速，否则减速
        }
        else if(!IsOnFloor()) //在空中,加速度稍微减小
        {
            acceleration = (direction != 0) ? 800 : 300; //如有输入则加速，否则减速
        }

        currentspeed = Mathf.MoveToward(currentspeed, targetspeed, acceleration * delta); //平滑过渡到目标速度
        currentspeed = Mathf.Clamp(currentspeed, -speed ,speed); //限制速度
        velocity.X = currentspeed; //设置速度

        if (direction != 0)
        {
            if (direction > 0) Flip(true);
            if (direction < 0) Flip(false);
        }       
    }

    private void HandleVerticalMovement(ref Vector2 velocity) // 处理垂直移动
    {  
         if (Input.IsActionJustPressed("jump")) //按下触发跳跃
         {
            jumpBufferTime = JumpBufferDuration; //重置跳跃缓冲时间
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
        velocity.Y = JumpVelocity; //初始速度

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
    }

    private void ApplyVariableGravity(ref Vector2 velocity, float delta) //可变重力曲线
    {
        if (!IsOnFloor())
        {
            if (isJumping) //在跳跃过程中
            {
                if (velocity.Y < 0) //上升阶段
                {
                    if (jumpKeyReleased) //跳跃键已释放
                    {
                        velocity.Y += VariableGravity * delta; //增加重力,可变重力
                    }
                    else
                    {
                        velocity.Y += BasicGravity * delta; //基础重力
                    }
                    if (velocity.Y > 0) //达到跳跃顶点
                    {
                        isJumping = false; //结束跳跃
                    }
                }              
            }
            else //下降阶段,跳跃顶点检查
            {
                velocity.Y += VariableGravity * 2f * delta; //增加重力

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
        }
    }

    private void Flip(bool facingright) //反转人物
    {
        if (isfacingright == facingright) return; //如果方向没有变化则返回

        isfacingright = facingright; //更新方向
        person.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);
        cat.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);
    }
}
