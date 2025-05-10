namespace Game.Script.UIs.Popups
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using GameFoundation.Signals;
    using TMPro;
    using UnityEngine.UI;

    public class UIPopupCommon : BaseView
    {
        public Button          BtnClose;
        public Button          BtnOk;
        public Button          BtnYes;
        public Button          BtnNo;
        public TextMeshProUGUI TxtTitle;
        public TextMeshProUGUI TxtContent;
        public TextMeshProUGUI TxtOk;
        public TextMeshProUGUI TxtYes;
        public TextMeshProUGUI TxtNo;
    }

    public enum ECommonPopupType
    {
        YesNo,
        Ok
    }

    public class UIPopupCommonModel
    {
        public ECommonPopupType PopupType;
        public string           Title;
        public string           Content;
        public string           OkText  = "ok";
        public string           YesText = "yes";
        public string           NoText  = "no";
        public Action           OkCallback;
        public Action           YesCallback;
        public Action           NoCallback;
    }

    [PopupInfo(nameof(UIPopupCommon))]
    public class UIPopupCommonPresenter : BasePopupPresenter<UIPopupCommon, UIPopupCommonModel>
    {
        private readonly LocalizationService localizationService;
        private          UIPopupCommonModel  model;

        public UIPopupCommonPresenter(SignalBus signalBus, ILogService logger, LocalizationService localizationService) : base(signalBus, logger) { this.localizationService = localizationService; }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            this.View.BtnClose.onClick.AddListener(this.CloseView);
            this.View.BtnOk.onClick.AddListener(this.OnOkButtonClicked);
            this.View.BtnYes.onClick.AddListener(this.OnYesButtonClicked);
            this.View.BtnNo.onClick.AddListener(this.OnNoButtonClicked);
        }

        public override UniTask BindData(UIPopupCommonModel popupModel)
        {
            this.model = popupModel;

            this.View.TxtTitle.text   = this.localizationService.GetTextWithKey(popupModel.Title);
            this.View.TxtContent.text = this.localizationService.GetTextWithKey(popupModel.Content);

            this.View.BtnOk.gameObject.SetActive(popupModel.PopupType  == ECommonPopupType.Ok);
            this.View.BtnYes.gameObject.SetActive(popupModel.PopupType == ECommonPopupType.YesNo);
            this.View.BtnNo.gameObject.SetActive(popupModel.PopupType  == ECommonPopupType.YesNo);

            this.View.TxtOk.text  = this.localizationService.GetTextWithKey(popupModel.OkText);
            this.View.TxtYes.text = this.localizationService.GetTextWithKey(popupModel.YesText);
            this.View.TxtNo.text  = this.localizationService.GetTextWithKey(popupModel.NoText);

            return UniTask.CompletedTask;
        }

        private void OnOkButtonClicked()
        {
            this.model.OkCallback?.Invoke();
            this.CloseView();
        }

        private void OnYesButtonClicked()
        {
            this.model.YesCallback?.Invoke();
            this.CloseView();
        }

        private void OnNoButtonClicked()
        {
            this.model.NoCallback?.Invoke();
            this.CloseView();
        }

        public override void Dispose()
        {
            this.View.BtnClose.onClick.RemoveListener(this.CloseView);
            this.View.BtnOk.onClick.RemoveListener(this.OnOkButtonClicked);
            this.View.BtnYes.onClick.RemoveListener(this.OnYesButtonClicked);
            this.View.BtnNo.onClick.RemoveListener(this.OnNoButtonClicked);
            base.Dispose();
        }
    }
}