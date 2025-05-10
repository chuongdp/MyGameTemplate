#if YANDEX_IMPLEMENT
using YandexMobileAds;
using YandexMobileAds.Base;
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

namespace DVAH
{
    public class AdUnitModuleBanner_Yandex : AdUnitModule
    {
        Dictionary<string, Banner> _bannerUnits = new Dictionary<string, Banner>();
        Dictionary<string, float> _unitRetryAttemps = new Dictionary<string, float>();
        Dictionary<string, bool> _isBannerCurrentlyShows = new Dictionary<string, bool>();

        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK, aD_TYPE, unitIDs);

            foreach (string s in unitIDs)
            {
                _bannerUnits.Add(s, null);
                _isBannerCurrentlyShows.Add(s, ModuleSDK.DVAH_Data.BannerDefaultShow);
                _unitRetryAttemps.Add(s, 0);
                int ID = this._unitIDs.IndexOf(s);
                Load(ID);
            }
            return this;
        }

    

        #region Banner Handle

        private void OnAdLoaded(object sender, EventArgs e)
        {
            Debug.Log($"{CONSTANT.Prefix}==>Load ad {adFomart}  loaded ");
             var banner = (Banner)sender;
            string adUnitId = this.getIDbySender(banner);

            if(adUnitId == null){
                  Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  loaded but cannot find ID from sender! ");
                  return;
            } 
           
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded,adUnitId ,null,_placement);

            _unitRetryAttemps[adUnitId] = 0;
            if(_moduleSDK.DVAH_Data.CHEAT_BUILD){
                 banner.Hide();
                 return;
            }
            if (!_isBannerCurrentlyShows[adUnitId]){
                banner.Hide();

                return;
            }

            banner.Show();
            int ID = this._unitIDs.IndexOf(adUnitId);

            if(AdBridge.Instant.CappingTimes.ContainsKey(AD_TYPE.Banner) && AdBridge.Instant.CappingTimes[AD_TYPE.Banner].ContainsKey(ID))
                 UnityMainThread.wkr.AddJob(() =>
                {
                    StartCoroutine( waitLoadAd(AdBridge.Instant.CappingTimes[AD_TYPE.Banner][ID].Invoke(),ID));
                });
        }


        private void HandleAdFailedToLoad(object sender, AdFailureEventArgs e)
        {
            Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed, code: " + e.Message + " <==");
            var banner = (Banner)sender;
            string adUnitId = this.getIDbySender(banner);

            if(adUnitId == null){
                  Debug.LogError($"{CONSTANT.Prefix}==>Load ad {adFomart}  failed but cannot find ID from sender! ");
                  return;
            } 
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, adUnitId, e.Message,_placement);

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
 

        private void OnAdClickedEvent(object sender, EventArgs e)
        { 
            Debug.Log($"{CONSTANT.Prefix}==>Click ad {adFomart} success! <==");
            var banner = (Banner)sender;
            string adUnitId = this.getIDbySender(banner);

            if(adUnitId == null){
                  Debug.LogError($"{CONSTANT.Prefix}==>Click ad {adFomart} but cannot find ID from sender! ");
                  return;
            } 
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, adUnitId,null,_placement);

            this.InvokeCallback(this._unitIDs.IndexOf(adUnitId), AdUnitState.Click);
             
        } 

        // private void OnAdPaid (AdValue adValue, string adUnitId)
        // {
        //     Debug.Log($"{CONSTANT.Prefix}==> ad {adFomart} paid! <==");
        //     //string adUnitId = adInfo.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceId;
        //     _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, adValue,_placement);
        // }

        private void HandleImpression(object sender, ImpressionData e)
        {
            var rawData = e == null ? "null" : e.rawData;
            Debug.Log($"HandleImpression event received with data: {rawData}");
            var data = JSON.Parse(rawData);

            var rev = data["revenueUSD"].AsDouble;
            var adUnitId = data["ad_unit_id"].ToString();
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, rev,_placement);
        }


        #endregion


        #region SHOW/LOAD ad

        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            base.Show(ID, callback); 
            _bannerUnits[this._unitIDs[ID]].Show();
            _isBannerCurrentlyShows[_unitIDs[ID]] = true;
        }

        public override void Hide(int ID = 0)
        {
            _bannerUnits[this._unitIDs[ID]].Hide();
            _isBannerCurrentlyShows[_unitIDs[ID]] = false;
        }

        public override bool IsLoaded(int ID = 0)
        {
            return _bannerUnits.ContainsKey(this._unitIDs[ID]) && _bannerUnits[this._unitIDs[ID]] != null;
        }

        AdPosition ConvertBannerPosition(BannerPosition dataPos)
        {
            return (AdPosition)dataPos;
        }

        public override void Load(int ID = 0)
        {
            Debug.Log(CONSTANT.Prefix + $"==> Start load {this._adFomart} ID:" + this._unitIDs[ID] + " <==");

            string adUnitId = this._unitIDs[ID];
            AdPosition bannerPosition = ConvertBannerPosition(_moduleSDK.DVAH_Data.BannerPosition);
            
            BannerAdSize bannerMaxSize = BannerAdSize.StickySize(GetScreenWidthDp());
            var banner = new Banner(adUnitId, bannerMaxSize, bannerPosition);

            AdRequest request = new AdRequest.Builder().Build();
            banner.LoadAd(request);

             banner.OnAdLoaded += OnAdLoaded;

            //  Called when there was an error loading the ad
            banner.OnAdFailedToLoad += HandleAdFailedToLoad;

            //Called when the app went inactive because the user tapped an ad and is about to switch to a different app (for example, a browser).
            //banner.OnLeftApplication += HandleLeftApplication;

            //  Called when the user returns to the app after a tap
            //banner.OnReturnedToApplication += HandleReturnedToApplication;

            //  Called when the user clicks the ad
            banner.OnAdClicked += OnAdClickedEvent;

            //  Called when an impression is registered
            banner.OnImpression += HandleImpression;

            this._bannerUnits[adUnitId] = banner;
        }

        #endregion

        private string getIDbySender(Banner banner){
            foreach(var item in _bannerUnits){
                if(banner.Equals(item))
                    return item.Key;
            }

            return null;
        }

        private int GetScreenWidthDp()
        {
            int screenWidth = (int)Screen.safeArea.width;
            return ScreenUtils.ConvertPixelsToDp(screenWidth);
        }
    }
}
#endif