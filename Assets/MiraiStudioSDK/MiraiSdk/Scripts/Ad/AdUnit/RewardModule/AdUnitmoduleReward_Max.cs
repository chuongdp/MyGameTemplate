#if MAX_IMPLEMENT
using System;
using System.Collections;
using System.Collections.Generic;
#if AMAZON
using AmazonAds;
#endif
using UnityEngine;

namespace DVAH
{
     public class AdUnitmoduleReward_Max : AdUnitModule
     {
          bool _isRewarded = false;
          Dictionary<string, float> _rewardedRetryAttempts = new Dictionary<string, float>();

          public override void Hide(int ID = 0)
          {
               throw new NotImplementedException();
          }

          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);
               Initialize();
               foreach (string s in unitIDs)
               {
                    _rewardedRetryAttempts.Add(s, 0);
               }
#if AMAZON && !UNITY_EDITOR
               var rewardedVideoAdRequest = new APSVideoAdRequest(320, 480, ModuleSDK.DVAH_Data.AmazonRewarID, new AdNetworkInfo(ApsAdNetwork.MAX));

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

          #region Reward Ad Methods

          public void Initialize()
          {
               Debug.Log($"==> {CONSTANT.Prefix} Ad {adFomart} init! <==");
               MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
               MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
               MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
               MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
               MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
               MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
               MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
               MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += Rewarded_OnAdRevenuePaidEvent;
          }

          void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad loaded " + adUnitId + " <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, adInfo, _placement);

               _rewardedRetryAttempts[adUnitId] = 0;
          }

          void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
          {
               Debug.LogError($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} load fail error: {errorInfo.Code} <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.Message, _placement);

               _rewardedRetryAttempts[adUnitId]++;
               double retryDelay = Math.Pow(2, Math.Min(6, _rewardedRetryAttempts[adUnitId]));
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

          void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
          {
               Debug.LogError($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} show fail error: {errorInfo.Code} <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitId, errorInfo.Message, _placement);

               if (!_moduleSDK.DVAH_Data.KeepTryReload[adFomart])
                    return;

               _isUnitShowed = false;
               Load(_unitIDs.IndexOf(adUnitId));

               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Interupt);
          }

          void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} showed!!! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, adInfo, _placement);

               _isRewarded = false;
               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Open);
          }

          void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} clicked!!! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, adInfo, _placement);

               InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Click);
          }

          void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} closed!!! <==");
               _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, adInfo, _placement);
               _isUnitShowed = false;
               Load(_unitIDs.IndexOf(adUnitId));

               void delayCall()
               {
                    InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Closed);

                    if (_isRewarded)
                    {
                         Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} rewarded!!! <==");
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnRewarded, adUnitId, adInfo, _placement);

                         InvokeCallback(_unitIDs.IndexOf(adUnitId), AdUnitState.Watched);
                    }
               }

               StartCoroutine(delayCallAction(0.2f, delayCall));
          }

          IEnumerator delayCallAction(float timeDelay, Action callback)
          {
               if (Time.timeScale != 0)
               {
               yield return new WaitForSeconds(timeDelay);
               }
               else
               {
                    yield return new WaitForEndOfFrame();
               }
               callback?.Invoke();
          }

          void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
          {
               _isRewarded = true;
          }


          void Rewarded_OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
          {
               Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} paid!!! <==");
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
               MaxSdk.LoadRewardedAd(_unitIDs[ID]);
          }

          public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
          {
               base.Show(ID, callback, placement);
               _isUnitShowed = IsLoaded(ID);
               MaxSdk.ShowRewardedAd(_unitIDs[ID], placement);
          }

          public override bool IsLoaded(int ID = 0)
          {
               return MaxSdk.IsInitialized() && MaxSdk.IsRewardedAdReady(_unitIDs[ID]);
          }

          #endregion
     }
}
#endif