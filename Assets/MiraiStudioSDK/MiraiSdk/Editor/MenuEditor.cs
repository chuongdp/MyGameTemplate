#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_ANDROID
#if FACEBOOK_IMPLEMENT
using Facebook.Unity.Settings;
#endif

#if ADMOB_IMPLEMENT
using GoogleMobileAds.Editor;
#endif
#endif
using DVAH;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace DVAH
{
    public class MenuEditor
    {
        [MenuItem("MIRAI/ResolveSymbols")]
        public static void ResolveSymbols()
        {

            string[] symbolsList = new string[0];
#if UNITY_ANDROID
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, out symbolsList);

#elif UNITY_IOS
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.iOS, out symbolsList);
#endif

            string[] files = Directory.GetFiles(Application.dataPath, "MaxSdk.cs", SearchOption.AllDirectories);

            symbolsList = checkSymbols(files.Length == 1 && checkAdNetWork(AD_NETWORK.Max), "MAX_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "LevelPlayService.cs", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length == 1 && checkAdNetWork(AD_NETWORK.IronSource), "IRONSOURCE_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "YandexMobileAdsClientFactory.cs", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length == 1 && checkAdNetWork(AD_NETWORK.Yandex), "YANDEX_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "PAGSdk.cs", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length == 1 && checkAdNetWork(AD_NETWORK.Pangle), "PANGLE_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "Facebook.Unity.Settings.dll", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length >= 1, "FACEBOOK_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "GoogleMobileAdsSettings.cs", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length == 1, "ADMOB_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "Firebase.Analytics.dll", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length >= 1, "FIREBASE_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "AppsFlyer.asmdef", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length >= 1, "APPFLYER_IMPLEMENT", symbolsList.ToList());

            files = Directory.GetFiles(Application.dataPath, "Google.Play.AppUpdate.asmdef", SearchOption.AllDirectories);
            symbolsList = checkSymbols(files.Length == 1, "UPDATE_IMPLEMENT", symbolsList.ToList());


            if (!EditorUtility.DisplayDialog("Attention", "Resolve done! Save it?", "Ok!", "Cancel"))
            {
                return;
            }


#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbolsList);

#elif UNITY_IOS
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbolsList);
#endif
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        public static bool checkAdNetWork(AD_NETWORK net)
        {
            DVAH_Data Data = MenuEditor.LoadDVAH();
            foreach (AD_TYPE adType in Enum.GetValues(typeof(AD_TYPE)))
            {
                foreach (var i in Data.adUnits[adType].AdUnitDatas)
                {
                    if (i.network == net)
                        return true;
                }
            }

            return false;
        }


        static DVAH_Data LoadDVAH()
        {
            string[] DVAH_Datas = UnityEditor.AssetDatabase.FindAssets("t:DVAH_Data");
            string path = "";
            if (DVAH_Datas.Length != 0)
            {
                path = UnityEditor.AssetDatabase.GUIDToAssetPath(DVAH_Datas[0]);

            }
            else
            {
                EditorGUILayout.LabelField("Can not find DVAH data file! Generate new one");
                DVAH_Data asset = ScriptableObject.CreateInstance<DVAH_Data>();

                path = "Assets/Resources/DVAH_Data.asset";
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();

                Selection.activeObject = asset;
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<DVAH_Data>(path);

        }

        public static string[] checkSymbols(bool IsSdk, string nameSym, List<string> symBols)
        {
            if (!IsSdk && symBols.Contains(nameSym))
                symBols.Remove(nameSym);

            if (IsSdk && !symBols.Contains(nameSym))
                symBols.Add(nameSym);

            return symBols.ToArray();
        }

        [MenuItem("MIRAI/Remove SDK")]
        public static void DeleteSDK()
        {
            if (!EditorUtility.DisplayDialog("Attention!!!!", "Do you want delete all SDK folder?", "Yes", "No"))
                return;

            string[] symbolsList = new string[0];
#if UNITY_ANDROID
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, out symbolsList);

#elif UNITY_IOS
            PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.iOS, out symbolsList);
#endif
            string[] files = new string[0];
            symbolsList = checkSymbols(false, "MAX_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "FACEBOOK_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "ADMOB_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "FIREBASE_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "APPFLYER_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "IRONSOURCE_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "UPDATE_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "YANDEX_IMPLEMENT", symbolsList.ToList());
            symbolsList = checkSymbols(false, "PANGLE_IMPLEMENT", symbolsList.ToList());

#if UNITY_ANDROID 
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbolsList);

#elif UNITY_IOS 
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbolsList);
#endif

            List<string> paths = new List<string>()
            {
                "/Adinmo",
                "/AppsFlyer",
                "/ExternalDependencyManager",
                "/FacebookSDK",
                "/Firebase",
                "/GeneratedLocalRepo",
                "/GoogleMobileAds",
                "/GoogleMobileAdsNative",
                "/GooglePlayPlugins",
                "/IronSourceAdQuality",
                "/MaxSdk",
                "/LevelPlay",
                "/Plugins/Android" ,
                "/Plugins/iOS" ,
                "/YandexMobileAds",
                "/PAG",
                "/Parse"
            };

            foreach (string s in paths)
            {
                if (!Directory.Exists(Application.dataPath + s))
                    continue;
                try
                {
                    Directory.Delete(Application.dataPath + s, true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        [MenuItem("MIRAI/Play")]
        public static void PlayGame()
        {
            if (EditorBuildSettings.scenes.Count() == 0)
            {
                EditorUtility.DisplayDialog("Error", "You must start at add a scene to build setting!", "Got it!");
                return;
            }

            if (EditorUtility.DisplayDialog("Attention", "Do you want clear all playerPrefb before run?", "Yes", "No"))
                PlayerPrefs.DeleteAll();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
            UnityEditor.EditorApplication.isPlaying = true;
        }


        public static void ClearPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog("Attention", "Clear all player prefb!", "Ok!", "Cancel"))
                return;

            PlayerPrefs.DeleteAll();
        }


        public static void MenuPushGit()
        {
            if (!EditorUtility.DisplayDialog("Attention Please!", "It will commit all change then push to branch production_hnn on remote. " +
               "You can check and merge to Production later!", "Got it!", "Stop"))
            {
                return;
            }

            PushGit(null);
        }

        public static void PushBackUp(string nameAPK)
        {
            string cmdPath = FindCommand();
            if (string.IsNullOrEmpty(cmdPath))
                return;

            string cmdLines = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                cmdLines = "#!/bin/sh\n\n" +
                "cd ../../\n" +
                "cd " + Application.dataPath + "\n" +
                "git add -A\n" +
                $"git commit -m \"build_{nameAPK} \"\n" +
                $"git push origin HEAD:{EditorUserBuildSettings.activeBuildTarget}_production_{PlayerSettings.bundleVersion} -f";
            }
            else
            {

                cmdLines = "/C git add -A&" +
                $"git commit -m \"build_{nameAPK} \"&" +
                $"git push origin HEAD:{EditorUserBuildSettings.activeBuildTarget}_production_{PlayerSettings.bundleVersion} -f";
            }

            string terminal = @"cmd.exe";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
                FileStream stream = new FileStream(cmdPath, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(cmdLines);

                    writer.Flush();
                    writer.Close();
                }

                System.Diagnostics.Process uploadProc = new System.Diagnostics.Process();
                uploadProc.StartInfo.FileName = terminal;
                uploadProc.StartInfo.Arguments = cmdPath;
                uploadProc.StartInfo.UseShellExecute = false;
                uploadProc.StartInfo.CreateNoWindow = false;
                uploadProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                uploadProc.Start();
            }
            else
            {
                terminal = @"C:\Windows\system32\cmd.exe";
                Process.Start(terminal, cmdLines);
            }


        }


        public static void UpdateLib()
        {
            if (!EditorUtility.DisplayDialog("Attention Please!", "Before update, all change will be commit (not push yet)!", "Got it!", "Stop"))
            {
                return;
            }

            UpdateLibCommand("production");
        }


        public static void UpdateLibDev()
        {
            if (!EditorUtility.DisplayDialog("Attention Please!", "Before update, all change will be commit (not push yet)!", "Got it!", "Stop"))
            {
                return;
            }

            UpdateLibCommand("develop");
        }

        public static void UpdateLibCommand(string branch)
        {
            string cmdPath = FindCommand();
            if (string.IsNullOrEmpty(cmdPath))
                return;

            var directory = new DirectoryInfo(Application.dataPath);
            while (directory.GetDirectories(".git").Length == 0)
            {
                directory = directory.Parent;

                if (directory == null)
                {
                    throw new DirectoryNotFoundException("We went all the way up to the system root directory and didn't find any \".git\" directory!");
                }
            }
            var repositoryPath = directory.FullName;

            string cmdLines = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                cmdLines = "#!/bin/sh\n\n" +
                "cd ../../\n" +
                "cd " + Application.dataPath + "\n" +
                "cd $(git rev-parse --show-cdup)\n" +
                "git add -A\n" +
                "git commit -m \"prepare update lib!!!!!!\"\n" +
                "git subtree pull --prefix " + Application.dataPath.Replace(repositoryPath + "/", "") + "/DVAH/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git " + branch + " --squash";
            }
            else
            {
                cmdLines = "/K cd " + repositoryPath + "&" +
                "git add -A&" +
                "git commit -m \"prepare update lib!!!!!!\"&" +
                "git subtree pull --prefix " + Application.dataPath.Replace(repositoryPath.Replace("\\", "/") + "/", "") + "/DVAH/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git " + branch + " --squash";
            }

            string terminal = @"cmd.exe";
            System.Diagnostics.Process updateProc = new System.Diagnostics.Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
                FileStream stream = new FileStream(cmdPath, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(cmdLines);

                    writer.Flush();
                    writer.Close();
                }


                updateProc.StartInfo.FileName = terminal;
                updateProc.StartInfo.Arguments = cmdPath;
                updateProc.StartInfo.UseShellExecute = false;
                updateProc.StartInfo.CreateNoWindow = false;
                updateProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                updateProc.EnableRaisingEvents = true;

                updateProc.Exited += UploadProc_Exited;
                updateProc.Start();
            }
            else
            {
                terminal = @"C:\Windows\system32\cmd.exe";
                updateProc.StartInfo.FileName = terminal;
                updateProc.StartInfo.Arguments = cmdLines;
                updateProc.EnableRaisingEvents = true;

                updateProc.Exited += UploadProc_Exited;
                updateProc.Start();

            }

        }

        public static void UploadProc_Exited(object sender, EventArgs e)
        {
            Debug.LogError("Process done!");
        }

        public static void PushGit(BuildReport _report)
        {
            string cmdPath = FindCommand();
            if (string.IsNullOrEmpty(cmdPath))
                return;

            string cmdLines = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                cmdLines = "#!/bin/sh\n\n" +
                "cd ../../\n" +
                "cd " + Application.dataPath + "\n" +
                "git add -A\n" +
                $"git commit -m \"{EditorUserBuildSettings.activeBuildTarget}_release_{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode} \"\n" +
                $"git push origin HEAD:{EditorUserBuildSettings.activeBuildTarget}_production_doNotCreateBranchFromHere -f";
            }
            else
            {

                cmdLines = "/C git add -A&" +
                $"git commit -m \"release_{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode} \"&" +
                $"git push origin HEAD:{EditorUserBuildSettings.activeBuildTarget}_production_doNotCreateBranchFromHere -f";
            }

            string terminal = @"cmd.exe";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                terminal = @"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
                FileStream stream = new FileStream(cmdPath, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(cmdLines);

                    writer.Flush();
                    writer.Close();
                }

                System.Diagnostics.Process uploadProc = new System.Diagnostics.Process();
                uploadProc.StartInfo.FileName = terminal;
                uploadProc.StartInfo.Arguments = cmdPath;
                uploadProc.StartInfo.UseShellExecute = false;
                uploadProc.StartInfo.CreateNoWindow = false;
                uploadProc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                uploadProc.Start();
            }
            else
            {
                terminal = @"C:\Windows\system32\cmd.exe";
                Process.Start(terminal, cmdLines);
            }


        }

