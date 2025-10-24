using Godot;
using System;

public partial class Attack2State : State //强化攻击状态
{
    public bool isenAttack = false; //是否正在攻击

    public override void Enter()
    {
        player.AnimationPlayback("attack2");
        isenAttack = true;

        if (player.cat.Visible)
        {
            GD.Print("连接强化攻击动画完成信号");
            player.cat.AnimationFinished += OnEnAttackAnimationFinished; //动画完成信号
        }
    }
	
    public override void Exit()
    {
        isenAttack = false;
        if (player.cat.Visible)
        {
            GD.Print("断开强化攻击动画完成信号");
            player.cat.AnimationFinished -= OnEnAttackAnimationFinished; //断开动画完成信号
        }
    }


    public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (isenAttack)
            return;

        if (absSpeed <= 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "IdleState"); //切换到待机状态
            return;
        }
        if (absSpeed <= 130f && absSpeed > 10f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }
        if (absSpeed > 130f && player.IsOnFloor())
        {
            EmitSignal(nameof(StateFinished), "RunState"); //切换到奔跑状态
            return;
        }
    }

    private void OnEnAttackAnimationFinished()
    {       
        GD.Print("强化攻击动画完成");
        isenAttack = false; //攻击结束
        
    }
}
