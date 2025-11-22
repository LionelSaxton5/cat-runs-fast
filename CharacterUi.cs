using Godot;
using System;

public partial class CharacterUi : CanvasLayer //角色界面
{
	private static CharacterUi _instance;
	public static CharacterUi Instance => _instance;

	private TextureProgressBar hp; //血条
	private TextureProgressBar mp; //魔法条
	private TextureProgressBar stamina; //体力条
	private Label hplabel; //血量数值
	private Label mplabel;//魔法数值

	private Player player; //玩家节点引用

	private bool isPlayerConnected = false; //玩家连接状态标志

    public override void _Ready()
	{
		_instance = this;
		Visible = false; //初始不可见

		hp = GetNode<TextureProgressBar>("HP");
		mp = GetNode<TextureProgressBar>("MP");
		stamina = GetNode<TextureProgressBar>("Stamina");
		hplabel = GetNode<Label>("HPLabel");
		mplabel = GetNode<Label>("MPLabel");		
	}

	public override void _Process(double delta)
	{
		if (isPlayerConnected)
			return;

		string currentScenePath = GetTree().CurrentScene.SceneFilePath; //获取当前场景路径

		if (currentScenePath == "res://changjing/MainInterface.tscn")
		{
            return; //在主菜单场景中不处理UI更新
        }

        InitializePlayerConnection(); //延迟初始化玩家连接，确保玩家节点已加载
    }


    private void InitializePlayerConnection()
	{       
        player = GetTree().GetNodesInGroup("Player")[0] as Player; //获取玩家节点引用(通过组查询)
		
        player.Attributes.HealthChanged += OnHealthChanged; //订阅血量变化信号
        player.Attributes.ManaChanged += OnManaChanged; //订阅魔法变化信号
        player.Attributes.StaminaChanged += OnStaminaChanged; //订阅体力变化信号

		OnHealthChanged(player.Attributes.CurrentHealth, player.Attributes.MaxHealth); //初始化血条UI
		OnManaChanged(player.Attributes.CurrentMana, player.Attributes.MaxMana); //初始化魔法条UI
		OnStaminaChanged(player.Attributes.CurrentStamina, player.Attributes.MaxStamina); //初始化体力条UI

		isPlayerConnected = true; //标记玩家已连接
    }


    public void OnHealthChanged(int currenthealth, int maxhealth)
	{
		GD.Print("更新血条UI");
        hp.MaxValue = maxhealth;
		hp.Value = currenthealth;
        hplabel.Text = $"{currenthealth} / {maxhealth}";
    }
	
	public void OnManaChanged(int currentmana, int maxmana)
	{
		mp.MaxValue = maxmana;
		mp.Value = currentmana;
		mplabel.Text = $"{currentmana} / {maxmana}";
	}

	public void OnStaminaChanged(int currentstamina, int maxstamina)
	{
        stamina.MaxValue = maxstamina;
        stamina.Value = currentstamina;
    }

    public override void _ExitTree()
    {
        // 取消订阅信号，防止内存泄漏
        if (player != null && player.Attributes != null)
        {
            player.Attributes.HealthChanged -= OnHealthChanged;
            player.Attributes.ManaChanged -= OnManaChanged;
			player.Attributes.StaminaChanged -= OnStaminaChanged;
        }

		isPlayerConnected = false; //重置连接状态
    }
}
