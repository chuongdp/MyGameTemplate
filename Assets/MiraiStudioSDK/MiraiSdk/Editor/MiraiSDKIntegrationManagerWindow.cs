/* * * * *
 * A simple helper for APERO checklist
 * ------------------------------
 * Written by Huynn7996
 * 2022-09-07
 *
 *
 * The MIT License (MIT)
 *
 * Copyright (c) Huy Nguyen Nhat (Huynn7996)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * * * * */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEditor.Compilation;
using DVAH.Lib;
#if ADMOB_IMPLEMENT
using GoogleMobileAds.Editor;
#endif

#if FIREBASE_IMPLEMENT
using Firebase.RemoteConfig;
using Firebase.Extensions;
#endif

#if FACEBOOK_IMPLEMENT
using Facebook.Unity.Settings;
#endif

namespace DVAH
{
    internal class MiraiSDKIntegrationManagerWindow : EditorWindow
    {
        private        Vector2      scrollPos;
        private static EditorWindow wnd;

        private GUIStyle TextRedStyles,
            TextGreenStyles,
            ButtonTextGreenStyles;

        private DVAH_Data DVAH_Data;

        private bool isShowKeyStorePass,
            isShowAliasPass;

        private Dictionary<AD_TYPE, int> numberAdUnitId = new()
        {
            { AD_TYPE.Aoa, 0 },
            { AD_TYPE.Banner, 0 },
            { AD_TYPE.Inter, 0 },
            { AD_TYPE.Reward, 0 },
            { AD_TYPE.Native, 0 },
            { AD_TYPE.NativeOverlay, 0 },
            { AD_TYPE.MRecs, 0 },
            { AD_TYPE.Collapse, 0 }
        };

#if MAX_IMPLEMENT
        private AppLovinSettings max = null;
#endif

#if ADMOB_IMPLEMENT
        private GoogleMobileAdsSettings gg = null;
#endif

#if FACEBOOK_IMPLEMENT
        FacebookSettings facebook;
#endif

        private        bool   isDownloadingPack = false;
        private static bool   isMenuExpand      = false;
        private        string newItemFirebaseRemoteKey, newItemFirebaseRemoteValue;

        [MenuItem("MIRAI/MiraiSDK Integration Manager", priority = 0)]
        public static void InitWindowEditor()
        {
            isMenuExpand = SessionState.GetBool("MenuFirebaseExpand", true);
            // This method is called when the user selects the menu item in the Editor
            wnd              = GetWindow<MiraiSDKIntegrationManagerWindow>();
            wnd.titleContent = new GUIContent("Mirai SDK version - Huynn 3rdLib!");

            //string[] symbolsList;
            //PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, out symbolsList);
            //if (symbolsList.ToList().Contains("NATIVE_AD"))
            //    usingAdNative = true;
            //if (symbolsList.ToList().Contains("IAP"))
            //    usingIAP = true;

            //createNewBranch = EditorPrefs.GetBool("NEW_BRANCH", true);
        }

        private void OnGUI()
        {
            this.ColorDefine();
            this.Init();

            EditorGUILayout.LabelField("Google Sheet:", this.TextGreenStyles);
            this.DVAH_Data.LinkGoogleSheet = EditorGUILayout.TextField("Link", this.DVAH_Data.LinkGoogleSheet);
            EditorGUILayout.BeginHorizontal();
            this.DVAH_Data.NameGoogleSheet = EditorGUILayout.TextField("Name Sheet", this.DVAH_Data.NameGoogleSheet);
            if (GUILayout.Button("Reload", this.ButtonTextGreenStyles)) this.getData();
            EditorGUILayout.EndHorizontal();

            MiraiCoroutineEditor downloadProgress;
            if (!this.isDownloadingPack)
            {
                if (GUILayout.Button("Import SDK"))
                {
                    this.isDownloadingPack = true;
                    downloadProgress = MiraiCoroutineEditor.StartCoroutine(this.DownloadPlugin("BaseSDK", CONSTANT.SdkUrl, () =>
                    {
                        this.isDownloadingPack = false;
                        AddSymbol();
                    }));
                }
            }
            else
            {
                EditorGUILayout.LabelField("====>Downloading....", this.TextGreenStyles);
            }

            EditorGUILayout.BeginVertical();
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(wnd.position.width), GUILayout.Height(wnd.position.height - 100));

            this.UIEditor();
            this.AppFlyer();
            this.FirebaseEditor();
            this.AdEditor();

            this.FaceBookEditor();

