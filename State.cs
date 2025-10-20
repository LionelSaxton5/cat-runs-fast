using Godot;
using System;

public partial class State : Node //状态基类
{
	[Signal] public delegate void StateFinishedEventHandler(string newState); //定义状态完成信号,参数为新的状态名称

    private Player Player { get; set; } //私有成员变量用于存储Player节点引用

    public Player player //公开属性用于外部访问player成员变量
    {
		get { return Player; } //返回内部存储的Player值
        set { Player = value; } //设置内部存储的值
    }
    public override void _Ready()
	{
        
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
