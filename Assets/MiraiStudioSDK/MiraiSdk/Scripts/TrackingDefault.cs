using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#if APPFLYER_IMPLEMENT
using AppsFlyerSDK;
#endif

#if FIREBASE_IMPLEMENT
using Firebase.Analytics;
#endif
using UnityEngine;

namespace DVAH
{
    public class TrackingDefault : Singleton<TrackingDefault>
    {
        private DVAH_Data _DVAH_Data;
        public DVAH_Data DVAH_Data
        {
            get
            {
                if (!_DVAH_Data)
                {
                    _DVAH_Data = Resources.Load<DVAH_Data>("DVAH_Data");
                }

                return _DVAH_Data;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }

        // Start is called before the first frame update
        public void Init()
        {
            Type thisType = this.GetType();
            foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE)))
            {
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnLoaded] += (net, datas) => { OnAdNormal(adType, "loaded", net, datas); };
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnLoadFailed] += (net, datas) => { OnAdFail(adType, "loadFailed", net, datas); };
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnShowed] += (net, datas) => { OnAdNormal(adType, "show", net, datas); };
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnShowFailed] += (net, datas) => { OnAdFail(adType, "fail", net, datas); };
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnRewarded] += (net, datas) => { OnAdNormal(adType, "complete", net, datas); };
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnClick] += (net, datas) => { OnAdNormal(adType, "click", net, datas); };

                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnPaid] += (net, datas) =>
                {
                    MethodInfo theMethod = thisType.GetMethod($"On{net}Paid");
                    Debug.Log(CONSTANT.Prefix + $"Function paid: [On{net}Paid]");
                    theMethod.Invoke(this, new object[] { datas[1], datas[2], adType });
                };
            }

#if APPFLYER_IMPLEMENT
            AppsFlyerAdRevenue.start();
#endif
        }


        private void OnDestroy()
        {
            Debug.LogError($"{CONSTANT.Prefix}==> Adbridge is destroying. Avoid this, may it will effect your logic!");
            foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE)))
            {
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnLoaded] = null;
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnLoadFailed] = null;
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnShowed] = null;
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnShowFailed] = null;
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnRewarded] = null;
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnClick] = null;
                AdBridge.Instant.Callbacks[adType].Event[AdUnitEvent.OnPaid] = null;
            }
        }
        public void OnBasePaid(object data)
        {

        }

        #region  REVENUE
        public void OnMaxPaid(object data1, object data2, AD_TYPE data3)
        {

#if MAX_IMPLEMENT

        var impressionData = (MaxSdkBase.AdInfo)data1;
        double revenue = impressionData.Revenue;
#if FIREBASE_IMPLEMENT
        var impressionParameters = new  Parameter[] {
            new Parameter("ad_platform", "AppLovin"),
            new Parameter("ad_source", impressionData.NetworkName),
            new Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
            new Parameter("ad_format", impressionData.AdFormat),
            new Parameter("value", revenue),
            new Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        
        FireBaseBridge.Instant.LogEventWithParameterAsync(DVAH_Data.ad_impression_key, impressionParameters);

        var info = new SDKAdInfo(impressionData.AdFormat,revenue,(string)data2,"AdMob");
        this.LogSDKRev(info);
#endif
#endif

#if MAX_IMPLEMENT && APPFLYER_IMPLEMENT
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.COUNTRY, "US");
            additionalParams.Add(AFAdRevenueEvent.AD_UNIT, impressionData.AdUnitIdentifier);
            additionalParams.Add(AFAdRevenueEvent.AD_TYPE,  impressionData.AdFormat); 
            AppsFlyerAdRevenue.logAdRevenue("AppLovin", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax, revenue, "USD", additionalParams);
#endif

#if MAX_IMPLEMENT && IRONSOURCE_IMPLEMENT
        ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue();
        customMediationRevenue.MediationNetwork = ISAdQualityMediationNetwork.MAX;
        customMediationRevenue.AdType = data3 == AD_TYPE.Inter? ISAdQualityAdType.INTERSTITIAL : data3 == AD_TYPE.Reward ? ISAdQualityAdType.REWARDED_VIDEO : ISAdQualityAdType.UNKNOWN;
        customMediationRevenue.Revenue = revenue;
        IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
#endif

        }
        public void OnIronSourcePaid(object data1, object data2, AD_TYPE data3)
        {
#if IRONSOURCE_IMPLEMENT
        var impressionData =(IronSourceImpressionData)data1; 
         if (impressionData == null)
                return;
            double revenue = impressionData.revenue.Value;
#if FIREBASE_IMPLEMENT
            var impressionParameters = new Parameter[] {
              new Parameter("ad_platform", "IronSource"),
            //   new Parameter("ad_source", impressionData.adNetwork),
            //   new Parameter("ad_unit_name",impressionData.instanceName ),
            //   new Parameter("ad_format", impressionData.adUnit),
              new Parameter("value", revenue),
              new Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            }; 
            FireBaseBridge.Instant.LogEventWithParameterAsync(DVAH_Data.ad_impression_key, impressionParameters);

            var info = new SDKAdInfo(impressionData.adUnit,revenue,(string)data2,impressionData.adNetwork);
            this.LogSDKRev(info);
#endif
#endif

#if IRONSOURCE_IMPLEMENT && APPFLYER_IMPLEMENT
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.COUNTRY, "US");
            // additionalParams.Add(AFAdRevenueEvent.AD_UNIT, impressionData.instanceName);
            // additionalParams.Add(AFAdRevenueEvent.AD_TYPE, impressionData.adUnit); 
            AppsFlyerAdRevenue.logAdRevenue("IronSource", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource, revenue, "USD", additionalParams);
