namespace Gameplay.StateMachine.Game.Interface
{
    using GameTemplate.UnityTemplate.Others.StateMachine.Controller;

    public interface IHaveStateMachine
    {
        public StateMachine StateMachine { get; set; }
    }
}