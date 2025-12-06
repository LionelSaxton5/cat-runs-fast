using Godot;
using System;
using System.Xml.Linq;

public partial class Dog1 : Enemy
{

	public override void _Ready()
	{
		base._Ready(); //调用父类的_Ready方法
		attributes.MaxHealth = 50;
		attributes.CurrentHealth = 50;
    }


	public override void _Process(double delta)
	{
	}
}
