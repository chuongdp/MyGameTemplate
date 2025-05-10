namespace Gameplay.StateMachine.Game.States
{
    using Gameplay.StateMachine.Game.Interface;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;

    public class GameWinState : IGameState
    {
        private readonly IScreenManager screenManager;

        public GameWinState(IScreenManager screenManager) { this.screenManager = screenManager; }

        public void Enter()
        {
            // Do the logic handle Win Game
        }

        public void Exit() { }
    }
}