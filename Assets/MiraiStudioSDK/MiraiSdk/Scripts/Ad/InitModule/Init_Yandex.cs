#if YANDEX_IMPLEMENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YandexMobileAds;

namespace DVAH{
public class Init_Yandex : Init_
{

    public override Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
    {
        this._adNetWork = AD_NETWORK.Yandex;
        base.InitSDK(dvah_data, callbacks);

        this.onSDKInitDone();
        return this;
    }

    public override bool isInitDone()
    {
        return _isInitDone;
    }

    public override void ShowAdDebugger()
    {
        MobileAds.ShowDebugPanel();
    }

}
}
#endif