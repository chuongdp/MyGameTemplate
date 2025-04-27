namespace Installer
{
    using VContainer;
    using UnityEngine;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using Gameplay.StateMachine.Game;
    using HyperGame.Script.Services.Internet;
    using MiraiGame.Script.Signals;
    using MiraiGame.Script.Systems;
    using MiraiGame.Script.Utilities;
    using UnityTemplateProjects.UIs.Screen.MainScreen;
    using VContainer.Unity;

    public sealed class MainSceneInstaller : SceneScope
    {
        [SerializeField] private UIRaycaster uiRaycaster;
        [SerializeField] private GameManager gameManager;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            this.SignalBusInstaller(builder);
            this.SystemInstall(builder);
            this.MonoInstaller(builder);

            builder.RegisterEntryPoint<InternetService>();

            GameStateMachineInstall.Install(builder);
        }

        private void SignalBusInstaller(IContainerBuilder builder) { builder.DeclareSignal<AttackSignal>(); }

        private void SystemInstall(IContainerBuilder builder) { builder.Register<UserInputSystem>(Lifetime.Singleton).AsInterfacesAndSelf(); }

        private void MonoInstaller(IContainerBuilder builder)
        {
            builder.RegisterInstance(this.uiRaycaster);
            builder.RegisterComponent(this.gameManager);
        }
    }
}