namespace MyGame.Script.Installer
{
    using GameFoundation.DI;
    using MyGame.Script.Services;
    using UnityEngine;
    using VContainer;

    public static class GameServicesVContainer
    {
        public static void RegisterGameServices(this IContainerBuilder builder, Transform rootTransform)
        {
            builder.Register<LocalDataHandleService>(Lifetime.Singleton);
            builder.Register<LocalSettingDataHandleService>(Lifetime.Singleton);
        }
    }
}