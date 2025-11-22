using Godot;
using System;

public partial class LickState : State //舔状态
{
    private bool isHurt = false;

    public override void Enter()
    {
        player.AnimationPlayback("lick"); //舔动画
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
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }
        if (Input.IsActionJustPressed("jump"))
        {
            EmitSignal(nameof(StateFinished), "JumpState"); 
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "Attack1State");
            return;
        }
        if (Input.IsActionJustPressed("scare") && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "ScareState");
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
