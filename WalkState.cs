using Godot;
using System;

public partial class WalkState : State //行走状态
{
	public override void Enter()
	{
		GD.Print("进入行走状态");
		player.AnimationPlayback("walk"); //播放行走动画
    }

	public override void Exit()
	{
		
	}


    public override void PhysicsUpdate(double delta)
    {
        //判断行走状态与会哪些状态进行转换
        float absSpeed = Mathf.Abs(player.currentspeed);
		
        if (absSpeed > 130f && player.IsOnFloor())
		{
			EmitSignal(nameof(StateFinished), "RunState"); //切换到奔跑状态
			return;
        }
		if (absSpeed < 10f && player.IsOnFloor())
		{
			EmitSignal(nameof(StateFinished), "IdleState"); //切换到待机状态
			return;
        }
		if (Input.IsActionJustPressed("jump"))
		{
			EmitSignal(nameof(StateFinished), "JumpState"); //切换到跳跃状态
			return;
        }		
		if (Input.IsActionJustPressed("attack"))
		{
			EmitSignal(nameof(StateFinished), "Attack1State"); //切换到攻击状态
			return;
        }
        if (Input.IsActionJustPressed("scare") && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "ScareState"); //切换到吓唬状态
            return;
        }
        if (Input.IsActionPressed("down") && Input.IsActionPressed("jump")) //冲刺状态
        {
            EmitSignal(nameof(StateFinished), "SprintState");
            return;
        }
    }
}
