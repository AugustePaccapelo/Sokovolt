using Godot;
using System;
using Com.IsartDigital.SokoVolt.Managers;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt {
	
	public static class Utils 
	{
		public const int TILE_SIZE = 64;
		public static Node2D Spawner(PackedScene pScene, int pX, int pY, Node2D pParent)
		{
			Node2D lNode = (Node2D)pScene.Instantiate();
			SetPosition(lNode, pX, pY, true);
			pParent.AddChild(lNode);
			return lNode;
		}

		public static Vector2 SetPosition(Node2D pNode, int pX, int pY, bool pAsignPos)
		{
			Vector2 lPos = GridManager.gridOffset + TILE_SIZE * new Vector2(pX, pY);
			if(pAsignPos) pNode.GlobalPosition = lPos;
			return lPos; 
		}

		public static Vector2 GetCellPos(Node2D pNode)
		{
			Vector2 lNodePos = pNode.GlobalPosition - GridManager.gridOffset;
			return new Vector2((int)(lNodePos.X / TILE_SIZE), (int)(lNodePos.Y / TILE_SIZE));
		}
	}
}
