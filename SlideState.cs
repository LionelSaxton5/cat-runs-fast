using Godot;
using System;

public partial class SlideState : State //墙上滑动
{
    public bool isSliding = false; //是否正在滑动
    private Vector2 originalOffset; //原始碰撞体偏移量
    private bool wallOnRight = true; //墙壁是否在右侧

    public override void Enter()
    {
        player.AnimationPlayback("slide"); //播放滑动动画
        isSliding = true;
        wallOnRight = player.isfacingright; //记录墙壁所在方向

        player.Velocity = new Vector2(player.Velocity.X, 0); //重置垂直速度

        if (player.cat != null && player.cat.Visible)
        {
            originalOffset = player.cat.Offset; //保存原始偏移量

            float offsetX = player.isfacingright ? 12f : -12f;
            player.cat.Offset = new Vector2(offsetX, originalOffset.Y); //调整猫咪偏移量
        }
    }

    public override void Exit()
    {
        isSliding = false;
        if (player.cat != null && player.cat.Visible)
        {
            player.cat.Offset = originalOffset; //恢复原始偏移量
        }
    }

    public override void PhysicsUpdate(double delta)
	{
        player.currentspeed = 0;

        if (Input.IsActionJustPressed("jump"))
        {
            float wallJumpDirection = wallOnRight ? -1f : 1f; //根据墙壁位置决定跳跃方向
            player.Velocity = new Vector2(wallJumpDirection * player.speed * 1.5f, player.JumpVelocity); //赋予登墙跳速度           

            EmitSignal(nameof(StateFinished), "JumpState");
            return;
        }       
	}
}
