namespace MiraiGame.Script.UIs.Popups.SettingComponents
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.MVP;
    using TMPro;
    using UnityEngine.UI;

    public class SettingSelectionItemView : TViewMono
    {
        public Button          BtnPrevious;
        public Button          BtnNext;
        public TextMeshProUGUI TxtName;
    }

    public class SettingSelectionItemModel
    {
        public Dictionary<int, string> Options = new();
        public Action<int>             PreviousCallback;
        public Action<int>             NextCallback;
        public int                     CurrentOption;
    }

    public class SettingSelectionItemPresenter : BaseUIItemPresenter<SettingSelectionItemView, SettingSelectionItemModel>
    {
        private SettingSelectionItemModel model;

        public SettingSelectionItemPresenter(IGameAssets gameAssets) : base(gameAssets) { }

        public override void OnViewReady()
        {
            base.OnViewReady();
            this.View.BtnPrevious.onClick.AddListener(this.PreviousOption);
            this.View.BtnNext.onClick.AddListener(this.NextOption);
        }

        public override void BindData(SettingSelectionItemModel param)
        {
            this.model             = param;
            this.View.TxtName.text = this.model.Options[this.model.CurrentOption];
        }

        private void ShowOptionName() { this.View.TxtName.text = this.model.Options[this.model.CurrentOption]; }

        private void PreviousOption()
        {
            // check condition
            this.model.CurrentOption--;
            if (this.model.CurrentOption < 0)
                this.model.CurrentOption = this.model.Options.Count - 1;
            this.model.PreviousCallback?.Invoke(this.model.CurrentOption);
            this.ShowOptionName();
        }

        private void NextOption()
        {
            // check condition
            this.model.CurrentOption++;
            if (this.model.CurrentOption > this.model.Options.Count - 1)
                this.model.CurrentOption = 0;
            this.model.NextCallback?.Invoke(this.model.CurrentOption);
            this.ShowOptionName();
        }

        public override void Dispose()
        {
            this.View.BtnPrevious.onClick.RemoveListener(this.PreviousOption);
            this.View.BtnNext.onClick.RemoveListener(this.NextOption);
            base.Dispose();
        }
    }
}