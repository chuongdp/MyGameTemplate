#if IRONSOURCE_IMPLEMENT
using DVAH;
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine; 

namespace DVAH
{
    public class AdUnitModuleInter_IronSource : AdUnitModule
    {
         #if UNITY_EDITOR
           AdUnitModule _fakeAD;
        #endif
        float interstitialRetryAttempt = 0;
        
        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
             #if UNITY_EDITOR
             _fakeAD = this.gameObject.AddComponent<AdUnitModuleInter_Base>().Init(ModuleSDK,aD_TYPE,unitIDs);
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
            //Add AdInfo Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent; 
        }
         

        private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} loaded <==");
            _moduleSDK.EventCallback(adFomart,AdUnitEvent.OnLoaded,adInfo.adUnit,adInfo,_placement);
              
            interstitialRetryAttempt = 0;
        }

        private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError) 
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to load with error code: " + ironSourceError.getErrorCode() + " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adFomart.ToString(), ironSourceError.getDescription(),_placement);

            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
            Invoke("Load", (float)retryDelay);
        }

        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} show! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adInfo.adUnit, adInfo,_placement);
            _isUnitShowed = true;

            this.InvokeCallback(0, AdUnitState.Open);
            
        }

        private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo) 
        {
            Debug.LogError(CONSTANT.Prefix + $"==> {adFomart} failed to display with error code: " + ironSourceError.getCode() + " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adInfo.adUnit, ironSourceError.getCode(),_placement);
            _isUnitShowed = false;

            this.InvokeCallback(0, AdUnitState.Interupt);
            
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
            StartCoroutine(waitLoad(0,(float)retryDelay));
        }
        

        private void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adInfo.adUnit, adInfo,_placement);

            this.InvokeCallback(0, AdUnitState.Click);
            
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
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

        private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo) 
        {
            Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> {adFomart} dismissed <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adInfo.adUnit, adInfo,_placement);
            _isUnitShowed = false;

            this.InvokeCallback(0, AdUnitState.Closed);
            this.InvokeCallback(0, AdUnitState.Watched);
             
            Load(0);
        }


        #endregion


        #region Load/Show
        public override void Load(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;

            Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {this._unitIDs[ID]} <==");
            
            IronSource.Agent.loadInterstitial();
            
            #if UNITY_EDITOR
                _fakeAD.Load(ID);
            #endif
        }

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
           
            base.Show(ID, callback,placement);
           
            #if UNITY_EDITOR
                _fakeAD.Show(ID, callback,placement);
            #else
                _isUnitShowed = this.IsLoaded(ID);
            #endif
            ((Init_IronSource)this._moduleSDK)._placement = placement;

            IronSource.Agent.showInterstitial();
           
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
            return IronSource.Agent.isInterstitialReady();
        }

        #endregion
    }

}
#endif