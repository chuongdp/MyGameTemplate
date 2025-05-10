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
     public class AdUnitModuleMrecs_Max : AdUnitModule

     {
          Dictionary<string, float> _bannerRetryAttempts = new Dictionary<string, float>();
          Dictionary<string, bool> _isBannerCurrentlyShows = new Dictionary<string, bool>();

          Dictionary<string, bool> _isBannerLoaded = new Dictionary<string, bool>();

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);
               Initialize();
               _isBannerCurrentlyShows.Clear();
               foreach (string s in unitIDs)
               {
                    _isBannerCurrentlyShows.Add(s, ModuleSDK.DVAH_Data.MrecsDefaultShow);
                    _isBannerLoaded.Add(s, false);

               }
#if AMAZON && !UNITY_EDITOR
               var rewardedVideoAdRequest = new APSVideoAdRequest(320, 480, ModuleSDK.DVAH_Data.AmazonMrecId, new AdNetworkInfo(ApsAdNetwork.MAX));

                    rewardedVideoAdRequest.onSuccess += (adResponse) =>
                    {
                         
                         foreach (string s in unitIDs)
                         {
                              int ID = _unitIDs.IndexOf(s);
                            
                              MaxSdk.SetRewardedAdLocalExtraParameter(s, "amazon_ad_response", adResponse.GetResponse());
                              Load(ID);
                         }
                    };
                    rewardedVideoAdRequest.onFailedWithError += (adError) =>
                    {
                         foreach (string s in unitIDs)
                         {
                              int ID = _unitIDs.IndexOf(s);
                             
                              MaxSdk.SetRewardedAdLocalExtraParameter(s, "amazon_ad_error", adError.GetAdError());
                              Load(ID);
                         }
                    };

                    rewardedVideoAdRequest.LoadAd();
#else
               foreach (string s in unitIDs)
               {
                    int ID = _unitIDs.IndexOf(s);

                    Load(ID);
               }
