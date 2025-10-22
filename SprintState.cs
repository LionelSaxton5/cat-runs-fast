using Godot;
using System;

public partial class SprintState : State //冲刺状态
{
	public bool isSprinting = false; //是否正在冲刺

    public override void Enter()
    {
        player.AnimationPlayback("sprint"); //冲刺动画
    }
	
	public override void PhysicsUpdate(double delta)
	{
        if (isSprinting) //已经在冲刺中则返回
        {
            player.Velocity = new Vector2(Math.Abs(200), player.Velocity.Y); //保持冲刺速度
        }

        float absSpeed = Mathf.Abs(player.currentspeed);

        if (absSpeed > 130f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "RunState"); //切换到奔跑状态
            return;
        }
        if (absSpeed <= 130f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WalkState"); //切换到行走状态
            return;
        }
        if (absSpeed < 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "IdleState"); //切换到待机状态
            return;
        }

    }
}
