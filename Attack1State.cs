using Godot;
using System;

public partial class Attack1State : State //普通攻击状态
{
    public int attackcount = 0; //攻击次数
    public bool isAttack = false; //是否正在攻击 
    private Timer attackTimer; //攻击计时器
    private bool enhancedattack = false; //是否进行强化攻击

    [Signal] public delegate void Attack1TriggeredEventHandler(); //普通攻击触发信号

    public override void Enter()
    {
        player.AnimationPlayback("attack1");

        EmitSignal(nameof(Attack1Triggered));

        isAttack = true;
        attackcount++;
        GD.Print("普通攻击次数: " + attackcount);

        if (player.cat.Visible)
        {
            GD.Print("连接攻击动画完成信号");
            player.cat.AnimationFinished += OnAttackAnimationFinished; //动画完成信号
        }

        //设置攻击计时器
        attackTimer = new Timer();
        attackTimer.WaitTime = 0.5f; //设置计时器时间为0.5秒
        attackTimer.OneShot = true; //设置为单次计时器
        AddChild(attackTimer); //将计时器添加为当前节点的子节点,使其生效
        attackTimer.Timeout += OnAttackTimerTimeout; //连接计时器超时信号
        attackTimer.Start(); //启动计时器
    }

    public override void Exit()
    {
        isAttack = false;
        if (player.cat.Visible)
        {
            GD.Print("断开攻击动画完成信号");
            player.cat.AnimationFinished -= OnAttackAnimationFinished; //断开动画完成信号
        }
        if (attackTimer != null)
        {
            attackTimer.Timeout -= OnAttackTimerTimeout; //断开计时器超时信号
            attackTimer.QueueFree(); //移除计时器节点
            attackTimer = null;
        }
    }

	public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (isAttack)
            return;
        if(attackcount == 3) enhancedattack = true; //准备进行强化攻击
        if (enhancedattack)
            return;

        if (absSpeed <= 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "IdleState"); //切换到待机状态
            return;
        }
        if (absSpeed > 130f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "RunState"); //切换到奔跑状态
            return;
        }
        if (absSpeed <= 130f && absSpeed > 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }          
    }
  
    public void OnAttackAnimationFinished()
    {
        isAttack = false;
        GD.Print("攻击动画完成");
        if (attackcount >= 3) //如果攻击次数达到3次，立即追击进行强化攻击
        {
            EmitSignal(nameof(StateFinished), "Attack2State"); //强化攻击状态
            attackcount = 0;
            enhancedattack = false;
            return;
        }
    }

    public void OnAttackTimerTimeout() //看门狗，防止卡在攻击状态
    {
        isAttack = false;
    }
}
