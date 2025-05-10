#if ADMOB_IMPLEMENT
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DVAH
{
    [Serializable]

    public class DictAdInitModule : SerializableDictionary<AD_NETWORK, Init_> { }
    [Serializable]
    public class DictUnitCallback : SerializableDictionary<AD_TYPE, VadtEventCallback> { }

    [Serializable]
    public class DictCappingTime : SerializableDictionary<AD_TYPE, Dictionary<int, DateTime?>> { }

    [Serializable]
    public class VadtEventCallback
    {
        public Dictionary<AdUnitEvent, Action<AD_NETWORK, object[]>> Event = new Dictionary<AdUnitEvent, Action<AD_NETWORK, object[]>>();
    }

    public enum AdUnitEvent
    {
        OnLoaded = 0,
        OnLoadFailed = 1,
        OnPreOpen = 9,
        OnShowed = 2,
        OnShowFailed = 3,
        OnClick = 4,
        OnClosed = 5,
        OnImpression = 6,
        OnPaid = 7,
        OnRewarded = 8,
        OnInterrupt = 10
    }

    public class AdBridge : Singleton<AdBridge>
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

        public DictUnitCallback Callbacks = new DictUnitCallback();

        [SerializeField]
        DictAdInitModule InitModules = new DictAdInitModule();

        [SerializeField]
        AdUnitModule[] adUnitModules = null;

        public static bool IsNoAds = false;

        public Canvas _topCanvas;
        GameObject _noAdText;

        int _devTapCount = 0;

        [SerializeField]
        DictCappingTime _cappingTimeCounts = new DictCappingTime();
        Dictionary<AD_TYPE, Dictionary<int, Func<float>>> _cappingTimes = new Dictionary<AD_TYPE, Dictionary<int, Func<float>>>();

        public Dictionary<AD_TYPE, Dictionary<int, Func<float>>> CappingTimes => _cappingTimes;

        bool _isComplateInitAllModule = false;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
            Debug.Log($"{CONSTANT.Prefix}==========><color=#00FF00>Adbridge start Init!</color><==========");
#if ADMOB_IMPLEMENT
            var GDPR = FindObjectsOfType<GDPRScript>();
            if (GDPR.Length == 0)
                this.gameObject.AddComponent<GDPRScript>();
#endif

            Debug.Log(CONSTANT.Prefix + $"Create Update Manager ==> {UpdateManager.Instant}");

            Debug.Log(CONSTANT.Prefix + $"Create main thread ==> {UnityMainThread.wkr}");
            FindAndInitAllSDK();

            TrackingDefault.Instant.Init();
        }

        public bool isInitDone()
        {
            foreach (var item in InitModules)
            {
                if (!item.Value.isInitDone())
                {
                    Debug.LogError($"{CONSTANT.Prefix}===> {item.Value.GetType()} still init ad unit!!!");
                    return false;
                }
            }

            return true;
        }

        void Update()
        {

            if (!DVAH_Data.IsMediationDebugger)
                return;

            if (Input.touchCount < 3)
            {
                if (_devTapCount != 0)
                    _devTapCount = 0;
                return;
            }
            UnityEngine.Debug.Log(CONSTANT.Prefix + $"===> 3 finger trigger {_devTapCount}--{Screen.width}--{Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position)}");

            if (Input.touchCount >= 4 && Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Ended)
            {
                _devTapCount++;
            }

            if (_devTapCount < 5)
                return;

            _devTapCount = 0;

            if (Input.touchCount > 4)
            {
                FireBaseBridge.Instant.ShowDataDebuggerAsync();
                return;
            }

            if (Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) < Screen.width / 2)
                InitModules[DVAH_Data.MEDIATION_NETWORK].ShowAdDebugger();
#if ADMOB_IMPLEMENT
            else
            {
                Debug.Log(CONSTANT.Prefix + $"===> Open admob Debugger");
                InitModules[AD_NETWORK.Admob].ShowAdDebugger();
            }