#if UNITY_ANDROID
        public static void FixAndroidManifestFB()
        {
#if FACEBOOK_IMPLEMENT
            string[] facebookSetting = UnityEditor.AssetDatabase.FindAssets("t:FacebookSettings");
            if (facebookSetting.Length == 0)
            {
                return;
            }

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(facebookSetting[0]);
            FacebookSettings facebook = UnityEditor.AssetDatabase.LoadAssetAtPath<FacebookSettings>(path);

            var appIds = facebook.GetType().GetProperty("AppIds");
            object facebookAppIDProp = null;
            facebookAppIDProp = appIds.GetValue(facebookAppIDProp, null);
            string fbAppID = ((List<string>)facebookAppIDProp)[0];

            if (string.IsNullOrEmpty(fbAppID))
            {
                fbAppID = PlayerSettings.applicationIdentifier;
            }


            string[] files = Directory.GetFiles(Application.dataPath, "AndroidManifest.xml", SearchOption.AllDirectories).ToArray();
            if (files.Length == 0)
            {
                return;
            }

            foreach (string filePath in files)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);


                foreach (XmlNode e in xmlDoc.GetElementsByTagName("meta-data"))
                {
                    if (!e.Attributes["android:name"].Value.Equals("com.facebook.sdk.ApplicationId"))
                    {
                        continue;
                    }

                    if (e.Attributes["android:value"].Value.Equals("fb" + fbAppID))
                    {
                        break;
                    }

                    try{
                        e.Attributes["android:value"].Value = ("fb" + fbAppID);
                    }catch(Exception){

                    }
                }


                foreach (XmlNode e in xmlDoc.GetElementsByTagName("provider"))
                {
                  
                    if (e.Attributes["android:authorities"].Value.Equals("com.facebook.app.FacebookContentProvider" + fbAppID))
                    {
                        continue;
                    }

                    try{
                        e.Attributes["android:authorities"].Value = ("com.facebook.app.FacebookContentProvider" + fbAppID);
                    }catch(Exception){

                    }
                
                }

                FileStream stream = new FileStream(filePath, FileMode.Create);

                xmlDoc.Save(stream);

                stream.Flush();
                stream.Close();
            }
