using Godot;
using System;
using static CombatSystem;

public partial class Attack2State : State //强化攻击状态
{
    public bool isAttack = false; //是否正在攻击

    public override void Enter()
    {       
        player.AnimationPlayback("attack2");
        animationPlayer.Play("Attack2");

        isAttack = true;

        if (player.cat.Visible)
        {
            player.cat.AnimationFinished += OnEnAttackAnimationFinished; //动画完成信号
            attackArea.BodyEntered += OnAttackAreaBodyEntered; //连接攻击范围碰撞信号
        }
    }
	
    public override void Exit()
    {
        isAttack = false;
        if (player.cat.Visible)
        {
            player.cat.AnimationFinished -= OnEnAttackAnimationFinished; //断开动画完成信号
            attackArea.BodyEntered -= OnAttackAreaBodyEntered; //连接攻击范围碰撞信号
        }
    }


    public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (isAttack)
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
        isAttack = false; //攻击结束        
    }

    public void OnAttackAreaBodyEntered(Node body)
    {
        if (isAttack && body is Enemy enemy)           
        {           
            DamageInfo damageInfo = new DamageInfo
            {
                DamageAmount = player.Attributes.AttackPower * (int)2.5f, //强化攻击伤害翻倍
                DamagePosition = player.GlobalPosition,
                KnockbackForce = player.Attributes.KnockbackForce,
                SourceDamage = this.player,
                TargetDamage = enemy
            };
            combatSystem.ApplyDamage(enemy, damageInfo);
        }
    }
}