#endif

#if IRONSOURCE_IMPLEMENT
        ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue();
        customMediationRevenue.MediationNetwork = ISAdQualityMediationNetwork.SELF_MEDIATED;
        customMediationRevenue.AdType = data3 == AD_TYPE.Inter? ISAdQualityAdType.INTERSTITIAL : data3 == AD_TYPE.Reward ? ISAdQualityAdType.REWARDED_VIDEO : ISAdQualityAdType.UNKNOWN;
        customMediationRevenue.Revenue = revenue;
        IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
#endif
        }
        public void OnAdmobPaid(object data1, object data2, AD_TYPE data3)
        {
#if ADMOB_IMPLEMENT
            var revenue = (GoogleMobileAds.Api.AdValue)data1;
            // AdValue revenue = impressionData;
            var realRevenue = (double)revenue.Value / 1000000;
#if FIREBASE_IMPLEMENT
            var impressionParameters = new Parameter[]
            {
                new Parameter("ad_platform", "AdMob"),
                // new Parameter("ad_source", impressionData.ad),
                // new Parameter("ad_unit_name", impressionData.),
                // new Parameter("ad_format", impressionData.),
                new Parameter("value", realRevenue),
                new Parameter("currency", revenue.CurrencyCode)
            };
            FireBaseBridge.Instant.LogEventWithParameterAsync(DVAH_Data.ad_impression_key, impressionParameters);

            var info = new SDKAdInfo(data3.ToString(), realRevenue, (string)data2, "AdMob");
            this.LogSDKRev(info);
#endif
#endif


#if ADMOB_IMPLEMENT && APPFLYER_IMPLEMENT
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.COUNTRY, revenue.CurrencyCode);
            AppsFlyerAdRevenue.logAdRevenue("AdMob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, realRevenue, revenue.CurrencyCode, additionalParams);
#endif

#if IRONSOURCE_IMPLEMENT && ADMOB_IMPLEMENT
            
            ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue();
            customMediationRevenue.MediationNetwork = ISAdQualityMediationNetwork.ADMOB;
            customMediationRevenue.AdType = data3 == AD_TYPE.Inter? ISAdQualityAdType.INTERSTITIAL : data3 == AD_TYPE.Reward ? ISAdQualityAdType.REWARDED_VIDEO : ISAdQualityAdType.UNKNOWN;
            customMediationRevenue.Revenue = realRevenue;
            IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
#endif

        }

        public void OnYandexPaid(object data1, object data2, AD_TYPE data3)
        {
#if YANDEX_IMPLEMENT
            var revenue = (Double)data1; 
#if FIREBASE_IMPLEMENT
            var impressionParameters = new Parameter[]
            {
                new Parameter("ad_platform", "Yandex"),
                new Parameter("value", revenue),
                new Parameter("currency", "USD")
            };
            FireBaseBridge.Instant.LogEventWithParameterAsync(DVAH_Data.ad_impression_key, impressionParameters);
            
            var info = new SDKAdInfo(data3.ToString(),revenue,(string)data2,"Yandex");
            this.LogSDKRev(info);
#endif
#endif


#if YANDEX_IMPLEMENT && APPFLYER_IMPLEMENT
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AFAdRevenueEvent.COUNTRY, "USD"); 
            AppsFlyerAdRevenue.logAdRevenue("Yandex", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, revenue, "USD", additionalParams);
#endif

