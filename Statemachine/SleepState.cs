using Godot;
using System;

public partial class SleepState : State //睡觉状态
{
    private bool isHurt = false;
    public override void Enter()
    {
        player.AnimationPlayback("sleep"); //睡觉

        player.Attributes.HurtChanged += OnHurtChanged;
    }

    public override void Exit()
    {
        player.Attributes.HurtChanged -= OnHurtChanged;
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
