using Godot;
using System;

public partial class JumpState : State //跳跃状态
{

    public override void Enter()
    {
		player.AnimationPlayback("jump");
    }

    public override void Exit()
    {
        
    }
	
	

	public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (absSpeed < 10f)
        {
            EmitSignal(nameof(StateFinished), "IdleState");
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "Attack1State");
            return;
        }
	}
}