#if IRONSOURCE_IMPLEMENT && YANDEX_IMPLEMENT
            
            ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue();
            customMediationRevenue.MediationNetwork = ISAdQualityMediationNetwork.OTHER;
            customMediationRevenue.AdType = data3 == AD_TYPE.Inter? ISAdQualityAdType.INTERSTITIAL : data3 == AD_TYPE.Reward ? ISAdQualityAdType.REWARDED_VIDEO : ISAdQualityAdType.UNKNOWN;
            customMediationRevenue.Revenue = revenue;
            IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
#endif

        }


        #endregion

        #region FUNCTION CALLBACK

        void OnAdNormal(AD_TYPE adType, string unitEvent, AD_NETWORK net, params object[] datas)
        {
            string adFormat = adType.ToString();

            Hashtable eventParams = new Hashtable(){
                {"network",net.ToString()}
            };

            if (datas.Length < 3 || datas[2] == null)
            {
                FireBaseBridge.Instant.LogEventWithParameterAsync($"ads_{adFormat}_{unitEvent}", eventParams);
                return;
            }

            eventParams.Add("placement", (string)datas[2]);
            FireBaseBridge.Instant.LogEventWithParameterAsync($"ads_{adFormat}_{unitEvent}", eventParams);

        }

        void OnAdFail(AD_TYPE adType, string unitEvent, AD_NETWORK net, params object[] datas)
        {
            string adFormat = adType.ToString();
            string errorMsg = (string)datas[1];

            Hashtable eventParams = new Hashtable(){
                {"errormsg",errorMsg},
                {"network",net.ToString()}
            };
            if (datas.Length >= 3 && datas[2] != null)
            {
                eventParams.Add("placement", (string)datas[2]);
            }
            FireBaseBridge.Instant.LogEventWithParameterAsync($"ads_{adFormat}_{unitEvent}", eventParams);
        }


        #endregion

        #region REVENUE_SDK
#if FIREBASE_IMPLEMENT
        void LogSDKRev(SDKAdInfo adInfo)
        {

            var impressionParameters = new Parameter[] {
              new Parameter("ad_format", adInfo.ad_format),
              new Parameter("value", adInfo.value),
              new Parameter("location", adInfo.location),
              new Parameter("ad_network", adInfo.ad_network),
              new Parameter("world_id", FireBaseBridge.Instant.CurrentWorldID),
              new Parameter("level", FireBaseBridge.Instant.CurrentLevel), // All AppLovin revenue is sent in USD
            };
            FireBaseBridge.Instant.LogEventWithParameterAsync("ad_revenue_sdk", impressionParameters);
        }
#endif
        #endregion

        #region  DOUBLE_CHECK_REV
#if MAX_IMPLEMENT
    public static void CheckMaxRev(MaxSdkBase.AdInfo impressionData){
        double revenue = impressionData.Revenue;
#if FIREBASE_IMPLEMENT
        var impressionParameters = new  Parameter[] {
            new Parameter("ad_platform", "AppLovin"),
            new Parameter("ad_source", impressionData.NetworkName),
            new Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
            new Parameter("ad_format", impressionData.AdFormat),
            new Parameter("value", revenue),
            new Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        
        FireBaseBridge.Instant.LogEventWithParameterAsync(CONSTANT.Check_rev, impressionParameters);
#endif
    }
#endif

#if ADMOB_IMPLEMENT
        public static void CheckAdMobRev(GoogleMobileAds.Api.AdValue impressionData)
        {
            double revenue = impressionData.Value / 1000000;
#if FIREBASE_IMPLEMENT
            var impressionParameters = new Parameter[] {
            new Parameter("ad_platform", "Admob"),
            new Parameter("value", revenue),
            new Parameter("currency", impressionData.CurrencyCode),
        };

            FireBaseBridge.Instant.LogEventWithParameterAsync(CONSTANT.Check_rev, impressionParameters);
#endif
        }
#endif

#if IRONSOURCE_IMPLEMENT
    public static void CheckISRev(IronSourceImpressionData impressionData){
        double revenue = impressionData.revenue.Value;
#if FIREBASE_IMPLEMENT
        var impressionParameters = new  Parameter[] {
            new Parameter("ad_platform", "IronSource"), 
            new Parameter("value", revenue),
            new Parameter("currency", "USD"),  
        };
        
        FireBaseBridge.Instant.LogEventWithParameterAsync(CONSTANT.Check_rev, impressionParameters);
#endif
    }
#endif
        #endregion
    }

}

class SDKAdInfo
{
    public string ad_format;
    public double value;
    public string location;
    public string ad_network;

    public SDKAdInfo(string ad_format, double value, string location, string ad_network)
    {
        this.ad_format = ad_format;
        this.value = value;
        this.location = location;
        this.ad_network = ad_network;
    }
}
