namespace Gameplay.StateMachine.Game
{
    using VContainer.Unity;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.LogService;
    using Gameplay.StateMachine.Game.States;
    using Gameplay.StateMachine.Game.Interface;
    using GameFoundation.Signals;
    using HyperGames.HyperCasual.Others.StateMachine.Interface;
    using HyperGames.UnityTemplate.UnityTemplate.Others.StateMachine.Controller;

    public class GameStateMachine : StateMachine, IInitializable
    {
        public GameStateMachine(List<IState> listState, SignalBus signalBus, ILogService logService) : base(listState,logService, signalBus)
        {
            listState.ForEach(state =>
            {
                if (state is IHaveStateMachine haveStateMachine) haveStateMachine.StateMachine = this;
            });
        }

        public void Initialize() { this.TransitionTo<GamePlayState>(); }
    }
}