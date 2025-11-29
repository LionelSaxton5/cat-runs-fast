using Godot;
using System;

public partial class JumpState : State //跳跃状态
{
    private bool isHurt = false; //是否受伤
    
    public override void Enter()
    {
		player.AnimationPlayback("jump");
        player.Attributes.HurtChanged += OnHurtChanged; //订阅血量变化信号
    }

    public override void Exit()
    {
        player.Attributes.HurtChanged -= OnHurtChanged;
    }
	
	public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (Input.IsActionJustPressed("sprint"))
        {
            EmitSignal(nameof(StateFinished), "SprintState");
            return;
        }
        if (player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "IdleState");
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "Attack1State");
            return;
        }
        if(player.wallDetector.IsColliding()) //检测到墙壁碰撞
        {
            bool pressingTowardWall = (player.isfacingright && Input.IsActionPressed("right") || 
                                       !player.isfacingright && Input.IsActionPressed("left")); //是否正朝墙壁方向按键
            bool isFalling = player.Velocity.Y > 0; //正在下落

            if (pressingTowardWall && isFalling)
            {
                EmitSignal(nameof(StateFinished), "SlideState"); //切换到滑墙状态
                return;
            }
        }
        if (isHurt)
        {
            EmitSignal(nameof(StateFinished), "HurtState");
            isHurt = false;
            return;
        }
    }
    public void OnHurtChanged()
    {
        isHurt = true;
    }
}
