using Godot;
using System;

public partial class RunJumpState : State //助跑跳状态
{
    public override void Enter()
    {
        player.AnimationPlayback("runjump"); //助跑跳动画
    }
	
	public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (player.wallDetector.IsColliding())
        {
            EmitSignal(nameof(StateFinished), "SlideState");
            return;
        }
        if (absSpeed > 130f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "RunState");
            return;
        }
        if(absSpeed <= 130f && absSpeed > 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }
        if (absSpeed <= 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "IdleState");
            return;
        }
        if (Input.IsActionJustPressed("sprint"))
        {
            EmitSignal(nameof(StateFinished), "SprintState");
            return;
        }
    }
}
