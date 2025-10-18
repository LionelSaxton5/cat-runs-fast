using Godot;
using System;

public partial class IdleState : State //待机状态
{	
    public override void Enter() //进入状态时调用,由StateMachine状态机调用
    {
		GD.Print("进入待机状态");
		player.AnimationPlayback("idle"); //播放待机动画
        player.idletimer.Start(); //开始待机计时
		player.idletimer.Timeout += OnIdleTimerTimeout;//连接计时器超时信号
    }

	public override void Exit() //退出状态时调用
	{
		player.idletimer.Stop(); //停止待机计时
		player.idletimer.Timeout -= OnIdleTimerTimeout; //断开计时器
    }

	public override void PhysicsUpdate(double delta) //每帧物理更新时调用
	{		
		//判断待机状态与会哪些状态进行转换
		if (Input.IsActionPressed("left") || Input.IsActionPressed("right"))
		{
			EmitSignal(nameof(StateFinished), "WalkState"); //切换到跑步状态
			return;
        }

		if (Input.IsActionJustPressed("jump") && player.IsOnFloor())
		{
			EmitSignal(nameof(StateFinished), "JumpState"); //切换到跳跃状态
			return;
        }

		if (Input.IsActionJustPressed("attack"))
		{
			EmitSignal(nameof(StateFinished), "Attack1State"); //切换到攻击状态
			return;
        }

    }

	private void OnIdleTimerTimeout() //计时器超时处理函数
	{
		if (player.cat.Visible)
		{
			Random random = new Random();//创建随机数生成器
			int randomNumber = random.Next(0, 2); //生成0或1的随机数

			if (randomNumber == 0) EmitSignal(nameof(StateFinished), "SleepState"); //切换到睡觉状态        
			else EmitSignal(nameof(StateFinished), "LickState"); //切换到舔状态  
		}
    }

}
