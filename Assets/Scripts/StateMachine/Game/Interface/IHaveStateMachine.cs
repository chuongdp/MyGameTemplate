namespace Gameplay.StateMachine.Game.Interface
{
    using HyperGames.UnityTemplate.UnityTemplate.Others.StateMachine.Controller;

    public interface IHaveStateMachine
    {
        public StateMachine StateMachine { get; set; }
    }
}