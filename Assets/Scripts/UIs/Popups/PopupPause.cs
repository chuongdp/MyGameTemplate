namespace Game.Script.UIs.Popups
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using UnityEngine;
    using UnityEngine.UI;

    public class PopupPause : BaseView
    {
        public Button BtnClose;
        public Button BtnSetting;
        public Button BtnShop;
        public Button BtnHome;
    }

    [PopupInfo(nameof(PopupPause))]
    public class PopupPausePresenter : BaseScreenPresenter<PopupPause>
    {
        private readonly IGameAssets    gameAssets;
        private readonly SceneDirector  sceneDirector;
        private readonly IScreenManager screenManager;

        public PopupPausePresenter(SignalBus      signalBus,  ILogService   logger,
                                   IGameAssets    gameAssets, SceneDirector sceneDirector,
                                   IScreenManager screenManager) : base(signalBus, logger)
        {
            this.gameAssets    = gameAssets;
            this.sceneDirector = sceneDirector;
            this.screenManager = screenManager;
        }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            this.Init();
        }

        public override UniTask BindData()
        {
            Time.timeScale = 0;

            return UniTask.CompletedTask;
        }

        private void Init()
        {
            this.View.BtnClose.onClick.AddListener(this.CloseView);
            this.View.BtnSetting.onClick.AddListener(this.OnSettingButtonClicked);
            this.View.BtnShop.onClick.AddListener(this.OnShopButtonClicked);
            this.View.BtnHome.onClick.AddListener(this.OnHomeButtonClicked);
        }

        private void OnSettingButtonClicked() { this.screenManager.OpenScreen<UIPopupSettingPresenter>(); }

        private void OnShopButtonClicked()
        {
            // Handle shop button click
        }

        private async void OnHomeButtonClicked()
        {
            // Handle home button click
            await this.sceneDirector.LoadSingleSceneAsync("ChapterSelection");
            Debug.Log($"Return to home");
            this.DestroyView();
        }

        public override void CloseView()
        {
            Time.timeScale = 1;
            base.CloseView();
        }

        public override void DestroyView()
        {
            Time.timeScale = 1;
            base.DestroyView();
        }
    }
}