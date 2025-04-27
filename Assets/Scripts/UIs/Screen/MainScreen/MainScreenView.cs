namespace UnityTemplateProjects.UIs.Screen.MainScreen
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.Utilities;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using MyGame.Script.Services;
    using HyperGames.UnityTemplate.UnityTemplate.Interfaces;
    using HyperGames.UnityTemplate.UnityTemplate.Services.Vibration;
    using MyGame.Script.UIs.Common;
    using UnityEngine;
    using UnityEngine.UI;

    public class MainScreenView : BaseView
    {
        public HeaderItem HeaderItem;
        public Button     BtnAttack;
    }

    [ScreenInfo(nameof(MainScreenView))]
    public class MainScreenPresenter : BaseScreenPresenter<MainScreenView>
    {
        private readonly SignalBus              signalBus;
        private readonly ILogService            logger;
        private readonly IAudioService          audioService;
        private readonly IVibrationService      vibrationService;
        private readonly IDependencyContainer   container;
        private readonly LocalDataHandleService localDataHandleService;

        private HeaderItemPresenter headerItemPresenter;

        public MainScreenPresenter(SignalBus signalBus,
            ILogService logger,
            IAudioService audioService,
            IVibrationService vibrationService,
            IDependencyContainer container,
            LocalDataHandleService localDataHandleService) : base(signalBus, logger)
        {
            this.signalBus              = signalBus;
            this.logger                 = logger;
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
            this.View.BtnAttack.onClick.AddListener(this.OnAttackPressed);
            this.headerItemPresenter ??= this.container.Instantiate<HeaderItemPresenter>();
            this.headerItemPresenter.SetView(this.View.HeaderItem);
            this.headerItemPresenter.BindData(new HeaderItemModel()
            {
                IsPauseButtonVisible = true
            });

            if (this.localDataHandleService.IsFirstTime)
                this.InitTutorial();
        }

        private void InitTutorial() { }

        public override UniTask BindData() { return UniTask.CompletedTask; }

        private void OnAttackPressed()
        {
            this.logger.Log($"Vibrate: {VibrationPresetType.LightImpact}");
            this.vibrationService.PlayPresetType(VibrationPresetType.LightImpact);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.View.BtnAttack.onClick.RemoveListener(this.OnAttackPressed);

            if (this.headerItemPresenter == null) return;
            this.headerItemPresenter.Dispose();
            this.headerItemPresenter = null;
        }
    }
}