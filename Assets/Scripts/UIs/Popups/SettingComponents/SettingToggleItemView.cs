namespace MiraiGame.Script.UIs.Popups.SettingComponents
{
    using System;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.MVP;
    using GameFoundation.Scripts.UIModule.Utilities.LoadImage;
    using UnityEngine.UI;

    public class SettingToggleItemView : TViewMono
    {
        public Button BtnToggle;
        public Image  ImgToggle;
    }

    public class SettingToggleItemModel
    {
        public bool         IsOn;
        public Action<bool> Callback;
    }

    public class SettingToggleItemPresenter : BaseUIItemPresenter<SettingToggleItemView, SettingToggleItemModel>
    {
        private const string ToggleOn  = "ToggleOn";
        private const string ToggleOff = "ToggleOff";
        private const float  PosX      = 30f;

        private readonly LoadImageHelper loadImageHelper;

        private SettingToggleItemModel model;

        public SettingToggleItemPresenter(IGameAssets gameAssets, LoadImageHelper loadImageHelper) : base(gameAssets) { this.loadImageHelper = loadImageHelper; }

        public override void OnViewReady()
        {
            base.OnViewReady();
            this.View.BtnToggle.onClick.AddListener(this.ToggleButton);
        }

        public override void BindData(SettingToggleItemModel param) { this.model = param; }

        private void ToggleButton()
        {
            this.model.IsOn = !this.model.IsOn;
            this.model.Callback?.Invoke(this.model.IsOn);
            this.UpdateToggleState();
        }

        private async void UpdateToggleState()
        {
            // Animate the toggle button
            var targetPos = this.model.IsOn ? PosX : -PosX;
            this.View.ImgToggle.transform.DOLocalMoveX(targetPos, 0.5f).SetEase(Ease.OutBack);
            // Update the toggle image based on the state
            this.View.ImgToggle.sprite = await this.loadImageHelper.LoadLocalSprite(this.model.IsOn ? ToggleOn : ToggleOff);
            this.model.Callback?.Invoke(this.model.IsOn);
        }
    }
}