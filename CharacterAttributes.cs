using Godot;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public partial class CharacterAttributes : Node //角色属性
{
    private Timer restoreTimer; //自然恢复计时器

    //===基础属性===
    [ExportGroup("角色属性")]
	[Export] public int MaxHealth { get; private set; } = 100; //最大生命值
    [Export] public int CurrentHealth { get; private set; } = 100; //当前生命值
    [Export] public int HealthSpeed { get; private set; } = 2; //生命回复速度
    [Export] public int MaxMana { get; private set; } = 100; //最大魔法值
    [Export] public int CurrentMana { get; private set; } = 100; //当前魔法值
    [Export] public int ManaSpeed { get; private set; } = 4; //魔法回复速度
    [Export] public int AttackPower { get; private set; } = 20; //攻击力
    [Export] public float KnockbackForce { get; private set; } = 100f; //击退力
    [Export] public int MaxStamina { get; private set; } = 100; //最大体力值
    [Export] public int CurrentStamina { get; private set; } = 100; //当前体力值
    [Export] public float MoveSpeedMultiplier { get; private set; } = 1.0f; //移动速度倍率
    [Export] public float JumpPowerMultiplier { get; private set; } = 1.0f; //跳跃力倍率

    //===状态相关属性===
    [ExportGroup("角色状态")]
    [Export] public bool IsDeath { get; private set; } = true; //是否存活
    [Export] public float SlideSpeed = 200f; //滑墙速度
    [Export] public float SprintSpeedMultiplier = 1.5f; //冲刺速度倍率
    [Export] public float AttackCooldown = 0.3f; //攻击冷却时间（秒）

    //===通信系统 - 用于状态机通信===
    [Signal] public delegate void HealthChangedEventHandler(int newHealth, int maxHealth); //生命值变化信号
    [Signal] public delegate void ManaChangedEventHandler(int newMana, int maxMana); //魔法值变化信号
    [Signal] public delegate void StaminaChangedEventHandler(int newStamina, int maxStamina); //体力值变化信号
    [Signal] public delegate void AttackChangedEventHandler(int newAttack); //攻击力变化信号
    [Signal] public delegate void IsDeathChangedEventHandler(); //死亡信号
    [Signal] public delegate void HurtChangedEventHandler(); //受伤信号

    public override void _Ready()
	{
        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;

        restoreTimer = GetNode<Timer>("RestoreTimer");
	}
   
    public void OnRestoreTimerimeout() //定时器超时处理函数(用于生命和魔法回复)
    {
        Heal(HealthSpeed); //生命回复
        MagicReply(ManaSpeed); //魔法回复
    }

    //===属性操作方法===
    public void TakeDamage(int damage) //受到伤害
    {
        GD.Print("角色受到伤害: " + damage);
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage); //取大值
        GD.Print("当前血量: " + CurrentHealth);
        EmitSignal(nameof(HealthChanged), CurrentHealth, MaxHealth); //血量变化信号
        EmitSignal(nameof(HurtChanged)); //受伤信号

        if (CurrentHealth <= 0 && IsDeath)
        {
            EmitSignal(nameof(IsDeathChanged));
        }
    }

    public void Heal(int amount) //血量回复
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth); //取小值
        EmitSignal(nameof(HealthChanged), CurrentHealth, MaxHealth);
    }

    public void MagicReduction(int magicamount) //魔法扣除
    {
        CurrentMana = Mathf.Max(0, CurrentMana - magicamount);
        EmitSignal(nameof(ManaChanged), CurrentMana, MaxMana); //魔法变化信号    
    }

    public void MagicReply(int amount) //魔法回复
    {
        CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
        EmitSignal(nameof(ManaChanged), CurrentMana, MaxMana);
    }

    public void StaminaReduction(int staminaamount) //体力扣除
    {
        CurrentStamina = Mathf.Max(0, CurrentStamina - staminaamount);
        EmitSignal(nameof(StaminaChanged), CurrentStamina, MaxStamina); //体力变化信号    
    }

    //===属性修改系统(用于状态机)===

    //临时属性修改(用于Buff/Debuff):"效果收藏夹" - 管理所有正在生效的修改器
    private Dictionary<string, AttributeModifier> _activeModifiers = new Dictionary<string, AttributeModifier>();

    //钥匙(Key) = 效果ID（如 "sprint_buff"）
    //值(Value) = 整个效果包裹对象

    public void AddModifier(string id, AttributeModifier modifier) //添加效果到收藏夹
    {
        
        _activeModifiers[id] = modifier; //添加或更新效果
        UpdateFinalAttributes(); //更新最终属性值

    }

    public void RemoveModifier(string id) //从收藏夹移除效果
    {
        if (_activeModifiers.ContainsKey(id)) //检查是否存在该效果
        {
            _activeModifiers.Remove(id);
            UpdateFinalAttributes(); //更新最终属性值
        }
    }

    public void UpdateFinalAttributes() //计算最终属性值,通知系统属性有变化
    {
        //添加：更新UI、播放音效等
    }

    //===状态机属性访问接口(使用效果)===
    public float GetEffectiveMoveSpeed(float baseSpeed) //获取移动速度
    {
        //基础速度 × 基础倍率 × 所有修改器的总效果
        return baseSpeed * MoveSpeedMultiplier * GetModifierValue("move_speed");//move_speed是被影响的属性名称:AffectedAttribute
    }

    public float GetEffectiveJumpPower(float baseJump) //获取跳跃力
    {
        //基础跳跃 × 基础倍率 × 所有修改器的总效果 
        return baseJump * JumpPowerMultiplier * GetModifierValue("jump_power");
    }

    public float GetModifierValue(string attributeName) //获取指定属性的修改值(效果值)
    {
        float totalModifier = 1.0f; //初始值为1.0（无修改）

        //遍历收藏夹里所有效果包裹
        foreach (var modifier in _activeModifiers.Values)
        {
            // 如果这个包裹是影响目标属性的，就乘上它的效果
            if (modifier.AffectedAttribute == attributeName) //找到匹配的属性名称(修改器)
            {
                totalModifier *= modifier.Multiplier; //累加修改值
            }
        }
        return totalModifier;
    }

    //===读档设置属性方法===
    public void SetAttributes(int _MaxHealth, int _CurrentHealth, int _MaxMana, int _CurrentMana, int _MaxStamina, int _CurrentStamina)
    {
        MaxHealth = _MaxHealth;
        CurrentHealth = _CurrentHealth;
        MaxMana = _MaxMana;
        CurrentMana = _CurrentMana;
        MaxStamina = _MaxStamina;
        CurrentStamina = _CurrentStamina;
    }
}
