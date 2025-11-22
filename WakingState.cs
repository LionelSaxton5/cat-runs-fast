using Godot;
using System;

public partial class WakingState : State //起身状态
{
    private bool isHurt = false;
    public override void Enter()
    {
		player.AnimationPlayback("waking"); //起身动画
        player.cat.AnimationFinished += OnWakingAnimationFinished;
        player.Attributes.HurtChanged += OnHurtChanged;
    }

    public override void Exit()
    {
        player.cat.AnimationFinished -= OnWakingAnimationFinished;
        player.Attributes.HurtChanged -= OnHurtChanged;
    }

    public override void PhysicsUpdate(double delta)
	{
       player.currentspeed = 0f; //起身时速度为0

        if (isHurt)
        {
            EmitSignal(nameof(StateFinished), "HurtState");
            isHurt = false;
            return;
        }
    }

    private void OnWakingAnimationFinished()
    {
        EmitSignal(nameof(StateFinished), "IdleState");
    }

    public void OnHurtChanged()
    {
        isHurt = true;
    }
}
