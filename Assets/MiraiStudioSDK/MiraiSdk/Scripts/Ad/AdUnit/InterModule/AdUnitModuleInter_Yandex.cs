#if YANDEX_IMPLEMENT
using System;
using System.Collections;
using System.Collections.Generic;
using DVAH;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace DVAH{
public class AdUnitModuleInter_Yandex : AdUnitModule
{
    private Dictionary<string,InterstitialAdLoader> _interstitialAdLoaders = new Dictionary<string, InterstitialAdLoader>();
    private Dictionary<string, Interstitial> _interstitials = new Dictionary<string, Interstitial>();

    Dictionary<string,float> _retryAttemps = new Dictionary<string, float>();

     public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
    {
        base.Init(ModuleSDK,aD_TYPE, unitIDs); 
        
        foreach (string s in unitIDs)
        {
            _interstitialAdLoaders.Add(s,null);
            _interstitials.Add(s,null);
            _retryAttemps.Add(s, 0);

            int ID = this._unitIDs.IndexOf(s);
            Load(ID);
        }
        return this;
    }

    #region  Handle Loader
    public void HandleInterstitialLoaded(object sender, InterstitialAdLoadedEventArgs args)
    {
        
        string unitID = args.Interstitial.GetInfo().AdUnitId;
        this._interstitials[unitID] = args.Interstitial;
        this._retryAttemps[unitID] = 0;
        
        this._interstitials[unitID].OnAdClicked += HandleAdClicked;
        this._interstitials[unitID].OnAdShown += HandleInterstitialShown;
        this._interstitials[unitID].OnAdFailedToShow += HandleInterstitialFailedToShow;
        this._interstitials[unitID].OnAdImpression += HandleImpression;
        this._interstitials[unitID].OnAdDismissed += HandleInterstitialDismissed;

         Debug.Log(CONSTANT.Prefix + $"==> Interstitial loaded <==");
        _moduleSDK.EventCallback(adFomart,AdUnitEvent.OnLoaded,unitID,args.Interstitial.GetInfo(),_placement); 
           
    }

    public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        int ID = this._unitIDs.IndexOf(args.AdUnitId);
        this._retryAttemps[args.AdUnitId]++;
        double retryDelay = Math.Pow(2, Math.Min(6,  this._retryAttemps[args.AdUnitId]));
        StartCoroutine(waitReload(ID, this._retryAttemps[args.AdUnitId]));

         Debug.LogError(CONSTANT.Prefix + $"==> Interstitial failed to load with error code: " +args.Message + " <==");
        _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, args.AdUnitId, args.Message,_placement);

       
    }
    #endregion


    #region  Handle Callback
 
    public void HandleAdClicked(object sender, EventArgs args)
    {
        Debug.Log(CONSTANT.Prefix + $"==> {adFomart} ad clicked <==");
        var inter = (Interstitial)sender;
        _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, inter.GetInfo().AdUnitId, inter.GetInfo(),_placement);

        this.InvokeCallback(this._unitIDs.IndexOf(inter.GetInfo().AdUnitId), AdUnitState.Click);
    }

    public void HandleInterstitialShown(object sender, EventArgs args)
    {
        Debug.Log(CONSTANT.Prefix + $"==> Interstitial show! <==");
          var inter = (Interstitial)sender;
        string adUnitID = inter.GetInfo().AdUnitId;

        _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, adUnitID, inter.GetInfo(),_placement);
        _isUnitShowed = true;

        this.InvokeCallback(this._unitIDs.IndexOf(adUnitID), AdUnitState.Open);
    }

    public void HandleInterstitialFailedToShow(object sender, AdFailureEventArgs args)
    {
        Debug.LogError(CONSTANT.Prefix + $"==> Interstitial failed to display with error code: " + args.Message + " <==");
        var inter = (Interstitial)sender;
        string adUnitID = inter.GetInfo().AdUnitId;
        _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowFailed, adUnitID, args.Message, _placement);
        _isUnitShowed = false;

        this.InvokeCallback(this._unitIDs.IndexOf(adUnitID), AdUnitState.Interupt);

        double retryDelay = Math.Pow(2, Math.Min(6, this._retryAttemps[adUnitID]));
        StartCoroutine(waitReload(_unitIDs.IndexOf(adUnitID),(float)retryDelay));
    }

    public void HandleInterstitialDismissed(object sender, EventArgs args)
    {
         Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> Interstitial dismissed <==");
        var inter = (Interstitial)sender;
        string adUnitID = inter.GetInfo().AdUnitId;
        _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, adUnitID, inter.GetInfo(),_placement);
        _isUnitShowed = false;

        void delayCall(){
            this.InvokeCallback(this._unitIDs.IndexOf(adUnitID), AdUnitState.Closed);
            this.InvokeCallback(this._unitIDs.IndexOf(adUnitID), AdUnitState.Watched);
        }
        
        StartCoroutine(delayCallAction(0.2f,delayCall));
        
        Load(this._unitIDs.IndexOf(adUnitID));
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        Debug.Log(CONSTANT.Prefix + $"{CONSTANT.Prefix}==> Interstitial Impression <==");
        
        var data = JSON.Parse(impressionData.rawData);

        var rev = data["revenueUSD"].AsDouble;
        var adUnitId = data["ad_unit_id"].ToString();
        _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPaid, adUnitId, rev,_placement);
    }

    IEnumerator delayCallAction(float timeDelay, Action callback){
        yield return new WaitForSeconds(timeDelay);
        callback?.Invoke();
    }

    #endregion
    IEnumerator waitReload(int ID, float time){
        yield return new WaitForSeconds(time);
        this.Load(ID);
    }

    public override void Load(int ID = 0)
    {
         var interstitialAdLoader = new InterstitialAdLoader();
        interstitialAdLoader.OnAdLoaded += HandleInterstitialLoaded;
        interstitialAdLoader.OnAdFailedToLoad += HandleInterstitialFailedToLoad;

        string adUnitId = this._unitIDs[ID];
        AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(adUnitId).Build();

        interstitialAdLoader.LoadAd(adRequestConfiguration);
        
        this._interstitialAdLoaders[this._unitIDs[ID]] = interstitialAdLoader;
    }

    public override bool IsLoaded(int ID = 0)
    {
        return  this._interstitials[this._unitIDs[ID]] != null;
    }

    public override void Hide(int ID = 0)
    {
        throw new System.NotImplementedException();
    }

    public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
    {
        base.Show(ID, callback, placement);
        if (this._interstitials[this._unitIDs[ID]] == null)
        {
            return;
           
        }

         this._interstitials[this._unitIDs[ID]].Show();
        
    }
    }
}
#endif