            this.UIBottom();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void UIEditor()
        {
            #region EDITOR

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Build Version:", this.TextGreenStyles);

            PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("Version Code", PlayerSettings.Android.bundleVersionCode);

            EditorGUILayout.BeginHorizontal();
            PlayerSettings.companyName = EditorGUILayout.TextField("Company Name", PlayerSettings.companyName);
            PlayerSettings.productName = EditorGUILayout.TextField("Product Name", PlayerSettings.productName);
            EditorGUILayout.EndHorizontal();

            PlayerSettings.bundleVersion = EditorGUILayout.TextField("App Version", PlayerSettings.bundleVersion);

            EditorGUILayout.BeginHorizontal();
            var applicationIdentifier = EditorGUILayout.TextField("Package Name", PlayerSettings.applicationIdentifier);

            if (!PlayerSettings.applicationIdentifier.StartsWith("com.") || PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
                EditorGUILayout.LabelField("Package name should in form \"com.X.Y\" other can cost a build error!", this.TextRedStyles);

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
            EditorGUILayout.EndHorizontal();

#if UNITY_ANDROID
            EditorGUILayout.BeginHorizontal();
            PlayerSettings.Android.useCustomKeystore = EditorGUILayout.Toggle("Custom KeyStore", PlayerSettings.Android.useCustomKeystore);

            string[] symbols;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out symbols);
            var tmpSymbols = symbols.ToList();
            //if (usingAdNative)
            //{
            //    if (!tmpSymbols.Contains("NATIVE_AD"))
            //    {
            //        tmpSymbols.Add("NATIVE_AD");
            //        symbols = tmpSymbols.ToArray();
            //    }
            //}
            //else
            //{
            //    if (symbols.Contains("NATIVE_AD"))
            //    {

            //        tmpSymbols.Remove("NATIVE_AD");
            //        symbols = tmpSymbols.ToArray();

            //    }
            //}

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbols);
            EditorGUILayout.EndHorizontal();

            var keyStorePath = "";
            if (PlayerSettings.Android.useCustomKeystore)
            {
                if (EditorGUILayout.LinkButton("KeyStore Path:                      " + keyStorePath))
                {
                    var path                                                  = EditorUtility.OpenFilePanel("Select keystore file", "", "keystore");
                    if (path.Length != 0) PlayerSettings.Android.keystoreName = path;
                }

                keyStorePath = PlayerSettings.Android.keystoreName;
                this.KeyStoreInfo();
            }
            else
            {
                keyStorePath = "                    Debug keystore!!!";
                EditorGUILayout.LabelField("KeyStore Path:                      Debug keystore!!!");
            }
#endif

            #endregion
        }

        private void AppFlyer()
        {
#if APPFLYER_IMPLEMENT
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("AppFlyer:", TextGreenStyles);
            DVAH_Data.AppFlyer_DevKey = EditorGUILayout.TextField("Dev key", DVAH_Data.AppFlyer_DevKey);
#endif
        }

        private void FirebaseEditor()
        {
            var btnWidth = 30;
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Firebase:", this.TextGreenStyles);
            if (GUILayout.Button("Download Firebase X86", GUILayout.Width(170))) MiraiCoroutineEditor.StartCoroutine(this.DownloadPlugin("FBX86", CONSTANT.FirebaseX86Url));

            if (GUILayout.Button("FetchData", this.ButtonTextGreenStyles)) this.fetchDataFirebase();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Check google-services.json")) MenuEditor.CheckFirebaseJson();
            if (GUILayout.Button("Check google-services.xml")) MenuEditor.FixGoogleXml();

            void createEnum()
            {
                var data                                                      = "using DVAH;\n public class FirebaseKeys{\n";
                foreach (var item in this.DVAH_Data.FirebaseRemoteDatas) data += $"public static HnnConfigValue {item.Key} =>  FireBaseBridge.GetValueRemote(\"{item.Key}\");\n";
                data += "}";
                try
                {
                    if (File.Exists(Environment.CurrentDirectory + "/Assets/Scripts/FirebaseKeys.cs"))
                        File.WriteAllText(Environment.CurrentDirectory + "/Assets/Scripts/FirebaseKeys.cs", "");
                    var stream = new FileStream(Environment.CurrentDirectory + "/Assets/Scripts/FirebaseKeys.cs", FileMode.OpenOrCreate);

                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(data);
                        writer.Flush();
                        writer.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                EditorUtility.SetDirty(this.DVAH_Data);
                AssetDatabase.SaveAssets();
                CompilationPipeline.RequestScriptCompilation();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            this.newItemFirebaseRemoteKey   = EditorGUILayout.TextField("New Remote Config Key:", this.newItemFirebaseRemoteKey);
            this.newItemFirebaseRemoteValue = EditorGUILayout.TextField("New Remote Config Key:", this.newItemFirebaseRemoteValue);
            if (GUILayout.Button("Add Remote Config Data", GUILayout.Width(btnWidth)))
            {
                this.DVAH_Data.FirebaseRemoteDatas.Add(this.newItemFirebaseRemoteKey, this.newItemFirebaseRemoteValue);
                this.newItemFirebaseRemoteKey   = "";
                this.newItemFirebaseRemoteValue = "";
                createEnum();
            }

            EditorGUILayout.EndHorizontal();

            var expandBtn = isMenuExpand ? "^" : "+";
            if (GUILayout.Button(expandBtn, GUILayout.Width(btnWidth)))
            {
                isMenuExpand = !isMenuExpand;
                SessionState.SetBool("MenuFirebaseExpand", isMenuExpand);
            }

            if (isMenuExpand)
                foreach (var item in this.DVAH_Data.FirebaseRemoteDatas.Keys.ToArray())
                {
                    EditorGUILayout.BeginHorizontal();
                    var text = EditorGUILayout.TextField(item, this.DVAH_Data.FirebaseRemoteDatas[item]);
                    if (this.DVAH_Data.FirebaseRemoteDatas != null && text != this.DVAH_Data.FirebaseRemoteDatas[item]) this.DVAH_Data.FirebaseRemoteDatas[item] = text;

                    EditorGUILayout.EndHorizontal();
                }
        }

        private void fetchDataFirebase()
        {
#if FIREBASE_IMPLEMENT
            void Awake()
            {
                Debug.Log($"{CONSTANT.Prefix}==========><color=#00FF00>Firebase start Fetch!</color><==========");

                Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        // Create and hold a reference to your FirebaseApp,
                        // where app is a Firebase.FirebaseApp property of your application class.
                        var app = Firebase.FirebaseApp.DefaultInstance;
                        FetchDataAsync();

                        // Set a flag here to indicate whether Firebase is ready to use by your app.
                    }
                    else
                    {
                        Debug.LogError(string.Format(
                            CONSTANT.Prefix + $"==> Could not resolve all Firebase dependencies: {0} <==", dependencyStatus));
                        // Firebase Unity SDK is not safe to use here.
                    }
                });
            }

