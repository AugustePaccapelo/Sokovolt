using Com.IsartDigital.SokoVolt.Tools;
using Godot;
using System;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class AnimationPiston : Node2D
	{
        [Export] public Node2D topPart;
		private const int START_POS = 600; 


		private void ApplyMaskRecursively(Node pNode)
		{
			CustomMaskOcluder.instance.ApplyOcclusionTo(pNode);

			foreach (Node lChild in pNode.GetChildren())
			{
				ApplyMaskRecursively(lChild);
			}
		}

		private void ClearMaskRecursively(Node node)
		{
			var originalMaterials = CustomMaskOcluder.instance.GetOriginalMaterials();

			if (node is CanvasItem canvasItem)
			{
				if (originalMaterials.ContainsKey(canvasItem))
				{
					canvasItem.Material = originalMaterials[canvasItem]; //Reset animated objects origin material 
				}
				else
				{
					canvasItem.Material = null;  // Base godot material 
				}
			}

			foreach (Node child in node.GetChildren())
			{
				ClearMaskRecursively(child);
			}
		}




     	public async void Launch(Cell pTargetCell, Vector2 pFinalPosition, float pDelay)
		{
		


			topPart.GlobalPosition = new Vector2(GlobalPosition.X, pFinalPosition.Y + START_POS);
			ZIndex -= 40; 
			if (pTargetCell == null) return;

			if (CustomMaskOcluder.instance != null)
			{
				ApplyMaskRecursively(topPart);
				ApplyMaskRecursively(GetChild(1)); 

			
				ApplyMaskRecursively(pTargetCell);

				var content = pTargetCell.GetContent();
				if (content != null)
					ApplyMaskRecursively(content);
				
			}

			// Scene start position 
			Vector2 lStartPos = pFinalPosition + new Vector2(0, START_POS);
			pTargetCell.GlobalPosition = lStartPos;
			
			if (pTargetCell.GetContent() != null)
				pTargetCell.GetContent().GlobalPosition = lStartPos;

			//  Time Between Animation
			await ToSignal(GetTree().CreateTimer(pDelay), TIME_OUT);

			// Down Piston Animation 
			Tween lPistonTween = CreateTween();
			lPistonTween.TweenProperty(topPart, POSITION_Y, 0, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);

			await ToSignal(GetTree().CreateTimer(0.1f), TIME_OUT);

			// Up Animation
			Tween lTileTween = CreateTween();
			lTileTween.Parallel().TweenProperty(pTargetCell, GLOBALPOSITION, pFinalPosition, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);

			if (pTargetCell.GetContent() != null)
			{
				lTileTween.Parallel().TweenProperty(pTargetCell.GetContent(), GLOBALPOSITION, pFinalPosition, 0.6f)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);
			}

			lTileTween.TweenProperty(topPart, POSITION_Y, START_POS - 100, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);

			lTileTween.Parallel().TweenProperty(this, POSITION_Y, GlobalPosition.Y + 250, 0.3f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.In);

			await ToSignal(lPistonTween, FINISHED);

			if (pTargetCell != null)
			{
				ClearMaskRecursively(pTargetCell);

				var content = pTargetCell.GetContent();
				if (content != null)
					ClearMaskRecursively(content);
			}
			

			await ToSignal(GetTree().CreateTimer(1f), TIME_OUT);
			ClearMaskRecursively(topPart);
			QueueFree(); 
		}

		private static void ButtonActivation()
		{
            HUD.GetInstance().mainMenuButton.Disabled = false;
            if (LevelCreator.inLevelCreator) LevelCreator.GetInstance().returnButton.Disabled = false;
        }
	}
}
