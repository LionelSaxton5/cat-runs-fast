using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SaveManager : Node //存档、加载
{
	private static SaveManager _instance;
	public static SaveManager Instance => _instance;

    //存储数据结构
    public class SaveData
    {
        public string LevelName; //当前关卡名称
        public Vector2 PlayerPosition; //玩家位置
        public int PlayerHealth; //玩家血量
        public int PlayerMana; //玩家魔法值
        public int PlayerMaxHealth; //玩家最大血量
        public int PlayerMaxMana; //玩家最大魔法值
        public bool Playerfacingright; //玩家朝向
        public string[] CompletedLevels; //已完成关卡列表
        public string[] UnlockedAbilities; //已解锁的能力ID列表(学会了哪些技能)
        public DateTime SaveTime; //保存时间
    }

    public void Save(string archiveName) //保存,archiveName存档文件名
    {
        Player player = GetTree().GetNodesInGroup("Player")[0] as Player;
        SaveData data = new SaveData();

        //存档纸条
        data.LevelName = player.GetTree().CurrentScene.SceneFilePath; //获取根场景路径
        data.PlayerPosition = player.GlobalPosition;
        data.PlayerHealth = player.Attributes.CurrentHealth;
        data.PlayerMana = player.Attributes.CurrentMana;
        data.PlayerMaxHealth = player.Attributes.MaxHealth;
        data.PlayerMaxMana = player.Attributes.MaxMana;
        data.Playerfacingright = player.isfacingright;
        //完成关卡
        //技能树
        data.SaveTime = DateTime.Now;

        //===保存文件处理===
        Variant variant = Variant.From(data); //将 SaveData 转换为 Godot.Variant
        string json = Json.Stringify(variant); //将数据变成json文本(序列化)

        string savePath = $"user://{archiveName}.save"; //存档文件路径
        FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write); //打开文件,写入模式
        if (file != null)
        {
            file.StoreString(json); //写入json文本
            file.Close(); //关闭文件
            GD.Print("存档成功了!");
        }
    }

    public async void Load(string archiveName) //读档
    {
        string savePath = $"user://{archiveName}.save"; //存档文件路径

        if (!FileAccess.FileExists(savePath))
        {
            GD.Print($"SaveManager.Load: 存档文件不存在: {savePath}");
            return;
        }

        FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read); //读取模式
        string json = file.GetAsText(); //读取文本
        file.Close(); //关闭文件

        //===反序列化===
        Variant variant = Json.ParseString(json); //将json文本解析为Variant
        
        await ApplicationData(variant); //应用存档数据,await等待异步完成
    }

    public async Task ApplicationData(Variant variant)  //应用数据
    {
        //===反序列化存档数据===
        var dataDict = variant.AsGodotDictionary<string, Variant>(); //将Variant转换为SaveData对象,Variant是通用类型容器,将Variant 转换为Godot字典并指定键值类型

        //===使用存档数据===
        SaveData saveData = new SaveData();
        saveData.LevelName = dataDict["LevelName"].AsString(); //当前关卡名称
        saveData.PlayerPosition = dataDict["PlayerPosition"].AsVector2(); //玩家位置
        saveData.PlayerHealth = dataDict["PlayerHealth"].AsInt32(); //玩家血
        saveData.PlayerMana = dataDict["PlayerMana"].AsInt32(); //玩家魔法值
        saveData.PlayerMaxHealth = dataDict["PlayerMaxHealth"].AsInt32(); //最大血量
        saveData.PlayerMaxMana = dataDict["PlayerMaxMana"].AsInt32(); //最大魔法值
        saveData.Playerfacingright = dataDict["Playerfacingright"].AsBool(); //玩家朝向
        //完成关卡
        //技能树

        if (GetTree().CurrentScene.SceneFilePath != saveData.LevelName)
        {
            //切换场景
            GetTree().ChangeSceneToFile(saveData.LevelName);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //等待场景切换完成,ProcessFrame信号在每帧处理时触发,等待它可以确保场景已切换
            Communicationsystem(saveData); //通信系统
        }
    }

    public void Communicationsystem(SaveData saveData) //通信系统
    {
        //===实现场景切换(恢复)后通讯===
        Player player = GetTree().GetNodesInGroup("Player")[0] as Player;

        //恢复玩家位置
        player.GlobalPosition = saveData.PlayerPosition;
        player.isfacingright = saveData.Playerfacingright;

        //恢复属性
        player.Attributes.SetAttributes(saveData.PlayerMaxHealth, saveData.PlayerHealth, saveData.PlayerMaxMana, saveData.PlayerMana);
    }
}
