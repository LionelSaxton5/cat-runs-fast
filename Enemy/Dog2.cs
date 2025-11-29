using Godot;
using System;

public partial class Dog2 : Enemy
{

	public override void _Ready()
	{
		base._Ready();
		attributes.MaxHealth = 100;
		attributes.AttackPower = 15;
		attributes.MoveSpeed = 30.0f;
    }


	public override void _Process(double delta)
	{
	}
}
