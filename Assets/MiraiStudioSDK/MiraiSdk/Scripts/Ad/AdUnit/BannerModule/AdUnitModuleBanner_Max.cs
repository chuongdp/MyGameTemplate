#if MAX_IMPLEMENT
using System;
using System.Collections;
using System.Collections.Generic;
#if AMAZON
using AmazonAds;
#endif
using UnityEngine;
using static MaxSdkBase;

namespace DVAH
{
    public class AdUnitModuleBanner_Max : AdUnitModule
    {
        Dictionary<string, float> _bannerRetryAttempts = new Dictionary<string, float>();
        Dictionary<string, bool> isBannerCurrentlyShows = new Dictionary<string, bool>();

        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK, aD_TYPE, unitIDs);
            Initialize();
            foreach (string s in unitIDs)
            {
                _bannerRetryAttempts.Add(s, 0);
                isBannerCurrentlyShows.Add(s, ModuleSDK.DVAH_Data.BannerDefaultShow);
            }
#if AMAZON && !UNITY_EDITOR
            const int width = 320;
            const int height = 50;

            var bannerAdRequest = new APSBannerAdRequest(width, height, ModuleSDK.DVAH_Data.AmazonBannerID, new AdNetworkInfo(ApsAdNetwork.MAX));
            bannerAdRequest.onFailedWithError += (adError) =>
            {
                foreach (string s in unitIDs)
                {
                    int ID = _unitIDs.IndexOf(s);

                    MaxSdk.SetBannerLocalExtraParameter(s, "amazon_ad_error", adError.GetAdError());
                    CreateBanner(ID);
                }
                bannerAdRequest.Dispose();
            };
            bannerAdRequest.onSuccess += (adResponse) =>
            {

                foreach (string s in unitIDs)
                {
                    int ID = _unitIDs.IndexOf(s);

                    MaxSdk.SetBannerLocalExtraParameter(s, "amazon_ad_response", adResponse.GetResponse());
                    CreateBanner(ID);
                }
                bannerAdRequest.Dispose();
            };
            bannerAdRequest.LoadAd();
#else
            foreach (string s in unitIDs)
            {
                int ID = _unitIDs.IndexOf(s);
                CreateBanner(ID);
            }
#endif

