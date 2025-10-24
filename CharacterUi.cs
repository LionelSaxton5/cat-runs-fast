using Godot;
using System;

public partial class CharacterUi : CanvasLayer
{
	private TextureProgressBar hp; //血条
	private TextureProgressBar mp; //魔法条
	private Label hplabel; //血量数值
	private Label mplabel;//魔法数值

	private Player player; //玩家节点引用

    public override void _Ready()
	{
		hp = GetNode<TextureProgressBar>("HP");
		mp = GetNode<TextureProgressBar>("MP");
		hplabel = GetNode<Label>("HPLabel");
		mplabel = GetNode<Label>("MPLabel");

		player = GetTree().GetNodesInGroup("Player")[0] as Player; //获取玩家节点引用(通过组查询)

        player.Attributes.HealthChanged += OnHealthChanged; //订阅血量变化信号
		player.Attributes.ManaChanged += OnManaChanged; //订阅魔法变化信号
    }

	public override void _Process(double delta)
	{
	}

	public void OnHealthChanged(int currenthealth, int maxhealth)
	{
		hp.Value = currenthealth / maxhealth;
		hplabel.Text = $"{currenthealth} / {maxhealth}";
    }
	
	public void OnManaChanged(int currentmana, int maxmana)
	{
		mp.Value = currentmana / maxmana;
		mplabel.Text = $"{currentmana} / {maxmana}";
	}
}
