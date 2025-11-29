using Godot;
using System;

public partial class Level1 : Node2D //关卡1
{
	private Camera2D camera; //相机节点引用
    private Node2D wapianGroup; //瓦片组父节点

    // 相机边界偏移量(预留缓冲区)
    [Export] private int boundaryPaddingLeft = 0;
    [Export] private int boundaryPaddingTop = -400;   // 向上扩展，防止看到空白
    [Export] private int boundaryPaddingRight = 200;
    [Export] private int boundaryPaddingBottom = 200;

    public override void _Ready()
	{
        GD.Print("Level1 已加载");
        InitializeCamera();
        SetupCameraLimitsFromTileMaps();
    }

	private void InitializeCamera()
	{
		var player = GetTree().GetNodesInGroup("Player");
		if (player != null && player.Count > 0)
		{
			var playerNode = player[0] as Player;
            camera = playerNode.GetNode<Camera2D>("Camera2D");
        }
    }

	private void SetupCameraLimitsFromTileMaps()
	{
		wapianGroup = GetNodeOrNull<Node2D>("wapian"); //获取瓦片组父节点

		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;

		foreach (Node child in wapianGroup.GetChildren())
		{
			if (child is TileMapLayer tileMapLayer)
			{
				Rect2I usedRect = tileMapLayer.GetUsedRect(); //获取瓦片地图的使用区域,rect2i包含位置和大小信息

				if (usedRect.Size.X == 0 || usedRect.Size.Y == 0)
				{
					continue; //如果瓦片地图没有使用区域,则跳过
				}

				TileSet tileSet = tileMapLayer.TileSet; //获取瓦片地图的瓦片集，tileSet包含瓦片的属性信息
                Vector2I tileSize = tileSet.TileSize; //获取瓦片集的瓦片大小,转为像素坐标

                int layerMinX = usedRect.Position.X * tileSize.X; //计算瓦片地图的最小X坐标
				int layerMinY = usedRect.Position.Y * tileSize.Y; //计算瓦片地图的最小Y坐标
				int layerMaxX = (usedRect.Position.X + usedRect.Size.X) * tileSize.X; //计算瓦片地图的最大X坐标,usedRect.Size是瓦片数量,乘以瓦片大小得到像素坐标
				int layerMaxY = (usedRect.Position.Y + usedRect.Size.Y) * tileSize.Y; //计算瓦片地图的最大Y坐标

				minX = Math.Min(minX, layerMinX); //更新整体最小X坐标
				minY = Math.Min(minY, layerMinY);
				maxX = Math.Max(maxX, layerMaxX);
				maxY = Math.Max(maxY, layerMaxY);
			}
		}

		// 设置相机边界，添加偏移量
		camera.LimitLeft = minX + boundaryPaddingLeft;
		camera.LimitTop = minY + boundaryPaddingTop;
		camera.LimitRight = maxX + boundaryPaddingRight;
		camera.LimitBottom = maxY + boundaryPaddingBottom;
	}    
}
