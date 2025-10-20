using Godot;
using System;

public partial class Attack1State : State //普通攻击状态
{
    public int attackcount = 0; //攻击次数
    public bool isAttack = false; //是否正在攻击 

    public override void Enter()
    {
        player.AnimationPlayback("attack1");
        isAttack = true;
        attackcount++;

        if (player.cat.Visible)
        {
            player.cat.AnimationFinished += OnAttackAnimationFinished; //动画完成信号
        }      
    }

    public override void Exit()
    {
        isAttack = false;
        if (player.cat.Visible)
        {
            player.cat.AnimationFinished -= OnAttackAnimationFinished; //断开动画完成信号
        }
    }

	public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (isAttack)
            return;
        
        if (absSpeed < 10f)
        {
            EmitSignal(nameof(StateFinished), "IdleState"); //切换到待机状态
            return;
        }
        if (absSpeed > 130f)
        {
            EmitSignal(nameof(StateFinished), "RunState"); //切换到奔跑状态
            return;
        }
        if (absSpeed < 130f)
        {
            EmitSignal(nameof(StateFinished), "WalkState");
            return;
        }          
    }
  
    public void OnAttackAnimationFinished()
    {
        isAttack = false;

        if (attackcount >= 3) // 如果攻击次数达到3次，立即追击进行强化攻击
        {
            EmitSignal(nameof(StateFinished), "Attack2State"); //强化攻击状态
            attackcount = 0;
            return;
        }

        if (Input.IsActionPressed("attack")) // 如果不是强化攻击，检查普通连击
        {
            // 继续普通攻击连击
            EmitSignal(nameof(StateFinished), "Attack1State");
            return;
        }
    }
}
