namespace Gameplay.StateMachine.Game.Interface
{
    using HyperGames.HyperCasual.Others.StateMachine.Interface;

    public interface IGameState : IState
    {
    }

    public interface IGameState<in TModel> : IState
    {
        public TModel Model { set; }
    }
}