using DVAH; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DVAH
{
    [Serializable]
    public class DictUnitModule: SerializableDictionary<AD_TYPE, AdUnitModule> { }
     
    public abstract class Init_ : MonoBehaviour
    {
        [SerializeField]
        protected DictUnitModule _adUnitModules = new DictUnitModule();
        public DictUnitModule AdUnitModule => _adUnitModules;

        [SerializeField]
        DictUnitCallback _listCallbacks = new DictUnitCallback();
        

        public AdBridge AdManager => AdBridge.Instant;
        private DVAH_Data _DVAH_Data;
        public DVAH_Data DVAH_Data => _DVAH_Data;

        protected AD_NETWORK _adNetWork;
        public AD_NETWORK adNetWork => _adNetWork;

        [SerializeField]
        protected bool _isInitDone = false;
        int _isCallingShow = 0;
         
        public abstract bool isInitDone();
 
        public virtual Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
        { 
            _isInitDone = false; 
            this._DVAH_Data = dvah_data;
            _listCallbacks = callbacks; 
             Debug.Log(CONSTANT.Prefix +$"AdnetWork {this._adNetWork} init!");
            return this;
        }
        
        protected virtual void onSDKInitDone()
        {
             Debug.Log(CONSTANT.Prefix +$"AdnetWork {this._adNetWork} init done!");
            foreach (AD_TYPE format in Enum.GetValues(typeof(AD_TYPE)))
            {
                checkUnitInit(format);
            } 
             _isInitDone = true;
        }

        protected virtual void checkUnitInit(AD_TYPE AD_TYPE)
        {
            List<string> IdUnits = this.DVAH_Data.getAdUnitIds(AD_TYPE, _adNetWork);
            if (IdUnits.Count == 0)
            {
                
                return;
            }
            
            string classN = string.Format(CONSTANT.AdUnitClassName, AD_TYPE, adNetWork.ToString());
            Debug.Log($"{CONSTANT.Prefix} ==> Create ad unit "+classN);
            Type unitClass = null;
            try
            {
                unitClass = Type.GetType(classN, true, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            if (unitClass == null)
            {
                Debug.LogError($"Unit {AD_TYPE} not implement yet! Consider create class {string.Format(CONSTANT.AdUnitClassName, AD_TYPE, adNetWork.ToString())}");
                return;
            }

            UnityMainThread.wkr.AddJob(()=>{
                GameObject g = new GameObject($"{AD_TYPE}Module");
                g.transform.SetParent(this.transform);
                g.AddComponent(unitClass);
                _adUnitModules.Add(AD_TYPE, g.GetComponent<AdUnitModule>().Init(this, AD_TYPE, IdUnits));  
                Debug.Log($"{CONSTANT.Prefix} ==> Add ad unit {classN} of object {g.name}");
            });
           
        }

        public abstract void ShowAdDebugger();
        

        public virtual void ShowAsync(AD_TYPE adFomart, Action<int, AdUnitState> callback = null, int ID = 0, string placement = null)
        {
            System.Threading.Interlocked.Exchange(ref _isCallingShow, 1);
            _= Show(adFomart,callback, ID,placement);
        }

        public async Task<Init_> Show(AD_TYPE adFomart, Action<int, AdUnitState> callback = null, int ID = 0,string placement = null)
        {
            while (!_isInitDone)
            {
                if(System.Threading.Interlocked.Add(ref _isCallingShow, 0) == 1){
                    Debug.Log($"{CONSTANT.Prefix}==> this module not showed ad yet cause it still init! Now Hide was invoke show it must stop wait now");
                    return this;
                }
                await Task.Delay(100);
            }

            if (!_adUnitModules.Contains(adFomart))
            {
                Debug.LogError($"{CONSTANT.Prefix}==> this module is {this.ToString()} which not contain adformat {adFomart}!!!" +
                    $"Please check Getmodule() again");
                UnityMainThread.wkr.AddJob(() => callback?.Invoke(ID, AdUnitState.None));
                return this;
            }

            _adUnitModules[adFomart].Show(ID, callback,placement);
            this.EventCallback(adFomart, AdUnitEvent.OnPreOpen, placement);
            return this;
        }

        public virtual Init_ Hide(AD_TYPE adFomart, int ID)
        {
            System.Threading.Interlocked.Exchange(ref _isCallingShow, 0);
            if (!_adUnitModules.Contains(adFomart))
            {
                Debug.LogError($"{CONSTANT.Prefix}==> this module is {this.ToString()} which not contain adformat {adFomart}!!!" +
                    $"Please check Getmodule() again");
                return this;
            }
            _adUnitModules[adFomart].Hide(ID);
            return this;
        }

        public virtual Init_ Load(AD_TYPE adFomart, int ID)
        {
            if (!_adUnitModules.Contains(adFomart))
            {
                Debug.LogError($"{CONSTANT.Prefix}==> this module is {this.ToString()} which not contain adformat {adFomart}!!!" +
                    $"Please check Getmodule() again");
                return this;
            }
            _adUnitModules[adFomart].Load(ID);
            return this;
        }

        public virtual bool IsAdLoaded(AD_TYPE adFomart,int ID)
        {
            if (!_adUnitModules.ContainsKey(adFomart))
            {
                Debug.LogError($"adformat {adFomart} not exist, count: {_adUnitModules.Count}, it may casuse MiraiSDK still initing!, dont worry! ");
                return false;
            }
            return _adUnitModules[adFomart].IsLoaded(ID);
        }

        public Init_ EventCallback(AD_TYPE adFomart, AdUnitEvent Event, params object[] datas)
        {
            try
            {
                Debug.Log(CONSTANT.Prefix +$"==> try EventCallback of {adFomart} - event {Event} "); 
                UnityMainThread.wkr.AddJob(() =>
                {
                    _listCallbacks[adFomart].Event[Event]?.Invoke(adNetWork, datas);
                });
                
            }catch (Exception e)
            {
                Debug.LogError(CONSTANT.Prefix +$"==> Faild on invoke EventCallback of {adFomart} - event {Event} error: {e}"); 
            }
            return this;
        }

        private void OnDestroy() {
            foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE)))
            {
                foreach (AdUnitEvent Event in Enum.GetValues(typeof(AdUnitEvent)))
                {
                    _listCallbacks[adType].Event[Event] = null;
                }
            }
           
        }
    }

    
}
