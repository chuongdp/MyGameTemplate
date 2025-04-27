namespace MyGame.Script.Installer
{
    using VContainer;
    using VContainer.Unity;
    using GameFoundation.DI;
    using GameFoundation.Scripts;
    using GameFoundation.Scripts.Network;
    using HyperGame.Script;
    using HyperGame.Script.NetworkRequest.Services;
    using HyperGames.UnityTemplate;
    using UnityEngine.EventSystems;

    public sealed class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            //GDK stuff
            builder.RegisterGameFoundation(this.transform);

            //Global UI event system
            builder.RegisterComponentInNewPrefabResource<EventSystem>(nameof(EventSystem), Lifetime.Singleton).UnderTransform(this.transform);
            builder.AutoResolve<EventSystem>();

            //UI template stuff
            builder.RegisterUnityTemplate(this.transform);

            // Registering the game services
            builder.RegisterGameServices(this.transform);

            // Registering network services
            this.BindNetworkServices(builder);
        }

        private void BindNetworkServices(IContainerBuilder builder)
        {
            var networkConfig = new NetworkConfig { Host = StaticValue.Host };
            builder.RegisterInstance(networkConfig);

            // Register Network Services
            builder.RegisterNetworkServices(networkConfig);

            // Register ApiHelper with NoWrappedRequestAndResponseService
            builder.Register<ApiHelper>(Lifetime.Singleton);
        }
    }
}