namespace MiraiGame.Script.Services
{
    using System;
    using Cysharp.Threading.Tasks;
    using DVAH;
    using GameFoundation.DI;
    using MiraiGame.Script.Services.Interface;
    using LocalData;
    using UnityEngine;

    public class AdServices : IAdServices, IInitializable
    {
        private readonly UserLocalData userLocalData;

        public AdBridge AdBridge => AdBridge.Instant;
        public bool     IsReady  => AdBridge.Instant.isInitDone();

        public AdServices(UserLocalData userLocalData) { this.userLocalData = userLocalData; }

        public async void Initialize() { Debug.Log($"Check AdServices Initialize: {AdBridge.Instant.isInitDone()}"); }

        #region Common

        public virtual void ShowAoa()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(AD_TYPE.Aoa);
        }

        public void ShowBanner()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(AD_TYPE.Banner);
        }

        public void HideBanner()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.HideAd(AD_TYPE.Banner);
        }

        public void ShowInterstitial(string placement)
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(AD_TYPE.Inter);
        }

        public void ShowRewardedAd(string placement)
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(AD_TYPE.Reward);
        }

        public void ShowMrec()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(AD_TYPE.MRecs);
        }

        public void HideMrec()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.HideAd(AD_TYPE.MRecs);
        }

        public void ShowNativeAd()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(AD_TYPE.Native);
        }

        public void HideNativeAd()
        {
            if (!this.IsReady) return;

            AdBridge.Instant.HideAd(AD_TYPE.Native);
        }

        #endregion

        #region Specific use

        public void ShowAdByType(AD_TYPE adType, int id = 0, Action<int, AdUnitState> callback = null, bool isShowNoAd = false, string placement = null, bool ignoreCapping = false,
                                 bool    allowDuplicate = false)
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(adType, id, callback, isShowNoAd, placement, ignoreCapping, allowDuplicate);
        }

        public void ShowNativeAdObj(Transform parent, int id = 0, string placement = "unknow", float minX = 0, float minY = 0, float maxX = 1, float maxY = 1)
        {
            if (!this.IsReady) return;

            AdBridge.Instant.ShowAd(parent, id, placement, minX, minY, maxX, maxY);
        }

        #endregion
    }
}