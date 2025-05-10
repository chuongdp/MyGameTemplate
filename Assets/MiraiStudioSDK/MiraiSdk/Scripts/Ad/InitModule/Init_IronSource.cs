#if IRONSOURCE_IMPLEMENT
using System;
using System.Threading.Tasks;
using com.unity3d.mediation;
using UnityEngine;

namespace DVAH
{
    public class Init_IronSource : Init_
    {
        public string _placement = "none";
        #region Init Method
        public override Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
        {
           
            this._adNetWork = AD_NETWORK.IronSource;
            base.InitSDK(dvah_data, callbacks);

            if(DVAH_Data.IsMediationDebugger && DVAH_Data.MEDIATION_NETWORK == this._adNetWork)
                IronSource.Agent.setMetaData("is_test_suite", "enable"); 

            IronSourceEvents.onImpressionDataReadyEvent += TrackAdRevenue;  
            #if UNITY_EDITOR
            this.onSDKInitDone();
            #endif

            IronSource.Agent.setConsent(true);
            IronSource.Agent.setMetaData("do_not_sell","true");
            IronSource.Agent.setMetaData("is_child_directed","false");
  
            LevelPlayAdFormat[] legacyAdFormats = new[] { LevelPlayAdFormat.REWARDED , LevelPlayAdFormat.BANNER, LevelPlayAdFormat.INTERSTITIAL};
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
       
            LevelPlay.Init(dvah_data.IS_App_Key,adFormats: legacyAdFormats); 
            
            AdQualitySdkInit adQualitySdkInit = new AdQualitySdkInit();
            ISAdQualityConfig adQualityConfig = new ISAdQualityConfig {
                AdQualityInitCallback = adQualitySdkInit
            };

            IronSourceAdQuality.Initialize(dvah_data.IS_App_Key, adQualityConfig);    
              

            return this;
        }

        private void SdkInitializationFailedEvent(LevelPlayInitError error)
        {
            Debug.LogError($" {CONSTANT.Prefix} ==> Levelplay init fail!!! <==");
            
        }

        private void SdkInitializationCompletedEvent(LevelPlayConfiguration configuration)
        {
            this.onSDKInitDone();
            IronSource.Agent.validateIntegration();
        }

        protected override void onSDKInitDone()
        {
            base.onSDKInitDone();
           
        }

        private void TrackAdRevenue(IronSourceImpressionData data)
        {
            Debug.Log($" {CONSTANT.Prefix} ==> Ironsource paid!!! <==");
            this.EventCallback(AD_TYPE.Reward,AdUnitEvent.OnPaid,"IS",data,this._placement);
            TrackingDefault.CheckISRev(data);
        }

        public override bool isInitDone()
        {
            return _isInitDone;
        }

        public override void ShowAdDebugger()
        {
            _= ShowAdDebuggerAsync();
        }

        async Task ShowAdDebuggerAsync(){
            while(!this._isInitDone){
                await Task.Delay(100);
            }

            if(DVAH_Data.IsMediationDebugger && DVAH_Data.MEDIATION_NETWORK == this._adNetWork)
                    IronSource.Agent.launchTestSuite();
        }


        #endregion


    }

}

public class AdQualitySdkInit: ISAdQualityInitCallback {

   public void adQualitySdkInitSuccess() {
       Debug.Log("unity: adQualitySdkInitSuccess");
   }
   public void adQualitySdkInitFailed(ISAdQualityInitError adQualitySdkInitError, string errorMessage) {
       Debug.Log("unity: adQualitySdkInitFailed " + adQualitySdkInitError + " message: " + errorMessage);
   }
}
#endif
