using Godot;
using System;

public partial class SprintState : State //冲刺状态
{
	public bool isSprinting = false; //是否正在冲刺
    private Timer sprintTimer; //冲刺计时器

    public override void Enter()
    {
        player.AnimationPlayback("sprint"); //冲刺动画

        player.cat.AnimationFinished += OnSprintAnimationFinished;

        isSprinting = true; //设置为冲刺状态
        sprintTimer = new Timer();
        sprintTimer.WaitTime = 0.2f; //冲刺持续时间0.2秒
        sprintTimer.IgnoreTimeScale = true; //忽略时间缩放
        sprintTimer.OneShot = true; //单次计时器
        AddChild(sprintTimer); //添加为子节点
        sprintTimer.Timeout += OnSprintTimerTimeout; //连接计时器超时信号
        sprintTimer.Start(); //启动计时器

        player.Attributes.StaminaReduction(10); //消耗体力
    }

    public override void Exit()
    {
        isSprinting = false; //退出冲刺状态
        player.cat.AnimationFinished -= OnSprintAnimationFinished;

        if (sprintTimer != null)
        {
            sprintTimer.Timeout -= OnSprintTimerTimeout; //断开计时器超时信号
            sprintTimer.QueueFree(); //移除计时器节点
            sprintTimer = null;
        }
    }
	
	public override void PhysicsUpdate(double delta)
	{
        if (isSprinting) //已经在冲刺中则返回
        {
            var direction = player.isfacingright ? 1 : -1;
            player.Velocity = new Vector2(direction * 400f, 100f);
            return;
        }

        float absSpeed = Mathf.Abs(player.currentspeed);

        if (player.wallDetector.IsColliding())
        {
            EmitSignal(nameof(StateFinished), "SlideState");
            return;
        }
        if (absSpeed > 130f )
        {
            EmitSignal(nameof(StateFinished), "RunState"); //切换到奔跑状态
            return;
        }
        if (absSpeed <= 130f && absSpeed > 10f)
        {
            EmitSignal(nameof(StateFinished), "WalkState"); //切换到行走状态
            return;
        }
        if (absSpeed < 10f)
        {
            EmitSignal(nameof(StateFinished), "IdleState"); //切换到待机状态
            return;
        }
        if (!player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "JumpState");
            return;
        }
    }

    public void OnSprintAnimationFinished()
    {
        isSprinting = false; //冲刺动画结束，设置为非冲刺状态
    }

    private void OnSprintTimerTimeout()
    {
        isSprinting = false; //冲刺计时器超时，防止卡住
    }
}
