using Godot;
using System;
using System.Reflection.Emit;

public partial class SavePoint : Node2D //存档点
{
	private TextureRect buttondisplay; //存档提示UI

    public override void _Ready()
	{
		buttondisplay = GetNode<TextureRect>("ButtonDisplay");
		buttondisplay.Visible = false; //初始隐藏存档提示UI
    }


	public override void _Process(double delta)
	{
		if (buttondisplay.Visible && Input.IsActionJustPressed("interaction"))
		{
			SaveManager.Instance.Save("Save1"); //调用存档管理器进行存档
        }
	}

	public void OnSavePointArea2DEntered(Area2D area2D)
	{
		buttondisplay.Visible = true; //显示存档提示UI			

		Tween tween = GetTree().CreateTween(); //创建补间动画
		tween.SetParallel(true);

        tween
			.TweenProperty(buttondisplay, "modulate:a", 1.0f, 0.5f); //淡入动画
        tween
            .TweenProperty(buttondisplay, "scale", new Vector2(0.2f, 0.2f), 0.4)
            .SetEase(Tween.EaseType.Out);
        tween
            .TweenProperty(buttondisplay, "scale", new Vector2(0.1f, 0.1f), 0.6)
            .SetTrans(Tween.TransitionType.Back);

		tween.Play(); //播放补间动画
    }
	public void OnSavePointArea2DExited(Area2D area2D)
	{
		buttondisplay.Visible = false;

        Tween tween = GetTree().CreateTween(); //创建补间动画
        tween.SetParallel(true);

        tween.TweenProperty(buttondisplay, "modulate:a", 0f, 0.5f); //淡出动画
        

        tween.Play(); //播放补间动画
    }
}
