using Godot;
using System;

public partial class State : Node //状态基类
{
	[Signal] public delegate void StateFinishedEventHandler(string newState); //定义状态完成信号,参数为新的状态名称

    protected Player player; //protected访问修饰符允许子类访问该成员变量

    public override void _Ready()
	{
		player = GetParent().GetParent<Player>();//获取Player节点的引用
    }


	public override void _Process(double delta)
	{
	}

	public virtual void Enter() { }  //进入状态时的初始化逻辑,virtual允许子类重写该方法
    public virtual void Exit() { }   //退出状态时的清理逻辑
    public virtual void Update(double delta) { } //每帧更新状态逻辑
	public virtual void PhysicsUpdate(double delta) { } //每帧物理更新状态逻辑
	public virtual void HandleInput(InputEvent @event) { } //处理输入逻辑
}
