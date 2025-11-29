using Godot;
using System;

public partial class Camera2d : Camera2D //人物相机
{
    //===相机下拉视角相关===
    public const int MaxOffsetY = 50; //最大相机偏移值
	private const float OffsetSpeed = 150f; //相机偏移速度
    private int currentoffsetY = 0; //当前相机偏移值

    //===攻击震动相关===
    private float vibrationTime = 0f; //震动计时
    private const float VibrationDuration = 0.2f; //震动持续时间
    private float VibrationRange = 1f; //震动范围
    private bool isVibrating = false; //是否可以震动
    private bool isRecovering = false; //是否正在恢复
    Random random = new Random();

    //===相机缩放相关===
    public float normalZoom = 3.51f; //正常缩放值
    public float attackZoom = 3.75f; //相机视口最小缩放值
    private float currentZoom = 3.51f; //当前相机缩放值

    //===是否命中===
    public bool isPlayerAttackHit = false; //玩家是否命中
    public bool isEnemyAttackHit = false; //敌人是否命中

    //===节点引用===
    private Player player;
    private Attack1State attack1State; //玩家攻击1
    private CombatSystem combatSystem; //战斗系统单例

    public override void _Ready()
	{
        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
        {
            player = players[0] as Player;
            GD.Print("Camera2d: 找到玩家节点");
        }
       
        attack1State = player.GetNode<Attack1State>("StateMachine/Attack1State");
        if (attack1State != null)
        {
            attack1State.Attack1Triggered += OnAttackTriggered;
            GD.Print("Camera2d: 成功连接攻击信号");
        }
        
        combatSystem = CombatSystem.Instance; //获取战斗系统单例
        combatSystem.PlayerAttackHit += OnPlayerAttackHit;
        combatSystem.EnemyAttackHit += OnEnemyAttackHit;
    }

	public override void _Process(double delta)
	{
		float deltaF = (float)delta;
        
        if (isVibrating || isRecovering)
        {            
            AttackVibration(deltaF);
        }       
        else if (Mathf.Abs(player.currentspeed) < 5f && player.IsOnFloor())
        {
            Pullperspective(deltaF);
        }

    }

    public void OnPlayerAttackHit()
    {
        attackZoom = 5.6f;
        VibrationRange = 2.5f;
        isPlayerAttackHit = true;

        isVibrating = true;
        isRecovering = false;
        vibrationTime = 0f;
        GD.Print("=== 玩家攻击命中 ===");
    }
    public void OnEnemyAttackHit()
    {
        attackZoom = 4.0f;
        VibrationRange = 1.5f;
        isEnemyAttackHit = true;

        isVibrating = true;
        isRecovering = false;
        vibrationTime = 0f;
        GD.Print("=== 敌人攻击命中 ===");
    }

    public void OnAttackTriggered()
    {
        if (!isPlayerAttackHit && !isEnemyAttackHit)
        {
            attackZoom = 3.75f;
            VibrationRange = 1.0f;
        }

        isVibrating = true;
        isRecovering = false;
        vibrationTime = 0f;
        GD.Print("=== 触发攻击震动 ===");
    }
    
    public void Pullperspective(float delta) //下拉视角
	{
		if (Input.IsActionPressed("perspective"))
		{
            currentoffsetY = (int)Mathf.MoveToward(currentoffsetY, MaxOffsetY, OffsetSpeed * delta);
		}
		else
		{			
			currentoffsetY = (int)Mathf.MoveToward(currentoffsetY, 0, OffsetSpeed * delta);			
		}

		Offset = new Vector2(0, currentoffsetY);
    }

    public void AttackVibration(float delta) //攻击震动、收缩
    {
        vibrationTime += delta;

        if (isVibrating)
        {
            float vibrationX = (float)(random.NextDouble() * 2 - 1) * VibrationRange; //范围 -1 到 1
            float vibrationY = (float)(random.NextDouble() * 2 - 1) * VibrationRange; //范围 -1 到 1         
            Offset = new Vector2(vibrationX, vibrationY); //应用震动

            currentZoom = Mathf.MoveToward(currentZoom, attackZoom, 5f * delta);
            Zoom = new Vector2(currentZoom, currentZoom); //应用缩放

            if (vibrationTime >= VibrationDuration)
            {
                isVibrating = false;
                isRecovering = true;
                isPlayerAttackHit = false;
                isEnemyAttackHit = false;
                GD.Print("开始恢复");
            }
        }
        else if (isRecovering)
        {
            Offset = Vector2.Zero; //重置偏移

            currentZoom = Mathf.MoveToward(currentZoom, normalZoom, 25f * delta); 
            Zoom = new Vector2(currentZoom, currentZoom); //恢复收缩

            if (Mathf.Abs(currentZoom - normalZoom) < 0.01f)
            {
                isRecovering = false;
                currentZoom = normalZoom;
                Zoom = new Vector2(currentZoom, currentZoom);
                Offset = Vector2.Zero;
            }
        }
    }
}