#endif

        }


        void FindAndInitAllSDK()
        {
            //init callback before create unit module
            foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE)))
            {
                VadtEventCallback vadtEventCallback = new VadtEventCallback();
                foreach (AdUnitEvent eventType in Enum.GetValues(typeof(AdUnitEvent)))
                {
                    vadtEventCallback.Event.Add(eventType, null);
                }
                Callbacks.Add(adType, vadtEventCallback);
            }

            //now start init module
            List<Init_> moduleInits = new List<Init_>();

            foreach (Type type in
                Assembly.GetAssembly(typeof(Init_)).GetTypes()
                .Where(myType => myType.IsClass &&
                                    !myType.IsAbstract &&
                                    myType.IsSubclassOf(typeof(Init_))))
            {
                this.gameObject.AddComponent(type);
            }

            Init_[] moduleSDKComponents = this.GetComponents<Init_>();
            foreach (Init_ I in moduleSDKComponents)
            {
                try
                {
                    I.InitSDK(DVAH_Data, Callbacks);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                InitModules.Add(I.adNetWork, I);
            }

            _isComplateInitAllModule = true;
        }

        public Init_ GetModule(AD_NETWORK ad_network)
        {
            if (!_isComplateInitAllModule)
            {
                Debug.LogError($"{CONSTANT.Prefix}===> Adbridge still initing! You call something tooo soon");
                return InitModules[AD_NETWORK.Base];
            }

            if (!this.isInitDone())
            {
                return InitModules[AD_NETWORK.Base];
            }

            if (!InitModules.ContainsKey(ad_network))
                return InitModules[AD_NETWORK.Base];
            return InitModules[ad_network];
        }

        public Init_ GetModule(AD_TYPE adType, int ID)
        {
            AD_NETWORK adNet;
            if (!this.DVAH_Data.adUnits.Contains(adType))
            {
                adNet = AD_NETWORK.Base;
                return this.GetModule(adNet);
            }

            if (this.DVAH_Data.adUnits[adType].AdUnitDatas.Count <= ID)
            {
                throw new Exception($"your {adType} only has {this.DVAH_Data.adUnits[adType].AdUnitDatas.Count} IDs! \n" +
                    $"what do u mean: \"Show {adType} ID number {ID}\"?");
            }

            adNet = this.DVAH_Data.adUnits[adType].AdUnitDatas[ID].network;
            return this.GetModule(adNet);
        }

        public bool FindCanvas()
        {
            if (!_noAdText)
            {
                GameObject g = Resources.Load<GameObject>("AD/NoAd");
                if (!g)
                    return false;

                _noAdText = Instantiate(g);
                _noAdText.SetActive(false);
            }

            if (_topCanvas)
            {
                _noAdText.transform.SetParent(_topCanvas.transform);
                return true;
            }

            _topCanvas = (new GameObject()).AddComponent<Canvas>();
            _topCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _topCanvas.sortingOrder = 10000;
            CanvasScaler scaler = _topCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _topCanvas.gameObject.AddComponent<GraphicRaycaster>();

            _noAdText.transform.SetParent(_topCanvas.transform);
            return true;
        }

        public AdUnitModule IsAnyUnitShowing(AD_TYPE currentAdType)
        {
            if (currentAdType == AD_TYPE.MRecs || currentAdType == AD_TYPE.Banner)
                return null;
            foreach (var item in InitModules)
            {
                foreach (var module in item.Value.AdUnitModule)
                {
                    if (!module.Value.IsUnitShowed)
                        continue;
                    Debug.Log($"{CONSTANT.Prefix}==> {module.Value.adFomart} is showing now!");
                    return module.Value;
                }
            }
            return null;
        }


        #region Controler AD
        /// <summary>
        /// Show AD with ID and callback
        /// <br>AdUnitState:</br>
        ///         <br>- None (không có ad, chưa đủ capping time, đang có ad khác show) </br>
        ///         <br>- PreOpen (trước khi gọi show ad) </br>
        ///         <br>- Open (khi ad show ra) </br>
        ///         <br>- Click </br>
        ///         <br>- Watched (khi user xem đủ thời lượng) </br>
        ///         <br>- Closed (khi user tắt ad ) </br>
        ///         <br>- Interupt (khi ad show error ) </br>
        /// </summary>
        /// <example>
        ///       /// example: 
        /// AdBridge.Instant.ShowAd(adType: AD_TYPE.Reward);
        /// </example>        /// <param name="adType"></param>
        /// <param name="ID"></param>
        /// <param name="callback"></param>
        public void ShowAd(AD_TYPE adType, int ID = 0, Action<int, AdUnitState> callback = null, bool isShowNoAd = false, string placement = null, bool ignoreCapping = false, bool allowDuplicate = false)
        {
            bool checkNoAds()
            {
                if (!DVAH_Data.NO_ADS)
                    return false;
                if (!DVAH_Data.NoReward && adType == AD_TYPE.Reward)
                    return false;
                return true;
            }

            if (checkNoAds())
            {
                void CallInvoke(int id, params AdUnitState[] adUnitStates)
                {
                    Debug.Log(CONSTANT.Prefix + $"==> Cheat ad {adType}!");
                    foreach (var adUnitState in adUnitStates)
                    {
                        UnityMainThread.wkr.AddJob(() => callback?.Invoke(id, adUnitState));
                    }
                }

                CallInvoke(ID, AdUnitState.Open, AdUnitState.Closed, AdUnitState.Watched);
                return;
            }

            Debug.Log(CONSTANT.Prefix + $"==> AdBridge Call Show Ad {adType}!");
            float timeRemain = 0;
            Init_ moduleNetWork = this.GetModule(adType, ID);

            if (!ignoreCapping && !this.checkCappingTime(out timeRemain, adType))
            {
                Debug.Log(CONSTANT.Prefix + $"==> {adType} still capping time in {timeRemain} seconds!");
                UnityMainThread.wkr.AddJob(() => callback?.Invoke(ID, AdUnitState.None));
                moduleNetWork.EventCallback(adType, AdUnitEvent.OnShowFailed, ID, "Capping_time", placement);
                return;
            }

            if (adType != AD_TYPE.Banner && !moduleNetWork.IsAdLoaded(adType, ID))
            {
                if (isShowNoAd && this.FindCanvas())
                {
                    _noAdText.SetActive(true);
                }
                Debug.Log(CONSTANT.Prefix + $"==> Ad {adType}_{moduleNetWork.adNetWork} not loaded yet !");
                UnityMainThread.wkr.AddJob(() => callback?.Invoke(ID, AdUnitState.None));
                moduleNetWork.EventCallback(adType, AdUnitEvent.OnShowFailed, ID, "AD_not_ready", placement);
                return;
            }

            AdUnitModule moduleShowing = IsAnyUnitShowing(adType);
            if (!allowDuplicate && moduleShowing != null)
            {
                Debug.Log(CONSTANT.Prefix + $"==> Other ad {moduleShowing.adFomart} showing, skip this {adType} !");
                UnityMainThread.wkr.AddJob(() => callback?.Invoke(ID, AdUnitState.None));
                moduleNetWork.EventCallback(adType, AdUnitEvent.OnShowFailed, ID, "Another_AD_showing", placement);
                return;
            }

            if (!ignoreCapping)
            {
                if (adType == AD_TYPE.Inter || adType == AD_TYPE.Reward || adType == AD_TYPE.Aoa)
                {
                    callback += (id, state) =>
                    {
                        if (state == AdUnitState.Closed)
                            this.ResetCappingTime(AD_TYPE.Inter, AD_TYPE.Aoa, AD_TYPE.Reward);
                    };
                }
                else
                {
                    ResetCappingTime(ID, adType);
                }
            }


            try
            {
                callback?.Invoke(ID, AdUnitState.PreOpen);
            }
            catch (Exception e)
            {
                Debug.LogError("Error on invoke PreOpen: " + e);
            }


            moduleNetWork.ShowAsync(adType, callback, ID, placement: placement);
        }