            // Start a fetch request.
            void FetchDataAsync()
            {
                Debug.Log("Fetching data...");

                var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                    TimeSpan.Zero);
                fetchTask.ContinueWithOnMainThread(FetchComplete);
            }

            void FetchComplete(Task fetchTask)
            {
                if (fetchTask.IsCanceled)
                    Debug.Log(CONSTANT.Prefix + $"==> Fetch canceled. <==");
                else if (fetchTask.IsFaulted)
                    Debug.Log(CONSTANT.Prefix + $"==> Fetch encountered an error <==");
                else if (fetchTask.IsCompleted) Debug.Log(CONSTANT.Prefix + $"==> Fetch completed successfully! Fetch status {FirebaseRemoteConfig.DefaultInstance.Info.LastFetchStatus} <==");

                var info = FirebaseRemoteConfig.DefaultInstance.Info;
                switch (info.LastFetchStatus)
                {
                    case LastFetchStatus.Success:
                        Task task = FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                        task.ContinueWithOnMainThread(_result =>
                        {
                            if (!_result.IsCompleted)
                                return;

                            var data = FirebaseRemoteConfig.DefaultInstance.AllValues;
                            this.DVAH_Data.FirebaseRemoteDatas.Clear();
                            foreach (var item in data) this.DVAH_Data.FirebaseRemoteDatas.Add(item.Key, item.Value.StringValue);
                            createEnum();
                            Debug.Log(string.Format(CONSTANT.Prefix + "==> Remote data loaded and ready (last fetch time {0}).<==",
                                info.FetchTime));
                            EditorUtility.DisplayDialog("Firebase Remote config", $"Fetch data done after {info.FetchTime}s! --> {info.ThrottledEndTime}", "Ok!", "Cancel");
                        });

                        break;
                    case LastFetchStatus.Failure:
                        switch (info.LastFetchFailureReason)
                        {
                            case FetchFailureReason.Error:
                                Debug.Log(CONSTANT.Prefix + "==> Fetch failed for unknown reason <==");

                                break;
                            case FetchFailureReason.Throttled:
                                Debug.Log(CONSTANT.Prefix + "==> Fetch throttled until " + info.ThrottledEndTime + " <==");

                                break;
                        }

                        break;
                    case LastFetchStatus.Pending:
                        Debug.Log(CONSTANT.Prefix + "==> Latest Fetch call still pending. <==");

                        break;
                }
            }

            void createEnum()
            {
                var data                                                      = "using DVAH;\n public class FirebaseKeys{\n";
                foreach (var item in this.DVAH_Data.FirebaseRemoteDatas) data += $"public static HnnConfigValue {item.Key} =>  FireBaseBridge.GetValueRemote(\"{item.Key}\");\n";
                data += "}";
                try
                {
                    if (File.Exists(Environment.CurrentDirectory + "/Assets/Scripts/FirebaseKeys.cs"))
                        File.WriteAllText(Environment.CurrentDirectory + "/Assets/Scripts/FirebaseKeys.cs", "");
                    var stream = new FileStream(Environment.CurrentDirectory + "/Assets/Scripts/FirebaseKeys.cs", FileMode.OpenOrCreate);

                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(data);
                        writer.Flush();
                        writer.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                EditorUtility.SetDirty(this.DVAH_Data);
                AssetDatabase.SaveAssets();
                CompilationPipeline.RequestScriptCompilation();
            }

            Awake();
#endif
        }

        private void createJsonFirebase()
        {
            var path = EditorUtility.SaveFilePanel("save file to", "", $"{PlayerSettings.productName}_FireBaseRemoteData", "json");

            if (path.Length == 0) return;

            var data = "{";

            if (data.EndsWith(","))
                data = data.Remove(data.Length - 1, 1);
            data += "}";
            Stream stream = File.Open(path, FileMode.OpenOrCreate);
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(data);
            }

            stream.Close();
        }

        private void FaceBookEditor()
        {
#if FACEBOOK_IMPLEMENT
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Facebook:", TextGreenStyles);
            DVAH_Data.Facebook_AppID = EditorGUILayout.TextField("App ID", DVAH_Data.Facebook_AppID);
            DVAH_Data.Facebook_ClientToken = EditorGUILayout.TextField("Client Token", DVAH_Data.Facebook_ClientToken);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();

            if (facebook == null)
            {
                string[] facebookSetting = UnityEditor.AssetDatabase.FindAssets("t:FacebookSettings");
                if (facebookSetting.Length != 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(facebookSetting[0]);
                    facebook = UnityEditor.AssetDatabase.LoadAssetAtPath<FacebookSettings>(path);
                    return;
                }
                Debug.LogError(CONSTANT.Prefix + $":Can not find MaxSdkSetting!");
                return;
            }

            var appIds = facebook.GetType().GetProperty("AppIds");

            if (appIds != null)
            {
                object facebookAppIDProp = null;
                if (string.IsNullOrEmpty(DVAH_Data.Facebook_AppID))
                {
                    facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
                    DVAH_Data.Facebook_AppID = ((List<string>)facebookAppIDProp)[0];
                }

                appIds.SetValue(facebook, new List<string>() { DVAH_Data.Facebook_AppID }, null);
            }
            else
                Debug.LogError(CONSTANT.Prefix + $":Can not find FB app ID field!");


            var clientToken = facebook.GetType().GetProperty("ClientTokens");

            if (clientToken != null)
            {
                object facebookClientTokenProps = null;
                if (string.IsNullOrEmpty(DVAH_Data.Facebook_ClientToken))
                {
                    facebookClientTokenProps = clientToken.GetValue(facebookClientTokenProps, null);
                    DVAH_Data.Facebook_ClientToken = ((List<string>)facebookClientTokenProps)[0];
                }

                clientToken.SetValue(facebook, new List<string>() { DVAH_Data.Facebook_ClientToken }, null);
            }
            else
                Debug.LogError(CONSTANT.Prefix + $":Can not find FB client token field!");

            var appsLabel = facebook.GetType().GetProperty("AppLabels");

            if (appsLabel != null)
            {

                appsLabel.SetValue(facebook, new List<string>() { DVAH_Data.Facebook_AppID }, null);
            }
            else
                Debug.LogError(CONSTANT.Prefix + $":Can not find FB labels field!");

            var keyStorePath = facebook.GetType().GetProperty("AndroidKeystorePath");
            if (keyStorePath != null)
            {
                keyStorePath.SetValue(facebook, PlayerSettings.Android.keystoreName, null);
            }

            EditorUtility.SetDirty(facebook);
#endif
        }

        private void AdEditor()
        {
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Ad NetWork:", this.TextGreenStyles);

            foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE))) this.spawAdUnits($"{adType.ToString()} Ids", adType);

            EditorGUILayout.Space(10);
            EditorGUILayout.PrefixLabel("Mediation net:");

