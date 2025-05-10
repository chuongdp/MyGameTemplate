namespace MiraiGame.Script.UIs.Screen.ChapterSelection
{
    using System;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.MVP;
    using MiraiGame.Script.Blueprints;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ChapterItem : TViewMono
    {
        public Button          BtnSelect;
        public TextMeshProUGUI TxtChapterName;
        public GameObject      ImgSelected;
    }

    public class ChapterItemModel
    {
        public ChapterData                  ChapterData;
        public Action<ChapterItemPresenter> OnClickChapter;
        public bool                         IsSelected;
    }

    public class ChapterItemPresenter : BaseUIItemPresenter<ChapterItem, ChapterItemModel>
    {
        private ChapterItemModel model;
        public  int              ChapterId => this.model.ChapterData.Id;

        public ChapterItemPresenter(IGameAssets gameAssets) : base(gameAssets) { }

        public override void BindData(ChapterItemModel param)
        {
            this.model = param;
            this.View.BtnSelect.onClick.AddListener(this.OnClickChapter);
            this.View.TxtChapterName.text = this.model.ChapterData.ChapterName;
            this.SetSelected(this.model.IsSelected);
        }

        private void OnClickChapter() { this.model.OnClickChapter?.Invoke(this); }

        public void SetSelected(bool isSelected) { this.View.ImgSelected.SetActive(isSelected); }

        public override void Dispose()
        {
            base.Dispose();
            this.View.BtnSelect.onClick.RemoveListener(this.OnClickChapter);
        }
    }
}