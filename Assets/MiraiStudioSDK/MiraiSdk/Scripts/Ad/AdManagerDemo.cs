 
using DVAH; 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdManagerDemo: DVAH.Singleton<AdManagerDemo> 
{
    AdBridge _adManager => AdBridge.Instant;
 

    private void Start()
    { 
        DontDestroyOnLoad(this.gameObject); 

    }
 

    #region FUNCTION SHOW/HIDE ADs
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ID"></param>
    public void ShowMRECs(int ID = 0)
    {
        _adManager.ShowAd(AD_TYPE.MRecs,ID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ID"></param>
    public void HideMRECs(int ID = 0)
    {
        _adManager.HideAd(AD_TYPE.MRecs, ID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ID"></param>
    public void ShowBanner(int ID = 0)
    {
        _adManager.ShowAd(AD_TYPE.Banner, ID);
    }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="ID"></param>
    public void HideBanner(int ID = 0)
    {
        _adManager.HideAd(AD_TYPE.Banner, ID);
    }

    /// <summary>
    /// Show Inter dựa theo ID là stt ở cửa sổ MiraiIntergrate,
    /// có thể thêm callback để bắt các sự kiện show, showfail, close, click ...
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="showNoAds"></param>
    /// <param name="ID"></param>
    public void ShowInterstitial(Action<int, AdUnitState> callback = null, bool showNoAds = false, int ID = 0 , string placement = null)
    {
        if(showNoAds && _adManager.IsAdLoaded(AD_TYPE.Inter, ID)){
            Debug.Log($"{CONSTANT.Prefix} ==> implement here, show popup for example!");
        }
        _adManager.ShowAd(AD_TYPE.Inter, ID,callback , placement:placement);
    }

    /// <summary>
    /// Show Reward dựa theo ID là stt ở cửa sổ MiraiIntergrate,
    /// có thể thêm callback để bắt các sự kiện show, showfail, close, click ...
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="showNoAds"></param>
    /// <param name="ID"></param>
    public void ShowRewardVideo(Action<int, AdUnitState> callback = null, bool showNoAds = false, int ID = 0 , string placement =null)
    { 
        _adManager.ShowAd(AD_TYPE.Reward, ID, callback,isShowNoAd: showNoAds ,placement: placement);
    }

    /// <summary>
    /// Show Aoa dựa theo ID là stt ở cửa sổ MiraiIntergrate,
    /// có thể thêm callback để bắt các sự kiện show, showfail, close, click ...
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="ID"></param>
    public void ShowAoa(Action<int, AdUnitState> callback = null, int ID = 0)
    {
        _adManager.ShowAd(AD_TYPE.Aoa, ID, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ID"></param>
    public void ShowNative(int ID)
    {
        _adManager.ShowAd(AD_TYPE.Native, ID);
    }

     /// <summary>
     /// 
     /// </summary>
     /// <param name="ID"></param>
    public void HideNativeAsync(int ID)
    {
        _adManager.HideAd(AD_TYPE.Native, ID);
    }

    #endregion

    #region FUNCTION CHECK LOAD ADs


    public bool IntersIsLoaded(int ID = 0)
    {
        return _adManager.IsAdLoaded(AD_TYPE.Inter, ID);
    }

    public bool RewardIsLoaded(int ID = 0)
    {
        return _adManager.IsAdLoaded(AD_TYPE.Reward, ID);
    }

    public bool AoaIsLoaded(int ID = 0)
    {
        return _adManager.IsAdLoaded(AD_TYPE.Aoa, ID);
    }

    public bool NativeAdLoaded(int ID)
    {
        return _adManager.IsAdLoaded(AD_TYPE.Native, ID);
    }


    #endregion

    #region FUNCTION CALLBACK

    void OnAdLoaded(AD_TYPE adType, params object[] datas){
        FireBaseBridge.Instant.LogEventWithOneParam($"ad_{adType}_load");
    }

    void OnAdLoadedFaild(AD_TYPE adType, params object[] datas){
        #if APPLOVIN_IMPLEMENT
        var errorMsg = ((MaxSdkBase.ErrorInfo)datas[1]).Message;
        #else
        var errorMsg = "";
        #endif

        Hashtable eventParams = new Hashtable(){
            {"errormsg",errorMsg}
        };

        FireBaseBridge.Instant.LogEventWithParameterAsync($"ad_{adType}_fail",eventParams);
    }

    void OnAdShowed(AD_TYPE adType, params object[] datas){
    
        FireBaseBridge.Instant.LogEventWithOneParam($"ad_{adType}_show");
    }

    void OnAdClicked(AD_TYPE adType, params object[] datas){
    
        FireBaseBridge.Instant.LogEventWithOneParam($"ad_{adType}_click");
    }
    
#if MAX_IMPLEMENT
           
    #region Inter
            

    #endregion

    #region Reward 
 
    #endregion
 
    public void OnAdMaxPaid(AD_NETWORK net, params object[] datas)
    { 
         
    }
#endif

#if ADMOB_IMPLEMENT
    public void OnAdMobPaid(AD_NETWORK net, params object[] datas)
    {
     
    }
#endif
    #endregion
     
}

 

