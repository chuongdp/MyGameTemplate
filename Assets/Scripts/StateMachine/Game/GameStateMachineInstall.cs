namespace Gameplay.StateMachine.Game
{
    using VContainer;
    using System.Linq;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.Extension;
    using HyperGames.HyperCasual.Others.StateMachine.Interface;

    public class GameStateMachineInstall
    {
        public static void Install(IContainerBuilder builder)
        {
            builder.Register<GameStateMachine>(Lifetime.Singleton)
                   .AsInterfacesAndSelf()
                   .WithParameter(container => typeof(IState).GetDerivedTypes().Select(type => (IState)container.Instantiate(type)).ToList());
        }
    }
}