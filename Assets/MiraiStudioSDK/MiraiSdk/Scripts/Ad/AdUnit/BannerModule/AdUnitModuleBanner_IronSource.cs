#if IRONSOURCE_IMPLEMENT
using DVAH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace DVAH
{
    public class AdUnitModuleBanner_IronSource : AdUnitModule
    {
         #if UNITY_EDITOR
             AdUnitModule _fakeAD;
            #endif
        float bannerRetryAttempt = 0;
        bool isBannerCurrentlyShows = false;

        bool isLoaded = false;
        
        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK,aD_TYPE, unitIDs); 
            Initialize();
            #if UNITY_EDITOR
             _fakeAD = this.gameObject.AddComponent<AdUnitModuleBanner_Base>().Init(ModuleSDK,aD_TYPE,unitIDs);
            #endif
            foreach (string s in unitIDs)
            {
                int ID = this._unitIDs.IndexOf(s); 
                Load(ID);
            }
            return this;
        }

        #region Banner Init Methods
        public void Initialize()
        {
            Debug.Log($"==> {CONSTANT.Prefix} Ad {adFomart} init! <==");
            //Add AdInfo Banner Events
            IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent += BannerOnAdLeftApplicationEvent;
            IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
            IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
            IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;
 
            isBannerCurrentlyShows = this._moduleSDK.DVAH_Data.BannerDefaultShow;
        }
         

        private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
        { 
            bannerRetryAttempt = 0;
              Debug.Log(CONSTANT.Prefix + $"==> {adFomart} loaded <==");
            _moduleSDK.EventCallback(adFomart,AdUnitEvent.OnLoaded,adInfo.adUnit,adInfo, _placement);
            isLoaded = true;
            if(_moduleSDK.DVAH_Data.NO_ADS){
                 this.Hide();
                 return;
            }
            int ID = this._unitIDs.IndexOf(adInfo.adUnit);
            if(isBannerCurrentlyShows){
                this.Show(ID);
                return;
            }

            this.Hide(ID);
             
            if(AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.Banner) && AdBridge.Instant.CappingTimes[AD_TYPE.Banner].ContainsKey(ID))
                 UnityMainThread.wkr.AddJob(() =>
                {
                    StartCoroutine( waitLoad(ID,AdBridge.Instant.CappingTimes[AD_TYPE.Banner][ID].Invoke()));
                });
        }

        private void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError) 
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to load with error code: " + ironSourceError.getErrorCode() + " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adFomart.ToString(), ironSourceError.getDescription(), _placement);

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
        

        private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adInfo.adUnit, adInfo,_placement);

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
            
            IronSourceBannerPosition position =
 (int)this._moduleSDK.DVAH_Data.BannerPosition < 6 ? IronSourceBannerPosition.TOP: IronSourceBannerPosition.BOTTOM;
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, position);

        }

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            #if UNITY_EDITOR
                _fakeAD.Show(ID, callback);
            #endif
            base.Show(ID, callback);
            isBannerCurrentlyShows = true;
            IronSource.Agent.displayBanner();
        }

        public override void Hide(int ID = 0)
        {
             #if UNITY_EDITOR
                _fakeAD.Hide(ID);
            #endif
            isBannerCurrentlyShows = false;
            IronSource.Agent.hideBanner();
        }

        public override bool IsLoaded(int ID = 0)
        {
            if (!this.CheckID(ID))
                return false;
            #if UNITY_EDITOR
                return _fakeAD.IsLoaded(ID);
            #endif
            return isLoaded;
        } 

        #endregion
    }

}
#endif