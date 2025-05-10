#if PANGLE_IMPLEMENT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PAG.Scripts.Api;
using UnityEngine;
using UnityEngine.UI; 

namespace DVAH {
    public class AdUnitModuleAoa_Pangle : AdUnitModule
    { 
        #if UNITY_EDITOR
        AdUnitModule _fakeAD;
        #endif
        Dictionary<int,PAGAppOpenAd> _pangleAdUnits = new Dictionary<int, PAGAppOpenAd>();
        Dictionary<int,float> _AoaRetryAttemps = new Dictionary<int, float>();
          
        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIds)
        {
            base.Init(ModuleSDK, aD_TYPE, unitIds);
            #if UNITY_EDITOR
             _fakeAD = this.gameObject.AddComponent<AdUnitModuleAoa_Base>().Init(ModuleSDK,aD_TYPE,unitIds);
            #endif
            
            for(int i = 0; i< this._unitIDs.Count; i++){
                if(!unitIds.Contains(this._unitIDs[i]))
                    continue;
                _AoaRetryAttemps.Add(i,0);
                _pangleAdUnits.Add(i,null);
                Load(i);
               
            }

            return this;
        }

        IEnumerator waitLoad(int ID, float delay)
        {
            yield return new WaitForSeconds(delay);
            Load(ID);
        }

        void Init(int ID = 0){
            string adUnitId = this._unitIDs[ID];
            PAGAppOpenAd _appOpenAd = new PAGAppOpenAd(adUnitId);
            _appOpenAd.OnLoad += (() =>
            {
                Debug.Log("[PAGAppOpen]-[Load]:successfully");
                Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart} - ID {this._unitIDs[ID]} success! <==");
                _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, _appOpenAd,_placement); 
                _AoaRetryAttemps[ID] = 0;
            });

            _appOpenAd.OnLoadFailed += ((code, msg) =>
            {
                 
                Debug.Log($"[PAGAppOpen]-[Load]:failed， code ={code}, error message = {msg}");
                 Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " +code + " <==");
                _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, msg,_placement);

                _AoaRetryAttemps[ID]++;
                double retryDelay = Math.Pow(2, Math.Min(6, _AoaRetryAttemps[ID]));
                UnityMainThread.wkr.AddJob(() =>
                {
                    StartCoroutine(waitLoad(ID, (float)retryDelay));
                }); 
            });

            _appOpenAd.OnAdShowed += (() => { 
                Debug.Log("[PAGAppOpen]-[interaction]:show"); 
                Debug.Log($"{CONSTANT.Prefix}==>Show ad {adFomart}  success! <==");
                _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, _appOpenAd,_placement);
                _isUnitShowed = true;

                 this.InvokeCallback(this._unitIDs.IndexOf(adUnitId), AdUnitState.Open);
            });

            _appOpenAd.OnAdClicked += (() => { 
                Debug.Log("[PAGAppOpen]-[interaction]:click"); 
                 Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
                _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, _appOpenAd,_placement);

                this.InvokeCallback(this._unitIDs.IndexOf(adUnitId), AdUnitState.Click);
            });

            _appOpenAd.OnAdDismissed += (() => { 
                Debug.Log("[PAGAppOpen]-[interaction]:dismiss"); 
                Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} close! <==");
                _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, _appOpenAd,_placement);
                _isUnitShowed = false;
 
                this.InvokeCallback(ID, AdUnitState.Closed);
                
                Load(ID);
            });

           
            _pangleAdUnits[ID] = _appOpenAd;
             
        }
        public override void Load(int ID = 0)
        { 
            #if UNITY_EDITOR
             _fakeAD.Load(ID);
            #endif
            Init(ID);
            var request = new PAGAppOpenRequest
            {
                //set timeout
                Timeout = 5000
            };
            //request an ad
             _pangleAdUnits[ID]?.Load(request);
        }
 
        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null,string placement = null)
        { 
            if (!this.CheckID(ID))
                return;
            #if UNITY_EDITOR
                _fakeAD.Show(ID, callback, placement);
            #else
                 _isUnitShowed = this.IsLoaded(ID);
            #endif
            base.Show(ID, callback, placement);
            _pangleAdUnits[ID].Show();
        }

        public override void Hide(int ID = 0)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsLoaded(int ID = 0)
        {
             #if UNITY_EDITOR
                return _fakeAD.IsLoaded(ID);
            #endif
            return _pangleAdUnits[ID] != null && _pangleAdUnits[ID].IsLoaded();
        }
    }

}
#endif