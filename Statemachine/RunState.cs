using Godot;
using System;

public partial class RunState : State //奔跑状态
{
    private bool isHurt = false; //是否受伤
    
    public override void Enter()
    {
        player.AnimationPlayback("run");
        player.Attributes.HurtChanged += OnHurtChanged; 
    }

    public override void Exit()
    {
        player.Attributes.HurtChanged -= OnHurtChanged;
    }


    public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (Input.IsActionJustPressed("jump") || !player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "RunJumpState"); //助跑跳状态
            return;
        }
        if (absSpeed <= 130f && absSpeed > 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }
        if (Input.IsActionJustPressed("attack"))
        {
            EmitSignal(nameof(StateFinished), "Attack1State"); //切换到攻击状态
            return;
        }
        if (Input.IsActionJustPressed("sprint"))
        {
            EmitSignal(nameof(StateFinished), "SprintState");
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
