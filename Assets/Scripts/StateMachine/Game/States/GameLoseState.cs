namespace Gameplay.StateMachine.Game.States
{
    using Gameplay.StateMachine.Game.Interface;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;

    public class GameLoseState : IGameState
    {
        private readonly IScreenManager screenManager;

        public GameLoseState(IScreenManager screenManager) { this.screenManager = screenManager; }

        public void Enter()
        {
            // Do the logic handle Lose Game
        }

        public void Exit() { }
    }
}