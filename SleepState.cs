using Godot;
using System;

public partial class SleepState : State //睡觉状态
{

    public override void Enter()
    {
        player.AnimationPlayback("sleep"); //睡觉
    }
	
	public override void PhysicsUpdate(double delta)
	{
        if (Input.IsActionPressed("left") || Input.IsActionPressed("right") && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WakingState"); //切换到起身状态
            return;
        }
        if (Input.IsActionJustPressed("jump"))
        {
            EmitSignal(nameof(StateFinished), "WakingState"); 
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "WakingState");
            return;
        }        
    }
}
