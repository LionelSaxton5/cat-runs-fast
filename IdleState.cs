using Godot;
using System;

public partial class IdleState : State //待机状态
{
	private Timer idletimer; //待机计时器
    private bool isTimerConnected = false; //计时器信号是否已连接

    public override void Enter() //进入状态时调用,由StateMachine状态机调用
    {
        if (idletimer == null)
        {
            idletimer = player.GetNodeOrNull<Timer>("IdleTimer");
        }

		player.AnimationPlayback("idle"); //播放待机动画

		if (idletimer != null)
		{
			idletimer.Start(); //开始待机计时
			idletimer.Timeout += OnIdleTimerTimeout;//连接计时器超时信号
			isTimerConnected = true;
        }
    }

	public override void Exit() //退出状态时调用
	{
		if (idletimer != null && isTimerConnected)
		{
			idletimer.Stop(); //停止待机计时
			idletimer.Timeout -= OnIdleTimerTimeout;
			isTimerConnected= false;
		}
    }

	public override void PhysicsUpdate(double delta) //每帧物理更新时调用
	{		
		//判断待机状态与会哪些状态进行转换
		if (Input.IsActionPressed("left") || Input.IsActionPressed("right") && player.IsOnFloor())
		{
			EmitSignal(nameof(StateFinished), "WalkState"); //切换到行走状态
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

	private void OnIdleTimerTimeout() //计时器超时处理函数
	{
		if (player.cat.Visible)
		{
			Random random = new Random();//创建随机数生成器
			int randomNumber = random.Next(0, 2); //生成0或1的随机数

			if (randomNumber == 0) EmitSignal(nameof(StateFinished), "LieDownState"); //切换到躺下状态        
			else EmitSignal(nameof(StateFinished), "LickState"); //切换到舔状态  
		}
    }

}
