using System; 
using System.Collections.Generic;
using UnityEngine; 
using System.Globalization;
using System.Text.RegularExpressions;
// using Unity.VisualScripting;

namespace DVAH
{ 
    public class DVAH_Data : ScriptableObject
    {
        [Range(0,100)]
        public int NativeTrickRate = 0;
        [HideInInspector]
        public string LinkGoogleSheet           = "",
                      NameGoogleSheet           = "";

        [Header("                             App Flyer                          ")]
        public string AppFlyer_DevKey = "";
#if UNITY_IOS
        public string AppFlyer_AppId = "";
#endif

        [Header("                             FACEBOOK                          ")] 
        public string Facebook_AppID        = "";
        public string Facebook_ClientToken  = "";

        [Header("                             AD                          ")]
        
        public string ad_impression_key = "ad_impression";
        public bool IsMediationDebugger = false;
        public AD_NETWORK MEDIATION_NETWORK = AD_NETWORK.Base;

        public string AmazonAppID = "", AmazonBannerID = "", AmazonInterID = "", AmazonRewarID = "", AmazonMrecId = "";


        public string AppLovin_SDK_Key = "";
 
        public string IS_App_Key = "";

        public string Pangle_App_ID = "";
        public string Google_Android_AppID  = "",
                      Google_IOS_AppID      = ""; 

        public DictionAdUnit  adUnits = new DictionAdUnit() {
            {AD_TYPE.Aoa        ,new ListAdUnitData() },
            {AD_TYPE.Banner     ,new ListAdUnitData() },
            {AD_TYPE.Inter      ,new ListAdUnitData() },
            {AD_TYPE.Reward     ,new ListAdUnitData() },
            {AD_TYPE.Native     ,new ListAdUnitData() },
            {AD_TYPE.MRecs      ,new ListAdUnitData() },
            {AD_TYPE.Collapse   ,new ListAdUnitData() }, 
            {AD_TYPE.NativeOverlay   ,new ListAdUnitData() }, 
        }; 

        public Dictionary<AD_TYPE,bool> KeepTryReload = new  Dictionary<AD_TYPE, bool>(){
            {AD_TYPE.Aoa        ,true },
            {AD_TYPE.Banner     ,true },
            {AD_TYPE.Inter      ,true },
            {AD_TYPE.Reward     ,true },
            {AD_TYPE.Native     ,true },
            {AD_TYPE.MRecs      ,true },
            {AD_TYPE.Collapse   ,true }, 
            {AD_TYPE.NativeOverlay   ,true }, 
        };
        

        public BannerPosition BannerPosition = BannerPosition.CenterLeft,
                               MrecsPosition = BannerPosition.Centered,
                               CollapsePosition = BannerPosition.Centered;

        public bool BannerDefaultShow = false,
                    MrecsDefaultShow = false, 
                    IsAdptiveBanner = false;

        [Header("                             FIRE BASE                          ")]
        [SerializeField]
        public DictRemoteData FirebaseRemoteDatas = new DictRemoteData(); 

        [HideInInspector]
        public bool NO_ADS = false;
        [DrawIf("MEDIATION_NETWORK", false, ComparisonType.Equals, DisablingType.DontDraw)]
        public bool NoReward = false;

        [HideInInspector]
        public string Report = "";

        internal List<string> getAdUnitIds(AD_TYPE aD_TYPE, AD_NETWORK? adNetWork = null)
        {
            if (!adUnits.Contains(aD_TYPE))
                adUnits.Add(aD_TYPE, new ListAdUnitData());

            List<string> adUnitIds = new List<string>();
            foreach (var i in adUnits[aD_TYPE].AdUnitDatas)
            {
                if (adNetWork != null && i.network != adNetWork)
                    continue;

                adUnitIds.Add(i.UnitId);
            }
            return adUnitIds;
        }  
        
        internal AD_NETWORK getAdNetOfID(AD_TYPE adType, int ID)
        {
            if (!adUnits.Contains(adType))
                return AD_NETWORK.Base;

            return adUnits[adType].AdUnitDatas[ID].network;
        }

        internal AD_NETWORK getAdNetOfID(AD_TYPE adType, string ID)
        {
            if (!adUnits.Contains(adType))
                return AD_NETWORK.Base;
            foreach(var s in adUnits[adType].AdUnitDatas){
                if(ID != s.UnitId)
                    continue;

                return s.network;
            }
            return AD_NETWORK.Base;
        }
    }

    [Serializable]
    public class AdUnitData
    {
        public AD_NETWORK network;
        public string UnitId; 
        public UnityEngine.Object Prefab;
        public Vector2 Size;

        public Vector2 Position;
    }

    [Serializable]
    public class ListAdUnitData
    {
        public List<AdUnitData> AdUnitDatas = new List<AdUnitData>(); 
    } 
 
    [Serializable]
    public class DictionAdUnit: SerializableDictionary<AD_TYPE, ListAdUnitData> { }

    public enum AD_NETWORK
    {
        Base        = 0,
        Max         = 1,
        Admob       = 2,
        IronSource  = 3,
        Yandex      = 4,
        Pangle      = 5
    }

[Serializable]
    public enum AD_TYPE
    {
        Aoa         = 0,
        Banner      = 1,
        Inter       = 2,
        Reward      = 3,
        Native      = 4,
        NativeOverlay = 7,
        MRecs       = 5,
        Collapse    = 6, 
    }

    public enum AdUnitState {
        None,
        PreOpen,
        Open,
        Click,
        Watched,
        Closed,
        Interupt,
        Impression
    }

    public enum BannerPosition
    {
        TopLeft         = 0,
        TopCenter       = 1,
        TopRight        = 2,
        Centered        = 3,
        CenterLeft      = 4,
        CenterRight     = 5,
        BottomLeft      = 6,
        BottomCenter    = 7,
        BottomRight     = 8,
        Custom          = -1
    }
   
    [Serializable] 
    public class DictFirebaseRemote: SerializableDictionary<string, string> { }
    
    [Serializable]
    public struct HnnConfigValue{
        internal static Regex booleanTruePattern = new Regex("^(1|true|t|yes|y|on)$", RegexOptions.IgnoreCase);

        internal static Regex booleanFalsePattern = new Regex("^(0|false|f|no|n|off|)$", RegexOptions.IgnoreCase);

        public bool BooleanValue
        {
            get
            {
                string stringValue = StringValue;
                if (booleanTruePattern.IsMatch(stringValue))
                {
                    return true;
                }

                if (booleanFalsePattern.IsMatch(stringValue))
                {
                    return false;
                }

                throw new FormatException($"ConfigValue '{stringValue}' is not a boolean value");
            }
        }


        public double DoubleValue => Convert.ToDouble(StringValue, CultureInfo.InvariantCulture);

        public float FloatValue => (float)this.DoubleValue;

        public long LongValue => Convert.ToInt64(StringValue, CultureInfo.InvariantCulture);

        public string StringValue => _data;

        string _data;

        string _key;

        public string Name => _key;
        
        public HnnConfigValue(string data, string key = "" )
        { 
            _data = data; 
            _key = key;
        }
    }

    [Serializable]
    public class DictRemoteData: SerializableDictionary<string,string>{};
}

 