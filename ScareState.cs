using Godot;
using System;

public partial class ScareState : State //吓唬状态
{
    public bool isscaring = false; //是否正在吓唬

    public override void Enter()
    {
        player.AnimationPlayback("scare"); //吓唬动画
        isscaring = true;
        player.cat.AnimationFinished += OnScareAnimationFinished; //连接动画完成信号

        player.Velocity = new Vector2(0, player.Velocity.Y); //吓唬时水平速度为0
    }
	
    public override void Exit()
    {
        isscaring = false;
        player.cat.AnimationFinished -= OnScareAnimationFinished; //断开动画完成信号
    }


    public override void PhysicsUpdate(double delta)
	{
        if (isscaring) return;

        float absSpeed = Mathf.Abs(player.currentspeed);

        if (absSpeed <= 10f)
        {
            EmitSignal(nameof(StateFinished), "IdleState");
            return;
        }
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
    }

    private void OnScareAnimationFinished()
    {       
        isscaring = false;                    
    }
}