#if ADMOB_IMPLEMENT
#if UNITY_ANDROID
            this.DVAH_Data.Google_Android_AppID = EditorGUILayout.TextField("Google App ID", this.DVAH_Data.Google_Android_AppID);
            if (!this.DVAH_Data.Google_Android_AppID.StartsWith("ca-app-pub")) this.DVAH_Data.Google_Android_AppID = "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IOS
                    DVAH_Data.Google_IOS_AppID = EditorGUILayout.TextField("Google App ID", DVAH_Data.Google_IOS_AppID);
                    if( !DVAH_Data.Google_IOS_AppID.StartsWith("ca-app-pub")){
                        DVAH_Data.Google_IOS_AppID = "ca-app-pub-3940256099942544~3347511713";
                    }
#endif

            this.SetGoogleAdId();
#endif

#if MAX_IMPLEMENT
            this.DVAH_Data.AppLovin_SDK_Key = EditorGUILayout.TextField("Applovin SDK Key", this.DVAH_Data.AppLovin_SDK_Key);

            if (this.max != null)
            {
                this.max.SdkKey = this.DVAH_Data.AppLovin_SDK_Key;

                AppLovinSettings.Instance.AdMobAndroidAppId = this.DVAH_Data.Google_Android_AppID;
                AppLovinSettings.Instance.AdMobIosAppId     = this.DVAH_Data.Google_IOS_AppID;
                PrefabUtility.RecordPrefabInstancePropertyModifications(this.max);
                EditorUtility.SetDirty(this.max);
            }
            else
            {
                var maxSetting = AssetDatabase.FindAssets("t:AppLovinSettings");
                if (maxSetting.Length != 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(maxSetting[0]);
                    this.max = AssetDatabase.LoadAssetAtPath<AppLovinSettings>(path);
                }
                else
                {
                    Debug.LogError(CONSTANT.Prefix + $":Can not find MaxSdkSetting!");
                }
            }
#endif

#if IRONSOURCE_IMPLEMENT
                DVAH_Data.IS_App_Key = EditorGUILayout.TextField("IronSource App Key", DVAH_Data.IS_App_Key);
#endif

#if PANGLE_IMPLEMENT
            DVAH_Data.Pangle_App_ID = EditorGUILayout.TextField("Pangle App ID", DVAH_Data.Pangle_App_ID);
            DVAH_Data.Pangle_App_ID = string.IsNullOrEmpty(DVAH_Data.Pangle_App_ID)? "8025677": DVAH_Data.Pangle_App_ID;
