namespace Game.Script.UIs.Common
{
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.MVP;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using HyperGame.Script.NetworkRequest.Services;
    using Game.Script.UIs.Popups;
    using UnityEngine;
    using UnityEngine.UI;

    public class HeaderItem : TViewMono
    {
        public Button BtnSetting;
        public Button BtnShop;
        public Button BtnPause;
    }

    public class HeaderItemModel
    {
        public bool IsPauseButtonVisible = false;
    }

    public class HeaderItemPresenter : BaseUIItemPresenter<HeaderItem, HeaderItemModel>
    {
        private readonly IScreenManager screenManager;
        private readonly ApiHelper      apiHelper;

        public HeaderItemPresenter(IGameAssets gameAssets, IScreenManager screenManager,
                                   ApiHelper   apiHelper) : base(gameAssets)
        {
            this.screenManager = screenManager;
            this.apiHelper     = apiHelper;
        }

        public override void BindData(HeaderItemModel param)
        {
            this.View.BtnSetting.onClick.AddListener(this.OnSettingButtonClicked);
            this.View.BtnShop.onClick.AddListener(this.OnShopButtonClicked);
            this.View.BtnPause.onClick.AddListener(this.OnPauseButtonClicked);
            this.View.BtnPause.gameObject.SetActive(param.IsPauseButtonVisible);
        }

        private void OnSettingButtonClicked() { this.screenManager.OpenScreen<UIPopupSettingPresenter>(); }

        private async void OnShopButtonClicked()
        {
            // Handle shop button click
            Debug.Log("Shop button clicked");
        }

        private void OnPauseButtonClicked()
        {
            // Handle pause button click
            this.screenManager.OpenScreen<PopupPausePresenter>();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.View.BtnSetting.onClick.RemoveListener(this.OnSettingButtonClicked);
            this.View.BtnShop.onClick.RemoveListener(this.OnShopButtonClicked);
        }
    }
}