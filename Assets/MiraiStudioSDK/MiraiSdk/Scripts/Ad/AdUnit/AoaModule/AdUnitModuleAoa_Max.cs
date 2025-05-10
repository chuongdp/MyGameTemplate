#if MAX_IMPLEMENT
     using System;
     using System.Collections;
     using System.Collections.Generic;
     using UnityEngine;
     using static MaxSdkBase;

     namespace DVAH
     {
          public class AdUnitModuleAoa_Max : AdUnitModule
          {
               Dictionary<string, float> _AoaRetryAttemps = new Dictionary<string, float>();

               public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
               {
                    base.Init(ModuleSDK, aD_TYPE, unitIDs);
                    Initialize();
                    getIDs(unitIDs);
                    return this;
               }

               public void Initialize()
               {
                    Debug.Log($"==> {CONSTANT.Prefix} Ad {adFomart} init! <==");
                    MaxSdkCallbacks.AppOpen.OnAdLoadedEvent        += AppOpen_OnAdLoadedEvent;
                    MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent    += AppOpenOnAdLoadFailedEvent;
                    MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += AppOpen_OnAdDisplayFailedEvent;
                    MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent     += AppOpen_OnAdDisplayedEvent;
                    MaxSdkCallbacks.AppOpen.OnAdClickedEvent       += AppOpen_OnAdClickedEvent;
                    MaxSdkCallbacks.AppOpen.OnAdHiddenEvent        += OnAppOpenDismissedEvent;
                    MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent   += AppOpen_OnAdRevenuePaidEvent;
               }

               void getIDs(List<string> unitIDs)
               {
                    foreach (string s in unitIDs)
                    {
                         int ID = _unitIDs.IndexOf(s);
                         _AoaRetryAttemps.Add(s, 0);
                         Load(ID);
                    }
               }

          #region Aoa Method

               void AppOpen_OnAdLoadedEvent(string adUnitId, AdInfo adInfo)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart} success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, adInfo, _placement);

                    _AoaRetryAttemps[adUnitId] = 0;
               }

               void AppOpenOnAdLoadFailedEvent(string adUnitId, ErrorInfo errorInfo)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + errorInfo.Code + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.Message, _placement);

                    _AoaRetryAttemps[adUnitId]++;
                    double retryDelay = Math.Pow(2, Math.Min(6, _AoaRetryAttemps[adUnitId]));
                    int    ID         = _unitIDs.IndexOf(adUnitId);

                    UnityMainThread.wkr.AddJob(() => { StartCoroutine(waitLoad(ID, (float)retryDelay)); });
               }

               IEnumerator waitLoad(int ID, float delay)
               {
                    yield return new WaitForSeconds(delay);
                    Load(ID);
               }

               void AppOpen_OnAdDisplayedEvent(string adUnitId, AdInfo adInfo)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Show ad {adFomart}  success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, adInfo, _placement);
                    _isUnitShowed = true;

                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Open);
               }

               void AppOpen_OnAdClickedEvent(string adUnitId, AdInfo adInfo)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, adInfo, _placement);

                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);
               }

               void AppOpen_OnAdDisplayFailedEvent(string adUnitId, ErrorInfo errorInfo, AdInfo adInfo)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==>show ad {adFomart} failed, code: " + errorInfo.Code + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitId, errorInfo.Message, _placement);

                    int ID = _unitIDs.IndexOf(adUnitId);
                    InvokeCallback(ID, AdUnitState.Interupt);
                    Load(ID);
               }


               public void OnAppOpenDismissedEvent(string adUnitId, AdInfo adInfo)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} close! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, adInfo, _placement);
                    _isUnitShowed = false;

                    int ID = _unitIDs.IndexOf(adUnitId);
                    InvokeCallback(ID, AdUnitState.Closed);

                    Load(ID);
               }

               void AppOpen_OnAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} paid! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adInfo, _placement);
                    TrackingDefault.CheckMaxRev(adInfo);
               }

          #endregion

          #region Show/Load Ad

               public override bool IsLoaded(int ID = 0)
               {
                    if (!CheckID(ID))
                         return false;
                    return MaxSdk.IsAppOpenAdReady(_unitIDs[ID]);
               }

               public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
               {
                    base.Show(ID, callback);
                    MaxSdk.ShowAppOpenAd(_unitIDs[ID],_placement);
               }

               public override void Load(int ID = 0)
               {
                    if (!CheckID(ID))
                         return;
                    Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {_unitIDs[ID]} <==");
                    MaxSdk.LoadAppOpenAd(_unitIDs[ID]);
               }

               public override void Hide(int ID = 0)
               {
                    throw new NotImplementedException();
               }

          #endregion
          }
     }
#endif