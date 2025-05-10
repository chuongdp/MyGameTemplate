#if PANGLE_IMPLEMENT
using System;
using System.Collections;
using System.Collections.Generic;
using PAG.Scripts.Api;
using UnityEngine;

namespace DVAH
{
    public class AdUnitModuleNative_Pangle : AdUnitModule
    {
        Dictionary<int,PAGNativeAd> _nativeAds = new Dictionary<int, PAGNativeAd>();
        private Dictionary<int,float> _unitRetryAttemps = new Dictionary<int, float>();

        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK, aD_TYPE, unitIDs);
             
            for (int i = 0; i < this._unitIDs.Count; i++)
            {
                if(!unitIDs.Contains(this._unitIDs[i]))
                    continue;
                _unitRetryAttemps.Add(i,0);
                Init(i);
            }
            return this;
        }

        void Init(int ID){

            var adUnitId = this._unitIDs[ID];
            var _nativeAd = new PAGNativeAd(adUnitId);

            _nativeAd.OnLoad += (() => { 
                Debug.Log("[PAGNative]-[Load]:success"); 
                 
                // Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart}  loaded ");
                // _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId,null,_placement);
                
                // _unitRetryAttemps[ID] = 0;
            });

            _nativeAd.OnLoadFailed += ((code, msg) =>
            {
                Debug.Log($"[PAGNative]-[Load]:failed, code ={code}, error message = {msg}");
                // Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + code + " <==");
                // _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, msg,_placement);
    
                // _unitRetryAttemps[ID]++;
                // double retryDelay = Math.Pow(2, Math.Min(6, _unitRetryAttemps[ID]));
                // UnityMainThread.wkr.AddJob(() =>
                //     {
                //         StartCoroutine( waitLoadAd((float)retryDelay, ID));
                //     });
            });
            _nativeAd.OnAdShowed += (() => { 
                Debug.Log("[PAGNative]-[interaction]:show"); 
                //  Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart} ID: {adUnitId}  showed! ");
                // _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, null, _placement); 
                // this.InvokeCallback(ID, AdUnitState.Open);
            });

            _nativeAd.OnAdClicked += (() => { 
                Debug.Log("[PAGNative]-[interaction]:click"); 
                // Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} ID:{adUnitId} success! <==");
                // _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId,null,_placement);

                // this.InvokeCallback(ID, AdUnitState.Click);
            });

            _nativeAd.OnAdDismissed += (() => { 
                Debug.Log("[PAGNative]-[interaction]:dismiss"); 
                // Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> Interstitial dismissed <==");
                // _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, _nativeAd,_placement); 
            
                // this.InvokeCallback(ID, AdUnitState.Closed);
            });

            _nativeAds.Add(ID,_nativeAd);

            Load (ID);
        }

        IEnumerator waitLoadAd(float delay, int ID)
        {
            yield return new WaitForSeconds(delay);
            Load(ID);
        }

        #region LOAD/SHOW

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            base.Show(ID, callback, placement);
            _nativeAds[ID].SetPosition(0,0);
            _nativeAds[ID]?.Show();
        }
        public override void Hide(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;
            _nativeAds[ID]?.DestroyAd();
            Load(ID);
        }

        public override bool IsLoaded(int ID = 0)
        {
            return _nativeAds[ID] != null && _nativeAds[ID].IsLoaded();
        }

        public override void Load(int ID = 0)
        {
            if (!this.CheckID(ID))
                return;
            var request = new PAGNativeRequest();
            _nativeAds[ID]?.Load(request);
        }
        #endregion
    }
}
#endif