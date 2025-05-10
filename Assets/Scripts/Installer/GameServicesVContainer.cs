namespace MiraiGame.Script.Installer
{
    using GameFoundation.DI;
    using MiraiGame.Script.Services;
    using UnityEngine;
    using VContainer;

    public static class GameServicesVContainer
    {
        public static void RegisterGameServices(this IContainerBuilder builder, Transform rootTransform)
        {
            builder.Register<AdServices>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<AnalyticsServices>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<LocalDataHandleService>(Lifetime.Singleton);
            builder.Register<LocalSettingDataHandleService>(Lifetime.Singleton);
        }
    }
}