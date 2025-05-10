
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions; 
using System.IO;
using UnityEditor; 
using UnityEngine.UI;
using System.Text;
using System.Globalization;

#if ADMOB_IMPLEMENT
using GoogleMobileAds.Api;
#endif

#if APPFLYER_IMPLEMENT
using AppsFlyerSDK;
#endif

#if FIREBASE_IMPLEMENT
using Firebase.Analytics;
using ConfigValue = Firebase.RemoteConfig.ConfigValue;
using ConfigInfo = Firebase.RemoteConfig.ConfigInfo;
using Firebase.RemoteConfig;
using Firebase.Extensions; 
#else
using ConfigValue = DVAH.ConfigValue;
using ConfigInfo = DVAH.ConfigInfo;

#endif

namespace DVAH
{ 
    public class FireBaseBridge : Singleton<FireBaseBridge>
    {
        private int _isFetchDone = 0;
        public bool isFetchDOne => _isFetchDone == 1;

        private static DVAH_Data _DVAH_Data;
        public static DVAH_Data DVAH_Data
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

        [SerializeField]
        private List<string> _keyConfigs = new List<string>();
        public List<string> keyConfigs => _keyConfigs;

        DateTime _timePlay = DateTime.Now; 

        long _timeStamp = 0;

        int _current_level = 0;
        public int CurrentLevel => _current_level;
        int _current_worldID = 0;
        private Canvas _topCanvas;
        private GameObject _infoPopUp;

        private Text _infoText;

        FirebaseRemoteConfig _configInfoFetch;

