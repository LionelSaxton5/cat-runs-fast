using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float speed = 1000; //移动速度
	[Export] public float JumpVelocity = -300; //初始跳跃速度
    [Export] public float BasicGravity = 200; //基础重力
	[Export] public float VariableGravity = 400; //可变重力

	private AnimatedSprite2D animatedSprite;

    private bool isfacingright = true; //是否面向右侧

    //速度相关
    private float currentspeed = 0; //当前速度
    private float acceleration = 0; //加速度

    public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("Person");
    }

	public override void _PhysicsProcess(double delta)
	{
        float deltaF = (float)delta;
        var velocity = Velocity; //初始化移动向量

        HorizontalMovement(ref velocity, deltaF);
        VerticalMovement(ref velocity, deltaF);


        Velocity = velocity;

        MoveAndSlide(); //内置方法移动
    }	

    private void HorizontalMovement(ref Vector2 velocity, float delta) //水平移动方法
    {

        var direction = Input.GetAxis("left", "right"); //获取水平输入
        var targetspeed = direction * speed; //计算目标速度

        if (IsOnFloor()) //在地面上
        {
            acceleration = direction != 0 ? 100 : 150; //如有输入则加速，否则减速
        }
        if(!IsOnFloor()) //在空中,加速度稍微减小
        {
            acceleration = direction != 0 ? 50 : 75; //如有输入则加速，否则减速
        }

        velocity.X = Mathf.MoveToward(currentspeed, targetspeed, acceleration * delta); //平滑过渡到目标速度
       
        if (direction > 0) Flip(true);
        if (direction <= 0) Flip(false);
        
    }

    private void VerticalMovement(ref Vector2 velocity, float delta)
    {
        if (Input.IsActionJustPressed("jump")) //按下触发跳跃
        {
            velocity.Y = JumpVelocity;
        }
        else if (Input.IsActionPressed("jump")) //持续按应用较小重力
        {
            velocity.Y += BasicGravity;
        }
        else if (Input.IsActionJustReleased("jump")) //松开跳跃键应用较大重力
        {
            velocity.Y += VariableGravity;
        }

    }

    private void Flip(bool facingright) //反转人物
    {
        isfacingright = facingright; //更新方向
        this.Scale = new Vector2(facingright ? Mathf.Abs(Scale.X) : -Mathf.Abs(Scale.X), Scale.Y);
    }
}
