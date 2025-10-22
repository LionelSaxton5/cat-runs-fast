using Godot;
using System;

public partial class WakingState : State //起身状态
{
    public override void Enter()
    {
		player.AnimationPlayback("waking"); //起身动画
        player.cat.AnimationFinished += OnWakingAnimationFinished;
    }

    public override void Exit()
    {
        player.cat.AnimationFinished -= OnWakingAnimationFinished;
    }

    public override void PhysicsUpdate(double delta)
	{
       player.currentspeed = 0f; //起身时速度为0
    }

    private void OnWakingAnimationFinished()
    {
        EmitSignal(nameof(StateFinished), "IdleState");
    }
}
