using Godot;
using System;

public partial class HurtState : State //受伤状态
{
    private double hurtTimer = 0f; //受伤计时器
    private const double HurtDuration = 0.6f; //受伤持续时间（秒）

    public override void Enter()
    {
        hurtTimer = 0f;
        player.AnimationPlayback("hurt");
    }

    public override void Exit()
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        hurtTimer += delta;

        if (hurtTimer < 0.3)
        {
            // 只在空中时应用重力，不减速水平移动
            if (!player.IsOnFloor())
            {
                player.Velocity += new Vector2(0, player.BasicGravity * (float)delta);
            }
        }
        else
        {
            // 0.3秒后开始减速
            player.Velocity = player.Velocity.MoveToward(Vector2.Zero, 200f * (float)delta);
        }

        // 强制等待受伤时间结束
        if (hurtTimer < HurtDuration)
        {
            return;
        }

        float absSpeed = Mathf.Abs(player.currentspeed);

        if (player.IsOnFloor() && absSpeed <= 10f)
        {
            EmitSignal(nameof(StateFinished), "IdleState");
            return;
        }
        if (Input.IsActionPressed("left") || Input.IsActionPressed("right") && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "Attack1State");
            return;
        }
        if (Input.IsActionJustPressed("jump"))
        {
            EmitSignal(nameof(StateFinished), "JumpState");
            return;
        }
    }
}
