using Godot;
using System;
using Com.IsartDigital.SokoVolt.Managers;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt {
	
	public static class Utils 
	{
		public const int TILE_WIDTH = 153;
		public const int TILE_HEIGHT = 90;

		public static Node2D Spawner(PackedScene pScene, int pX, int pY, Node2D pParent)
		{
			Node2D lNode = (Node2D)pScene.Instantiate();
			SetPosition(lNode, pX, pY, true);
			pParent.AddChild(lNode);
			return lNode;
		}


		public static Vector2 SetPosition(Node2D pNode, int pX, int pY, bool pAsignPos)
		{
			Vector2 lModelPoint = new Vector2(pX, pY);
			Vector2 lIsoPos = IsoManager.ModelToIsoView(lModelPoint); 

			lIsoPos += GridManager.gridOffset;

			if (pAsignPos)
			{
				pNode.GlobalPosition = lIsoPos;
			}

			return lIsoPos;
		}


		public static Vector2 GetCellPos(Node2D pNode)
		{
			Vector2 lNodePos = pNode.GlobalPosition - GridManager.gridOffset;

			Vector2 lModelPos = IsoManager.IsoViewToModel(lNodePos);

			return new Vector2((int)(lModelPos.X), (int)(lModelPos.Y));
		}

	}
}
