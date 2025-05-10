using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
     public class AdUnitModuleBanner_Base : AdUnitModule
     {
          GameObject               unitPrefab;
          Dictionary<int, bool>    isLoadeds               = new Dictionary<int, bool>();
          Dictionary<string, bool> _isBannerCurrentlyShows = new Dictionary<string, bool>();
          int                      ID                      = 0;

          const float bannerHeigh = 0.12f;

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);
               Initialize();
               foreach (string s in unitIDs)
               {
                    int ID = _unitIDs.IndexOf(s);
                    isLoadeds.Add(ID, false);
                    _isBannerCurrentlyShows.Add(s, ModuleSDK.DVAH_Data.BannerDefaultShow);
                    Load(ID);
               }

               return this;
          }

          public void Initialize()
          {
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} Init! <==");
               if (!unitPrefab)
               {
                    GameObject tmpPrefab = Resources.Load<GameObject>("AD/Banner");
                    unitPrefab = Instantiate(tmpPrefab, transform);
                    RectTransform reactTrans = unitPrefab.transform.GetChild(0).GetComponent<RectTransform>();


                    switch (_moduleSDK.DVAH_Data.BannerPosition)
                    {
                         case BannerPosition.BottomCenter:
                              reactTrans.anchorMax = new Vector2(1 - bannerHeigh, bannerHeigh);
                              reactTrans.anchorMin = new Vector2(bannerHeigh, 0);
                              break;
                         case BannerPosition.BottomLeft:
                              reactTrans.anchorMax = new Vector2(0.5f, bannerHeigh);
                              reactTrans.anchorMin = Vector2.zero;
                              break;
                         case BannerPosition.BottomRight:
                              reactTrans.anchorMax = new Vector2(1, bannerHeigh);
                              reactTrans.anchorMin = new Vector2(0.5f, 0);
                              break;


                         case BannerPosition.TopCenter:
                              reactTrans.anchorMax = new Vector2(1              - bannerHeigh, 1);
                              reactTrans.anchorMin = new Vector2(bannerHeigh, 1 - bannerHeigh);
                              break;
                         case BannerPosition.TopLeft:
                              reactTrans.anchorMax = new Vector2(0.5f, 1f);
                              reactTrans.anchorMin = new Vector2(0, 1 - bannerHeigh);
                              break;
                         case BannerPosition.TopRight:
                              reactTrans.anchorMax = Vector2.one;
                              reactTrans.anchorMin = new Vector2(0.5f, 1 - bannerHeigh);
                              break;

                         case BannerPosition.Centered:
                              reactTrans.anchorMax = new Vector2(1              - bannerHeigh, 1);
                              reactTrans.anchorMin = new Vector2(bannerHeigh, 1 - bannerHeigh);
                              break;
                         case BannerPosition.CenterLeft:
                              reactTrans.anchorMax = new Vector2(0.5f, 1f);
                              reactTrans.anchorMin = new Vector2(0, 1 - bannerHeigh);
                              break;
                         case BannerPosition.CenterRight:
                              reactTrans.anchorMax = Vector2.one;
                              reactTrans.anchorMin = new Vector2(0.5f, 1 - bannerHeigh);
                              break;
                    }

                    //reactTrans.transform.localScale = Vector3.one;
                    //reactTrans.transform.localPosition = Vector3.zero;
                    //reactTrans.sizeDelta = Vector2.zero;
               }
          }

          public override void Load(int ID = 0)
          {
               unitPrefab.SetActive(false);
               StartCoroutine(fakeLoadDone(ID));
          }

          IEnumerator fakeLoadDone(int ID)
          {
               yield return new WaitForSeconds(1);
               if (UnityEngine.Random.Range(0, 2) > 0)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> {adFomart} loaded! <==");
                    isLoadeds[ID] = true;
                    if (_isBannerCurrentlyShows[_unitIDs[ID]])
                         Show(ID);

                    yield break;
               }

               isLoadeds[ID] = false;
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} load failed! <==");


               StartCoroutine(fakeLoadDone(ID));
          }


          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
               if (_moduleSDK.DVAH_Data.NO_ADS)
               {
                    return;
               }

               _unitCallback = callback;
               if (UnityEngine.Random.Range(0, 10) == 0)
               {
                    Debug.LogError(
                         $"{CONSTANT.Prefix}==> {adFomart} show failed! This is fake sitiuation for testing purpose,"
                       + $"dont worry, your code is OK <==");


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

               this.ID = ID;
               unitPrefab.SetActive(true);
               unitPrefab.GetComponentInChildren<Text>().text =
                    $"Mirai SDK {adFomart} base \n"
                  + $" ID: {_unitIDs[ID]} \n"
                  + $" Ad network: {_moduleSDK.DVAH_Data.getAdNetOfID(adFomart, ID)}";

               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} show! <==");


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
               unitPrefab.gameObject.SetActive(false);
          }

          public override bool IsLoaded(int ID = 0)
          {
               return isLoadeds.ContainsKey(ID) && isLoadeds[ID];
          }
     }
}