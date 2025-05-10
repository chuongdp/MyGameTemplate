#if IRONSOURCE_IMPLEMENT
using com.unity3d.mediation;
using DVAH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace DVAH
{
    public class AdUnitModuleMrecs_IronSource : AdUnitModule
    {
         #if UNITY_EDITOR
             AdUnitModule _fakeAD;
            #endif
        float bannerRetryAttempt = 0;
        bool isBannerCurrentlyShows = false;

        private Dictionary<string, LevelPlayBannerAd> bannerAds = new Dictionary<string, LevelPlayBannerAd>();
        
        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK,aD_TYPE, unitIDs); 
           // Initialize();
            #if UNITY_EDITOR
             _fakeAD = this.gameObject.AddComponent<AdUnitModuleMrecs_Base>().Init(ModuleSDK,aD_TYPE,unitIDs);
            #endif
            foreach (string s in unitIDs)
            {
                int ID = this._unitIDs.IndexOf(s); 
                Load(ID);
            }
            return this;
        }

        #region Banner Init Methods
      

        private void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        { 
            bannerRetryAttempt = 0;

            if(isBannerCurrentlyShows)
                 this.bannerAds[adInfo.adUnitId].ShowAd();
            else
                 this.bannerAds[adInfo.adUnitId].HideAd();
            
             Debug.Log(CONSTANT.Prefix + $"==> {adFomart} loaded <==");
            _moduleSDK.EventCallback(adFomart,AdUnitEvent.OnLoaded,adInfo.adUnitId,adInfo,_placement);
             
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError) 
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to load with error code: " + ironSourceError.ErrorCode + " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adFomart.ToString(), ironSourceError.ErrorCode,_placement);

            bannerRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));
            Invoke("Load", (float)retryDelay);
        }

        private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} show! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adInfo.adUnit, adInfo,_placement);
         
            this.InvokeCallback(0, AdUnitState.Open);
            
        }
        

        private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adInfo.adUnitId, adInfo,_placement);

            this.InvokeCallback(0, AdUnitState.Click);
            
            double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));
              UnityMainThread.wkr.AddJob(() =>
                {
                     StartCoroutine(waitLoad(0, (float)retryDelay));
                });
           
        }

        IEnumerator waitLoad(int ID, float delay)
        {
            yield return new WaitForSeconds(delay);
            Load(ID);
        }

        private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> {adFomart} dismissed <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adInfo.adUnit, adInfo,_placement);

            this.InvokeCallback(0, AdUnitState.Closed);
             
            Load(0);
        }


        #endregion


        #region Load/Show
        public override void Load(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;

            Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {this._unitIDs[ID]} <==");

            string bannerAdUnitId = this._unitIDs[ID];
            LevelPlayAdSize adSize = LevelPlayAdSize.MEDIUM_RECTANGLE;
            
            LevelPlayBannerPosition position =
 (int)this._moduleSDK.DVAH_Data.MrecsPosition < 6 ? LevelPlayBannerPosition.TopCenter: LevelPlayBannerPosition.BottomCenter;
            
            var tmpBannerAd = new LevelPlayBannerAd(bannerAdUnitId, adSize, LevelPlayBannerPosition.TopCenter);
            tmpBannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            tmpBannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent; 
            tmpBannerAd.OnAdClicked += BannerOnAdLeftApplicationEvent; 
            tmpBannerAd.LoadAd();
            bannerAds.Add(bannerAdUnitId, tmpBannerAd);  
        }

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            #if UNITY_EDITOR
                _fakeAD.Show(ID, callback);
            #endif
            base.Show(ID, callback);
            isBannerCurrentlyShows = true;
            this.bannerAds[this._unitIDs[ID]].ShowAd();

            ((Init_IronSource)this._moduleSDK)._placement = placement;
        }

        public override void Hide(int ID = 0)
        {
             #if UNITY_EDITOR
                _fakeAD.Hide(ID);
            #endif
            isBannerCurrentlyShows = false;
             this.bannerAds[this._unitIDs[ID]].HideAd();
        }

        public override bool IsLoaded(int ID = 0)
        {
            if (!this.CheckID(ID))
                return false;
            #if UNITY_EDITOR
                return _fakeAD.IsLoaded(ID);
            #endif
            return true;
        } 

        #endregion
    }

}
#endif