#endif

            EditorUtility.SetDirty(this.DVAH_Data);
        }

        private void SetGoogleAdId()
        {
#if ADMOB_IMPLEMENT
            if (this.gg)
            {
                this.gg.GoogleMobileAdsAndroidAppId = this.DVAH_Data.Google_Android_AppID;
                this.gg.GoogleMobileAdsIOSAppId     = this.DVAH_Data.Google_IOS_AppID;

                PrefabUtility.RecordPrefabInstancePropertyModifications(this.gg);
                EditorUtility.SetDirty(this.gg);

                return;
            }

            var ggSetting = AssetDatabase.FindAssets("t:GoogleMobileAdsSettings");
            if (ggSetting.Length != 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(ggSetting[0]);
                this.gg = AssetDatabase.LoadAssetAtPath<GoogleMobileAdsSettings>(path);

                if (!this.gg.GoogleMobileAdsAndroidAppId.StartsWith("ca-app-pub")) this.gg.GoogleMobileAdsAndroidAppId = "ca-app-pub-3940256099942544~3347511713";

                if (!this.gg.GoogleMobileAdsIOSAppId.StartsWith("ca-app-pub")) this.gg.GoogleMobileAdsIOSAppId = "ca-app-pub-3940256099942544~3347511713";

                this.DVAH_Data.Google_Android_AppID = this.gg.GoogleMobileAdsAndroidAppId;
                this.DVAH_Data.Google_IOS_AppID     = this.gg.GoogleMobileAdsIOSAppId;

                return;
            }

            EditorGUILayout.LabelField("Can not find GoogleMobileAdsSettings!");
#endif
        }

        private void spawAdUnits(string title, AD_TYPE adType)
        {
            var btnWidth = 100;
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            var titleContent = new GUIContent(title, "Number of ad unit IDs");
            if (this.numberAdUnitId[adType] > 0)
                this.numberAdUnitId[adType] = EditorGUILayout.IntField(titleContent, this.numberAdUnitId[adType], this.TextGreenStyles, GUILayout.Width(200));
            else
                this.numberAdUnitId[adType] = EditorGUILayout.IntField(titleContent, this.numberAdUnitId[adType], this.TextRedStyles, GUILayout.Width(200));
            PrefabUtility.RecordPrefabInstancePropertyModifications(this.DVAH_Data);
            EditorUtility.SetDirty(this.DVAH_Data);

            var Datas = this.DVAH_Data.adUnits[adType].AdUnitDatas;
            if (this.numberAdUnitId[adType] > Datas.Count()) Datas.AddRange(new AdUnitData[this.numberAdUnitId[adType] - Datas.Count()]);

            if (this.numberAdUnitId[adType] < Datas.Count())
            {
                var numberRemove = Datas.Count() - this.numberAdUnitId[adType];
                Datas.RemoveRange(this.numberAdUnitId[adType], numberRemove);
            }

            if (this.numberAdUnitId[adType] > 0)
            {
                if (adType == AD_TYPE.Banner)
                {
                    var bannerPos              = this.DVAH_Data.BannerPosition.ToString();
                    var dropDownBannerSelected = EditorGUILayout.DropdownButton(new GUIContent(bannerPos), FocusType.Passive, GUILayout.Width(btnWidth));

                    if (dropDownBannerSelected)
                    {
                        var menuBanner = new GenericMenu();
                        foreach (BannerPosition banner in Enum.GetValues(typeof(BannerPosition)))
                            this.AddMenuBannerItem(menuBanner, banner, item => { this.DVAH_Data.BannerPosition = (BannerPosition)item; }, this.DVAH_Data.BannerPosition == banner);
                        menuBanner.ShowAsContext();
                    }

                    var originalValue = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth      = btnWidth / 2;
                    this.DVAH_Data.BannerDefaultShow = EditorGUILayout.Toggle("Is Show?", this.DVAH_Data.BannerDefaultShow);
                    this.DVAH_Data.IsAdptiveBanner   = EditorGUILayout.Toggle("Is Adaptive?", this.DVAH_Data.IsAdptiveBanner);

                    EditorGUIUtility.labelWidth = originalValue;
                }

                if (adType == AD_TYPE.MRecs)
                {
                    var bannerPos              = this.DVAH_Data.MrecsPosition.ToString();
                    var dropDownBannerSelected = EditorGUILayout.DropdownButton(new GUIContent(bannerPos), FocusType.Passive, GUILayout.Width(btnWidth));

                    if (dropDownBannerSelected)
                    {
                        var menuBanner = new GenericMenu();
                        foreach (BannerPosition banner in Enum.GetValues(typeof(BannerPosition)))
                            this.AddMenuBannerItem(menuBanner, banner, item => { this.DVAH_Data.MrecsPosition = (BannerPosition)item; }, this.DVAH_Data.MrecsPosition == banner);
                        menuBanner.ShowAsContext();
                    }

                    var originalValue = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth     = btnWidth / 2;
                    this.DVAH_Data.MrecsDefaultShow = EditorGUILayout.Toggle("Is Show?", this.DVAH_Data.MrecsDefaultShow);
                    EditorGUIUtility.labelWidth     = originalValue;
                }

                if (adType == AD_TYPE.Collapse)
                {
                    var bannerPos              = this.DVAH_Data.CollapsePosition.ToString();
                    var dropDownBannerSelected = EditorGUILayout.DropdownButton(new GUIContent(bannerPos), FocusType.Passive, GUILayout.Width(btnWidth));

                    if (dropDownBannerSelected)
                    {
                        var menuBanner = new GenericMenu();
                        foreach (BannerPosition banner in Enum.GetValues(typeof(BannerPosition)))
                            this.AddMenuBannerItem(menuBanner, banner, item => { this.DVAH_Data.CollapsePosition = (BannerPosition)item; }, this.DVAH_Data.CollapsePosition == banner);
                        menuBanner.ShowAsContext();
                    }
                }
            }

            EditorGUILayout.BeginVertical();
            var tmpTitle = new GUIContent("Size (%)", "Size of daptive banner");
            for (var i = 0; i < Datas.Count(); i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));
                if (Datas[i] == null)
                    Datas[i] = new AdUnitData();
                if (Datas[i].UnitId == null)
                    Datas[i].UnitId = "";
                Datas[i].UnitId = EditorGUILayout.TextField(Datas[i].UnitId.Replace(" ", ""));

                if (adType == AD_TYPE.Native) Datas[i].Prefab = EditorGUILayout.ObjectField(Datas[i].Prefab, typeof(UnityEngine.Object), true);

                if (adType == AD_TYPE.Banner && this.DVAH_Data.IsAdptiveBanner)
                {
                    Datas[i].Size.x = EditorGUILayout.FloatField(tmpTitle, Datas[i].Size.x, GUILayout.Width(200));
                    if (Datas[i].Size.x < 10) Datas[i].Size.x  = 10;
                    if (Datas[i].Size.x > 100) Datas[i].Size.x = 100;
                }

                if (adType == AD_TYPE.MRecs && this.DVAH_Data.MrecsPosition == BannerPosition.Custom) Datas[i].Position = EditorGUILayout.Vector2Field("Custom position (%)", Datas[i].Position);

                this.SpawUnitNetwork(adType, i);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            PrefabUtility.RecordPrefabInstancePropertyModifications(this.DVAH_Data);
            EditorUtility.SetDirty(this.DVAH_Data);
        }

        private void SpawUnitNetwork(AD_TYPE adType, int ID)
        {
            var btnWidth         = 100;
            var adNetWork        = this.DVAH_Data.adUnits[adType].AdUnitDatas[ID].network.ToString();
            var dropDownSelected = EditorGUILayout.DropdownButton(new GUIContent(adNetWork), FocusType.Passive, GUILayout.Width(btnWidth));

            if (dropDownSelected)
            {
                var menu = new GenericMenu();
                foreach (AD_NETWORK adNet in Enum.GetValues(typeof(AD_NETWORK)))
                    this.AddMenuItem(menu, adNet, item => { this.DVAH_Data.adUnits[adType].AdUnitDatas[ID].network = (AD_NETWORK)item; },
                        this.DVAH_Data.adUnits[adType].AdUnitDatas[ID].network == adNet);
                menu.ShowAsContext();
            }
        }

        private void KeyStoreInfo()
        {
            EditorGUILayout.BeginHorizontal();
            if (!this.isShowKeyStorePass)
                PlayerSettings.keystorePass = EditorGUILayout.PasswordField("Keystore Pass", PlayerSettings.keystorePass);
            else
                PlayerSettings.keystorePass = EditorGUILayout.TextField("Keystore Pass", PlayerSettings.keystorePass);
            this.isShowKeyStorePass = EditorGUILayout.Toggle("Show", this.isShowKeyStorePass);
            EditorGUILayout.EndHorizontal();

            PlayerSettings.Android.keyaliasName = EditorGUILayout.TextField("Keystore Alias", PlayerSettings.Android.keyaliasName);
            EditorGUILayout.BeginHorizontal();
            if (!this.isShowAliasPass)
                PlayerSettings.keyaliasPass = EditorGUILayout.PasswordField("Keystore Pass", PlayerSettings.keyaliasPass);
            else
                PlayerSettings.keyaliasPass = EditorGUILayout.TextField("Keystore Pass", PlayerSettings.keyaliasPass);
            this.isShowAliasPass = EditorGUILayout.Toggle("Show", this.isShowAliasPass);
            EditorGUILayout.EndHorizontal();
        }

        private void UIBottom()
        {
#if UNITY_ANDROID
            if (GUILayout.Button("Fix AndroidManifest FbID"))
            {
                EditorUtility.DisplayDialog("Attention Pleas?",
                    "This will change your AndroidManifest for match FBID!!", "Ok");
                MenuEditor.FixAndroidManifestFB();
            }
#endif

            EditorGUILayout.BeginHorizontal();

            var originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth            = 70;
            this.DVAH_Data.NO_ADS                  = EditorGUILayout.Toggle("Build Cheat", this.DVAH_Data.NO_ADS);
            EditorUserBuildSettings.buildAppBundle = EditorGUILayout.Toggle("Build aab", EditorUserBuildSettings.buildAppBundle);

            EditorGUIUtility.labelWidth        = originalValue;
            this.DVAH_Data.IsMediationDebugger = EditorGUILayout.Toggle("Build Mediation Debugger", this.DVAH_Data.IsMediationDebugger);
            if (this.DVAH_Data.IsMediationDebugger)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Mediation net");
                var adNetWork        = this.DVAH_Data.MEDIATION_NETWORK.ToString();
                var dropDownSelected = EditorGUILayout.DropdownButton(new GUIContent(adNetWork), FocusType.Passive, GUILayout.Width(300));

                EditorGUILayout.EndHorizontal();

                if (dropDownSelected)
                {
                    var menu = new GenericMenu();
                    foreach (AD_NETWORK adNet in Enum.GetValues(typeof(AD_NETWORK)))
                        this.AddMenuItem(menu, adNet, item => { this.DVAH_Data.MEDIATION_NETWORK = (AD_NETWORK)item; }, this.DVAH_Data.MEDIATION_NETWORK == adNet);
                    menu.ShowAsContext();
                }
            }

            //createNewBranch = EditorGUILayout.Toggle("New branch after build", createNewBranch);
            //EditorPrefs.SetBool("NEW_BRANCH", createNewBranch);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fix Firebase JSON")) MenuEditor.CheckFirebaseJson();

            if (GUILayout.Button("Fix Firebase JSON xml")) MenuEditor.CheckFirebaseXml();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build"))
            {
                if (!PlayerSettings.applicationIdentifier.StartsWith("com."))
                    EditorUtility.DisplayDialog("Attention Pleas?",
                        "Your package name should in form \"com.\". This can make you can't build your project, consider change it ASAP!!", "Ok");

                if (PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
                    EditorUtility.DisplayDialog("Attention Pleas?",
                        "Your package name is not in format 'com.X.Y' . This can make you can't build your project, consider change it ASAP!!", "Ok");

                BuildProject();
            }

            if (GUILayout.Button("Build Seting")) GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Close"))
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                this.Close();
                GUIUtility.ExitGUI();
            }
        }

        public static void BuildProject()
        {
            try
            {
                var buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.locationPathName = "huynn";
                buildPlayerOptions                  = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(buildPlayerOptions);

                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Init()
        {
            if (this.TextRedStyles == null)
            {
                this.TextRedStyles                  = new GUIStyle(EditorStyles.label);
                this.TextRedStyles.normal.textColor = Color.red;
            }

            if (this.TextGreenStyles == null)
            {
                this.TextGreenStyles                  = new GUIStyle(EditorStyles.label);
                this.TextGreenStyles.normal.textColor = Color.green;
            }

            if (this.ButtonTextGreenStyles == null)
            {
                this.ButtonTextGreenStyles                  = new GUIStyle(GUI.skin.button);
                this.ButtonTextGreenStyles.normal.textColor = Color.green;
            }

            if (!wnd || EditorApplication.isPlaying)
            {
                this.Close();
                GUIUtility.ExitGUI();

                return;
            }

            if (!this.DVAH_Data)
            {
                var DVAH_Datas = AssetDatabase.FindAssets("t:DVAH_Data");
                var path       = "";
                if (DVAH_Datas.Length != 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(DVAH_Datas[0]);
                }
                else
                {
                    EditorGUILayout.LabelField("Can not find DVAH data file! Generate new one");
                    var asset = CreateInstance<DVAH_Data>();

                    path = "Assets/Resources/DVAH_Data.asset";
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();

                    EditorUtility.FocusProjectWindow();

                    Selection.activeObject = asset;
                }

                this.DVAH_Data = AssetDatabase.LoadAssetAtPath<DVAH_Data>(path);

                foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE))) this.numberAdUnitId[adType] = this.DVAH_Data.adUnits[adType].AdUnitDatas.Count;
            }
        }

        private void ColorDefine()
        {
            if (this.TextRedStyles == null)
            {
                this.TextRedStyles                  = new GUIStyle(EditorStyles.label);
                this.TextRedStyles.normal.textColor = Color.red;
            }

            if (this.TextGreenStyles == null)
            {
                this.TextGreenStyles                  = new GUIStyle(EditorStyles.label);
                this.TextGreenStyles.normal.textColor = Color.green;
            }

            if (this.ButtonTextGreenStyles == null)
            {
                this.ButtonTextGreenStyles                  = new GUIStyle(GUI.skin.button);
                this.ButtonTextGreenStyles.normal.textColor = Color.green;
            }
        }

        public IEnumerator DownloadPlugin(string nameCache, string url, Action ondone = null, Action onUpdate = null)
        {
            EditorUtility.ClearProgressBar();
            Debug.Log($"{CONSTANT.Prefix} ==> Start download {nameCache}");
            var path            = Path.Combine(Application.temporaryCachePath, nameCache);
            var downloadHandler = new DownloadHandlerFile(path);
            var webRequest = new UnityWebRequest(url)
            {
                method          = UnityWebRequest.kHttpVerbGET,
                downloadHandler = downloadHandler
            };

            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                //Debug.Log($"{CONSTANT.Prefix} ==> Downloading {nameCache} --> {operation.progress*100}%");
                EditorUtility.DisplayProgressBar($"Download {nameCache}", $"Downloading... {operation.progress * 100}%", operation.progress);

                yield return new WaitForSeconds(0.1f); // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
            }

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result != UnityWebRequest.Result.Success)
#else
            if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
                Debug.LogError(webRequest.error);
            else
                AssetDatabase.ImportPackage(path, true);
            EditorUtility.ClearProgressBar();
            webRequest.Dispose();
            ondone?.Invoke();
        }

        private static void AddSymbol()
        {
            var symbolsList = new string[0];
#if UNITY_ANDROID
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, out symbolsList);

