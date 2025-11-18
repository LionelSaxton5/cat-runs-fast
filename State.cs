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

    private Area2D _attackArea;
    public Area2D attackArea => _attackArea ??= player?.GetNodeOrNull<Area2D>("AttackArea2D");

    private CombatSystem _combatSystem;
    public CombatSystem combatSystem => _combatSystem ??= GetNode<CombatSystem>("/root/CombatSystem");

    private AnimationPlayer _animationPlayer;
    public AnimationPlayer animationPlayer => _animationPlayer ??= player?.GetNodeOrNull<AnimationPlayer>("AttackArea2D/AnimationPlayer");

   
	public virtual void Enter() { }  //进入状态时的初始化逻辑,virtual允许子类重写该方法
    public virtual void Exit() { }   //退出状态时的清理逻辑
    public virtual void Update(double delta) { } //每帧更新状态逻辑
	public virtual void PhysicsUpdate(double delta) { } //每帧物理更新状态逻辑
	public virtual void HandleInput(InputEvent @event) { } //处理输入逻辑
}
