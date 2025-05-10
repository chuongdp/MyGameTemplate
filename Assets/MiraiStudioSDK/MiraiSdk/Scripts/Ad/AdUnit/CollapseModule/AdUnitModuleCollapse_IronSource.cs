#if IRONSOURCE_IMPLEMENT
using com.unity3d.mediation;
using DVAH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace DVAH
{
    public class AdUnitModuleCollapse_IronSource : AdUnitModule
    {
         #if UNITY_EDITOR
             AdUnitModule _fakeAD;
            #endif
        Dictionary<int,float> _bannerRetryAttempt = new Dictionary<int, float>();
        Dictionary<int,bool> _isBannerCurrentlyShows = new Dictionary<int, bool>();

        Dictionary<int,bool> _isLoaded = new Dictionary<int, bool>();

        Dictionary<int,LevelPlayBannerAd> _bannerViews = new Dictionary<int, LevelPlayBannerAd>();
        
        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK,aD_TYPE, unitIDs); 
            Debug.Log($"==> {CONSTANT.Prefix} Ad {adFomart} init! <==");
            #if UNITY_EDITOR
             _fakeAD = this.gameObject.AddComponent<AdUnitModuleCollapse_Base>().Init(ModuleSDK,aD_TYPE,unitIDs);
            #endif
            foreach (string s in this._unitIDs)
            {
                if(unitIDs.IndexOf(s) <= -1)
                    continue;
                int ID = this._unitIDs.IndexOf(s); 
                _isLoaded.Add(ID,false);
                _isBannerCurrentlyShows.Add(ID,false);
                _bannerRetryAttempt.Add(ID,0);
                Init(ID);
            }
            return this;
        }

        #region Banner Init Methods
        private void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        { 
            int ID = this._unitIDs.IndexOf(adInfo.adUnitId);
            _bannerRetryAttempt[ID] = 0;
              Debug.Log(CONSTANT.Prefix + $"==> {adFomart} loaded <==");
            _moduleSDK.EventCallback(adFomart,AdUnitEvent.OnLoaded,adInfo.adUnitId,adInfo, _placement);
            _isLoaded[ID] = true;
            if(_moduleSDK.DVAH_Data.NO_ADS){
                 this.Hide();
                 return;
            }
             
            if(_isBannerCurrentlyShows[ID]){
                this.Show(ID);
                return;
            }

            this.Hide(ID);
             
            if(AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.Collapse) && AdBridge.Instant.CappingTimes[AD_TYPE.Collapse].ContainsKey(ID))
                 UnityMainThread.wkr.AddJob(() =>
                {
                    StartCoroutine( waitLoad(ID,AdBridge.Instant.CappingTimes[AD_TYPE.Collapse][ID].Invoke()));
                });
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError) 
        {
             int ID = this._unitIDs.IndexOf(ironSourceError.AdUnitId);
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to load with error code: " + ironSourceError.ErrorCode+ " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adFomart.ToString(), ironSourceError.ErrorMessage, _placement);

             _bannerRetryAttempt[ID]++;
            double retryDelay = Math.Pow(2, Math.Min(6,  _bannerRetryAttempt[ID]));
          
            UnityMainThread.wkr.AddJob(() =>
            {
                StartCoroutine( waitLoad(ID,(float)retryDelay));
            });
        }

        private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adInfo.adUnitId, adInfo,_placement);

            this.InvokeCallback(0, AdUnitState.Click);
             int ID = this._unitIDs.IndexOf(adInfo.adUnitId);
            double retryDelay = Math.Pow(2, Math.Min(6, _bannerRetryAttempt[ID]));
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
 
        private void BannerOnAdExpandedEvent(LevelPlayAdInfo info)
        {
            throw new NotImplementedException();
        }

        private void BannerOnAdCollapsedEvent(LevelPlayAdInfo info)
        {
            Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> {adFomart} dismissed <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, info.adUnitId, info,_placement);

            this.InvokeCallback(0, AdUnitState.Closed);
             
            Load(this._unitIDs.IndexOf(info.adUnitId));
        }

        private void BannerOnAdClickedEvent(LevelPlayAdInfo info)
        {
             Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> {adFomart} clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, info.adUnitId, info,_placement);
            this.InvokeCallback(0, AdUnitState.Click);
        }

        private void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError error)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} show fail! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, error.DisplayLevelPlayAdInfo.adUnitId, error,_placement);
         
            this.InvokeCallback(0, AdUnitState.Interupt);
        }

        private void BannerOnAdDisplayedEvent(LevelPlayAdInfo info)
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} show! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, info.adUnitId, info,_placement);
         
            this.InvokeCallback(0, AdUnitState.Open);
        }


        #endregion

        void Init(int ID = 0){
            
            LevelPlayAdSize adSize = LevelPlayAdSize.CreateAdaptiveAdSize();
             
            LevelPlayBannerAd bannerAd = new LevelPlayBannerAd(this._unitIDs[ID], adSize);
            bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
            bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
            bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
            bannerAd.OnAdClicked += BannerOnAdClickedEvent;
            bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
            bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
            bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

            if(_bannerViews.ContainsKey(ID)){ 
                _bannerViews.Add(ID,bannerAd);
            }
            _bannerViews[ID] = bannerAd;

            Load(ID);
        }

        #region Load/Show
        public override void Load(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;

            Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {this._unitIDs[ID]} <=="); 
        
            _bannerViews[ID].LoadAd();

        } 
        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            #if UNITY_EDITOR
                _fakeAD.Show(ID, callback);
            #endif
            base.Show(ID, callback);
            _isBannerCurrentlyShows[ID] = true;
            _bannerViews[ID].ShowAd();
        }

        public override void Hide(int ID = 0)
        {
             #if UNITY_EDITOR
                _fakeAD.Hide(ID);
            #endif
            _isBannerCurrentlyShows[ID] = false;
            _bannerViews[ID].HideAd();
        }

        public override bool IsLoaded(int ID = 0)
        {
            if (!this.CheckID(ID))
                return false;
            #if UNITY_EDITOR
                return _fakeAD.IsLoaded(ID);
            #endif
            return _isLoaded[ID];
        } 

        #endregion
    }

}
#endif