#endif
        }
#endif
        [MenuItem("MIRAI/Play_2")]
        public static void FixGoogleXml(bool isShowOk = true)
        {

            XmlDocument xmlDoc = new XmlDocument();
            string googleServiceXmlPath = CheckFirebaseXml();
            if (string.IsNullOrEmpty(googleServiceXmlPath))
            {
                if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                    "Missing google-service.xml. All firebase services may not work?", "Continue", "Stop"))
                {
                    StopBuildWithMessage("Missing google-service.xml");
                }
                return;
            }

            if (!CheckFirebaseJson(false))
                return;
            Debug.Log(googleServiceXmlPath);
            using (StreamReader reader = new StreamReader(Directory.GetFiles(Application.dataPath, "*google-services.json", SearchOption.AllDirectories)[0]))
            {
                var dataParsed = JSON.Parse(reader.ReadToEnd());

                string errors = "";
                Debug.LogError(googleServiceXmlPath);
                xmlDoc.Load(googleServiceXmlPath);
                var root = xmlDoc.GetElementsByTagName("string");

                var project_info = dataParsed["project_info"];
                var client = dataParsed["client"][0];
                var apiKey = client["api_key"][0];
                var default_web_client_id = client["services"]["appinvite_service"]["other_platform_oauth_client"][0]["client_id"];

                foreach (XmlNode e in root)
                {
                    if (e.Attributes["name"].Value == "gcm_defaultSenderId")
                    {
                        if (e.InnerText != project_info["project_number"])
                        {
                            errors += "gcm_defaultSenderId wrong!   \n";
                        }
                    }

                    if (e.Attributes["name"].Value == "google_storage_bucket")
                    {
                        if (e.InnerText != project_info["storage_bucket"])
                        {
                            errors += "google_storage_bucket wrong! \n";
                        }
                    }

                    if (e.Attributes["name"].Value == "project_id")
                    {
                        if (e.InnerText != project_info["project_id"])
                        {
                            errors += "project_id wrong!  \n";
                        }
                    }

                    if (e.Attributes["name"].Value == "google_api_key")
                    {
                        if (e.InnerText != apiKey["current_key"])
                        {
                            errors += "google_api_key wrong! \n";
                        }
                    }

                    if (e.Attributes["name"].Value == "google_crash_reporting_api_key")
                    {
                        if (e.InnerText != apiKey["current_key"])
                        {
                            errors += "google_crash_reporting_api_key wrong! \n";
                        }
                    }

                    if (e.Attributes["name"].Value == "google_app_id")
                    {
                        if (e.InnerText != client["client_info"]["mobilesdk_app_id"])
                        {
                            errors += "default_web_client_id wrong!  \n";
                        }
                    }

                    if (default_web_client_id && e.Attributes["name"].Value == "default_web_client_id")
                    {
                        if (e.InnerText != default_web_client_id)
                        {
                            errors += "default_web_client_id wrong! \n";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(errors))
                {
                    if (EditorUtility.DisplayDialog("Oop, something wrong?",
                        "data different between google-service.xml and google-services.json: \n" +
                        errors +
                        " All firebase services may not work, auto fix it?", "Ok!", "Fuck off"))
                    {
                        string data = "<?xml version='1.0' encoding='utf-8'?>\n" +
                            "<resources xmlns:tools=\"http://schemas.android.com/tools\" tools:keep=\"@string/gcm_defaultSenderId," +
                            "@string/google_storage_bucket," +
                            "@string/project_id,@string/google_api_key," +
                            "@string/google_crash_reporting_api_key,@string/google_app_id," +
                            "@string/default_web_client_id\">\n  " +
                            "<string name=\"gcm_defaultSenderId\" translatable=\"false\">" + project_info["project_number"] + "</string>\n  " +
                            "<string name=\"google_storage_bucket\" translatable=\"false\">" + project_info["storage_bucket"] + "</string>\n  " +
                            "<string name=\"project_id\" translatable=\"false\">" + project_info["project_id"] + "</string>\n  " +
                            "<string name=\"google_api_key\" translatable=\"false\">" + client["api_key"][0]["current_key"] + "</string>\n  " +
                            "<string name=\"google_crash_reporting_api_key\" translatable=\"false\">" + client["api_key"][0]["current_key"] + "</string>\n  " +
                            "<string name=\"google_app_id\" translatable=\"false\">" + client["client_info"]["mobilesdk_app_id"] + "</string>\n  " +
                            "<string name=\"default_web_client_id\" translatable=\"false\">" + default_web_client_id + "</string>\n" +
                            "</resources>\n";

                        FileStream stream = new FileStream(CheckFirebaseXml(), FileMode.Create);
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(data);

                            writer.Flush();
                            writer.Close();
                        }
                    }

                }

                reader.Close();

            }

            if (isShowOk)
                EditorUtility.DisplayDialog("Hi, your captain here!",
                   "google-services.xml: Oke oke", "Ok!");
        }


        public static bool CheckFirebaseJson(bool isShowOk = true)
        {
#if UNITY_ANDROID
            string[] files = Directory.GetFiles(Application.dataPath, "*.json*", SearchOption.AllDirectories)
                                .Where(f => f.EndsWith("google-services.json")).ToArray();
            if (files.Length == 0)
            {
                UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project doesnt contain google-services.json. Firebase may not work!!!!!<==");
                if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                    "Missing google-service.js. All firebase services may not work?", "Continue", "Stop"))
                {
                    StopBuildWithMessage("Missing google-service.js");
                    return false;
                }

                return false;
            }

            if (files.Length > 1)
            {
                UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project contain more than one file google-services.json. Firebase may not work wrong!!!!!<==");
                if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                    "Too many google-service.js. All firebase services may not work?", "Continue", "Stop"))
                {
                    StopBuildWithMessage("Too many google-service.js");
                    return false;
                }
                return false;
            }

            if (isShowOk)
                EditorUtility.DisplayDialog("Ok, Nothing wrong!",
                   "Your file google-services.json exist and seem to be oke!", "Close");

            return true;
