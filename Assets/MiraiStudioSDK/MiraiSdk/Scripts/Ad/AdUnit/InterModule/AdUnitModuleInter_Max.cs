#if MAX_IMPLEMENT
#if AMAZON
using AmazonAds;
#endif
using DVAH;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MaxSdkBase;


namespace DVAH
{
     public class AdUnitModuleInter_Max : AdUnitModule

     {
          Dictionary<string, float> interstitialRetryAttempts = new Dictionary<string, float>();

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);
               Initialize();
               foreach (string s in unitIDs)
               {

                    interstitialRetryAttempts.Add(s, 0);
               }
#if AMAZON && !UNITY_EDITOR
               var interstitialAdRequest = new APSInterstitialAdRequest(ModuleSDK.DVAH_Data.AmazonInterID, new AdNetworkInfo(ApsAdNetwork.MAX));

                    interstitialAdRequest.onSuccess += (adResponse) =>
                    {
                         foreach (string s in unitIDs)
                         {
                              MaxSdk.SetInterstitialLocalExtraParameter(s, "amazon_ad_response", adResponse.GetResponse());
                              int ID = _unitIDs.IndexOf(s);
                              Load(ID);
                         }
                         
                    };
                    interstitialAdRequest.onFailedWithError += (adError) =>
                    {
                         foreach (string s in unitIDs)
                         {
                              MaxSdk.SetInterstitialLocalExtraParameter(s, "amazon_ad_error", adError.GetAdError());
                              int ID = _unitIDs.IndexOf(s);
                              Load(ID);
                         }
                          
                    };
                    interstitialAdRequest.LoadAd();
#else


               foreach (string s in unitIDs)
               {
                    int ID = _unitIDs.IndexOf(s);
                    Load(ID);
               }
#endif

               return this;
          }

          #region Interstitial Init Methods

          public void Initialize()
          {
               Debug.Log($"==> {CONSTANT.Prefix} Ad {adFomart} init! <==");
               MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
               MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
               MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
               MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += Interstitial_OnAdDisplayedEvent;
               MaxSdkCallbacks.Interstitial.OnAdClickedEvent += Interstitial_OnAdClickedEvent;
               MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
               MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += Interstitial_OnAdRevenuePaidEvent;

          }


          void OnInterstitialLoadedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log(CONSTANT.Prefix + $"==> Interstitial loaded <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, adInfo, _placement);
               interstitialRetryAttempts[adUnitId] = 0;
          }

          void OnInterstitialFailedEvent(string adUnitId, ErrorInfo errorInfo)
          {
               Debug.LogError(CONSTANT.Prefix
                            + $"==> Interstitial failed to load with error code: "
                            + errorInfo.Code
                            + " <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.Message, _placement);

               if (!_moduleSDK.DVAH_Data.KeepTryReload[adFomart])
                    return;

               interstitialRetryAttempts[adUnitId]++;
               double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempts[adUnitId]));
               StartCoroutine(waitLoad(_unitIDs.IndexOf(adUnitId), (float)retryDelay));
          }

          void Interstitial_OnAdDisplayedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log(CONSTANT.Prefix + $"==> Interstitial show! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, adInfo, _placement);
               _isUnitShowed = true;

               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Open);
          }

          void InterstitialFailedToDisplayEvent(string adUnitId, ErrorInfo errorInfo, AdInfo adInfo)
          {
               Debug.LogError(CONSTANT.Prefix
                            + $"==> Interstitial failed to display with error code: "
                            + errorInfo.Code
                            + " <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitId, errorInfo.Message, _placement);
               _isUnitShowed = false;

               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Interupt);

               double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempts[adUnitId]));
               StartCoroutine(waitLoad(_unitIDs.IndexOf(adUnitId), (float)retryDelay));
          }


          void Interstitial_OnAdClickedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, adInfo, _placement);

               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);

               double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempts[adUnitId]));
               UnityMainThread.wkr.AddJob(() =>
               {
                    StartCoroutine(waitLoad(_unitIDs.IndexOf(adUnitId), (float)retryDelay));
               });
          }

          IEnumerator waitLoad(int ID, float delay)
          {
               yield return new WaitForSeconds(delay);
               Load(ID);
          }

          void OnInterstitialDismissedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> Interstitial dismissed <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, adInfo, _placement);
               _isUnitShowed = false;

               void delayCall()
               {
                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Closed);
                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Watched);
               }

               StartCoroutine(delayCallAction(0.2f, delayCall));

               Load(_unitIDs.IndexOf(adUnitId));
          }

          IEnumerator delayCallAction(float timeDelay, Action callback)
          {
               yield return new WaitForSeconds(timeDelay);
               callback?.Invoke();
          }

          void Interstitial_OnAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> Interstitial Paid <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adInfo, _placement);
               TrackingDefault.CheckMaxRev(adInfo);
          }

          #endregion


          #region Load/Show

          public override void Load(int ID = 0)
          {
               if (!CheckID(ID))
                    return;

               Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {_unitIDs[ID]} <==");

               MaxSdk.LoadInterstitial(_unitIDs[ID]);
          }

          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
               base.Show(ID, callback, placement);
               _isUnitShowed = IsLoaded(ID);
               MaxSdk.ShowInterstitial(_unitIDs[ID], placement);
          }

          public override void Hide(int ID = 0)
          {
               if (!CheckID(ID))
                    return;
               throw new NotImplementedException();
          }

          public override bool IsLoaded(int ID = 0)
          {
               if (!CheckID(ID))
                    return false;
               return MaxSdk.IsInitialized() && MaxSdk.IsInterstitialReady(_unitIDs[ID]);
          }

          #endregion
     }
}
#endif