using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
     public class AdUnitModuleAoa_Base : AdUnitModule
     {
          GameObject            unitPrefab;
          Dictionary<int, bool> isLoadeds = new Dictionary<int, bool>();
          int                   openedID  = 0;

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIds)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIds);
               Initialize(unitIds);
               return this;
          }

          public void Initialize(List<string> unitIds)
          {
               if (!unitPrefab)
               {
                    GameObject tmpPrefab = Resources.Load<GameObject>("AD/Aoa");
                    unitPrefab = Instantiate(tmpPrefab, transform);
                    unitPrefab.GetComponentInChildren<Button>().onClick.AddListener(fakeCloseAd);
               }


               foreach (string id in unitIds)
               {
                    int ID = _unitIDs.IndexOf(id);
                    isLoadeds.Add(ID, false);
                    Load(ID);
               }
          }

          public override void Load(int ID = 0)
          {
               unitPrefab.SetActive(false);
               isLoadeds[ID] = false;
               if (AdBridge.IsNoAds)
                    return;
               StartCoroutine(fakeLoadDone(ID));
          }

          IEnumerator fakeLoadDone(int ID)
          {
               yield return new WaitForSeconds(1);
               if (UnityEngine.Random.Range(0, 2) > 0)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> {adFomart} loaded! <==");

                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, _unitIDs[ID]);
                    isLoadeds[ID] = true;


                    yield break;
               }

               isLoadeds[ID] = false;
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} load failed! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, _unitIDs[ID], "fake load fail, dont worry!");

               StartCoroutine(fakeLoadDone(ID));
          }

          void fakeCloseAd()
          {
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} close! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, _unitIDs[openedID]);
               _isUnitShowed = false;

               InvokeCallback(openedID, AdUnitState.Closed);

               Load();
          }

          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
               if (AdBridge.IsNoAds)
               {
                    callback?.Invoke(ID, AdUnitState.Closed);
                    return;
               }

               _unitCallback = callback;
               if (UnityEngine.Random.Range(0, 10) == 0)
               {
                    Debug.LogError(
                         $"{CONSTANT.Prefix}==> {adFomart} show failed! This is fake sitiuation for testing purpose,"
                       + $"dont worry, your code is OK <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, _unitIDs[ID], "fake show fail!");

                    try
                    {
                         callback?.Invoke(ID, AdUnitState.Interupt);
                    }
                    catch (Exception e)
                    {
                         Debug.LogError($"call back on {adFomart} fail: Error: ");
                         Debug.LogException(e);
                    }

                    return;
               }

               openedID = ID;
               unitPrefab.SetActive(true);
               unitPrefab.GetComponentInChildren<Text>().text =
                    $"Mirai SDK {adFomart} base \n"
                  + $" ID: {_unitIDs[ID]} \n"
                  + $" Ad network: {_moduleSDK.DVAH_Data.getAdNetOfID(adFomart, ID)}";

               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} show! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, _unitIDs[ID]);
               _isUnitShowed = true;
               try
               {
                    callback?.Invoke(ID, AdUnitState.Open);
               }
               catch (Exception e)
               {
                    Debug.LogError($"call back on {adFomart} fail: Error: {e} ");
               }
          }

          public override void Hide(int ID = 0)
          {
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, _unitIDs[ID]);
          }

          public override bool IsLoaded(int ID = 0)
          {
               return isLoadeds[ID];
          }
     }
}