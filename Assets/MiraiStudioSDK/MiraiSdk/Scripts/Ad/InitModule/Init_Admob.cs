#if ADMOB_IMPLEMENT
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
// using GoogleMobileAds.Mediation.LiftoffMonetize.Api;
// using GoogleMobileAds.Mediation.Mintegral.Api;  

namespace DVAH
{
    public class Init_Admob : Init_
    {
        
        public override Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
        { 
            this._adNetWork = AD_NETWORK.Admob; 
            base.InitSDK(dvah_data, callbacks);
           
            MobileAds.Initialize(HandleInit);
            #if UNITY_IOS
            MobileAds.SetiOSAppPauseOnBackground(true);
            #endif
            return this;
        }

        public override bool isInitDone()
        {
            return _isInitDone;
        }

        public override void ShowAdDebugger()
        {
            _=ShowAdDebuggerAsync();
        }

        async Task ShowAdDebuggerAsync(){
            while(!this._isInitDone){
                await Task.Delay(100);
            }

            if(this.DVAH_Data.IsMediationDebugger )
                MobileAds.OpenAdInspector(error => {
                    Debug.LogError(CONSTANT.Prefix +$"==> ADMOB debugger error: {error.GetMessage()}");
                });
        }

        void HandleInit(InitializationStatus initStatus)
        {
            Debug.Log($"Admob sdk init done {initStatus}");

            // LiftoffMonetize.SetGDPRStatus(true, "v1.0.0");
            // #if UNITY_IPHONE
            // LiftoffMonetize.SetGDPRMessageVersion("v1.0.0");
            // #endif
            // LiftoffMonetize.SetCCPAStatus(true);
            //
            // Mintegral.SetConsentStatus(true); 
            

            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                case AdapterState.NotReady:
                    // The adapter initialization did not complete.
                    MonoBehaviour.print("Adapter: " + className + " not ready.");
                    break;
                case AdapterState.Ready:
                    // The adapter was successfully initialized.
                    MonoBehaviour.print("Adapter: " + className + " is initialized.");
                    break;
                }
            }
            this.onSDKInitDone();
        } 
    }
}
#endif
