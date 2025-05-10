#if IRONSOURCE_IMPLEMENT
using DVAH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace DVAH
{
    public class AdUnitModuleReward_IronSource : AdUnitModule
    {
        #if UNITY_EDITOR
             AdUnitModule _fakeAD;
            #endif
        bool _Rewarded = false;
       float rewardlRetryAttempt = 0;
        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            #if UNITY_EDITOR
             _fakeAD = this.gameObject.AddComponent<AdUnitmoduleReward_Base>().Init(ModuleSDK,aD_TYPE,unitIDs);
            #endif
            base.Init(ModuleSDK,aD_TYPE, unitIDs); 
            Initialize();
            foreach (string s in unitIDs)
            {
                int ID = this._unitIDs.IndexOf(s);
                Load(ID);
            }
            return this;
        }

        #region Interstitial Init Methods
        public void Initialize()
        {
            Debug.Log($"==> {CONSTANT.Prefix} Ad {adFomart} init! <==");
 
            //Add AdInfo Rewarded Video Events
            
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent; 
        }
         

        private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} loaded <==");
            _moduleSDK.EventCallback(adFomart,AdUnitEvent.OnLoaded, adInfo.adUnit,adInfo,_placement);
              
            rewardlRetryAttempt = 0;
        }


        private void RewardedVideoOnAdUnavailable() 
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to load with error code: IS not return here!  <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed,"reward", adFomart.ToString(),_placement);

            rewardlRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardlRetryAttempt));
            Invoke("Load", (float)retryDelay);
        }

        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            _Rewarded = false;
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} show! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adInfo.adUnit, adInfo,_placement);
            _isUnitShowed = true;

            this.InvokeCallback(0, AdUnitState.Open);
            
        }

        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            this._Rewarded = true;
           
        }

        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to display with error code: " + error.getCode() + " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adInfo.adUnit, error.getDescription(),_placement);
            _isUnitShowed = false;

            this.InvokeCallback(0, AdUnitState.Interupt);
            
            double retryDelay = Math.Pow(2, Math.Min(6, rewardlRetryAttempt));
             UnityMainThread.wkr.AddJob(() =>
                {
                   StartCoroutine(waitLoad(0,(float)retryDelay));
                });
            
        }
        

        private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adInfo.adUnit, adInfo,_placement);

            this.InvokeCallback(0, AdUnitState.Click);
            
            double retryDelay = Math.Pow(2, Math.Min(6, rewardlRetryAttempt));
           
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

        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> {adFomart} dismissed <==");
            _isUnitShowed = false;
            this.InvokeCallback(0, AdUnitState.Closed);
            Load(0);
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adInfo.adUnit, adInfo,_placement);

            if(_Rewarded){
                 Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adInfo.adUnit} rewarded!!! <==");
                _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnRewarded, adInfo.adUnit, adInfo,_placement);

                this.InvokeCallback(0, AdUnitState.Watched);
                _Rewarded = false;
            }
        }


        #endregion


        #region Load/Show
        public override void Load(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;
             #if UNITY_EDITOR
             _fakeAD.Load(ID);
            #endif
            Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {this._unitIDs[ID]} <==");
            
            IronSource.Agent.loadRewardedVideo();
        }

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            if (!this.CheckID(ID))
                return;
            #if UNITY_EDITOR
                _fakeAD.Show(ID, callback, placement);
            #else
                 _isUnitShowed = this.IsLoaded(ID);
            #endif
            base.Show(ID, callback, placement);
           ((Init_IronSource)this._moduleSDK)._placement = placement;
            IronSource.Agent.showRewardedVideo();
            
        }

        public override void Hide(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;
            throw new NotImplementedException();
        }

        public override bool IsLoaded(int ID = 0)
        {
            if (!this.CheckID(ID))
                return false;
            #if UNITY_EDITOR
                return _fakeAD.IsLoaded(ID);
            #endif
            return IronSource.Agent.isRewardedVideoAvailable();
        }



        #endregion
    }

}
#endif