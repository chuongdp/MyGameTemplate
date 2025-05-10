#if ADMOB_IMPLEMENT
     using GoogleMobileAds.Api;
     using System;
     using System.Collections;
     using System.Collections.Generic;
     using UnityEngine;

     namespace DVAH
     {
          public class AdUnitModuleMrecs_Admob : AdUnitModule
          {
               Dictionary<int, BannerView> _unitObjs               = new Dictionary<int, BannerView>();
               Dictionary<int, float>      _unitRetryAttemps       = new Dictionary<int, float>();
               Dictionary<int, bool>       _isBannerCurrentlyShows = new Dictionary<int, bool>();
               Dictionary<int, bool>       _isMrecLoaded           = new Dictionary<int, bool>();

               Dictionary<int, AdRequest> _mrecRequest = new Dictionary<int, AdRequest>();

               Coroutine waitReloadCoroutine;

               public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
               {
                    base.Init(ModuleSDK, aD_TYPE, unitIDs);
                    for (int i = 0; i < _unitIDs.Count; i++)
                    {
                         if (unitIDs.IndexOf(_unitIDs[i]) == -1)
                              continue;
                         int ID = i;
                         _unitObjs.Add(ID, null);
                         _isBannerCurrentlyShows.Add(ID, false);
                         _unitRetryAttemps.Add(ID, 0);
                         _isMrecLoaded.Add(ID, false);
                         _mrecRequest.Add(ID, null);
                         Load(ID);
                    }

                    return this;
               }


          #region Banner Handle

               void OnAdLoaded(string adUnitId, int ID)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart}  loaded ");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, null, _placement);

                    _unitRetryAttemps[ID] = 0;
                    _isMrecLoaded[ID]     = true;
                    if (_moduleSDK.DVAH_Data.NO_ADS)
                    {
                         _unitObjs[ID].Hide();
                         return;
                    }

                    if (!_isBannerCurrentlyShows[ID])
                    {
                         _unitObjs[ID].Hide();

                         return;
                    }

                    if (_unitObjs[ID].IsDestroyed)
                    {
                         _unitObjs[ID].Show();
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, null, _placement);
                         InvokeCallback(ID, AdUnitState.Open);
                    }

                    SetPositionMrec(ID);
                    if (!AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.MRecs))
                    {
                         return;
                    }

                    if (!AdBridge.Instant.CappingTimes[AD_TYPE.MRecs].ContainsKey(ID))
                         return;

                    float timeReload = AdBridge.Instant.CappingTimes[AD_TYPE.MRecs][ID].Invoke();
                    if (timeReload <= 0)
                         return;

                    UnityMainThread.wkr.AddJob(() =>
                    {
                         if (waitReloadCoroutine != null) StopCoroutine(waitReloadCoroutine);
                         waitReloadCoroutine = StartCoroutine(waitReloadAd(timeReload, ID));
                    });
               }


               void OnAdLoadFailedEvent(LoadAdError errorInfo, string adUnitId, int ID)
               {
                    Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + errorInfo.GetCode() + " <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.GetMessage(), _placement);

                    _unitRetryAttemps[ID]++;
                    double retryDelay = Math.Pow(2, Math.Min(6, _unitRetryAttemps[ID]));
                    UnityMainThread.wkr.AddJob(() =>
                    {
                         if (waitReloadCoroutine != null) StopCoroutine(waitReloadCoroutine);
                         waitReloadCoroutine = StartCoroutine(waitLoadAd((float)retryDelay, ID));
                    });
               }

               IEnumerator waitLoadAd(float delay, int ID)
               {
                    yield return new WaitForSeconds(delay);
                    Load(ID);
               }

               IEnumerator waitReloadAd(float delay, int ID)
               {
                    yield return new WaitForSeconds(delay);

                    _unitObjs[ID].Hide();
                    _unitObjs[ID].Destroy();

                    Load(ID);
               }


               void OnAdClickedEvent(string adUnitId, int ID)
               {
                    Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, null, _placement);

                    InvokeCallback(ID, AdUnitState.Click);
               }

               void OnAdPaid(AdValue adValue, string adUnitId, int ID)
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
                    if (_isBannerCurrentlyShows[ID])
                         return;

                    _isBannerCurrentlyShows[ID] = true;
                    if (IsLoaded(ID))
                    {
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, _unitIDs[ID], null, _placement);
                         InvokeCallback(ID, AdUnitState.Open);

                         if (AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.MRecs)
                          && AdBridge.Instant.CappingTimes[AD_TYPE.MRecs].ContainsKey(ID))
                         {
                              UnityMainThread.wkr.AddJob(() =>
                              {
                                   if (waitReloadCoroutine != null) StopCoroutine(waitReloadCoroutine);
                                   waitReloadCoroutine =
                                        StartCoroutine(waitReloadAd(AdBridge.Instant.CappingTimes[AD_TYPE.MRecs][ID].Invoke(),
                                             ID));
                              });
                         }
                    }

                    _unitObjs[ID].Show();
                    SetPositionMrec(ID);
               }

               void SetPositionMrec(int ID)
               {
                    AdPosition bannerPosition = ConvertBannerPosition(_moduleSDK.DVAH_Data.MrecsPosition);

                    if (_moduleSDK.DVAH_Data.MrecsPosition != BannerPosition.Custom)
                    {
                         _unitObjs[ID].SetPosition(bannerPosition);
                    }
                    else
                    {
                         Vector2 pos = _moduleSDK.DVAH_Data.adUnits[AD_TYPE.MRecs].AdUnitDatas[ID].Position;

                         float aspect = (float)Screen.height / Screen.width;

                         float   width      = Screen.width  * pos.x / 100 * 160 / Screen.dpi;
                         float   height     = Screen.height * pos.y / 100 * 160 / Screen.dpi;
                         Vector2 posOnPixel = new Vector2(width - 150, height - 125);
                         _unitObjs[ID].SetPosition((int)posOnPixel.x, (int)posOnPixel.y);
                    }
               }

               public override void Hide(int ID = 0)
               {
                    _unitObjs[ID].Hide();
                    _isBannerCurrentlyShows[ID] = false;
                    if (waitReloadCoroutine != null) StopCoroutine(waitReloadCoroutine);

                    if (!AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.MRecs))
                    {
                         return;
                    }

                    if (!AdBridge.Instant.CappingTimes[AD_TYPE.MRecs].ContainsKey(ID))
                         return;

                    float timeReload = AdBridge.Instant.CappingTimes[AD_TYPE.MRecs][ID].Invoke();
                    if (timeReload > 0)
                         return;

                    Load(ID);
               }

               public override bool IsLoaded(int ID = 0)
               {
                    return _unitObjs.ContainsKey(ID) && _unitObjs[ID] != null && _isMrecLoaded[ID];
               }

               AdPosition ConvertBannerPosition(BannerPosition dataPos)
               {
                    switch (_moduleSDK.DVAH_Data.BannerPosition)
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
                    AdPosition bannerPosition = ConvertBannerPosition(_moduleSDK.DVAH_Data.MrecsPosition);

                    if (_moduleSDK.DVAH_Data.MrecsPosition != BannerPosition.Custom)
                    {
                         _unitObjs[ID] = new BannerView(adID, AdSize.MediumRectangle, bannerPosition);
                    }
                    else
                    {
                         Vector2 pos = _moduleSDK.DVAH_Data.adUnits[AD_TYPE.MRecs].AdUnitDatas[ID].Position;

                         float aspect = (float)Screen.height / Screen.width;

                         float   width      = Screen.width  * pos.x / 100 * 160 / Screen.dpi;
                         float   height     = Screen.height * pos.y / 100 * 160 / Screen.dpi;
                         Vector2 posOnPixel = new Vector2(width - 150, height - 125);
                         _unitObjs[ID] = new BannerView(adID, AdSize.MediumRectangle, (int)posOnPixel.x, (int)posOnPixel.y);
                    }


                    // Raised when an ad is loaded into the banner view.
                    _unitObjs[ID].OnBannerAdLoaded += () => { OnAdLoaded(adID, ID); };
                    // Raised when an ad fails to load into the banner view.
                    _unitObjs[ID].OnBannerAdLoadFailed += (LoadAdError error) => { OnAdLoadFailedEvent(error, adID, ID); };
                    // Raised when the ad is estimated to have earned money.
                    _unitObjs[ID].OnAdPaid += (AdValue adValue) =>
                    {
                         OnAdPaid(adValue, adID, ID);
                         TrackingDefault.CheckAdMobRev(adValue);
                    };
                    // Raised when an impression is recorded for an ad.
                    _unitObjs[ID].OnAdImpressionRecorded += () => { Debug.Log("Banner view recorded an impression."); };
                    // Raised when a click is recorded for an ad.
                    _unitObjs[ID].OnAdClicked += () => { OnAdClickedEvent(adID, ID); };
                    // Raised when an ad opened full screen content.
                    _unitObjs[ID].OnAdFullScreenContentOpened += () =>
                    {
                         Debug.Log("Banner view full screen content opened.");
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adID, _unitObjs[ID], _placement);

                         InvokeCallback(ID, AdUnitState.Open);
                    };
                    // Raised when the ad closed full screen content.
                    _unitObjs[ID].OnAdFullScreenContentClosed += () =>
                    {
                         Debug.Log("Banner view full screen content closed.");
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adID, _unitObjs[ID], _placement);

                         InvokeCallback(ID, AdUnitState.Closed);
                    };

                    // Create our request used to load the ad.
                    _isMrecLoaded[ID] = false;
                    if (_mrecRequest[ID] == null)
                         _mrecRequest[ID] = new AdRequest();
                    _unitObjs[ID].LoadAd(_mrecRequest[ID]);
               }

               void Reload(int ID)
               {
                    if (_mrecRequest[ID] == null)
                         _mrecRequest[ID] = new AdRequest();
                    _unitObjs[ID].LoadAd(_mrecRequest[ID]);
               }

          #endregion
          }
     }
#endif