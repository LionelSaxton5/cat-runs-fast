using Godot;
using System;

public partial class ClimbState : State //攀爬
{
    public Vector2 originalOffset; //原始碰撞体偏移量
    public override void Enter()
    {
        player.AnimationPlayback("climb"); //攀爬动画

        if (!player.cat.Visible && player.cat != null)
        {
            originalOffset = player.cat.Offset; //保存原始偏移量
            player.cat.Offset = new Vector2(0, 30); //调整猫咪偏移量
        }
    }

    public override void Exit()
    {
        if (!player.cat.Visible && player.cat != null)
        {
            player.cat.Offset = originalOffset; //恢复原始偏移量
        }
    }


    public override void PhysicsUpdate(double delta)
	{
        float absSpeed = Mathf.Abs(player.currentspeed);

        if (absSpeed <= 10f)
        {
            EmitSignal(nameof(StateFinished), "IdleState");
            return;
        }
	}
}