#elif UNITY_IOS
            string[] files = Directory.GetFiles(Application.dataPath, "*.plist*", SearchOption.AllDirectories)
                                   .Where(f => f.EndsWith("GoogleService-Info.plist")).ToArray();
            if (files.Length == 0)
            {
                UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project doesnt contain GoogleService-Info.plist. Firebase may not work!!!!!<==");
                if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                    "Missing GoogleService-Info.plist. All firebase services may not work?", "Continue", "Stop"))
                {
                    StopBuildWithMessage("Missing GoogleService-Info.plist");
                }

                return false;
            }

            if (files.Length > 1)
            {
                UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project contain more than one file GoogleService-Info.plist. Firebase may not work wrong!!!!!<==");
                if (!EditorUtility.DisplayDialog("Oop, something wrong?",
                    "Too many GoogleService-Info.plist. All firebase services may not work?", "Continue", "Stop"))
                {
                    StopBuildWithMessage("Too many GoogleService-Info.plist");
                }
                return false;
            }

            if (isShowOk)
                EditorUtility.DisplayDialog("Ok, Nothing wrong!",
                   "Your file GoogleService-Info.plist exist and seem to be oke!", "Close");

            return true;

#endif
            return false;
        }

        public static string CheckFirebaseXml()
        {

            string[] files = Directory.GetFiles(Application.dataPath, "*google-services.xml", SearchOption.AllDirectories).ToArray();
            if (files.Length == 1)
            {
                return files[0];
            }
            Debug.LogError(CONSTANT.Prefix + $"==> google-service.xml missing, auto create!");
            TextAsset textAsset = Resources.Load<TextAsset>("google-services");
            FileStream file = new FileStream(Application.dataPath + "/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml", FileMode.OpenOrCreate);
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(textAsset.text);
                writer.Flush();
                writer.Close();
            }

            return Application.dataPath + "/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml";
        }



        public static void StopBuildWithMessage(string message)
        {
            string prefix = CONSTANT.Prefix + $"";
#if UNITY_2017_1_OR_NEWER
            throw new BuildFailedException(prefix + message);
#else
        throw new OperationCanceledException(prefix + message);
#endif
        }


        static string FindCommand()
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*git_cmd.sh", SearchOption.AllDirectories).ToArray();
            if (files.Length == 1)
            {
                return files[0];
            }

            UnityEngine.Debug.LogError(CONSTANT.Prefix + $"==>Project dont have require .sh file. Generate!!!!!<==");
            FileStream cmdFile = new FileStream($"{Application.dataPath}/git_cmd.sh", FileMode.OpenOrCreate);
            return $"{Application.dataPath}/git_cmd.sh";
        }

    }

#if UNITY_ANDROID

    [InitializeOnLoad]
    public class StartEditor{
        static StartEditor(){ 
            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                MenuEditor.FixAndroidManifestFB();
            }
        }
    }
#endif

}

#endif