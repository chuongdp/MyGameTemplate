namespace MiraiGame.Script
{
    using System.Collections;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
#if UNITY_ANDROID
    using Google.Play.Review;
#endif

    public class UIPopupRateUs : BaseView
    {
        public Button       btnClose;
        public List<Button> btnStars;

        public Sprite grayStar;
        public Sprite goldStar;
    }

    [PopupInfo(nameof(UIPopupRateUs))]
    public class UIPopupRateUsPresenter : BasePopupPresenter<UIPopupRateUs>
    {
        private readonly SignalBus   signalBus;
        private readonly ILogService logger;

        public UIPopupRateUsPresenter(SignalBus signalBus,
            ILogService logger) : base(signalBus, logger)
        {
            this.signalBus = signalBus;
            this.logger    = logger;
        }

        public override UniTask BindData() { return UniTask.CompletedTask; }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            this.InitStarState();
            this.InitStarListener();
            this.View.btnClose.onClick.AddListener(() =>
            {
                this.InitStarState();
                this.CloseView();
            });
        }

        private void InitStarState()
        {
            foreach (var item in this.View.btnStars) item.image.sprite = this.View.grayStar;

            for (var i = 0; i < 3; i++) this.View.btnStars[i].image.sprite = this.View.goldStar;
        }

        private void InitStarListener()
        {
            for (var i = 0; i < this.View.btnStars.Count; i++)
            {
                var index = i;
                this.View.btnStars[i].onClick.AddListener(() => this.HandleClickStar(index));
            }
        }

        private void HandleClickStar(int index)
        {
            foreach (var item in this.View.btnStars) item.image.sprite = this.View.grayStar;

            for (var i = 0; i <= index; i++) this.View.btnStars[i].image.sprite = this.View.goldStar;

            if (index < 4) return;

            this.Rate();
            this.InitStarState();
            this.CloseView();
        }

        private void Rate() { this.InitReview().Forget(); }

        private async UniTaskVoid InitReview()
        {
            this.DirectlyOpen();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        private async void DirectlyOpen()
        {
            var linkApp = "";
#if UNITY_EDITOR
            linkApp = "https://play.google.com/store/apps/details?id=" + Application.identifier;
#elif UNITY_IOS
            linkApp = "itms-apps://itunes.apple.com/app/id" + appleAppId;
#elif UNITY_ANDROID
            linkApp = "market://details?id=" + Application.identifier;
#endif
            Application.OpenURL(linkApp);
        }
    }
}