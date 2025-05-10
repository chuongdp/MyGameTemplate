#if PANGLE_IMPLEMENT
using System.Collections;
using System.Collections.Generic;
using PAG.Scripts.Api;
using PAG.Scripts.Api.Constant;
using UnityEngine; 

namespace DVAH{
public class Init_Pangle : Init_
{

    public override Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
    {
        this._adNetWork = AD_NETWORK.Pangle;
        base.InitSDK(dvah_data, callbacks);
 
        PAGSdk.OnInitFinish += (result, code, message) =>
        {
            //result means the result of SDK initilization statues，true means success，false means fail
            Debug.Log($"result = {result}, error code = {code}, message = {message}");
            if(result)
                this.onSDKInitDone();
        };

        var config = new PAGConfig.Builder()
            .SetAppId(DVAH_Data.Pangle_App_ID)
            .SetGDPRConsent( PAGGDPRConsentType.PAGGDPRConsentTypeConsent)
            .SetDoNotSell(PAGDoNotSellType.PAGDoNotSellTypeNotSell)
            .SetChildDirected(PAGChildDirectedType.PAGChildDirectedTypeNonChild)
            .SetDebugLog(true)
            .Build();
        
        PAGSdk.Init(config); 
        return this;
    }

    public override bool isInitDone()
    {
        //string version = PAGSdk.GetSDKVersion();//get SDK's version
        return _isInitDone && PAGSdk.IsInitSuccess();//check the initialization status
    }

    public override void ShowAdDebugger()
    {
        
    }

}
}
#endif