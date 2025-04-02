using Godot;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
    public partial class CustomSignals : Node2D
    {
        // ---------- VARIABLES ---------- \\

        #region // ----- Singleton ----- \\

        static private CustomSignals instance;

        static public CustomSignals GetInstance()
        {
            if (instance == null) instance = new CustomSignals();
            return instance;
        }

		#endregion

        // UI Sinals
		[Signal] public delegate void GoToLoginScreenEventHandler();
        [Signal] public delegate void GoToMainMenuEventHandler();
        [Signal] public delegate void GoToLevelSelectorEventHandler();
        [Signal] public delegate void GoToLevelCreatorEventHandler();

        // Level gestion signals
        [Signal] public delegate void GoToNextLevelEventHandler(int pLevel);
        [Signal] public delegate void LoadLevelEventHandler(int pLevel);
        [Signal] public delegate void UnLoadLevelEventHandler();
        [Signal] public delegate void LevelUnlockEventHandler(int pLevelIndex); // informs that a level has been unlocked in the json
        [Signal] public delegate void LevelCompletedEventHandler(int pLevelIndex, int pStars, int pTotalScore); // informs that the player has finished a level with the score and stars

        // In Game states signals
        [Signal] public delegate void MoveEventHandler(Vector2 pDirection);
        [Signal] public delegate void PlayerMovedEventHandler();
        [Signal] public delegate void BoxTeslaMovedEventHandler();
        [Signal] public delegate void BoxTeslaCalculsDoneEventHandler();
        [Signal] public delegate void GoalBulbStateChangedEventHandler();
        [Signal] public delegate void StartRechercheEventHandler();

        // Grid signals
        [Signal] public delegate void UndoRedoEventHandler(int pPosition);
        [Signal] public delegate void UndoButtonEventHandler();
        [Signal] public delegate void RedoButtonEventHandler();
        [Signal] public delegate void RetryEventHandler();

        // Endgame signals
        [Signal] public delegate void GameFinishedEventHandler(int pNumStar, int pScore, int pNumStep);
        [Signal] public delegate void EndLevelAnimationEventHandler();
    }
}