        public int CurrentWorldID => _current_worldID;

#if FIREBASE_IMPLEMENT
        protected override void Awake()
        {
            base.Awake();
            Debug.Log($"{CONSTANT.Prefix}==========><color=#00FF00>Firebase start Init!</color><==========");
            DontDestroyOnLoad(this);
            InitRemoteDataDefault();
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                    FetchDataAsync();


                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      CONSTANT.Prefix + $"==> Could not resolve all Firebase dependencies: {0} <==", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.


                }
            });
        }

        void InitRemoteDataDefault(){
            if(!PlayerPrefs.HasKey(CONSTANT.REMOTE_DEFAULT))
                return;
            var data = JSON.Parse(PlayerPrefs.GetString(CONSTANT.REMOTE_DEFAULT)).AsObject;
            
            foreach(var item in data.Dict){
                Debug.Log(CONSTANT.Prefix +$"==>Init Default Remote Config {item.Key} : {item.Value.AsString} ");
               
                if(DVAH_Data.FirebaseRemoteDatas.ContainsKey(item.Key)){ 
                    DVAH_Data.FirebaseRemoteDatas[item.Key] = item.Value.AsString; 
                    continue;
                }
                DVAH_Data.FirebaseRemoteDatas.Add(item.Key,item.Value.AsString); 
            }
        }
        // Start a fetch request.
        public Task FetchDataAsync()
        {
            Debug.Log("Fetching data...");

            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }

        void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log(CONSTANT.Prefix + $"==> Fetch canceled. <==");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log(CONSTANT.Prefix + $"==> Fetch encountered an error <==");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log(CONSTANT.Prefix + $"==> Fetch completed successfully! <==");
            }
            
            _configInfoFetch = FirebaseRemoteConfig.DefaultInstance;
             
             
            switch (_configInfoFetch.Info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    Task task = FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    task.ContinueWithOnMainThread(_result =>
                    { 
                        if (!_result.IsCompleted)
                            return;

                        Debug.Log(String.Format(CONSTANT.Prefix + "==> Remote data loaded and ready, total {1} keys (last fetch time {0} -  ThrottledEndTime {2}).<==",
                            _configInfoFetch.Info.FetchTime,FirebaseRemoteConfig.DefaultInstance.AllValues.Keys.Count, _configInfoFetch.Info.ThrottledEndTime ));
                             
                        var data = FirebaseRemoteConfig.DefaultInstance.AllValues;
                        string rawJSON = "{";
                        foreach(var item in data){
                            Debug.Log(CONSTANT.Prefix +$"==>Fetch {item.Key} : {item.Value.StringValue} ");
                            rawJSON += $"\"{item.Key}\":\"{item.Value.StringValue}\",";
                            if(DVAH_Data.FirebaseRemoteDatas.ContainsKey(item.Key)){ 
                                DVAH_Data.FirebaseRemoteDatas[item.Key] = item.Value.StringValue; 
                                continue;
                            }
                            DVAH_Data.FirebaseRemoteDatas.Add(item.Key,item.Value.StringValue); 
                        }
                        if(rawJSON.EndsWith(",")) rawJSON = rawJSON.Remove(rawJSON.Length-1);
                        rawJSON += "}";
                        PlayerPrefs.SetString(CONSTANT.REMOTE_DEFAULT,rawJSON);
                        System.Threading.Interlocked.Exchange(ref _isFetchDone, 1);

                        _keyConfigs = FirebaseRemoteConfig.DefaultInstance.AllValues.Keys.ToList();
                       
                    });

                    break;
                case LastFetchStatus.Failure:
                    switch (_configInfoFetch.Info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            Debug.Log(CONSTANT.Prefix + "==> Fetch failed for unknown reason <==");
                            break;
                        case FetchFailureReason.Throttled:
                            Debug.Log(CONSTANT.Prefix + "==> Fetch throttled until " + _configInfoFetch.Info.ThrottledEndTime + " <==");
                            break;
                    }
                    break;
                case LastFetchStatus.Pending:
                    Debug.Log(CONSTANT.Prefix + "==> Latest Fetch call still pending. <==");
                    break;
            }
        }
 
        public void OnInitDone(Action callback){
            _ =  OnInitDoneAsync(callback);
        }

        async Task OnInitDoneAsync(Action callback){
             
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 )
            { 
                await Task.Delay(100);
            } 
            UnityMainThread.wkr.AddJob(callback);
        }

        async Task GetValueRemote(string key, Action<HnnConfigValue> waitOnDone)
        {

            double countTime = 0;
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 && countTime < 180000f)
            {
                countTime += 1000;
                await Task.Delay(1000);
            }


            if (countTime >= 180000f)
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Fetch data {0} fail, becuz time out, using default! Check your network please!<==", key));
                 
            } 
            
            var obj = FireBaseBridge.GetValueRemote(key);
            UnityMainThread.wkr.AddJob(()=>{
                waitOnDone?.Invoke(obj);
            });
            
        }

        public async Task<HnnConfigValue> GetConfigValueRemote(string key)
        {

            double countTime = 0;
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 && countTime < 360000f)
            {
                countTime += 1000;
                await Task.Delay(1000);
            }

            if (countTime >= 360000f)
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Fetch data {0} fail, becuz time out! Check your network please!<==", key));
                return new HnnConfigValue();
            }

            if (!_keyConfigs.Contains(key))
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Remote dont have key {0} !<==", key));
                return new HnnConfigValue();
            }

            return FireBaseBridge.GetValueRemote(key);
        }

        /// <summary>
        /// Wait to get a value from Firebase remote config
        /// </summary>
        /// <param name="key">key name on Firebase remote</param>
        /// <param name="waitOnDone">callback when get data success</param> 
        public void GetValueRemoteAsync(string key, Action<HnnConfigValue> waitOnDone)
        {
            _ = GetValueRemote(key, waitOnDone);
        }

        public static HnnConfigValue GetValueRemote(string key){
            if(!DVAH_Data.FirebaseRemoteDatas.ContainsKey(key)){
                Debug.LogError(CONSTANT.Prefix+$"==> Some how error make data remote key {key} not exist!");
                return new HnnConfigValue("0");
            }
            return new HnnConfigValue(DVAH_Data.FirebaseRemoteDatas[key],key);
        }
        async Task LogEventWithParameter(string event_name, Hashtable hash)
        {
            Debug.Log(CONSTANT.Prefix + " ==> call LogEventWithParameter async " + event_name);
            double countTime = 0;
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 && countTime < 360000f)
            { 
                countTime += 1000;
                await Task.Delay(1000);
            }

            if (countTime >= 360000f)
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==> Logevent {0} fail, becuz time out! Check your network please!<==", event_name));
                 
            }

            Firebase.Analytics.Parameter[] parameter = new Firebase.Analytics.Parameter[hash.Count];
        
            //List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();
            if (hash != null && hash.Count > 0)
            {
                int i = 0; 
                foreach (DictionaryEntry item in hash)
                {
                    if (item.Equals((DictionaryEntry)default)) continue;
                    string key = this.Checker(item.Key.ToString());
                    string value = this.Checker(item.Value.ToString());

                    parameter[i] = (new Firebase.Analytics.Parameter(key, value));
                    Debug.Log(CONSTANT.Prefix + $"==> LogEvent " + event_name.ToString() + "- Key = " + key + " -  Value =" + value + " <==");
                    i++;
                }
               
                Firebase.Analytics.FirebaseAnalytics.LogEvent(
                           event_name,
                           parameter);
                return;
            }

            Debug.LogError(CONSTANT.Prefix + " ==> call LogEventWithParameter but hash null or count = 0 " + event_name);
        }

        async Task LogEventWithParameter(string event_name, Parameter[] parameter)
        {
            double countTime = 0;
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 && countTime < 360000f)
            {
                countTime += 1000;
                await Task.Delay(1000);
            }

            if (countTime >= 360000f)
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Logevent {0} fail, becuz time out! Check your network please!<==", event_name));
                
            } 
          
            if (parameter.Length > 0)
            { 
                UnityMainThread.wkr.AddJob(()=>{
                    Firebase.Analytics.FirebaseAnalytics.LogEvent(
                           event_name,
                           parameter);
                });

                return;
            }
            UnityMainThread.wkr.AddJob(()=>{
            Firebase.Analytics.FirebaseAnalytics.LogEvent(event_name);
            });
           
        }

        public void LogLevelStart(int levelID, int worldID = 0)
        {
            #if UNITY_EDITOR
            if(levelID <= 0){
                 EditorUtility.DisplayDialog("Attention Pleas?",
                       "Level must greater than ZERO, consider change it ASAP!!", "Ok");
            }
            #endif
            
            _timePlay = DateTime.Now;
            _timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _current_level = levelID;
            _current_worldID = worldID;
            try{
             Parameter[] LevelStartParameters = new  Parameter[] {
                new Parameter("level", levelID), 
                new Parameter("world_id", worldID),
                new Parameter("time_stamp", _timeStamp),
                new Parameter("play_mode", $"{worldID}_{levelID}"),
            }; 
            this.LogEventWithParameterAsync("level_start", LevelStartParameters); 
            }catch(Exception e){
                Debug.LogError($"{CONSTANT.Prefix} ==> Error on log start level!");
            }
        }

        public void LogLevelEnd(int levelID, LevelState isWin,int worldID = 0){
            #if UNITY_EDITOR
            if(levelID <= 0){
                 EditorUtility.DisplayDialog("Attention Pleas?",
                       "Level must greater than ZERO, consider change it ASAP!!", "Ok");
            }
            #endif
          
            
            try{
            Parameter[] LevelEndParameters = new Parameter[] { 
                new Parameter("level", levelID),  
                new Parameter("status", isWin.ToString()),
                new Parameter("world_id", worldID),
                new Parameter("time_play", (float)DateTime.Now.Subtract(_timePlay).TotalSeconds),
                new Parameter("time_stamp", _timeStamp),
                new Parameter("success", isWin == LevelState.win? "true":"false"),
                new Parameter("play_mode", $"{worldID}_{levelID}"),
                new Parameter("error", levelID == _current_level ? "none":$"{levelID}_not_start!")
            }; 
            this.LogEventWithParameterAsync("level_end", LevelEndParameters);

            _current_level = 0;
            _current_worldID = 0;
            }catch(Exception e){
                Debug.LogError($"{CONSTANT.Prefix} ==> Error on log end level!");
            }
        }
        
                // Using by Rocket Project
        public void LogLevelWin(int levelID, int worldID = 0){
#if UNITY_EDITOR
            if(levelID <= 0){
                EditorUtility.DisplayDialog("Attention Pleas?",
                                            "Level must greater than ZERO, consider change it ASAP!!", "Ok");
            }
#endif
            try{
                Parameter[] LevelWinParameters = new Parameter[] { 
                    new Parameter("level",      levelID),  
                    new Parameter("world_id",   worldID),
                    new Parameter("time_play",  (float)DateTime.Now.Subtract(_timePlay).TotalSeconds),
                    new Parameter("time_stamp", _timeStamp),
                    new Parameter("play_mode",  $"{worldID}_{levelID}"),
                    new Parameter("error",      levelID == _current_level ? "none":$"{levelID}_not_start!")
                }; 
                this.LogEventWithParameterAsync("level_win", LevelWinParameters);

                _current_level   = 0;
                _current_worldID = 0;
            }catch(Exception e){
                Debug.LogError($"{CONSTANT.Prefix} ==> Error on log end level!");
            }
        }

        // Using by Rocket Project
        public void LogLevelLose(int levelID, int worldID = 0){
#if UNITY_EDITOR
            if(levelID <= 0){
                EditorUtility.DisplayDialog("Attention Pleas?",
                                            "Level must greater than ZERO, consider change it ASAP!!", "Ok");
            }
#endif
            try{
                Parameter[] LevelLoseParameters = new Parameter[] { 
                    new Parameter("level",      levelID),  
                    new Parameter("world_id",   worldID),
                    new Parameter("time_play",  (float)DateTime.Now.Subtract(_timePlay).TotalSeconds),
                    new Parameter("time_stamp", _timeStamp),
                    new Parameter("play_mode",  $"{worldID}_{levelID}"),
                    new Parameter("error",      levelID == _current_level ? "none":$"{levelID}_not_start!")
                }; 
                this.LogEventWithParameterAsync("level_lose", LevelLoseParameters);

                _current_level   = 0;
                _current_worldID = 0;
            }catch(Exception e){
                Debug.LogError($"{CONSTANT.Prefix} ==> Error on log end level!");
            }
        }

