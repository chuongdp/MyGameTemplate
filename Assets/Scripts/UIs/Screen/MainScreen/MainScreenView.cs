namespace UnityTemplateProjects.UIs.Screen.MainScreen
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.Utilities;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using MiraiGame.Script.Services;
    using HyperGames.UnityTemplate.UnityTemplate.Interfaces;
    using HyperGames.UnityTemplate.UnityTemplate.Services.Vibration;
    using MiraiGame.Script.Signals;
    using MiraiGame.Script.UIs.Common;
    using UnityEngine.UI;
    using UnityTemplateProjects.UIs.Screen.MainScreen.Chat;

    public class MainScreenView : BaseView
    {
        public HeaderItem         HeaderItem;
        public Button             BtnPlayRandomAnimation;
        public Button             BtnPlayRandomEmotion;
        public Button             BtnShowChat;
        public ChatWindowItemView ChatWindowItemView;
    }

    [ScreenInfo(nameof(MainScreenView))]
    public class MainScreenPresenter : BaseScreenPresenter<MainScreenView>
    {
        private const string TutorialName = "control";

        private readonly SignalBus              signalBus;
        private readonly ILogService            logger;
        private readonly AdServices             adServices;
        private readonly IAudioService          audioService;
        private readonly IVibrationService      vibrationService;
        private readonly IDependencyContainer   container;
        private readonly LocalDataHandleService localDataHandleService;

        private HeaderItemPresenter     headerItemPresenter;
        private ChatWindowItemPresenter chatWindowItemPresenter;

        public MainScreenPresenter(SignalBus              signalBus,
                                   ILogService            logger,
                                   AdServices             adServices,
                                   IAudioService          audioService,
                                   IVibrationService      vibrationService,
                                   IDependencyContainer   container,
                                   LocalDataHandleService localDataHandleService) : base(signalBus, logger)
        {
            this.signalBus              = signalBus;
            this.logger                 = logger;
            this.adServices             = adServices;
            this.audioService           = audioService;
            this.vibrationService       = vibrationService;
            this.container              = container;
            this.localDataHandleService = localDataHandleService;
        }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            this.Init();
        }

        private void Init()
        {
            this.View.BtnPlayRandomAnimation.onClick.AddListener(this.OnPlayRandomAnimationPressed);
            this.View.BtnPlayRandomEmotion.onClick.AddListener(this.OnPlayRandomEmotionPressed);
            this.View.BtnShowChat.onClick.AddListener(this.OnShowChatButtonPressed);
            this.InitItems();
        }

        private void InitItems()
        {
            this.headerItemPresenter ??= this.container.Instantiate<HeaderItemPresenter>();
            this.headerItemPresenter.SetView(this.View.HeaderItem);
            this.headerItemPresenter.BindData(new HeaderItemModel()
            {
                IsPauseButtonVisible = false
            });
            
            this.chatWindowItemPresenter ??= this.container.Instantiate<ChatWindowItemPresenter>();
            this.chatWindowItemPresenter.SetView(this.View.ChatWindowItemView);
            this.chatWindowItemPresenter.BindData(new ChatWindowItemModel
            {
                OnClose = () =>
                {
                    this.View.BtnShowChat.gameObject.SetActive(true);
                }
            });
            this.chatWindowItemPresenter.OnViewReady();
        }

        public override UniTask BindData() { return UniTask.CompletedTask; }

        private void OnShowChatButtonPressed()
        {
            this.View.ChatWindowItemView.gameObject.SetActive(true);
            this.View.BtnShowChat.gameObject.SetActive(false);
        }

        private void OnPlayRandomAnimationPressed()
        {
            this.signalBus.Fire(new PlayAnimationSignal());
            this.vibrationService.PlayPresetType(VibrationPresetType.LightImpact);
        }

        private void OnPlayRandomEmotionPressed()
        {
            this.signalBus.Fire(new PlayEmotionSignal());
            this.vibrationService.PlayPresetType(VibrationPresetType.LightImpact);
        }

        private void OnAttackPressed()
        {
            this.logger.Log($"Vibrate: {VibrationPresetType.LightImpact}");
            this.vibrationService.PlayPresetType(VibrationPresetType.LightImpact);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (this.headerItemPresenter == null) return;
            this.headerItemPresenter.Dispose();
            this.headerItemPresenter = null;
        }
    }
}