namespace HyperGame.Script.Services.Internet
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using Game.Script;
    using UnityEngine;

    public class InternetService : IInitializable, IInternetService
    {
        private const int MaxRetryCount = 3;

        private readonly IScreenManager screenManager;

        private int retryCount = 0;

        public InternetService(IScreenManager screenManager) { this.screenManager = screenManager; }

        private bool isInternetAvailable = true;

        public void Initialize() { this.CheckConnection().Forget(); }

        private void GetInternetConnectionStatus() { this.isInternetAvailable = Application.internetReachability != NetworkReachability.NotReachable; }

        private async UniTaskVoid CheckConnection()
        {
            this.GetInternetConnectionStatus();
            if (!this.IsInternetConnectionAvailable)
            {
                this.retryCount++;

                if (this.retryCount >= MaxRetryCount)
                {
                    this.screenManager.OpenScreen<UIPopupNoInternetPresenter>();
                    Debug.LogError("trying to open no internet popup automatically");

                    return;
                }
            }
            else
            {
                this.retryCount = 0;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            this.CheckConnection().Forget();
        }

        public void ReCheckConnection() { this.CheckConnection().Forget(); }

        public bool IsInternetConnectionAvailable => this.isInternetAvailable;
    }

    public interface IInternetService
    {
        bool IsInternetConnectionAvailable { get; }

        void ReCheckConnection();
    }
}