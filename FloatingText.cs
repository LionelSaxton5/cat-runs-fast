using Godot;
using System;
using static CombatSystem;
using static System.Net.Mime.MediaTypeNames;

public partial class FloatingText : Node2D //飘字动画
{
	public Label label;
	private CombatSystem combatSystem;
	private Tween tween; //补间动画节点
    Random random;

    public override void _Ready()
	{
		label = GetNode<Label>("Label");
		combatSystem = CombatSystem.Instance;
    }

	public void FloatingtextAnimation(Node2D target, DamageInfo damageInfo) //飘字动画
	{
		tween = CreateTween(); //创建补间动画节点
        tween.SetParallel(true);

        random = new Random(); //创建随机数生成器
        float offsetX = (float)(random.NextDouble() * 40 - 20); //生成-20到20之间的随机X偏移

        if (target.IsInGroup("Player")) //怪物攻击玩家
		{			
            label.Modulate = new Color(1, 0.2f, 0.2f); //设置飘字颜色为红色
        }
		else if (target.IsInGroup("Enemy")) //玩家攻击怪物
		{
			label.Modulate = new Color(1, 1, 1); //设置飘字颜色为白色
        }

        label.Text = damageInfo.DamageAmount.ToString(); //ToString()方法将整数转换为字符串,设置飘字文本
        label.GlobalPosition = damageInfo.TargetDamage.GlobalPosition; //设置飘字位置

        //设置飘字动画位置
        tween.TweenProperty(label, "position", label.Position + new Vector2(offsetX, -100), 1.2)
            .SetEase(Tween.EaseType.Out) //设置缓动类型
            .SetTrans(Tween.TransitionType.Quad); //设置过渡类型(平滑减速)

        //设置飘字透明度动画(淡出效果)
        tween.TweenProperty(label, "modulate:a", 0.0f, 1.2)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);//平滑加速

        //设置飘字缩放动画
        Tween scaleTween = CreateTween();

        scaleTween
            .TweenProperty(label, "scale", new Vector2(1f, 1f), 0.4)
            .SetEase(Tween.EaseType.Out);
        scaleTween
            .TweenProperty(label, "scale", new Vector2(0.6f, 0.6f), 0.6)
            .SetTrans(Tween.TransitionType.Back); //回弹效果


        tween.Play(); //播放补间动画
        tween.Finished += () => //补间动画完成后执行
        {
            QueueFree(); //删除飘字节点
        };
    }
}
