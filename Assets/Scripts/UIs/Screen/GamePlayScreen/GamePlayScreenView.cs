namespace MiraiGame.Script.UIs.Screen.GamePlayScreen
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using LocalData;
    using TMPro;
    using UnityEngine.UI;

    public class GamePlayScreenModel
    {
    }

    public class GamePlayScreenView : BaseView
    {
        public TextMeshProUGUI txtChapter;
        public Button          btnPlay;
        public Button          btnShowInter;
        public Button          btnShowReward;
    }

    [ScreenInfo(nameof(GamePlayScreenView))]
    public class GamePlayScreenPresenter : BaseScreenPresenter<GamePlayScreenView, GamePlayScreenModel>
    {
        private readonly UserLocalData  userLocalData;
        private readonly IGameAssets    gameAssets;
        private readonly SignalBus      signalBus;
        private readonly IScreenManager screenManager;
        private readonly ILogService    logger;

        public GamePlayScreenPresenter(UserLocalData  userLocalData,
                                       IGameAssets    gameAssets,
                                       SignalBus      signalBus,
                                       IScreenManager screenManager,
                                       ILogService    logger) : base(signalBus, null)
        {
            this.userLocalData = userLocalData;
            this.gameAssets    = gameAssets;
            this.signalBus     = signalBus;
            this.screenManager = screenManager;
            this.logger        = logger;
        }

        protected override void OnViewReady()
        {
            this.View.btnPlay.onClick.AddListener(this.CloseView);
            this.View.btnShowInter.onClick.AddListener(this.ShowBanner);
            this.View.btnShowReward.onClick.AddListener(this.ShowReward);
            base.OnViewReady();
        }

        public override UniTask BindData(GamePlayScreenModel screenModel)
        {
            this.InitUI();

            return UniTask.CompletedTask;
        }

        private void InitUI() { }

        private void ShowBanner() { }

        private void ShowReward() { }

        public override UniTask CloseViewAsync() { return base.CloseViewAsync(); }
    }
}