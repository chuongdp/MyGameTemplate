#if APPFLYER_IMPLEMENT
using AppsFlyerSDK;
#endif
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

namespace DVAH
{

    public class AppFlyerBridge : Singleton<AppFlyerBridge>
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

            Init();
        }

        void Init()
        { 
#if APPFLYER_IMPLEMENT
             if(FindObjectOfType<AppsFlyerObjectScript>() == null){
                AppsFlyerObjectScript AF = this.gameObject.AddComponent<AppsFlyerObjectScript>();
                AF.devKey = DVAH_Data.AppFlyer_DevKey; 
              }
                AppsFlyerAdRevenue.start();
#endif

        }
    }
}

