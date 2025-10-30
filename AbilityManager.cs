using Godot;
using System;
using System.Collections.Generic;

public partial class AbilityManager : Node //能力管理器
{
	private static AbilityManager _instance;
	public static AbilityManager Instance => _instance;

    private Dictionary<string, AbilityData> _allAbilities = new(); //所有能力数据字典，键为能力ID，值为AbilityData对象
    private HashSet<string> _unlockedAbilities = new(); //已解锁的能力ID集合,HashSet用于快速查找(集合)

	[Signal] public delegate void AbilityUnlockedEventHandler(string abilityId); //能力解锁信号

    public override void _Ready()
    {
        if (_instance == null)
        {  
            _instance = this; //单例模式初始化
            InitializeAbilities(); //初始化能力数据
        }        
    }

    private void InitializeAbilities()
    {
        foreach (var kep in AbilityData.abilityDataDictionary)
        {
            _allAbilities[kep.Value.AbilityId] = kep.Value; //将能力数据添加到字典中,能力数据的Id作为键
        }
    }

    public void UnlockAbility(string abilityId) //解锁能力
    {
        //技能是否存在且未解锁
        if (_allAbilities.ContainsKey(abilityId) && !_unlockedAbilities.Contains(abilityId))
        {
            _unlockedAbilities.Add(abilityId); //将能力ID添加
            EmitSignal(nameof(AbilityUnlocked), abilityId); //发出能力解锁信号
        }
    }

    public bool IsAbilityUnlocked(string abilityId) //检查能力是否已解锁
    {
        return _unlockedAbilities.Contains(abilityId); //检查集合中是否包含该能力ID,返回布尔值
    }

    public bool CanEnterState(string stateName)  //检查是否可以进入某个状态
    {
        var stateAbilityMap = new Dictionary<string, string> //状态与能力的映射关系
        {
            { "SprintState", "sprint" },
            { "SlideState", "slide" },
            { "ScareState", "scare" }
        };

        if (stateAbilityMap.ContainsKey(stateName)) //检查映射关系中是否包含该状态名称
        {
            return IsAbilityUnlocked(stateAbilityMap[stateName]); //检查对应的能力是否已解锁,返回布尔值   
        }
        return true; //如果状态不需要能力,则默认可以进入
    }


    //===用于存档系统===
    public List<string> GetUnlockedAbilityIds() 
    {
        return new List<string>(_unlockedAbilities); //返回已解锁能力ID的列表
    }

    //===用于读档系统===
    public void LoadUnlockedAbilities(List<string> abilityIds)
    {
        _unlockedAbilities.Clear(); //清空当前已解锁能力集合
        foreach (var id in abilityIds)  //遍历解锁能力ID列表
        {
            if (_allAbilities.ContainsKey(id)) //检查能力ID是否存在于所有能力数据字典中
            {
                _unlockedAbilities.Add(id); //将能力ID添加到已解锁集合中
            }
        }
    }




}
