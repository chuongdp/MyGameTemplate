namespace MiraiGame.Script
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using HyperGame.Script.Services.Internet;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIPopupNoInternet : BaseView
    {
        public Button btnReconnect;
        public Button btnClose;
    }

    [PopupInfo(nameof(UIPopupNoInternet))]
    public class UIPopupNoInternetPresenter : BasePopupPresenter<UIPopupNoInternet>
    {
        private readonly SignalBus        signalBus;
        private readonly ILogService      logService;
        private readonly IInternetService internetService;

        public UIPopupNoInternetPresenter(SignalBus        signalBus,
                                          ILogService      logger,
                                          IInternetService internetService) : base(signalBus, logger)
        {
            this.signalBus       = signalBus;
            this.logService      = logger;
            this.internetService = internetService;
        }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            Time.timeScale = 0;
            this.View.btnClose.onClick.AddListener(this.CloseView);
            this.View.btnReconnect.onClick.AddListener(this.OnReconnect);
        }

        public override UniTask BindData() { return UniTask.CompletedTask; }

        private void OnReconnect()
        {
            this.internetService.ReCheckConnection();
            if (this.internetService.IsInternetConnectionAvailable) this.CloseView();
        }

        public override void CloseView()
        {
            Time.timeScale = 1;
            base.CloseView();
        }
    }
}