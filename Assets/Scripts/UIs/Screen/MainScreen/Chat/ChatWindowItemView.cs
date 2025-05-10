namespace UnityTemplateProjects.UIs.Screen.MainScreen.Chat
{
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.MVP;
    using HyperGame.Script.NetworkRequest.Services;
    using TMPro;
    using UnityEngine.UI;

    public class ChatWindowItemView : TViewMono
    {
        public ChatOSA        ChatOSA;
        public Button         BtnClose;
        public Button         BtnSend;
        public TMP_InputField InputField;
    }

    public class ChatWindowItemModel
    {
        public Action OnClose;
    }

    public class ChatWindowItemPresenter : BaseUIItemPresenter<ChatWindowItemView, ChatWindowItemModel>
    {
        private readonly IGameAssets gameAssets;
        private readonly ApiHelper   apiHelper;

        private ChatWindowItemModel model;

        public ChatWindowItemPresenter(IGameAssets gameAssets, ApiHelper apiHelper) : base(gameAssets)
        {
            this.gameAssets = gameAssets;
            this.apiHelper  = apiHelper;
        }

        public override void OnViewReady()
        {
            base.OnViewReady();
            this.View.BtnClose.onClick.AddListener(this.OnCloseButtonClicked);
            this.View.BtnSend.onClick.AddListener(this.OnSendButtonClicked);
        }

        public override void BindData(ChatWindowItemModel param) { this.model = param; }

        private void OnCloseButtonClicked()
        {
            this.View.gameObject.SetActive(false);
            this.model.OnClose?.Invoke();
        }

        private async void OnSendButtonClicked()
        {
            if (string.IsNullOrEmpty(this.View.InputField.text)) return;
            var requestText = this.View.InputField.text;
            this.AddChat(this.View.ChatOSA.GetItemsCount(), new ChatMessageModel
            {
                timestampSec = (int)(DateTime.UtcNow.Subtract(ChatMessageModel.EPOCH_START_TIME)).TotalSeconds,
                Text         = requestText,
                IsMine       = true,
                ImageIndex   = -1
            });

            this.View.InputField.text = string.Empty;
            this.ShowBotThinking();
            var res = await this.apiHelper.SendGeminiChatRequest(requestText);
            this.RemoveChat(this.View.ChatOSA.GetItemsCount() - 1);

            await UniTask.Delay(200);

            if (res == null) return;
            this.AddChat(this.View.ChatOSA.GetItemsCount(), new ChatMessageModel
            {
                timestampSec = (int)(DateTime.UtcNow.Subtract(ChatMessageModel.EPOCH_START_TIME)).TotalSeconds,
                Text         = res.Candidates.First().Contents.Parts.First().Text,
                IsMine       = false,
                ImageIndex   = -1
            });
        }

        private void ShowBotThinking()
        {
            this.AddChat(this.View.ChatOSA.GetItemsCount(), new ChatMessageModel
            {
                timestampSec = (int)(DateTime.UtcNow.Subtract(ChatMessageModel.EPOCH_START_TIME)).TotalSeconds,
                Text         = String.Empty,
                IsMine       = false,
                ImageIndex   = -1
            });
        }

        private void AddChat(int index, ChatMessageModel chatModel) { this.View.ChatOSA.Data.InsertOne(index, chatModel, true); }

        private void RemoveChat(int index)
        {
            if (this.View.ChatOSA.Data.Count <= 0) return;

            this.View.ChatOSA.Data.RemoveOneFromEnd();
        }
    }
}