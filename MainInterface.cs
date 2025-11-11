using Godot;
using System;
using System.Threading.Tasks;
using static SaveManager;

public partial class MainInterface : CanvasLayer //开始菜单
{
	private Button startButton; //开始按钮
	private Button loadButton; //读档按钮
	private Button endButton; //结束按钮

	private SaveManager saveManager; //存档管理器

    public override void _Ready()
	{
		startButton = GetNode<Button>("Control/VBoxContainer/Button");
		loadButton = GetNode<Button>("Control/VBoxContainer/Button2");
		endButton = GetNode<Button>("Control/VBoxContainer/Button3");

		saveManager = SaveManager.Instance; //获取存档管理器单例

		//===按钮事件===
		startButton.Pressed += OnStartButtonPressed;
		loadButton.Pressed += OnLoadButtonPressed;
		endButton.Pressed += OnEndButtonPressed;

        // 检查存档文件，如果不存在则禁用读档按钮
        UpdateLoadButtonState();

    }

	public void UpdateLoadButtonState()
	{
		string savePath = "user://save.save"; //默认存档文件名
		bool saveExists = FileAccess.FileExists(savePath); //检查存档文件是否存在

		loadButton.Disabled = !saveExists; //如果存档不存在,禁用读档按钮
		if (!saveExists)
		{
			loadButton.Text = "继续游戏(无存档)";
		}
		else
		{
			loadButton.Text = "继续游戏";
        }
    }


    public void OnStartButtonPressed()
	{
		GD.Print("开始游戏按钮被按下");
		//加载新游戏场景
		GetTree().ChangeSceneToFile("res://changjing/Level1.tscn");				
    }

	public void OnLoadButtonPressed()
	{
		GD.Print("读档按钮被按下");
		//加载读档场景
		string archiveName = "save"; //默认存档文件名
		string savePath = $"user://{archiveName}.save"; //存档文件路径

		if (FileAccess.FileExists(savePath))
		{
			if (saveManager != null)
			{
				saveManager.Load(archiveName); //调用存档管理器的读档方法
			}
			else
			{
				GD.PrintErr("无法找到存档管理器实例！");
			}
		}
		else
		{
			GD.PrintErr("没有找到存档文件，无法读档！");
        }
    }

	public void OnEndButtonPressed()
	{
		//退出游戏
		GetTree().Quit();
    }	
}
