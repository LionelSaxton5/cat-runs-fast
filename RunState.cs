using Godot;
using System;

public partial class RunState : State //奔跑状态
{

    public override void Enter()
    {
        player.AnimationPlayback("run");
    }
	


	public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (Input.IsActionJustPressed("jump") && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "RunJumpState"); //助跑跳状态
            return;
        }
        if (absSpeed < 130f)
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "Attack1State"); //切换到攻击状态
            return;
        }
    }
}
