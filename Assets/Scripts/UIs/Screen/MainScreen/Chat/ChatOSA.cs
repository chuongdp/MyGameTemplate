namespace UnityTemplateProjects.UIs.Screen.MainScreen.Chat
{
    using System;
    using Com.ForbiddenByte.OSA.Core;
    using Com.ForbiddenByte.OSA.CustomParams;
    using Com.ForbiddenByte.OSA.DataHelpers;
    using Cysharp.Threading.Tasks;
    using frame8.Logic.Misc.Other.Extensions;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ChatOSA : OSA<MyParams, ChatMessageViewsHolder>
    {
        public SimpleDataHelper<ChatMessageModel> Data { get; private set; }

        #region OSA implementation

        protected override void Awake()
        {
            base.Awake();

            this.Data = new SimpleDataHelper<ChatMessageModel>(this);
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            if (!this.IsInitialized)
                return;

            for (var i = 0; i < this.VisibleItemsCount; i++)
            {
                var visibleVH = this.GetItemViewsHolder(i);
                if (visibleVH.IsPopupAnimationActive)
                    visibleVH.UpdatePopupAnimation(this.Time);
            }
        }

        /// <inheritdoc/>
        protected override ChatMessageViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ChatMessageViewsHolder();
            instance.Init(this._Params.ItemPrefab, this._Params.Content, itemIndex);

            return instance;
        }

        /// <inheritdoc/>
        protected override void OnItemHeightChangedPreTwinPass(ChatMessageViewsHolder vh)
        {
            base.OnItemHeightChangedPreTwinPass(vh);

            this.Data[vh.ItemIndex].HasPendingVisualSizeChange = false;
        }

        /// <inheritdoc/>
        protected override void UpdateViewsHolder(ChatMessageViewsHolder newOrRecycled)
        {
            // Initialize the views from the associated model
            var model = this.Data[newOrRecycled.ItemIndex];

            newOrRecycled.UpdateFromModel(model, this._Params);

            if (model.HasPendingVisualSizeChange)
            {
                // Height will be available before the next 'twin' pass, inside OnItemHeightChangedPreTwinPass() callback (see above)
                newOrRecycled.MarkForRebuild(); // will enable the content size fitter
                //newOrRecycled.contentSizeFitter.enabled = true;
                this.ScheduleComputeVisibilityTwinPass(true);
            }

            if (!newOrRecycled.IsPopupAnimationActive && newOrRecycled.itemIndexInView == this.GetItemsCount() - 1) // only animating the last one
                newOrRecycled.ActivatePopulAnimation(this.Time);
        }

        /// <inheritdoc/>
        protected override void OnBeforeRecycleOrDisableViewsHolder(ChatMessageViewsHolder inRecycleBinOrVisible, int newItemIndex)
        {
            inRecycleBinOrVisible.DeactivatePopupAnimation();

            base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);
        }

        /// <inheritdoc/>
        protected override void RebuildLayoutDueToScrollViewSizeChange()
        {
            // Invalidate the last sizes so that they'll be re-calculated
            this.SetAllModelsHavePendingSizeChange();

            base.RebuildLayoutDueToScrollViewSizeChange();
        }

        /// <summary>
        /// When the user resets the count or refreshes, the OSA's cached sizes are cleared so we can recalculate them. 
        /// This is provided here for new users that just want to call Refresh() and have everything updated instead of telling OSA exactly what has updated.
        /// But, in most cases you shouldn't need to ResetItems() or Refresh() because of performace reasons:
        /// - If you add/remove items, InsertItems()/RemoveItems() is preferred if you know exactly which items will be added/removed;
        /// - When just one item has changed externally and you need to force-update its size, you'd call ForceRebuildViewsHolderAndUpdateSize() on it;
        /// - When the layout is rebuilt (when you change the size of the viewport or call ScheduleForceRebuildLayout()), that's already handled
        /// So the only case when you'll need to call Refresh() (and override ChangeItemsCount()) is if your models can be changed externally and you'll only know that they've changed, but won't know which ones exactly.
        /// </summary>
        public override void ChangeItemsCount(ItemCountChangeMode changeMode, int itemsCount, int indexIfInsertingOrRemoving = -1, bool contentPanelEndEdgeStationary = false,
                                              bool                keepVelocity = false)
        {
            if (changeMode == ItemCountChangeMode.RESET) this.SetAllModelsHavePendingSizeChange();

            base.ChangeItemsCount(changeMode, itemsCount, indexIfInsertingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
        }

        #endregion

        private void SetAllModelsHavePendingSizeChange()
        {
            foreach (var model in this.Data)
                model.HasPendingVisualSizeChange = true;
        }
    }

    /// <summary><see cref="HasPendingVisualSizeChange"/> is set to true each time a property that can affect the height changes</summary>
    public class ChatMessageModel
    {
        public static readonly DateTime EPOCH_START_TIME = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public int timestampSec;

        public DateTime TimestampAsDateTime
        {
            get
            {
                // Unix timestamp is seconds past epoch
                var dtDateTime = EPOCH_START_TIME.AddSeconds(this.timestampSec).ToLocalTime();

                return dtDateTime;
            }
        }

        public string Text
        {
            get => this._Text;
            set
            {
                if (this._Text == value)
                    return;

                this._Text                      = value;
                this.HasPendingVisualSizeChange = true;
            }
        }

        public int ImageIndex
        {
            get => this._ImageIndex;
            set
            {
                if (this._ImageIndex == value)
                    return;

                this._ImageIndex                = value;
                this.HasPendingVisualSizeChange = true;
            }
        }

        public bool IsMine { get; set; }

        /// <summary>This will be true when the item size may have changed and the ContentSizeFitter component needs to be updated</summary>
        public bool HasPendingVisualSizeChange { get; set; }

        private string _Text;
        private int    _ImageIndex;
    }

    [Serializable] // serializable, so it can be shown in inspector
    public class MyParams : BaseParamsWithPrefab
    {
        public Sprite[] availableChatImages; // used to randomly generate models;
    }

    /// <summary>The ContentSizeFitter should be attached to the item itself</summary>
    public class ChatMessageViewsHolder : BaseItemViewsHolder
    {
        public TextMeshProUGUI timeText, text;
        public Image           leftIcon, rightIcon;
        public Image           image;
        public Image           messageContentPanelImage;

        private ContentSizeFitter ContentSizeFitter       { get; set; }
        private float             PopupAnimationStartTime { get; set; }
        public  bool              IsPopupAnimationActive  => this.isAnimating;

        private const float PopupAnimationTime = .2f;

        private bool                isAnimating;
        private VerticalLayoutGroup rootLayoutGroup,   messageContentLayoutGroup;
        private int                 paddingAtIconSide, paddingAtOtherSide;
        private Color               colorAtInit;
        private bool                isThinking;

        public override void CollectViews()
        {
            base.CollectViews();

            this.rootLayoutGroup    = this.root.GetComponent<VerticalLayoutGroup>();
            this.paddingAtIconSide  = this.rootLayoutGroup.padding.right;
            this.paddingAtOtherSide = this.rootLayoutGroup.padding.left;

            this.ContentSizeFitter         = this.root.GetComponent<ContentSizeFitter>();
            this.ContentSizeFitter.enabled = false; // the content size fitter should not be enabled during normal lifecycle, only in the "Twin" pass frame
            this.root.GetComponentAtPath("MessageContentPanel", out this.messageContentLayoutGroup);
            this.messageContentPanelImage = this.messageContentLayoutGroup.GetComponent<Image>();
            this.messageContentPanelImage.transform.GetComponentAtPath("Image",   out this.image);
            this.messageContentPanelImage.transform.GetComponentAtPath("txtTime", out this.timeText);
            this.messageContentPanelImage.transform.GetComponentAtPath("txtChat", out this.text);
            this.root.GetComponentAtPath("LeftIconImage",  out this.leftIcon);
            this.root.GetComponentAtPath("RightIconImage", out this.rightIcon);
            this.colorAtInit = this.messageContentPanelImage.color;
        }

        public override void MarkForRebuild()
        {
            base.MarkForRebuild();
            if (this.ContentSizeFitter) this.ContentSizeFitter.enabled = true;
        }

        public override void UnmarkForRebuild()
        {
            if (this.ContentSizeFitter) this.ContentSizeFitter.enabled = false;
            base.UnmarkForRebuild();
        }

        /// <summary>Utility getting rid of the need of manually writing assignments</summary>
        public void UpdateFromModel(ChatMessageModel model, MyParams parameters)
        {
            this.isThinking = string.IsNullOrEmpty(model.Text);
            if (string.IsNullOrEmpty(model.Text))
            {
                this.text.text     = string.Empty;
                this.timeText.text = string.Empty;

                // animate the text to show the thinking animation with ...
                this.AnimateThinking();
                this.SetMessageContent(false);

                return;
            }

            this.timeText.text = model.TimestampAsDateTime.ToString("HH:mm");

            var messageText                                   = model.Text;
            if (this.text.text != messageText) this.text.text = messageText;

            this.leftIcon.gameObject.SetActive(!model.IsMine);
            this.rightIcon.gameObject.SetActive(model.IsMine);
            if (model.ImageIndex < 0)
            {
                this.image.gameObject.SetActive(false);
            }
            else
            {
                this.image.gameObject.SetActive(true);
                this.image.sprite = parameters.availableChatImages[model.ImageIndex];
            }

            this.SetMessageContent(model.IsMine);
        }

        private async void AnimateThinking()
        {
            // animate the text to show the thinking animation with . .. ...
            while (this.isThinking)
            {
                if (this.text.text.Length > 2)
                    this.text.text = ".";
                else
                    this.text.text += ".";
                await UniTask.Delay(500);
            }
        }

        private void SetMessageContent(bool isMine)
        {
            this.messageContentPanelImage.rectTransform.pivot = isMine ? new Vector2(1.4f, .5f) : new Vector2(-.4f, .5f);
            this.messageContentPanelImage.color               = isMine ? new Color(.75f, 1f, 1f, this.colorAtInit.a) : this.colorAtInit;
            this.rootLayoutGroup.childAlignment               = this.messageContentLayoutGroup.childAlignment = isMine ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            this.text.alignment                               = isMine ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            this.rootLayoutGroup.padding.right                = isMine ? this.paddingAtIconSide : this.paddingAtOtherSide;
            this.rootLayoutGroup.padding.left                 = isMine ? this.paddingAtOtherSide : this.paddingAtIconSide;
            this.leftIcon.gameObject.SetActive(!isMine);
            this.rightIcon.gameObject.SetActive(isMine);
        }

        public void DeactivatePopupAnimation()
        {
            this.messageContentPanelImage.transform.localScale = Vector3.one;
            this.isAnimating                                   = false;
        }

        public void ActivatePopulAnimation(float unityTime)
        {
            var s = this.messageContentPanelImage.transform.localScale;
            s.x                                                = 0;
            this.messageContentPanelImage.transform.localScale = s;
            this.PopupAnimationStartTime                       = unityTime;
            this.isAnimating                                   = true;
        }

        internal void UpdatePopupAnimation(float unityTime)
        {
            var   elapsed = unityTime - this.PopupAnimationStartTime;
            float t01;
            if (elapsed > PopupAnimationTime)
                t01 = 1f;
            else
                // Normal in, sin slow out
                t01 = Mathf.Sin(elapsed / PopupAnimationTime * Mathf.PI / 2);

            var s = this.messageContentPanelImage.transform.localScale;
            s.x                                                = t01;
            this.messageContentPanelImage.transform.localScale = s;

            if (t01 == 1f) this.DeactivatePopupAnimation();
        }
    }
}