#if AMAZON
using AmazonAds;
#endif
#if MAX_IMPLEMENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace DVAH
{
    public class Init_Applovin : Init_
    {

        Action<object[]> onInitDone;

        #region Init Method
        public override Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
        {

            this._adNetWork = AD_NETWORK.Max;
            base.InitSDK(dvah_data, callbacks);

#if AMAZON && !UNITY_EDITOR
        try{
            Amazon.Initialize(AdManager.DVAH_Data.AmazonAppID);
            //Amazon.EnableTesting(true);
            Amazon.EnableLogging(true);
            Amazon.UseGeoLocation(true);
            Amazon.IsLocationEnabled();
            Amazon.SetMRAIDPolicy(Amazon.MRAIDPolicy.CUSTOM); 
            Amazon.SetMRAIDSupportedVersions(new string[] { "1.0", "2.0", "3.0" }); 

        }catch(Exception e){
            Debug.LogError("Init Amazon fail!!!");
        }

#endif

            MaxSdkCallbacks.OnSdkInitializedEvent += MaxSdkCallbacks_OnSdkInitializedEvent;

            MaxSdk.SetVerboseLogging(true);
            MaxSdk.SetHasUserConsent(true);
            //MaxSdk.SetIsAgeRestrictedUser(false);
            MaxSdk.SetDoNotSell(true);

            MaxSdk.SetSdkKey(AdManager.DVAH_Data.AppLovin_SDK_Key);
            MaxSdk.InitializeSdk();


            return this;
        }

        private void MaxSdkCallbacks_OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration obj)
        {
            this.onSDKInitDone();

        }

        public override bool isInitDone()
        {
            return MaxSdk.IsInitialized() && _isInitDone;
        }

        public override void ShowAdDebugger()
        {
            _ = ShowAdDebuggerAsync();
        }

        async Task ShowAdDebuggerAsync()
        {
            while (!this._isInitDone)
            {
                await Task.Delay(100);
            }

            if (this.DVAH_Data.IsMediationDebugger && DVAH_Data.MEDIATION_NETWORK == this._adNetWork)
                MaxSdk.ShowMediationDebugger();
        }


        #endregion


    }

}
#endif