#if ADMOB_IMPLEMENT
        public void ShowAd(Transform parent, int ID = 0, string placement = "unknow", float MinX = 0, float MinY = 0, float MaxX = 1, float MaxY = 1)
        {
            DictNativeObject tmp = this.getNativeObject();

            if (tmp == null)
                return;
            if (!tmp.ContainsKey(ID))
                return;
            if (tmp[ID] == null)
                return;

            tmp[ID].transform.SetParent(parent);
            this.ShowAd(AD_TYPE.Native, ID, placement: placement, callback: (id, state) =>
            {
                if (state == AdUnitState.Open)
                    tmp[id].Show(id, MinX, MinY, MaxX, MaxY);
                if (state == AdUnitState.None || state == AdUnitState.Interupt)
                    this.HideAd(AD_TYPE.Native, id);
            }, ignoreCapping: true, allowDuplicate: true);

        }
#else
            public void ShowAd(Transform parent,int ID = 0,GameObject prefab = null,float MinX = 1, float MinY = 1, float MaxX = 1, float MaxY = 1){
                throw new NotImplementedException();
            }
#endif


        /// <summary>
        /// Hide AD with ID
        /// <code>
        /// example: 
        /// _adManager.HideAd(AD_TYPE.Banner, ID);
        /// </code>
        /// </summary>
        /// <param name="adType"></param>
        /// <param name="ID"></param>
        /// <param name="callback"></param>
        public void HideAd(AD_TYPE adType, int ID = 0)
        {
            Init_ moduleNetWork = this.GetModule(adType, ID);
            if (!moduleNetWork.isInitDone())
            {
                Debug.LogWarning($"Module {moduleNetWork.adNetWork} not init done yet! can't hide {adType} - ID {ID}." +
                "If this not affect your logic then all be fine! Ignore this!");
                return;
            }
            moduleNetWork.Hide(adType, ID);
        }

        /// <summary>
        /// Hide AD with ID
        /// <code>
        /// example: 
        /// _adManager.HideAd(AD_TYPE.Banner, ID);
        /// </code>
        /// </summary>
        /// <param name="adType"></param>
        /// <param name="ID"></param>
        /// <param name="callback"></param>
        public void LoadAd(AD_TYPE adType, int ID = 0)
        {
            Init_ moduleNetWork = this.GetModule(adType, ID);
            moduleNetWork.Load(adType, ID);
        }

        /// <summary>
        /// Check is AD ID loaded
        /// <code>
        /// example:  
        /// return _adManager.IsAdLoaded(AD_TYPE.Reward, ID);
        /// </code>
        /// </summary>
        /// <param name="adType"></param>
        /// <param name="ID"></param>
        /// <param name="callback"></param>
        public bool IsAdLoaded(AD_TYPE adType, int ID = 0)
        {
            Init_ moduleNetWork = this.GetModule(adType, ID);
            Debug.Log(CONSTANT.Prefix + "==>check ad loaded " + moduleNetWork.adNetWork);
            return moduleNetWork.IsAdLoaded(adType, ID);
        }
        #endregion

        public void SetCappingTime(Func<float> time, AD_TYPE adType = AD_TYPE.Inter, bool isShowStart = false, int ID = 0)
        {
            if (!this._cappingTimeCounts.ContainsKey(adType))
            {
                var tmp = new Dictionary<int, DateTime?>();
                tmp.Add(ID, DateTime.Now);

                this._cappingTimeCounts.Add(adType, tmp);

                var tmp2 = new Dictionary<int, Func<float>>();
                tmp2.Add(ID, time);
                this._cappingTimes.Add(adType, tmp2);
            }

            if (!this._cappingTimeCounts[adType].ContainsKey(ID))
            {
                this._cappingTimeCounts[adType].Add(ID, DateTime.Now);
            }

            this._cappingTimeCounts[adType][ID] = isShowStart ? null : DateTime.Now;

            if (!this._cappingTimes[adType].ContainsKey(ID))
            {
                this._cappingTimes[adType].Add(ID, time);
            }
            this._cappingTimes[adType][ID] = time;
        }

        public void ResetCappingTime(params AD_TYPE[] adTypes)
        {

            foreach (var item in adTypes)
            {
                this.ResetCappingTime(item);
            }

        }

        public void ResetCappingTime(int ID, AD_TYPE adType)
        {
            if (!this._cappingTimeCounts.ContainsKey((AD_TYPE)adType))
            {
                return;
            }

            if (!this._cappingTimeCounts[(AD_TYPE)adType].ContainsKey(ID))
            {
                return;
            }
            this._cappingTimeCounts[(AD_TYPE)adType][ID] = DateTime.Now;
        }

        public void ResetCappingTime(AD_TYPE adType)
        {
            if (!this._cappingTimeCounts.ContainsKey((AD_TYPE)adType))
            {
                return;
            }

            var cappingCounts = new List<int>(this._cappingTimeCounts[(AD_TYPE)adType].Keys);
            foreach (var item in cappingCounts)
            {
                this._cappingTimeCounts[(AD_TYPE)adType][item] = DateTime.Now;
            }

        }


        /// <summary>
        /// Check capping time of ad type
        /// </summary>
        /// <param name="adType"></param>
        /// <returns>True if you can show ad, False if not</returns>
        public bool checkCappingTime(out float timeRemain, AD_TYPE adType = AD_TYPE.Inter, int ID = 0)
        {

            timeRemain = 0;
            if (adType == AD_TYPE.Banner || adType == AD_TYPE.MRecs)
                return true;

            if (!this._cappingTimeCounts.ContainsKey(adType))
                return true;
            if (!this._cappingTimes.ContainsKey(adType))
                return true;

            if (this._cappingTimeCounts[adType] == null)
            {
                return true;
            }

            if (!this._cappingTimeCounts[adType].ContainsKey(ID))
            {
                return true;
            }

            if (this._cappingTimeCounts[adType][ID] == null)
                return true;
            float countTime = (float)DateTime.Now.Subtract(this._cappingTimeCounts[adType][ID].Value).TotalSeconds;
            timeRemain = this._cappingTimes[adType][ID].Invoke() - countTime;
            Debug.Log(CONSTANT.Prefix + $"==> Check capping: time remain: {timeRemain} -- count time: {countTime} ");
            if (timeRemain > 0)
            {

                return false;
            }

            return true;
        }

        /// <summary>
        /// Check capping time of ad type
        /// </summary>
        /// <param name="adType"></param>
        /// <returns>True if you can show ad, False if not</returns>
        public bool checkCappingTime(AD_TYPE adType = AD_TYPE.Inter)
        {
            return this.checkCappingTime(out float timeRemain, adType);
        }
