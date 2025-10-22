using Godot;
using System;

public partial class LieDownState : State //躺下状态
{

    public override void Enter()
    {
        player.AnimationPlayback("liedown"); //躺下动画 
        player.cat.AnimationFinished += OnLieDownAnimationFinished;
    }
	
    public override void Exit()
    {
        player.cat.AnimationFinished -= OnLieDownAnimationFinished;
    }


    public override void PhysicsUpdate(double delta)
	{
	}

    public void OnLieDownAnimationFinished()
    {
        EmitSignal(nameof(StateFinished), "SleepState");
    }
}
