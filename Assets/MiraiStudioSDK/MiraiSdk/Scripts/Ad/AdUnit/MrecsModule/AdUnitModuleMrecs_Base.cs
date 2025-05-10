using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
     public class AdUnitModuleMrecs_Base : AdUnitModule
     {
          GameObject                                unitPrefab;
          [SerializeField] Dictionary<string, bool> isLoadeds               = new Dictionary<string, bool>();
          Dictionary<string, bool>                  isFullSizes             = new Dictionary<string, bool>();
          Dictionary<string, bool>                  _isBannerCurrentlyShows = new Dictionary<string, bool>();
          int                                       ID                      = 0;

          const float bannerHeigh = 0.3f, bannerMin = 0.12f;

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);
               Initialize();
               foreach (string s in unitIDs)
               {
                    int ID = _unitIDs.IndexOf(s);
                    _isBannerCurrentlyShows.Add(s, ModuleSDK.DVAH_Data.MrecsDefaultShow);
                    Load(ID);
               }

               return this;
          }

          public void Initialize()
          {
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} Init! <==");
               if (!unitPrefab)
               {
                    GameObject tmpPrefab = Resources.Load<GameObject>("AD/Mrecs");
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

                    Button btncollapse = unitPrefab.GetComponentInChildren<Button>();
                    btncollapse.onClick.AddListener(() => { fakeCollapse(btncollapse, reactTrans); });
               }
          }

          public override void Load(int ID = 0)
          {
               Debug.LogError(name + "---" + ID);
               unitPrefab.SetActive(false);
               isLoadeds.Add(_unitIDs[ID], false);
               isFullSizes.Add(_unitIDs[ID], false);
               StartCoroutine(fakeLoadDone(ID));
          }

          IEnumerator fakeLoadDone(int ID)
          {
               yield return new WaitForSeconds(1);
               if (UnityEngine.Random.Range(0, 2) > 0)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> {adFomart} loaded! <==");
                    isLoadeds[_unitIDs[ID]] = true;
                    if (_isBannerCurrentlyShows[_unitIDs[ID]])
                         Show(ID);
                    yield break;
               }

               isLoadeds[_unitIDs[ID]] = false;
               Debug.Log($"{CONSTANT.Prefix}==> {adFomart} load failed! <==");


               StartCoroutine(fakeLoadDone(ID));
          }

          void fakeCollapse(Button btn, RectTransform reactTrans)
          {
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, _unitIDs[ID]);

               InvokeCallback(ID, AdUnitState.Click);

               float size = isFullSizes[_unitIDs[ID]] ? bannerMin : bannerHeigh;
               isFullSizes[_unitIDs[ID]]               = !isFullSizes[_unitIDs[ID]];
               btn.GetComponentInChildren<Text>().text = isFullSizes[_unitIDs[ID]] ? "Collapse" : "Full";

               switch (_moduleSDK.DVAH_Data.BannerPosition)
               {
                    case BannerPosition.BottomCenter:
                         reactTrans.anchorMax = new Vector2(1 - size, size);
                         reactTrans.anchorMin = new Vector2(size, 0);
                         break;
                    case BannerPosition.BottomLeft:
                         reactTrans.anchorMax = new Vector2(0.5f, size);
                         reactTrans.anchorMin = Vector2.zero;
                         break;
                    case BannerPosition.BottomRight:
                         reactTrans.anchorMax = new Vector2(1, size);
                         reactTrans.anchorMin = new Vector2(0.5f, 0);
                         break;


                    case BannerPosition.TopCenter:
                         reactTrans.anchorMax = new Vector2(1       - size, 1);
                         reactTrans.anchorMin = new Vector2(size, 1 - size);
                         break;
                    case BannerPosition.TopLeft:
                         reactTrans.anchorMax = new Vector2(0.5f, 1f);
                         reactTrans.anchorMin = new Vector2(0, 1 - size);
                         break;
                    case BannerPosition.TopRight:
                         reactTrans.anchorMax = Vector2.one;
                         reactTrans.anchorMin = new Vector2(0.5f, 1 - size);
                         break;

                    case BannerPosition.Centered:
                         reactTrans.anchorMax = new Vector2(1       - size, 1);
                         reactTrans.anchorMin = new Vector2(size, 1 - size);
                         break;
                    case BannerPosition.CenterLeft:
                         reactTrans.anchorMax = new Vector2(0.5f, 1f);
                         reactTrans.anchorMin = new Vector2(0, 1 - size);
                         break;
                    case BannerPosition.CenterRight:
                         reactTrans.anchorMax = Vector2.one;
                         reactTrans.anchorMin = new Vector2(0.5f, 1 - size);
                         break;
               }
          }

          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
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
                    unitPrefab.GetComponentInChildren<Button>().onClick.Invoke();
                    callback?.Invoke(ID, AdUnitState.Open);
               }
               catch (Exception e)
               {
                    Debug.LogError($"call back on {adFomart} fail: Error: {e} ");
               }
          }

          public override void Hide(int ID = 0)
          {
               unitPrefab.SetActive(false);
          }

          public override bool IsLoaded(int ID = 0)
          {
               return isLoadeds.ContainsKey(_unitIDs[ID]) && isLoadeds[_unitIDs[ID]];
          }
     }
}