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
			SetPosition(lNode, pX, pY);
			pParent.AddChild(lNode);
			return lNode;
		}

		public static void SetPosition(Node2D pNode, int pX, int pY)
		{
			pNode.GlobalPosition = GridManager.gridOffset + TILE_SIZE * new Vector2(pX, pY);
		}

		public static Vector2 GetCellPos(Node2D pNode)
		{
			return new Vector2((int)(pNode.GlobalPosition.X / TILE_SIZE), (int)(pNode.GlobalPosition.Y / TILE_SIZE));
		}
	}
}
