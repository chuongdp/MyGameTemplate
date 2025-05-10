namespace MiraiGame.Script
{
    using BlueprintFlow.BlueprintControlFlow;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using GameFoundation.Signals;
    using HyperGames.UnityTemplate.UnityTemplate.Scenes.Loading;
    using HyperGames.UnityTemplate.UnityTemplate.UserData;

    public class LoadingScreenView : UnityTemplateLoadingScreenView
    {
    }

    [ScreenInfo(nameof(LoadingScreenView))]
    public class LoadingScreenPresenter : UnityTemplateLoadingScreenPresenter
    {
        private readonly ILogService logger;

        protected override string NextSceneName { get; } = "ChapterSelection";

        protected LoadingScreenPresenter(SignalBus signalBus,
            ILogService logger,
            BlueprintReaderManager blueprintManager,
            UserDataManager userDataManager,
            IGameAssets gameAssets,
            ObjectPoolManager objectPoolManager) : base(signalBus, logger, blueprintManager, userDataManager, gameAssets, objectPoolManager)
        {
            this.logger = logger;
        }

        protected override UniTask ShowAoa() { return UniTask.CompletedTask; }
    }
}