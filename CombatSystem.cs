using Godot;
using System;

public partial class CombatSystem : Node //战斗系统
{
    private static CombatSystem _Instance; //单例实例
    public static CombatSystem Instance => _Instance;

    //===伤害信息结构体==
    public struct DamageInfo
    {
        public int DamageAmount { get; set; }      //伤害数值
        public Vector2 DamagePosition { get; set; }//伤害位置
        public float KnockbackForce { get; set; }  //击退力
        public Node2D SourceDamage { get; set; }   //伤害来源
        public Node2D TargetDamage { get; set; }   //伤害目标
    }

    //===攻击震动相关===
    private float vibrationTime = 0f; //震动计时
    private const float VibrationDuration = 0.2f; //震动持续时间
    private bool isVibrating = false; //是否震动
    private bool isRecovering = false; //是否正在恢复
    private bool EnemyAttackVibration = false; //敌人攻击震动开关
    Random random = new Random();

    //===相机缩放相关===
    public float normalZoom = 3.51f; //正常缩放值
    public float attackZoom = 4.0f; //相机视口最小缩放值
    private float currentZoom = 3.51f; //当前相机缩放值

    public bool IsVibrating => isVibrating; //外部访问震动状态

    private Camera2D camera;

    public override void _Ready()
	{
        if (_Instance == null)
        {
            _Instance = this; //单例模式初始化
        }

        Node parent = GetParent();
        camera = parent.GetNodeOrNull<Camera2D>("Player/Camera2D");   
    }

	public override void _Process(double delta)
	{
        float deltaF = (float)delta;

        if (isVibrating || isRecovering)
        {
            AttackVibration(deltaF);
        }
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
                ApplyKnockback(target, damageInfo);

                EnemyAttackVibration = true; //敌人攻击震动开关开启               
            }
        }
        else if (target.IsInGroup("Enemy")) //玩家攻击怪物
        {
            Enemy enemy = target as Enemy;
            if (enemy != null) 
            {
                enemy.attributes.TakeDamage(damageInfo.DamageAmount);
                ApplyKnockback(target, damageInfo);               
            }
        }
        isVibrating = true;
        isRecovering = false;
        vibrationTime = 0f;
    }

    private void ApplyKnockback(Node2D target, DamageInfo damageInfo) //应用击退
    {
        if (target.IsInGroup("Player"))
        {
            Player player = target as Player;
            Vector2 direction = (player.GlobalPosition - damageInfo.DamagePosition).Normalized();
            int facingright = direction.X > 0 ? 1 : -1;
            player.Velocity += new Vector2(facingright * damageInfo.KnockbackForce, damageInfo.KnockbackForce / 2);
        }
        else if(target.IsInGroup("Enemy"))
        {
            Enemy enemy = target as Enemy;
            Vector2 direction = (damageInfo.DamagePosition - enemy.GlobalPosition).Normalized();
            int facingright = direction.X > 0 ? 1 : -1;
            enemy.Velocity += new Vector2(facingright * damageInfo.KnockbackForce, damageInfo.KnockbackForce / 2);
        }
    }

    public void AttackVibration(float delta) //攻击震动、收缩
    {
        vibrationTime += delta;

        if (isVibrating)
        {
            if (EnemyAttackVibration)
            {
                float enemyvibrationX = (float)(random.NextDouble() * 2 - 1) * 2f; //范围 -2 到 2
                float enemyvibrationY = (float)(random.NextDouble() * 2 - 1) * 2f; //范围 -2 到 2           
                camera.Offset = new Vector2(enemyvibrationX, enemyvibrationX); //应用震动
            }
            else
            {
                float vibrationX = (float)(random.NextDouble() * 2 - 1) * 2f; //范围 -2 到 2
                float vibrationY = (float)(random.NextDouble() * 2 - 1) * 2f; //范围 -2 到 2           
                camera.Offset = new Vector2(vibrationX, vibrationY); //应用震动

                currentZoom = Mathf.MoveToward(currentZoom, attackZoom, 10f * delta);
                camera.Zoom = new Vector2(currentZoom, currentZoom); //应用缩放
            }

            if (vibrationTime >= VibrationDuration)
            {
                isVibrating = false;
                isRecovering = true;
                EnemyAttackVibration = false;
                GD.Print("开始恢复");
            }
        }
        else if (isRecovering)
        {
            camera.Offset = Vector2.Zero; //重置偏移

            currentZoom = Mathf.MoveToward(currentZoom, normalZoom, 20f * delta);
            camera.Zoom = new Vector2(currentZoom, currentZoom); //恢复收缩

            if (Mathf.Abs(currentZoom - normalZoom) < 0.01f)
            {
                isRecovering = false;
                currentZoom = normalZoom;
                camera.Zoom = new Vector2(currentZoom, currentZoom);
                camera.Offset = Vector2.Zero;
            }
        }
    }

    private void DamageDisplay(Node2D target, DamageInfo damageInfo) //伤害显示,暂未使用(怪物与玩家不同颜色)
    { }
}