#endif

               return this;
          }

          public void Initialize()
          {
               Debug.Log($" {CONSTANT.Prefix} ==> Init {adFomart}! <==");
               MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
               MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
               MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
               MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += MRec_OnAdRevenuePaidEvent;

               MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
               MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
          }

          #region Mrecs method

          public void OnMRecAdLoadedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad loaded " + adUnitId + " <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, adInfo, _placement);
               _bannerRetryAttempts[adUnitId] = 0;
               _isBannerLoaded[adUnitId] = true;

               if (!_isBannerCurrentlyShows[adUnitId])
               {
                    MaxSdk.HideMRec(adUnitId);
                    return;
               }

               MaxSdk.ShowMRec(adUnitId);

               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, null, _placement);
               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Open);

               if (_moduleSDK.AdManager.CappingTimes.ContainsKey(AD_TYPE.MRecs))
               {
                    MaxSdk.StopMRecAutoRefresh(adUnitId);
                    int ID = _unitIDs.IndexOf(adUnitId);
                    float delay = _moduleSDK.AdManager.CappingTimes[AD_TYPE.MRecs][ID].Invoke();
                    if (delay > 0)
                    {
                         UnityMainThread.wkr.AddJob(() => { StartCoroutine(waitReLoad(adUnitId, delay)); });
                    }

                    return;
               }

               MaxSdk.StartBannerAutoRefresh(adUnitId);
          }

          public void OnMRecAdLoadFailedEvent(string adUnitId, ErrorInfo errorInfo)
          {
               Debug.LogError($" {CONSTANT.Prefix} ==> {adFomart} ad failed to load with error code: "
                            + errorInfo.Code
                            + " <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.Message, _placement);

               if (_isBannerCurrentlyShows[adUnitId])
                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Interupt);
               if (!_moduleSDK.DVAH_Data.KeepTryReload[adFomart])
                    return;

               _bannerRetryAttempts[adUnitId]++;
               double retryDelay = Math.Pow(2, Math.Min(6, _bannerRetryAttempts[adUnitId]));

               UnityMainThread.wkr.AddJob(() => { StartCoroutine(waitReLoad(adUnitId, (float)retryDelay)); });
          }

          IEnumerator waitReLoad(string ID, float delay)
          {
               yield return new WaitForSeconds(delay);
               Load(_unitIDs.IndexOf(ID));
          }

          public void OnMRecAdClickedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad clicked <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, adInfo, _placement);
               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);
          }

          public void OnMRecAdExpandedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad expanded! <==");
          }

          public void OnMRecAdCollapsedEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad collapse! <==");
          }

          void MRec_OnAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad paid <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adInfo, _placement);
               TrackingDefault.CheckMaxRev(adInfo);
          }

          #endregion


          #region Show/Load Ads

          public override bool IsLoaded(int ID = 0)
          {
               return MaxSdk.IsInitialized()
                   && _isBannerLoaded.ContainsKey(_unitIDs[ID])
                   && _isBannerLoaded[_unitIDs[ID]] == true;
          }

          public override void Load(int ID = 0)
          {
               if (!CheckID(ID))
                    return;

               if (_moduleSDK.DVAH_Data.MrecsPosition != BannerPosition.Custom)
               {
                    MaxSdk.CreateMRec(_unitIDs[ID], (AdViewPosition)_moduleSDK.DVAH_Data.MrecsPosition);
               }
               else
               {
                    Vector2 pos = _moduleSDK.DVAH_Data.adUnits[AD_TYPE.MRecs].AdUnitDatas[ID].Position;


                    float aspect = (float)Screen.height / Screen.width;

                    float width = Screen.width * pos.x / 100 * 160 / Screen.dpi;
                    float height = Screen.height * pos.y / 100 * 160 / Screen.dpi;

                    Vector2 posOnPixel = new Vector2(width - 150, height - 125);

                    MaxSdk.CreateMRec(_unitIDs[ID], posOnPixel.x, posOnPixel.y);
               }

               Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {_unitIDs[ID]} <==");
               MaxSdk.LoadMRec(_unitIDs[ID]);
               _isBannerLoaded[_unitIDs[ID]] = false;
               if (!_bannerRetryAttempts.ContainsKey(_unitIDs[ID]))
                    _bannerRetryAttempts.Add(_unitIDs[ID], 0);

               _bannerRetryAttempts[_unitIDs[ID]] = 0;
          }

          void Reload(string adUnitID)
          {
               MaxSdk.LoadMRec(adUnitID);
          }

          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
               base.Show(ID, callback);
               string adUnitId = _unitIDs[ID];
               MaxSdk.ShowMRec(adUnitId);
               _isBannerCurrentlyShows[_unitIDs[ID]] = true;
               if (!IsLoaded(ID))
               {
                    return;
               }

               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, _unitIDs[ID], null, _placement);
               InvokeCallback(_unitIDs.IndexOf(_unitIDs[ID]), AdUnitState.Open);

               if (_moduleSDK.AdManager.CappingTimes.ContainsKey(AD_TYPE.MRecs))
               {
                    MaxSdk.StopMRecAutoRefresh(adUnitId);

                    float delay = _moduleSDK.AdManager.CappingTimes[AD_TYPE.MRecs][ID].Invoke();
                    if (delay > 0)
                    {
                         UnityMainThread.wkr.AddJob(() => { StartCoroutine(waitReLoad(adUnitId, delay)); });
                    }
               }
          }

          public override void Hide(int ID = 0)
          {
               string adUnitId = _unitIDs[ID];
               MaxSdk.HideMRec(adUnitId);
               _isBannerCurrentlyShows[adUnitId] = false;

               if (!_moduleSDK.AdManager.CappingTimes.ContainsKey(AD_TYPE.MRecs))
               {
                    return;
               }

               MaxSdk.StopMRecAutoRefresh(adUnitId);

               float delay = _moduleSDK.AdManager.CappingTimes[AD_TYPE.MRecs][ID].Invoke();
               if (delay <= 0)
               {
                    Load(ID);
               }
          }

          #endregion
     }
}

#endif