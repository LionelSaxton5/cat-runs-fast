using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class StateMachine : Node //状态机节点
{
	[Export] private NodePath initialState; //Nodepath用于引用场景树中的节点路径,initialState表示状态机的初始状态节点路径
	private State currentState; //当前状态节点
	private Dictionary<string,State> states = new Dictionary<string,State>(); //状态字典,用于存储状态名称和对应的状态节点

    public override void _Ready()
	{
        // 获取Player节点
        Player player = GetParent<Player>();
        if (player == null)
        {
            GD.PrintErr("StateMachine: 无法找到Player父节点！");
            return;
        }

        foreach (Node child in GetChildren()) //遍历当前节点的所有子节点
		{
			if (child is State state) //检查子节点是否是State类型,如果是则进行类型转换
            {
				states[state.Name] = state; //将状态名称和对应的状态节点存储到字典中
				state.StateFinished += OnStateFinished; //订阅状态完成信号,当状态完成时调用OnStateFinished方法
				state.SetProcess(false); //初始时禁用状态节点的处理
				state.SetPhysicsProcess(false); //初始时禁用状态节点的物理处理
                state.player = player; //将Player节点赋值给状态节点的player成员变量
            }
        }
        
		if (initialState != null && states.ContainsKey(GetNode<Node>(initialState).Name)) //如果初始状态节点路径不为空,并且状态字典中包含该状态名称
		{
            ChangeState(GetNode<Node>(initialState).Name); //GetNode<Node>(initialState).Name获取初始状态节点的名称,并调用ChangeState方法切换到该状态
        }
	}

	public void ChangeState(string stateName) //切换状态方法,参数为要切换到的状态名称
    {
		if (states.ContainsKey(stateName)) //检查状态字典中是否包含指定的状态名称
		{
			GD.Print($"切换状态: {currentState?.Name} -> {stateName}");
			currentState?.Exit(); //调用当前状态的退出逻辑
			currentState = states[stateName]; //切换到新的状态节点
			currentState.Enter(); //调用新状态的进入逻辑
		}
		else
		{
			GD.PrintErr($"状态机中不存在状态: {stateName}");
        }
    }

	private void OnStateFinished(string nextState)
	{
		ChangeState(nextState); //当状态完成时,切换到下一个状态,由状态子节点通过信号传递下一个状态名称
    }

	public override void _Process(double delta) //调用子节点的每帧更新逻辑
    {
		currentState ?.Update(delta); //调用当前状态的每帧更新逻辑
    }

	public override void _PhysicsProcess(double delta)
	{
		currentState ?.PhysicsUpdate(delta); //调用当前状态的每帧物理更新逻辑
    }

	public override void _UnhandledInput(InputEvent @event)
	{
		currentState ?.HandleInput(@event); //调用当前状态的输入处理逻辑
    }	
}
