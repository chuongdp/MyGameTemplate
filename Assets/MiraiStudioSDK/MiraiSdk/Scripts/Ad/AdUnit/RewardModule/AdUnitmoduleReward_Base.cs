using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
     public class AdUnitmoduleReward_Base : AdUnitModule
     {
          GameObject                unitPrefab;
          Dictionary<int, bool>     isLoadeds = new Dictionary<int, bool>();
          int                       ID        = 0;
          [SerializeField] Button[] buttons;

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);
               Initialize();
               foreach (string s in unitIDs)
               {
                    int ID = _unitIDs.IndexOf(s);
                    isLoadeds.Add(ID, false);
                    Load(ID);
               }

               return this;
          }

          public void Initialize()
          {
               if (!unitPrefab)
               {
                    GameObject tmpPrefab = Resources.Load<GameObject>("AD/Rewarded");
                    unitPrefab = Instantiate(tmpPrefab, transform);
                    ;
                    buttons = unitPrefab.GetComponentsInChildren<Button>();

                    buttons[0].onClick.AddListener(fakeCloseAd);
                    buttons[1].onClick.AddListener(fakeRewarded);
               }
          }

          void fakeRewarded()
          {
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} reward! <==");

               InvokeCallback(ID, AdUnitState.Watched);
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnRewarded);
          }

          public override void Load(int ID = 0)
          {
               unitPrefab.SetActive(false);

               StartCoroutine(fakeLoadDone(ID));
          }

          IEnumerator fakeLoadDone(int ID)
          {
               yield return new WaitForSeconds(1f);
               if (UnityEngine.Random.Range(0, 2) > 0)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> {adFomart} - {_unitIDs[ID]} loaded! <==");
                    isLoadeds[ID] = true;


                    yield break;
               }

               isLoadeds[ID] = false;
               Debug.Log($"{CONSTANT.Prefix}==>  {adFomart} - {_unitIDs[ID]}  load failed! <==");


               StartCoroutine(fakeLoadDone(ID));
          }

          void fakeCloseAd()
          {
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} close! <==");
               _isUnitShowed = false;
               InvokeCallback(ID, AdUnitState.Closed);

               Load(ID);
          }

          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
               _unitCallback = callback;
               if (UnityEngine.Random.Range(0, 10) == 0)
               {
                    Debug.LogError(
                         $"{CONSTANT.Prefix}==> {adFomart} - {_unitIDs[this.ID]} show failed! This is fake sitiuation for testing purpose,"
                       + $"dont worry, your code is OK <==");


                    try
                    {
                         callback?.Invoke(ID, AdUnitState.Interupt);
                    }
                    catch (Exception e)
                    {
                         Debug.LogError($"call back on {adFomart} - {_unitIDs[this.ID]} fail: Error: ");
                         Debug.LogException(e);
                    }

                    return;
               }

               this.ID = ID;
               unitPrefab.SetActive(true);
               unitPrefab.GetComponentInChildren<Text>().text =
                    $"Mirai SDK {adFomart} base \n"
                  + $" ID: {_unitIDs[ID]} \n"
                  + $" Ad network: {_moduleSDK.DVAH_Data.getAdNetOfID(adFomart, ID)}";

               _isUnitShowed = true;
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} - {_unitIDs[this.ID]} show! <==");


               try
               {
                    callback?.Invoke(ID, AdUnitState.Open);
               }
               catch (Exception e)
               {
                    Debug.LogError($"call back on {adFomart} - {_unitIDs[this.ID]} fail: Error: {e} ");
               }
          }

          public override void Hide(int ID = 0)
          {
               throw new NotImplementedException();
          }

          public override bool IsLoaded(int ID = 0)
          {
               return isLoadeds.ContainsKey(ID) && isLoadeds[ID];
          }
     }
}