#else
        protected override void Awake()
        {
            base.Awake();
            Debug.Log($"{CONSTANT.Prefix}==========><color=#00FF00>Firebase start Init!</color><==========");
            DontDestroyOnLoad(this.gameObject);
             
        }

       
        /// <summary>
        /// Wait to get a value from Firebase remote config
        /// </summary>
        /// <param name="key">key name on Firebase remote</param>
        /// <param name="waitOnDone">callback when get data success</param> 
        public void GetValueRemoteAsync(string key, Action<ConfigValue> waitOnDone)
        {
            if (!_keyConfigs.Contains(key))
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Remote dont have key {0} !<==", key));
                return;
            }
             
            waitOnDone?.Invoke(new ConfigValue());
        }

        async Task<ConfigValue> GetConfigValueRemote(string key)
        {

            double countTime = 0;
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 && countTime < 360000f)
            {
                countTime += 1000;
                await Task.Delay(1000);
            }

            if (countTime >= 360000f)
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Fetch data {0} fail, becuz time out! Check your network please!<==", key));
                return new ConfigValue();
            }

            if (!_keyConfigs.Contains(key))
            {
                Debug.LogError(string.Format(CONSTANT.Prefix + "==>Remote dont have key {0} !<==", key));
                return new ConfigValue();
            }

            return new ConfigValue();

        }

        public static HnnConfigValue GetValueRemote(string key){
            return new HnnConfigValue();
        }
 
        internal void LogLevelEnd(int v1, LevelState skip, int v2)
        {
            throw new NotImplementedException();
        }

         internal void LogLevelStart(int v1, int v2)
        {
            throw new NotImplementedException();
        }

