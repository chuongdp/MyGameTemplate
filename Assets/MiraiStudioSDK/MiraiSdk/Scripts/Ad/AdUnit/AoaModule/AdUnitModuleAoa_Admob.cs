#if ADMOB_IMPLEMENT
     using GoogleMobileAds.Api;
     using System;
     using System.Collections;
     using System.Collections.Generic;
     using UnityEngine;

     namespace DVAH
     {
          public class AdUnitModuleAoa_Admob : AdUnitModule
          {
               Dictionary<string, AppOpenAd> _unitObjs         = new Dictionary<string, AppOpenAd>();
               Dictionary<string, float>     _unitRetryAttemps = new Dictionary<string, float>();
               Dictionary<string, DateTime>  _expireTimes      = new Dictionary<string, DateTime>();

               public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
               {
                    base.Init(ModuleSDK, aD_TYPE, unitIDs);

                    foreach (string s in unitIDs)
                    {
                         _expireTimes.Add(s, DateTime.Now);
                         _unitObjs.Add(s, null);
                         _unitRetryAttemps.Add(s, 1);
                         int ID = _unitIDs.IndexOf(s);
                         Load(ID);
                    }

                    return this;
               }


          #region AoA Handle

               void AppOpenOnAdLoadFailedEvent(LoadAdError errorInfo, string adUnitId)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + errorInfo.GetCode() + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.GetMessage(), _placement);

                    if (!_moduleSDK.DVAH_Data.KeepTryReload[adFomart])
                         return;

                    _unitRetryAttemps[adUnitId] *= 2;
                    double retryDelay = _unitRetryAttemps[adUnitId];

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

               void AppOpen_OnAdDisplayedEvent(AppOpenAd ad, string adUnitId)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Show ad {adFomart}  success! <==");
                    _isUnitShowed = true;
                    //string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();

                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, ad, _placement);
                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Open);
               }

               void AppOpen_OnAdClickedEvent(AppOpenAd ad, string adUnitId)
               {
                    Debug.Log(CONSTANT.Prefix + $"==>Click open/resume success! <==");
                    // string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    string adNetWork = ad.GetResponseInfo().GetMediationAdapterClassName();

                    Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, ad, _placement);

                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);
               }

               void AppOpen_OnAdDisplayFailedEvent(AppOpenAd ad, AdError errorInfo, string adUnitId)
               {
                    //string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    string adNetwork = ad.GetResponseInfo().GetMediationAdapterClassName();

                    Debug.LogError($"{CONSTANT.Prefix}==>show ad {adFomart} failed, code: " + errorInfo.GetCode() + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitId, errorInfo.GetMessage(), _placement);

                    int ID = _unitIDs.IndexOf(adUnitId);
                    InvokeCallback(ID, AdUnitState.Interupt);

                    Load(ID);
               }


               public void OnAppOpenDismissedEvent(AppOpenAd ad, string adUnitId)
               {
                    //string adUnitId = ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} - ID:{adUnitId} close! <==");
                    _isUnitShowed = false;
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, ad, _placement);

                    int ID = _unitIDs.IndexOf(adUnitId);
                    InvokeCallback(ID, AdUnitState.Closed);

                    Load(ID);
               }

               void Ad_OnAdPaid(AppOpenAd adInfo, AdValue adValue, string adUnitId)
               {
                    Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} paid! <==");
                    //string adUnitId = adInfo.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adValue, _placement);
               }

          #endregion


          #region SHOW/LOAD ad

               public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
               {
                    base.Show(ID, callback);
                    _unitObjs[_unitIDs[ID]].Show();
               }

               public override void Hide(int ID = 0)
               {
                    throw new NotImplementedException();
               }

               public override bool IsLoaded(int ID = 0)
               {
                    return _unitObjs[_unitIDs[ID]] != null
                        && DateTime.Now            < _expireTimes[_unitIDs[ID]]
                        && _unitObjs[_unitIDs[ID]].CanShowAd();
               }

               public override void Load(int ID = 0)
               {
                    Debug.Log(CONSTANT.Prefix + $"==> Start load {_adFomart} ID:" + _unitIDs[ID] + " <==");

                    string adID = _unitIDs[ID];
                    if (IsLoaded(ID))
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
                    AppOpenAd.Load(adID, adRequest, (AppOpenAd ad, LoadAdError error) =>
                    {
                         // if error is not null, the load request failed.
                         if (error != null || ad == null)
                         {
                              Debug.LogError("app open ad failed to load an ad " + "with error : " + error);
                              AppOpenOnAdLoadFailedEvent(error, adID);
                              return;
                         }

                         Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart} success! <==");
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adID, ad.GetResponseInfo());

                         _unitRetryAttemps[adID] = 1;

                         _expireTimes[adID] = DateTime.Now + TimeSpan.FromHours(4);

                         ad.OnAdPaid += (AdValue value) =>
                         {
                              Ad_OnAdPaid(ad, value, _unitIDs[ID]);
                              TrackingDefault.CheckAdMobRev(value);
                         };


                         // Raised when an impression is recorded for an ad.
                         ad.OnAdImpressionRecorded += () => { Debug.Log("App open ad recorded an impression."); };
                         // Raised when a click is recorded for an ad.
                         ad.OnAdClicked += () => { AppOpen_OnAdClickedEvent(ad, _unitIDs[ID]); };
                         // Raised when an ad opened full screen content.
                         ad.OnAdFullScreenContentOpened += () => { AppOpen_OnAdDisplayedEvent(ad, _unitIDs[ID]); };
                         // Raised when the ad closed full screen content.
                         ad.OnAdFullScreenContentClosed += () => { OnAppOpenDismissedEvent(ad, _unitIDs[ID]); };
                         // Raised when the ad failed to open full screen content.
                         ad.OnAdFullScreenContentFailed += (AdError error) =>
                         {
                              AppOpen_OnAdDisplayFailedEvent(ad, error, _unitIDs[ID]);
                         };

                         _unitObjs[adID] = ad;
                    });
               }

          #endregion
          }
     }
#endif