using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneTransition : Node //场景切换
{
	private static SceneTransition _instance;
	public static SceneTransition Instance => _instance;

	private Player player;
	private int _spawnIndex = 0; //出生点索引

    public override void _Ready()
	{
		_instance = this;
        player = GetTree().GetNodesInGroup("Player")[0] as Player;

    }
	
	public async Task TransitionToScene(string scenePath, int spawnIndex)
	{
		_spawnIndex = spawnIndex; //设置出生点索引

        PackedScene newScene = GD.Load<PackedScene>(scenePath); //加载场景资源
		if (newScene != null)
		{
			GetTree().ChangeSceneToPacked(newScene); //切换场景
			await ToSignal(GetTree(), "scene_changed"); //等待场景切换完成
			SpawnPlayerAtDoor(); //在指定门处生成玩家
        }
		else
		{
			GD.PrintErr($"无法加载场景: {scenePath}");
        }
    }

	public void SpawnPlayerAtDoor()
    {
		var doors = GetTree().GetNodesInGroup("doors"); //获取所有门节点
		foreach (Node door in doors)
		{
			if (door is Area2D area && area.HasMeta("door_index"))//检查节点是否是Area2D类型且具有"door_index"元数据
			{
				int doorIndex = (int)area.GetMeta("door_index"); //获取门的索引
				if (doorIndex == _spawnIndex) //匹配出生点索引
				{
					player.GlobalPosition = area.GlobalPosition; //设置玩家位置到门的位置
					return; //找到匹配的门后退出方法
                }
            }
        }
    }
}
