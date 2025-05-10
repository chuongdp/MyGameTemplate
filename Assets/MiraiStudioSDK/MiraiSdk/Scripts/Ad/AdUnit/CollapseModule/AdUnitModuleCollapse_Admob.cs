#if ADMOB_IMPLEMENT
     using GoogleMobileAds.Api;
     using System;
     using System.Collections;
     using System.Collections.Generic;
     using UnityEngine;

     namespace DVAH
     {
          public class AdUnitModuleCollapse_Admob : AdUnitModule
          {
               Dictionary<string, BannerView> _unitObjs               = new Dictionary<string, BannerView>();
               Dictionary<string, float>      _unitRetryAttemps       = new Dictionary<string, float>();
               Dictionary<string, bool>       _isBannerCurrentlyShows = new Dictionary<string, bool>();

               Coroutine waitReload;

               public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
               {
                    base.Init(ModuleSDK, aD_TYPE, unitIDs);

                    foreach (string s in unitIDs)
                    {
                         _unitObjs.Add(s, null);

                         _unitRetryAttemps.Add(s, 0);
                    }

                    return this;
               }


          #region Banner Handle

               void OnAdLoaded(string adUnitId)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart}  loaded ");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, null, _placement);

                    _unitRetryAttemps[adUnitId] = 0;
                    _unitObjs[adUnitId].Show();
               }


               void OnAdLoadFailedEvent(LoadAdError errorInfo, string adUnitId)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + errorInfo.GetCode() + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.GetMessage(), _placement);

                    _unitRetryAttemps[adUnitId]++;
                    double retryDelay = Math.Pow(2, Math.Min(6, _unitRetryAttemps[adUnitId]));
                    UnityMainThread.wkr.AddJob(() =>
                    {
                         if (waitReload != null)
                              StopCoroutine(waitReload);

                         waitReload = StartCoroutine(waitLoadAd((float)retryDelay, _unitIDs.IndexOf(adUnitId)));
                    });
               }

               IEnumerator waitLoadAd(float delay, int ID)
               {
                    yield return new WaitForSeconds(delay);
                    Load(ID);
               }


               void OnAdClickedEvent(string adUnitId)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, null, _placement);

                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);
               }

               void OnAdPaid(AdValue adValue, string adUnitId)
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

                    // if(_unitObjs[_unitIDs[ID]] != null)
                    //     _unitObjs[this._unitIDs[ID]].Show(); 

                    //_isBannerCurrentlyShows[_unitIDs[ID]] = true; 
                    Load(ID);
               }

               public override void Hide(int ID = 0)
               {
                    if (_unitObjs[_unitIDs[ID]] == null)
                         return;
                    _unitObjs[_unitIDs[ID]].Hide();
                    //_isBannerCurrentlyShows[_unitIDs[ID]] = false;
               }

               public override bool IsLoaded(int ID = 0)
               {
                    return true;
               }

               AdPosition ConvertBannerPosition(BannerPosition dataPos)
               {
                    switch (dataPos)
                    {
                         case BannerPosition.TopCenter:
                              return AdPosition.Top;
                         case BannerPosition.TopLeft:
                              return AdPosition.TopLeft;
                         case BannerPosition.TopRight:
                              return AdPosition.TopRight;

                         case BannerPosition.Centered:
                              return AdPosition.Center;

                         case BannerPosition.BottomLeft:
                              return AdPosition.BottomLeft;
                         case BannerPosition.BottomRight:
                              return AdPosition.BottomRight;
                         case BannerPosition.BottomCenter:
                              return AdPosition.Bottom;
                    }

                    return AdPosition.Top;
               }

               public override void Load(int ID = 0)
               {
                    Debug.Log(CONSTANT.Prefix + $"==> Start load {_adFomart} ID:" + _unitIDs[ID] + " <==");

                    string     adID           = _unitIDs[ID];
                    AdPosition bannerPosition = ConvertBannerPosition(_moduleSDK.DVAH_Data.CollapsePosition);

                    if (_unitObjs[adID] == null)
                    {
                         _unitObjs[adID] = new BannerView(adID, AdSize.Banner, bannerPosition);
                         // Raised when an ad is loaded into the banner view.
                         _unitObjs[adID].OnBannerAdLoaded += () => { OnAdLoaded(adID); };
                         // Raised when an ad fails to load into the banner view.
                         _unitObjs[adID].OnBannerAdLoadFailed += (LoadAdError error) => { OnAdLoadFailedEvent(error, adID); };
                         // Raised when the ad is estimated to have earned money.
                         _unitObjs[adID].OnAdPaid += (AdValue adValue) =>
                         {
                              OnAdPaid(adValue, adID);
                              TrackingDefault.CheckAdMobRev(adValue);
                         };
                         // Raised when an impression is recorded for an ad.
                         _unitObjs[adID].OnAdImpressionRecorded += () =>
                         {
                              InvokeCallback(_unitIDs.IndexOf(adID), AdUnitState.Watched);
                              Debug.Log("Banner view recorded an impression.");
                         };
                         // Raised when a click is recorded for an ad.
                         _unitObjs[adID].OnAdClicked += () => { OnAdClickedEvent(adID); };
                         // Raised when an ad opened full screen content.
                         _unitObjs[adID].OnAdFullScreenContentOpened += () =>
                         {
                              InvokeCallback(_unitIDs.IndexOf(adID), AdUnitState.Open);
                              _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adID);
                              Debug.Log("Banner view full screen content opened.");
                         };
                         // Raised when the ad closed full screen content.
                         _unitObjs[adID].OnAdFullScreenContentClosed += () =>
                         {
                              Debug.Log("Banner view full screen content closed.");
                              _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adID);

                              InvokeCallback(_unitIDs.IndexOf(adID), AdUnitState.Closed);
                         };
                    }

                    // Create our request used to load the ad.
                    AdRequest adRequest = new AdRequest();
                    adRequest.Extras.Add("collapsible", bannerPosition.ToString().ToLower());
                    adRequest.Extras.Add("collapsible_request_id", Guid.NewGuid().ToString());

                    _unitObjs[adID].LoadAd(adRequest);
               }

          #endregion
          }
     }
#endif