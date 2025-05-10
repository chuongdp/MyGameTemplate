#if ADMOB_IMPLEMENT
     using GoogleMobileAds.Api;
     using System;
     using System.Collections;
     using System.Collections.Generic;
     using UnityEngine;

     namespace DVAH
     {
          public class AdUnitModuleInter_Admob : AdUnitModule
          {
               Dictionary<string, InterstitialAd> _unitObjs         = new Dictionary<string, InterstitialAd>();
               Dictionary<string, float>          _unitRetryAttemps = new Dictionary<string, float>();

               public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
               {
                    base.Init(ModuleSDK, aD_TYPE, unitIDs);

                    foreach (string s in unitIDs)
                    {
                         _unitObjs.Add(s, null);
                         _unitRetryAttemps.Add(s, 0);
                         int ID = _unitIDs.IndexOf(s);
                         Load(ID);
                    }

                    return this;
               }


          #region Inter Handle

               void OnAdLoadFailedEvent(LoadAdError errorInfo, string adUnitId)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + errorInfo.GetCode() + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.GetMessage(), _placement);

                    _unitRetryAttemps[adUnitId]++;
                    double retryDelay = Math.Pow(2, Math.Min(6, _unitRetryAttemps[adUnitId]));
                    UnityMainThread.wkr.AddJob(() =>
                    {
                         StartCoroutine(waitLoadAd((float)retryDelay, _unitIDs.IndexOf(adUnitId)));
                    });
               }

               IEnumerator waitLoadAd(float delay, int ID)
               {
                    yield return new WaitForSeconds(delay);
                    Load(ID);
               }

               void OnAdDisplayedEvent(InterstitialAd ad, string adUnitId)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Show ad {adFomart}  success! <==");

                    _isUnitShowed = true;
                    //string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();

                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, ad, _placement);

                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Open);
               }

               void OnAdClickedEvent(InterstitialAd ad, string adUnitId)
               {
                    // string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();

                    Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, ad, _placement);

                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);
               }

               void OnAdDisplayFailedEvent(InterstitialAd ad, AdError errorInfo, string adUnitId)
               {
                    //string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    string adNetwork = ad.GetResponseInfo().GetMediationAdapterClassName();
                    _isUnitShowed = false;
                    Debug.LogError($"{CONSTANT.Prefix}==>show ad {adFomart} failed, code: " + errorInfo.GetCode() + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitId, errorInfo.GetMessage(), _placement);

                    int ID = _unitIDs.IndexOf(adUnitId);
                    InvokeCallback(ID, AdUnitState.Interupt);

                    Load(ID);
               }


               public void OnAdDismissedEvent(InterstitialAd ad, string adUnitId)
               {
                    //string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} - ID:{adUnitId} close! <==");

                    _isUnitShowed = false;
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, ad, _placement);

                    int ID = _unitIDs.IndexOf(adUnitId);
                    InvokeCallback(ID, AdUnitState.Closed);
                    InvokeCallback(ID, AdUnitState.Watched);

                    Load(ID);
               }

               void OnAdPaid(InterstitialAd adInfo, AdValue adValue, string adUnitId)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} paid! <==");
                    //string adUnitId = adInfo.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adInfo, _placement);
               }

          #endregion


          #region SHOW/LOAD ad

               public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
               {
                    base.Show(ID, callback, placement);
                    _isUnitShowed = IsLoaded(ID);
                    _unitObjs[_unitIDs[ID]].Show();
               }

               public override void Hide(int ID = 0)
               {
                    throw new NotImplementedException();
               }

               public override bool IsLoaded(int ID = 0)
               {
                    return _unitObjs[_unitIDs[ID]] != null && _unitObjs[_unitIDs[ID]].CanShowAd();
               }

               public override void Load(int ID = 0)
               {
                    Debug.Log(CONSTANT.Prefix + $"==> Start load {_adFomart} ID:" + _unitIDs[ID] + " <==");

                    string adID = _unitIDs[ID];
                    if (_unitObjs[adID] != null && _unitObjs[adID].CanShowAd())
                    {
                         return;
                    }

                    // Clean up the old ad before loading a new one.
                    if (_unitObjs[adID] != null)
                    {
                         _unitObjs[adID].Destroy();
                         _unitObjs[adID] = null;
                    }

                    Debug.Log("Loading the app open ad.");

                    // Create our request used to load the ad.
                    AdRequest adRequest = new AdRequest();
                    InterstitialAd.Load(_unitIDs[ID], adRequest, (InterstitialAd ad, LoadAdError error) =>
                    {
                         // if error is not null, the load request failed.
                         if (error != null || ad == null)
                         {
                              Debug.LogError("app open ad failed to load an ad " + "with error : " + error);
                              OnAdLoadFailedEvent(error, adID);
                              return;
                         }

                         Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart} success! <==");
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, _placement, adID, ad.GetResponseInfo());

                         _unitRetryAttemps[adID] = 0;

                         ad.OnAdPaid += (AdValue value) =>
                         {
                              OnAdPaid(ad, value, _unitIDs[ID]);
                              TrackingDefault.CheckAdMobRev(value);
                         };


                         // Raised when an impression is recorded for an ad.
                         ad.OnAdImpressionRecorded += () => { Debug.Log("App open ad recorded an impression."); };
                         // Raised when a click is recorded for an ad.
                         ad.OnAdClicked += () => { OnAdClickedEvent(ad, _unitIDs[ID]); };
                         // Raised when an ad opened full screen content.
                         ad.OnAdFullScreenContentOpened += () => { OnAdDisplayedEvent(ad, _unitIDs[ID]); };
                         // Raised when the ad closed full screen content.
                         ad.OnAdFullScreenContentClosed += () => { OnAdDismissedEvent(ad, _unitIDs[ID]); };
                         // Raised when the ad failed to open full screen content.
                         ad.OnAdFullScreenContentFailed += (AdError error) =>
                         {
                              OnAdDisplayFailedEvent(ad, error, _unitIDs[ID]);
                         };

                         _unitObjs[adID] = ad;
                    });
               }

          #endregion
          }
     }
#endif