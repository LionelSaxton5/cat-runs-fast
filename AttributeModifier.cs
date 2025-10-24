using Godot;
using System;

public partial class AttributeModifier //属性修改器
{
    //要改变什么属性、改变多少、持续多久
    public string Id { get; set; } //属性名称
    public string AffectedAttribute { get; set; } //被影响的属性名称,要改变什么属性
    public float Multiplier { get; set; } = 1.0f; //属性变化值
    public float Duration { get; set; } = 0f; //持续时间（秒）,0表示永久
    public bool IsTemporary => Duration > 0; //是否为临时效果,如果 Duration 大于 0，返回 true（临时效果）
}
