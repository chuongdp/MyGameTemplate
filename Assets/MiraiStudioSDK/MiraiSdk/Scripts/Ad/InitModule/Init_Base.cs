using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

namespace DVAH
{
    public class Init_Base : Init_
    {
        public override Init_ InitSDK(DVAH_Data dvah_data, DictUnitCallback callbacks)
        { 
            this._adNetWork = AD_NETWORK.Base;
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
            throw new NotImplementedException();
        }

        protected override void checkUnitInit(AD_TYPE AD_TYPE)
        {
            _isInitDone = false;
            Dictionary<AD_NETWORK, List<string>> result = MapNetVsUnits(AD_TYPE);
            foreach (KeyValuePair<AD_NETWORK,List<string>> pair in result)
            {
                if (_adUnitModules.ContainsKey(AD_TYPE))
                    continue;
                string classN = string.Format(CONSTANT.AdUnitClassName, AD_TYPE, pair.Key.ToString());
                Type unitClass = null;
                try
                {
                    unitClass = Type.GetType(classN, true, true);
                }catch (Exception e)
                {

                }
                if(unitClass == null || pair.Key == AD_NETWORK.Base)
                {
                    classN = string.Format(CONSTANT.AdUnitClassName, AD_TYPE, AD_NETWORK.Base);
                    unitClass = Type.GetType(classN, true, true);

                    GameObject g = new GameObject($"{AD_TYPE}Module");
                    g.transform.SetParent(this.transform);
                    g.AddComponent(unitClass);

                   
                    _adUnitModules.Add(AD_TYPE, g.GetComponent<AdUnitModule>().Init(this, AD_TYPE, pair.Value));
                }

            }
            _isInitDone = true;
        }

        Dictionary<AD_NETWORK,List<string>> MapNetVsUnits(AD_TYPE AD_TYPE) 
        {
            Dictionary<AD_NETWORK, List<string>> result = new Dictionary<AD_NETWORK, List<string>>();
            List<AdUnitData> listAdUnitData = DVAH_Data.adUnits[AD_TYPE].AdUnitDatas;

            foreach (AdUnitData adUnit in listAdUnitData)
            {
                if (!result.ContainsKey(adUnit.network))
                    result.Add(adUnit.network, new List<string>());

                result[adUnit.network].Add(adUnit.UnitId);
            } 
            return result;
        } 
    }


     
}