#endif
 

        public string Checker(string str)
        {
            str = str.Replace(" ", "_");
            return Regex.Replace(str, "[^0-9A-Za-z_+-]", "");
        }

        public void LogEventWithOneParam(string eventName)
        {
            Debug.Log(CONSTANT.Prefix + $"==> LogEvent " + eventName + " <==");
            #if FIREBASE_IMPLEMENT
            _ = this.LogEventWithParameter(eventName, new Hashtable() { { "value", 1 } });
            #endif

        }

       
        /// <summary>
        /// Wait to log event to firebase analytics!
        /// </summary>
        /// <param name="event_name">name of event</param>
        /// <param name="hash">A hash table which contain value and parameter</param> 
        public void LogEventWithParameterAsync(string event_name, Hashtable hash)
        {
             Debug.Log(CONSTANT.Prefix + " ==> call LogEventWithParameter " + event_name);
             #if FIREBASE_IMPLEMENT
            _ = LogEventWithParameter(event_name, hash);
            #endif
        }

         async Task ShowDataDebugger()
        {
           
            double countTime = 0;
            while (System.Threading.Interlocked.Add(ref _isFetchDone, 0) == 0 && countTime < 360000f)
            { 
                countTime += 1000;
                UnityMainThread.wkr.AddJob(()=> 
                _infoText.text = $"<color=yellow>Fetching data! {(countTime)}millis</color>"
                );
                await Task.Delay(1000); 
            }
         
            UnityMainThread.wkr.AddJob(()=> {
                    _infoText.text = $"<color=green>App name: {_configInfoFetch.App.Name}</color>\n\n\n";
                    _infoText.text = $"<color=green>ThrottledEndTime: {_configInfoFetch.Info.ThrottledEndTime}</color>\n\n\n";
                    _infoText.text += $"<color=green>Facebook App ID:  {DVAH_Data.Facebook_AppID}</color>\n\n";
                    _infoText.text += $"<color=yellow>Firebase Keys:</color>\n";
                    foreach(var item in DVAH_Data.FirebaseRemoteDatas){ 
                        _infoText.text += $"<color=red>{item.Key}</color>:                       {item.Value}\n";
                    }
            });

            
        }
       
        public void ShowDataDebuggerAsync(){
            if(!FindCanvas())
                return;
            
            _topCanvas.gameObject.SetActive(true);
            _=ShowDataDebugger();
        }

        bool FindCanvas(){

            if(!_infoPopUp){
                GameObject g = Resources.Load<GameObject>("FirebasePopUp");
                if(!g){
                      Debug.LogError(CONSTANT.Prefix + " ==>FirebasePopUp missing  ");
                    return false;
                }
                _infoPopUp = Instantiate(g);
                
            } 

            if(!_infoText){
                _infoText = _infoPopUp.GetComponentInChildren<Text>();
            }

            if(_topCanvas) {
                _infoPopUp.transform.SetParent(_topCanvas.transform);
                _infoPopUp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _infoPopUp.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

                _infoPopUp.GetComponentInChildren<Button>().onClick.AddListener(()=>{_topCanvas.gameObject.SetActive(false);});
                return true;
            }

            _topCanvas = (new GameObject()).AddComponent<Canvas>();
            _topCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _topCanvas.sortingOrder = 10000;
            CanvasScaler scaler = _topCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080);
            _topCanvas.gameObject.AddComponent<GraphicRaycaster>();
             
            _infoPopUp.transform.SetParent(_topCanvas.transform);
            _infoPopUp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _infoPopUp.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
             _infoPopUp.GetComponentInChildren<Button>().onClick.AddListener(()=>{_topCanvas.gameObject.SetActive(false);});
            return true;
        }

