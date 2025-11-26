using Godot;
using System;

public partial class CombatSystem : Node //战斗系统
{
    private static CombatSystem _Instance; //单例实例
    public static CombatSystem Instance => _Instance;

    private PackedScene floatingTextScene;

    //===伤害信息结构体==
    public struct DamageInfo
    {
        public int DamageAmount { get; set; }      //伤害数值
        public Vector2 DamagePosition { get; set; }//伤害位置
        public float KnockbackForce { get; set; }  //击退力
        public Node2D SourceDamage { get; set; }   //伤害来源
        public Node2D TargetDamage { get; set; }   //伤害目标
    }
   
    //===信号===
    [Signal] public delegate void PlayerAttackHitEventHandler(); //玩家攻击命中信号
    [Signal] public delegate void EnemyAttackHitEventHandler(); //敌人攻击命中信号

    public override void _Ready()
	{
        if (_Instance == null)
        {
            _Instance = this; //单例模式初始化
        }
        floatingTextScene = GD.Load<PackedScene>("res://changjing/FloatingText.tscn"); //加载飘字场景
    }
	
    //统一伤害处理
    public void ApplyDamage(Node2D target, DamageInfo damageInfo)
    {
        if (target.IsInGroup("Player")) //通过组查询,怪物攻击玩家
        {
            Player player = target as Player;
            if (player != null)
            {
                player.Attributes.TakeDamage(damageInfo.DamageAmount);
                EmitSignal(nameof(EnemyAttackHit)); //发出敌人攻击命中信号
                ApplyKnockback(target, damageInfo);
                DamageDisplay(target, damageInfo);
            }
        }
        else if (target.IsInGroup("Enemy")) //玩家攻击怪物
        {
            Enemy enemy = target as Enemy;
            if (enemy != null) 
            {               
                enemy.attributes.TakeDamage(damageInfo.DamageAmount);
                EmitSignal(nameof(PlayerAttackHit)); //发出玩家攻击命中信号
                ApplyKnockback(target, damageInfo);
                DamageDisplay(target, damageInfo);
            }
        }
        
    }

    private void ApplyKnockback(Node2D target, DamageInfo damageInfo) //应用击退
    {
        if (target.IsInGroup("Player")) //怪物攻击玩家
        {
            Player player = target as Player;
            Vector2 direction = (player.GlobalPosition - damageInfo.DamagePosition).Normalized(); //计算击退方向
            int facingright = direction.X > 0 ? 1 : -1; //确定击退方向
            player.Velocity += new Vector2(facingright * damageInfo.KnockbackForce, -damageInfo.KnockbackForce / 2);
        }
        else if(target.IsInGroup("Enemy")) //玩家攻击怪物
        {
            Enemy enemy = target as Enemy;

            if (!enemy.CanAcceptKnockback())
            {
                GD.Print("怪物正在被击退，无法击退");
                return;               
            }

            Vector2 direction = (enemy.GlobalPosition - damageInfo.DamagePosition).Normalized();
            int facingright = direction.X > 0 ? 1 : -1;
            Vector2 knockbackVelocity = new Vector2(facingright * damageInfo.KnockbackForce, -damageInfo.KnockbackForce / 3);

            enemy.LimitKnockbackVelocity(ref knockbackVelocity);

            enemy.StartKnockback();
            enemy.Velocity += knockbackVelocity;            
        }
    }
    
    private void DamageDisplay(Node2D target, DamageInfo damageInfo) //伤害显示,暂未使用(怪物与玩家不同颜色)
    {
        FloatingText floatingText = floatingTextScene.Instantiate<FloatingText>();
        GetTree().Root.AddChild(floatingText);
        floatingText.FloatingtextAnimation(target,damageInfo);
    }
}
