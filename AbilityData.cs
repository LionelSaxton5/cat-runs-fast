using Godot;
using System;
using System.Collections.Generic;

public partial class AbilityData //能力数据
{
    public string AbilityId { get; set; } //能力ID
    public string AbilityName { get; set; } //能力名称
    public string Description { get; set; } //能力描述
    public Texture2D Icon { get; set; } //能力图标

    public enum abilityDatalist //能力类型列表
    {
        sprint, //冲刺
        slide,  //滑墙
        scare   //恐吓
    } 
    
    public static Dictionary<abilityDatalist, AbilityData> abilityDataDictionary = new Dictionary<abilityDatalist, AbilityData>() //能力数据字典,static让字典在所有地方共享同一个实例
    {
        {
            abilityDatalist.sprint,
            new AbilityData
            {
                AbilityId = "sprint",
                AbilityName = "冲刺",
                Description = "让猫咪能够短时间内快速奔跑，穿越危险地形。",
                //Icon = GD.Load<Texture2D>("res://assets/icons/ability_sprint.png"),
            }
        },
        {
            abilityDatalist.slide,
            new AbilityData
            {
                AbilityId = "slide",
                AbilityName = "滑墙",
                Description = "让猫咪能够在墙壁上滑行，避开障碍物。",
                //Icon = GD.Load<Texture2D>("res://assets/icons/ability_slide.png"),
            }
        },
        {
            abilityDatalist.scare,
            new AbilityData
            {
                AbilityId = "scare",
                AbilityName = "吓唬",
                Description = "让猫咪能够发出恐吓声，震慑敌人。",
                //Icon = GD.Load<Texture2D>("res://assets/icons/ability_scare.png"),
            }
        }
    };   
}
