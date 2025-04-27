namespace Gameplay.StateMachine.Game.States
{
    using Gameplay.StateMachine.Game.Interface;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using MyGame.Script.UIs.Screen.GamePlayScreen;
    using HyperGames.UnityTemplate.UnityTemplate.Others.StateMachine.Controller;
    using UnityEngine;
    using UnityTemplateProjects.UIs.Screen.MainScreen;

    public class GamePlayState : IGameState, IHaveStateMachine
    {
        private readonly IScreenManager screenManager;

        public async void Enter()
        {
            // Open Scene Home when enter GamePlayState
            Debug.Log($"Open Scene Home when enter GamePlayState");
            await this.screenManager.OpenScreen<MainScreenPresenter>();
        }

        public async void Exit() { }

        public StateMachine StateMachine { get; set; }

        public GamePlayState(IScreenManager screenManager) { this.screenManager = screenManager; }
    }
}