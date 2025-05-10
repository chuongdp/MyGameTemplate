#if YANDEX_IMPLEMENT
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace DVAH
{
    public class AdUnitModuleReward_Yandex : AdUnitModule
    {
        private Dictionary<string, RewardedAdLoader> _rewardedAdLoaders = new Dictionary<string, RewardedAdLoader>();
        Dictionary<string, RewardedAd> _rewardAdUnits = new Dictionary<string, RewardedAd>();
        Dictionary<string, float> _unitRetryAttemps = new Dictionary<string, float>();

        bool _isCanReward = false;

        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK, aD_TYPE, unitIDs);

            foreach (string s in unitIDs)
            {
                _rewardAdUnits.Add(s, null);
                _unitRetryAttemps.Add(s, 0);
                _rewardedAdLoaders.Add(s,null);
                int ID = this._unitIDs.IndexOf(s);
                Load(ID);
            }
            return this;
        }


        #region Reward Handle
        private void HandleAdLoaded(object sender, RewardedAdLoadedEventArgs args)
        {
    
            string adUnitId = args.RewardedAd.GetInfo().AdUnitId;
            if(adUnitId == null){
                  Debug.LogError($"{CONSTANT.Prefix}==>Click ad {adFomart} but cannot find ID from sender! ");
                  return;
            } 

            // Add events handlers for ad actions
            args.RewardedAd.OnAdClicked += HandleAdClicked;
            args.RewardedAd.OnAdShown += HandleAdShown;
            args.RewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
            args.RewardedAd.OnAdImpression += HandleImpression;
            args.RewardedAd.OnAdDismissed += HandleAdDismissed;
            args.RewardedAd.OnRewarded += HandleRewarded;

            this._rewardAdUnits[adUnitId] = args.RewardedAd;

        }


        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + args.Message+ " <==");
            string adUnitId = args.AdUnitId;
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, args.Message,_placement);

            _unitRetryAttemps[adUnitId]++;
            double retryDelay = Math.Pow(2, Math.Min(6, _unitRetryAttemps[adUnitId]));
            
            UnityMainThread.wkr.AddJob(() =>
                {
                    StartCoroutine( waitLoadAd((float)retryDelay, this._unitIDs.IndexOf(adUnitId)));
                });
        }

        IEnumerator waitLoadAd(float delay, int ID)
        {
            yield return new WaitForSeconds(delay);
            Load(ID);
        }

        private void HandleAdShown(object sender, EventArgs args)
        {
            Debug.Log($"{CONSTANT.Prefix}==>Show ad {adFomart}  success! <==");
            _isUnitShowed = true;
            _isCanReward = false;
            
            var reward = (RewardedAd)sender;
            string adUnitId = reward.GetInfo().AdUnitId;
            
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitId, reward,_placement);
            this.InvokeCallback(this._unitIDs.IndexOf(adUnitId), AdUnitState.Open);
            
        }

        private void HandleAdClicked(object sender, EventArgs args)
        {
            var reward = (RewardedAd)sender;
            string adUnitId = reward.GetInfo().AdUnitId;
            
            Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId, reward,_placement);

            this.InvokeCallback(this._unitIDs.IndexOf(adUnitId), AdUnitState.Click);
             
        }

        private void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
        {
            Debug.LogError($"{CONSTANT.Prefix}==>show ad {adFomart} failed, code: {args.Message } <==");
            _isUnitShowed = false;
            var reward = (RewardedAd)sender;
            string adUnitId = reward.GetInfo().AdUnitId;
           
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitId, args.Message, _placement);
            int ID = this._unitIDs.IndexOf(adUnitId);
             if(ID < 0){
                  Debug.LogError($"{CONSTANT.Prefix}==>show fail ad {adFomart} but cannot find ID {adUnitId} from sender! ");
                  return;
            } 

            this.InvokeCallback(ID, AdUnitState.Interupt);
             
            Load(ID);
        }


        public void HandleAdDismissed(object sender, EventArgs args)
        {
            Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} close! <==");
            _isUnitShowed = false;
            var reward = (RewardedAd)sender; 
            string adUnitId = reward.GetInfo().AdUnitId;
            
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitId, reward,_placement);

            int ID = this._unitIDs.IndexOf(adUnitId);
            if(ID < 0){
                  Debug.LogError($"{CONSTANT.Prefix}==>Close ad {adFomart} but cannot find ID {adUnitId} from sender! ");
                  return;
            } 
            this.InvokeCallback(ID, AdUnitState.Closed);
             
            Load(ID);
        }

        // private void OnAdPaid(RewardedAd adInfo, AdValue adValue, string adUnitId)
        // {
        //     Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} paid! <==");
        //     //string adUnitId = adInfo.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
        //     _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adInfo, _placement);
        // }

        public void HandleImpression(object sender, ImpressionData impressionData)
        {
             Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} paid! {impressionData.rawData} <==");

             var data = JSON.Parse(impressionData.rawData);
              
             var rev = data["revenueUSD"].AsDouble;
             var adUnitId = data["ad_unit_id"].ToString();
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, rev,_placement);
        }

        public void HandleRewarded(object sender, Reward args)
        {
             Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} rewarded! <==");
            _isCanReward = true;
            var reward = (RewardedAd)sender;
            string adUnitId = reward.GetInfo().AdUnitId;
             
            Debug.Log($" {CONSTANT.Prefix} ==> {adFomart} ad {adUnitId} rewarded!!! <==");
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnRewarded, adUnitId, reward,_placement);

            this.InvokeCallback(this._unitIDs.IndexOf(adUnitId), AdUnitState.Watched);
        }
        #endregion


        #region SHOW/LOAD ad

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            base.Show(ID, callback, placement);
            _isUnitShowed = this.IsLoaded(ID);
            _rewardAdUnits[this._unitIDs[ID]].Show();
        }

        public override void Hide(int ID = 0)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsLoaded(int ID = 0)
        {
            return _rewardAdUnits[this._unitIDs[ID]] != null;
        }

        public override void Load(int ID = 0)
        {
             string adID = this._unitIDs[ID];
            Debug.Log(CONSTANT.Prefix + $"==> Start load {this._adFomart} ID: {adID} <==");
            
             // Clean up the old ad before loading a new one.
            if (_rewardAdUnits[adID] != null)
            {
                _rewardAdUnits[adID].Destroy();
                _rewardAdUnits[adID] = null;
            }

            var rewardedAdLoader = new RewardedAdLoader();
            rewardedAdLoader.OnAdLoaded += HandleAdLoaded;
            rewardedAdLoader.OnAdFailedToLoad += HandleAdFailedToLoad;

           
            AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(adID).Build();
            rewardedAdLoader.LoadAd(adRequestConfiguration);
            _rewardedAdLoaders[adID] = rewardedAdLoader;
        }
        #endregion
 
    }
}
#endif

//Data Impression format
// {"currency":"RUB",
// "revenueUSD":"0.000899996",
// "precision":"estimated",
// "revenue":"0.083441000",
// "requestId":"1729579355745782-1246649832573561351300309-production-app-host-vla-pcode-443",
// "blockId":"demo-rewarded-yandex",
// "adType":"rewarded",
// "ad_unit_id":"demo-rewarded-yandex",
// "network":{"name":"Yandex","adapter":"Yandex","ad_unit_id":"demo-rewarded-yandex"}
// }