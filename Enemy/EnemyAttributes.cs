using Godot;
using System;

public partial class EnemyAttributes : Node //怪物属性
{
    //===基础属性===
    public int MaxHealth { get; set; } = 100; //最大生命值
	public int CurrentHealth { get; set; } = 100; //当前生命值
    public int AttackPower { get; set; } = 20; //攻击力
    public float MoveSpeed { get; set; } = 50.0f; //移动速度
    public float KnockbackForce { get; set; } = 50f; //击退力
    public float BasicGravity { get; set; } = 2000.0f; //基础重力

    ///===状态相关属性===
	public bool IsAlive { get; set; } = true; //是否存活
    public float AttackCooldown { get; set; } = 1.0f; //攻击冷却时间（秒）

	///===通信系统 - 用于状态机通信===
	[Signal] public delegate void IsHurtChangedEventHandler(); //生命值变化信号
    [Signal] public delegate void IsDeathChangedEventHandler(); //死亡信号

    public EnemyAttributes() { }//构造函数,支持初始化时设置属性

    public EnemyAttributes(int maxHealth, int attackPower, float moveSpeed)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        AttackPower = attackPower;
        MoveSpeed = moveSpeed;
    }    

    public virtual void TakeDamage(int damage) //怪物受伤
    {
		CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
		EmitSignal(nameof(IsHurtChanged));

        if (CurrentHealth <= 0 && IsAlive)
		{
			IsAlive = false;
			EmitSignal(nameof(IsDeathChanged));
        }
    }

    public virtual void Heal(int amount) //怪物治疗
	{
		CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }    
}