            return this;
        }

        public void Initialize()
        {
            Debug.Log($"{CONSTANT.Prefix}==> Init {adFomart}!!! <==");

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += Banner_OnAdRevenuePaidEvent;
        }

        #region Banner method

        void OnBannerAdLoadedEvent(string adUnitId, AdInfo adInfo)
        {
            Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad loaded " + adUnitId + " <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, adUnitId, adInfo, _placement);

            _bannerRetryAttempts[adUnitId] = 0;

            if (_moduleSDK.DVAH_Data.NO_ADS)
            {
                MaxSdk.HideBanner(adUnitId);
                return;
            }

            int ID = _unitIDs.IndexOf(adUnitId);
            Debug.Log("banner ads load event");
            if (AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.Banner)
                && AdBridge.Instant.CappingTimes[AD_TYPE.Banner].ContainsKey(ID))
            {
                MaxSdk.StopBannerAutoRefresh(_unitIDs[ID]);
                UnityMainThread.wkr.AddJob(() => { StartCoroutine(waitLoadAd(AdBridge.Instant.CappingTimes[AD_TYPE.Banner][ID].Invoke(), ID)); });
            }

            if (isBannerCurrentlyShows.ContainsKey(adUnitId) && isBannerCurrentlyShows[adUnitId])
            {
                MaxSdk.ShowBanner(adUnitId);

                return;
            }

            MaxSdk.HideBanner(adUnitId);
        }

        void OnBannerAdFailedEvent(string adUnitId, ErrorInfo errorInfo)
        {
            Debug.LogError($" {CONSTANT.Prefix} ==> {adFomart} ad failed to load with error code: "
                           + errorInfo.Message
                           + " <==");
            Debug.LogError("Waterfall Name: ");
            Debug.LogError(errorInfo.WaterfallInfo.Name + " and Test Name: " + errorInfo.WaterfallInfo.TestName);

            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, errorInfo.Message, _placement);

            if (!_moduleSDK.DVAH_Data.KeepTryReload[adFomart])
                return;

            _bannerRetryAttempts[adUnitId]++;
            double retryDelay = Math.Pow(2, Math.Min(6, _bannerRetryAttempts[adUnitId]));
            int ID = _unitIDs.IndexOf(adUnitId);

            UnityMainThread.wkr.AddJob(() => { StartCoroutine(waitLoad(ID, (float)retryDelay)); });

            string waterfallInfoStr = "";
            foreach (NetworkResponseInfo networkResponse in errorInfo.WaterfallInfo.NetworkResponses)
            {
                waterfallInfoStr = "\nNetwork -> "
                                   + networkResponse.MediatedNetwork
                                   + "\n...adLoadState: "
                                   + networkResponse.AdLoadState;

                if (networkResponse.Error != null)
                {
                    waterfallInfoStr += "\n...error: " + networkResponse.Error;
                }
            }

            Debug.LogError(waterfallInfoStr);
        }

        IEnumerator waitLoad(int ID, float delay)
        {
            yield return new WaitForSeconds(delay);
            Load(ID);
        }

        void OnBannerAdClickedEvent(string adUnitId, AdInfo adInfo)
        {
            Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad clicked <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, adInfo, _placement);

            int ID = _unitIDs.IndexOf(adUnitId);
            InvokeCallback(ID, AdUnitState.Click);
            TrackingDefault.CheckMaxRev(adInfo);
        }

        void Banner_OnAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
        {
            Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad paid <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adInfo, _placement);
            TrackingDefault.CheckMaxRev(adInfo);
        }

        IEnumerator waitLoadAd(float delay, int ID)
        {
            yield return new WaitForSeconds(delay);
            Load(ID);
        }

        #endregion

        #region Show/Hide/Load

        public override bool IsLoaded(int ID = 0) { return MaxSdk.IsInitialized(); }

        void CreateBanner(int ID = 0)
        {
            if (!CheckID(ID))
                return;
            MaxSdk.CreateBanner(_unitIDs[ID], (MaxSdkBase.BannerPosition)_moduleSDK.DVAH_Data.BannerPosition);
            MaxSdk.SetBannerBackgroundColor(_unitIDs[ID], new Color(1, 1, 1, 0));

            if (!_moduleSDK.AdManager.CappingTimes.ContainsKey(adFomart))
            {
                MaxSdk.StartBannerAutoRefresh(_unitIDs[ID]);
            }
            else
            {
                MaxSdk.StopBannerAutoRefresh(_unitIDs[ID]);
            }

            if (!_moduleSDK.DVAH_Data.IsAdptiveBanner)
            {
                Load(ID);

                return;
            }

            float perWith = _moduleSDK.DVAH_Data.adUnits[AD_TYPE.Banner].AdUnitDatas[ID].Size.x;

            if (perWith < 100)
            {
                float density = MaxSdkUtils.GetScreenDensity();
                float dp = Screen.width / perWith / density;
                MaxSdk.SetBannerWidth(_unitIDs[ID], dp);
            }

            Load(ID);
        }

        public override void Load(int ID = 0)
        {
            Debug.Log(CONSTANT.Prefix + $"==>Start load {adFomart} {_unitIDs[ID]} <==");
            MaxSdk.LoadBanner(_unitIDs[ID]);
        }

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            base.Show(ID, callback);
            isBannerCurrentlyShows[_unitIDs[ID]] = true;
            MaxSdk.ShowBanner(_unitIDs[ID]);
        }

        public override void Hide(int ID = 0)
        {
            isBannerCurrentlyShows[_unitIDs[ID]] = false;
            MaxSdk.HideBanner(_unitIDs[ID]);
        }

        #endregion
    }
}

#endif