#if ADMOB_IMPLEMENT
        public DictNativeObject getNativeObject()
        {
            if (!InitModules.ContainsKey(AD_NETWORK.Admob))
                return null;

            var module = this.GetModule(AD_NETWORK.Admob).AdUnitModule;
            if (!module.ContainsKey(AD_TYPE.Native))
                return null;
            return ((AdUnitModuleNative_Admob)module[AD_TYPE.Native]).NativeObjects;
        }
#endif

        public void SetNoAds(bool IsNoAds)
        {
            DVAH_Data.NO_ADS = IsNoAds;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
                this.ResetCappingTime(0, AD_TYPE.Aoa);
            try
            {
                var appStateObject = FindObjectsOfType<MonoBehaviour>().OfType<IAppStateChange>();
                foreach (IAppStateChange singleObject in appStateObject)
                {
                    if ((singleObject as MonoBehaviour).gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
                        continue;
                    singleObject.OnAppStateChanged(focus ? AppState.Foreground : AppState.Background);
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void SetReload(bool isReload, params AD_TYPE[] aD_TYPEs)
        {
            foreach (var item in aD_TYPEs)
            {
                if (!DVAH_Data.KeepTryReload.ContainsKey(item))
                    continue;
                DVAH_Data.KeepTryReload[item] = isReload;
            }

        }

        public void AddCallback(AD_TYPE aD_TYPE, AdUnitEvent adUnitState, Action<AD_NETWORK, object[]> action)
        {
            _ = AddCallbackAsync(aD_TYPE, adUnitState, action);
        }

        async Task AddCallbackAsync(AD_TYPE aD_TYPE, AdUnitEvent adUnitState, Action<AD_NETWORK, object[]> action)
        {
            while (!this.Callbacks.ContainsKey(aD_TYPE))
            {
                await Task.Delay(100);
            }

            while (!this.Callbacks[aD_TYPE].Event.ContainsKey(adUnitState))
            {
                await Task.Delay(100);
            }

            this.Callbacks[aD_TYPE].Event[adUnitState] += action;
        }

        private void OnDisable()
        {
            Debug.Log($"{CONSTANT.Prefix}==========><color=#Ff0000>Adbridge Disable!</color><==========");
        }

        private void OnDestroy()
        {
            Debug.Log($"{CONSTANT.Prefix}==========><color=#Ff0000>Adbridge destroy!</color><==========");
        }

    }
}


