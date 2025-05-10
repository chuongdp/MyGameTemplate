using DVAH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AdUnitModule : MonoBehaviour
{
     [SerializeField] protected AD_TYPE _adFomart;
     public                     AD_TYPE adFomart => _adFomart;

     protected Init_        _moduleSDK;
     protected List<string> _unitIDs => _moduleSDK.DVAH_Data.getAdUnitIds(adFomart);

     [SerializeField] protected bool _isUnitShowed = false;
     public                     bool IsUnitShowed => _isUnitShowed;

     Coroutine _waitCheckInterrupt;

     public virtual AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
     {
          _adFomart  = aD_TYPE;
          _moduleSDK = ModuleSDK;
          Debug.Log(CONSTANT.Prefix + $"Module {_moduleSDK.adNetWork}->{_adFomart} start init!");
          return this;
     }

     protected Action<int, AdUnitState> _unitCallback;

     protected string _placement = "none";

     public abstract void Load(int ID = 0);

     public abstract bool IsLoaded(int ID = 0);

     public virtual void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
     {
          if (!CheckID(ID))
          {
               UnityMainThread.wkr.AddJob(() => callback?.Invoke(ID, AdUnitState.None));

               return;
          }

          Debug.Log($"{CONSTANT.Prefix} ==> {adFomart}_{_moduleSDK.adNetWork} call show! object {GetType()}");
          if (!IsLoaded(ID) && adFomart != AD_TYPE.Banner)
          {
               Debug.Log($"{CONSTANT.Prefix} ==> {adFomart} not loaded yet!");

               UnityMainThread.wkr.AddJob(() => callback?.Invoke(ID, AdUnitState.None));
               return;
          }

          Debug.Log($"{CONSTANT.Prefix} ==> {adFomart}-{ID} register Reload on INterrupt");
          callback += (id, state) =>
          {
               if (state == AdUnitState.Interupt)
                    Load(ID);
          };

          _unitCallback = callback;
          _placement    = placement;
     }

     public abstract void Hide(int ID = 0);

     protected bool CheckID(int ID)
     {
          if (ID >= _moduleSDK.DVAH_Data.adUnits[adFomart].AdUnitDatas.Count)
          {
               return false;
          }


          string UnitID = _moduleSDK.DVAH_Data.adUnits[adFomart].AdUnitDatas[ID].UnitId;
          ID = _unitIDs.IndexOf(UnitID);
          AD_NETWORK requireNet = _moduleSDK.DVAH_Data.adUnits[adFomart].AdUnitDatas[ID].network;

          if (_moduleSDK.adNetWork != AD_NETWORK.Base && requireNet != _moduleSDK.adNetWork)
          {
               Debug.LogError(CONSTANT.Prefix + $"==>wrong ad net {requireNet} - but current is {_moduleSDK.adNetWork}");
               return false;
          }

          return true;
     }

     protected void InvokeCallback(int id, AdUnitState adUnitState)
     {
          void InvokeCallbackOnMainThread()
          {
               try
               {
                    _unitCallback?.Invoke(id, adUnitState);
               }
               catch (Exception e)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==> Callback {adUnitState} - {adFomart} error: ");
                    Debug.LogException(e);
               }
          }

          UnityMainThread.wkr.AddJob(InvokeCallbackOnMainThread);
     }

     void OnApplicationFocus(bool focusStatus)
     {
          if (focusStatus && _isUnitShowed)
          {
               if (_waitCheckInterrupt != null)
               {
                    StopCoroutine(_waitCheckInterrupt);
               }

               _waitCheckInterrupt = StartCoroutine(waitCheckInterrupt());
          }
     }

     IEnumerator waitCheckInterrupt()
     {
          Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} - call from focus back to app! <==");
          yield return new WaitForSecondsRealtime(10);
          if (_isUnitShowed)
          {
               Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} - interrupt! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnInterrupt, _placement);
               InvokeCallback(0, AdUnitState.Interupt);
               _isUnitShowed = false;
          }
     }
}