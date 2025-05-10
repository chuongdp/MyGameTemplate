namespace MiraiGame.Script
{
    using BlueprintFlow.BlueprintControlFlow;
    using Cysharp.Threading.Tasks;
    using DVAH;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using GameFoundation.Signals;
    using MiraiGame.Script.Services;
    using HyperGames.UnityTemplate.UnityTemplate.Scenes.Loading;
    using HyperGames.UnityTemplate.UnityTemplate.UserData;

    public class LoadingScreenView : UnityTemplateLoadingScreenView
    {
    }

    [ScreenInfo(nameof(LoadingScreenView))]
    public class LoadingScreenPresenter : UnityTemplateLoadingScreenPresenter
    {
        private readonly ILogService       logger;
        private readonly AdServices        adServices;
        private readonly AnalyticsServices analyticsServices;

        protected override string NextSceneName { get; } = "ChapterSelection";

        protected LoadingScreenPresenter(SignalBus              signalBus,
                                         ILogService            logger,
                                         BlueprintReaderManager blueprintManager,
                                         UserDataManager        userDataManager,
                                         IGameAssets            gameAssets,
                                         ObjectPoolManager      objectPoolManager,
                                         AdServices             adServices,
                                         AnalyticsServices      analyticsServices) : base(signalBus, logger, blueprintManager, userDataManager, gameAssets, objectPoolManager)
        {
            this.logger            = logger;
            this.adServices        = adServices;
            this.analyticsServices = analyticsServices;
        }

        protected override UniTask ShowAoa()
        {
            UniTask.Delay(10000);
            UniTask.WaitUntil(() => this.adServices.AdBridge.GetModule(AD_TYPE.Aoa, 0).isInitDone());

            this.logger.Log($"Show AOA Ad: {this.adServices.IsReady}");
            var isAoaFinish = false;
            this.adServices.ShowAdByType(AD_TYPE.Banner, callback: (adId, state) =>
            {
                this.logger.Log($"Check AOA Ad Showed: {adId} - {state}");
                isAoaFinish = state != AdUnitState.None;
            });

            return UniTask.CompletedTask;
        }
    }
}