#elif UNITY_IOS
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.iOS, out symbolsList);
#endif
            var files = new string[0];
            symbolsList = MenuEditor.checkSymbols(true, "MAX_IMPLEMENT", symbolsList.ToList());
            symbolsList = MenuEditor.checkSymbols(true, "FACEBOOK_IMPLEMENT", symbolsList.ToList());
            symbolsList = MenuEditor.checkSymbols(true, "ADMOB_IMPLEMENT", symbolsList.ToList());
            symbolsList = MenuEditor.checkSymbols(true, "FIREBASE_IMPLEMENT", symbolsList.ToList());
            symbolsList = MenuEditor.checkSymbols(true, "APPFLYER_IMPLEMENT", symbolsList.ToList());
            symbolsList = MenuEditor.checkSymbols(true, "IRONSOURCE_IMPLEMENT", symbolsList.ToList());

#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbolsList);

#elif UNITY_IOS
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbolsList);
#endif
        }

        public RequestBase getData()
        {
            var urlSplits       = this.DVAH_Data.LinkGoogleSheet.Split("/");
            var idBeforeISsheet = urlSplits.ToList().IndexOf("spreadsheets") + 2;

            if (idBeforeISsheet >= urlSplits.Length)
                return null;
            var sctualURL = string.Format(CONSTANT.GoogleSheetUrl, urlSplits[idBeforeISsheet], this.DVAH_Data.NameGoogleSheet);

            Debug.Log(sctualURL);

            var requestBase = new RequestBase(sctualURL);
            _ = requestBase.Send(result =>
            {
                if (!this.DVAH_Data)
                    return;

                var data = JSON.Parse(result.response)["values"].AsArray;

                foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE))) this.DVAH_Data.adUnits[adType].AdUnitDatas.Clear();

                foreach (JSONNode item in data)
                {
                    if (item.Count <= 1)
                        continue;

                    if (item.AsArray[0].ToString().ToLower().Contains("name")) PlayerSettings.productName = item.AsArray[1];

                    if (item.AsArray[0].ToString().ToLower().Contains("bundleid"))
                    {
                        var applicationIdentifier = item.AsArray[1].ToString().Replace(" ", "");

                        if (!PlayerSettings.applicationIdentifier.StartsWith("com.") || PlayerSettings.applicationIdentifier.Split('.').Count() < 3)
                            EditorGUILayout.LabelField("Package name should in form \"com.X.Y\" other can cost a build error!", this.TextRedStyles);

                        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationIdentifier);
                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("applovin")) this.DVAH_Data.AppLovin_SDK_Key = item.AsArray[1];

                    if (item.AsArray[0].ToString().ToLower().Contains("ironsource")) this.DVAH_Data.IS_App_Key = item.AsArray[1];

                    if (item.AsArray[0].ToString().ToLower().Contains("admob"))
                    {
#if UNITY_ANDROID
                        this.DVAH_Data.Google_Android_AppID = item.AsArray[1];
                        if (!this.DVAH_Data.Google_Android_AppID.StartsWith("ca-app-pub")) this.DVAH_Data.Google_Android_AppID = "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IOS
                        DVAH_Data.Google_IOS_AppID = item.AsArray[1];
                        if (!DVAH_Data.Google_IOS_AppID.StartsWith("ca-app-pub"))
                        {
                            DVAH_Data.Google_IOS_AppID = "ca-app-pub-3940256099942544~3347511713";
                        }
#endif
                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("appflyer")) this.DVAH_Data.AppFlyer_DevKey = item.AsArray[1];

                    if (item.AsArray[0].ToString().ToLower().Contains("facebook"))
                    {
                        this.DVAH_Data.Facebook_AppID = item.AsArray[1];
                        if (item.Count >= 3) this.DVAH_Data.Facebook_ClientToken = item.AsArray[2];
                    }

                    foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE)))
                    {
                        if (item.Count < 2 || string.IsNullOrEmpty(item.AsArray[1].ToString()))
                            continue;
                        var adunitData = new AdUnitData();

                        if (item.AsArray[0].ToString().ToLower().Contains(adType.ToString().ToLower()))
                        {
                            adunitData.UnitId = item.AsArray[1];
                            if (item.Count >= 3) Enum.TryParse(item.AsArray[2], true, out adunitData.network);

                            this.DVAH_Data.adUnits[adType].AdUnitDatas.Add(adunitData);
                            this.numberAdUnitId[adType] = this.DVAH_Data.adUnits[adType].AdUnitDatas.Count;
                        }
                    }
                }
#if UNITY_ANDROID
                MenuEditor.FixAndroidManifestFB();
#endif
                Debug.Log(CONSTANT.Prefix + $"==> Done fetch data fromm sheet {this.DVAH_Data.NameGoogleSheet}");
            });

            return requestBase;
        }

        private void AddMenuItem(GenericMenu menu, AD_NETWORK value, Action<object> clickCallback, bool isSelected = false)
        {
            // the menu item is marked as selected if it matches the current value of m_Color
            menu.AddItem(new GUIContent(value.ToString()), isSelected, item => { clickCallback?.Invoke(item); }, value);
        }

        private void AddMenuBannerItem(GenericMenu menu, BannerPosition value, Action<object> clickCallback, bool isSelected = false)
        {
            // the menu item is marked as selected if it matches the current value of m_Color
            menu.AddItem(new GUIContent(value.ToString()), isSelected, item => { clickCallback?.Invoke(item); }, value);
        }
    }
}
#endif