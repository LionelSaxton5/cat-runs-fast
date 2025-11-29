using Godot;
using System;

public partial class LieDownState : State //躺下状态
{
    private bool isHurt = false;

    public override void Enter()
    {
        player.AnimationPlayback("liedown"); //躺下动画 
        player.cat.AnimationFinished += OnLieDownAnimationFinished;
        player.Attributes.HurtChanged += OnHurtChanged;
    }
	
    public override void Exit()
    {
        player.cat.AnimationFinished -= OnLieDownAnimationFinished;
        player.Attributes.HurtChanged -= OnHurtChanged;
    }


    public override void PhysicsUpdate(double delta)
	{
        if (isHurt)
        {
            EmitSignal(nameof(StateFinished), "HurtState");
            isHurt = false;
            return;
        }
    }

    public void OnLieDownAnimationFinished()
    {
        EmitSignal(nameof(StateFinished), "SleepState");
    }

    public void OnHurtChanged()
    {
        isHurt = true;
    }
}