#if FIREBASE_IMPLEMENT
         /// <summary>
        /// Wait to log event to firebase analytics!
        /// </summary>
        /// <param name="event_name">name of event</param>
        /// <param name="hash">A hash table which contain value and parameter</param> 
        public void LogEventWithParameterAsync(string event_name, Parameter[] hash)
        {
            _ = LogEventWithParameter(event_name, hash);
        }
#endif

    }

public enum LevelState{
    win,
    lose,
    skip
}

#if !FIREBASE_IMPLEMENT
    public struct ConfigValue
    {
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

        public IEnumerable<byte> ByteArrayValue => Data;

        public double DoubleValue => Convert.ToDouble(StringValue, CultureInfo.InvariantCulture);

        public long LongValue => Convert.ToInt64(StringValue, CultureInfo.InvariantCulture);

        public string StringValue => Encoding.UTF8.GetString(Data);

        internal byte[] Data { get; set; }

        public ValueSource Source { get; internal set; }

        public ConfigValue(byte[] data, ValueSource source)
        {
            this = default(ConfigValue);
            Data = data;
            Source = source;
        }
    }

    public enum ValueSource
    {
        StaticValue,
        RemoteValue,
        DefaultValue
    }

    public sealed class ConfigInfo
    {
        private DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public DateTime FetchTime { get; internal set; }

        public DateTime ThrottledEndTime { get; internal set; }

        public string LastFetchStatus { get; internal set; }

        public string LastFetchFailureReason { get; internal set; }
 
    }
#endif
}

#if UNITY_EDITOR
    [InitializeOnLoad]
    public class CheckFireBaseSDKImplement
    {

        static CheckFireBaseSDKImplement()
        {

            string[] symbolsList = new string[0];
#if UNITY_ANDROID
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, out symbolsList);

#elif UNITY_IOS
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.iOS, out symbolsList);
#endif

            string[] files = Directory.GetFiles(Application.dataPath, "Firebase.Analytics.dll", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length >= 1, "FIREBASE_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "AppsFlyer.asmdef", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length >= 1, "APPFLYER_IMPLEMENT", symbolsList.ToList());
    }

        static string[] checkSymbols(bool IsSdk, string nameSym, List<string> symBols)
        {
            if (!IsSdk && symBols.Contains(nameSym))
                symBols.Remove(nameSym);

            if (IsSdk && !symBols.Contains(nameSym))
                symBols.Add(nameSym);
#if UNITY_ANDROID
            //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symBols.ToArray());

#elif UNITY_IOS
           // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symBols.ToArray());
#endif
            return symBols.ToArray();
        }
    }
#endif