
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DVAH;

#if ADMOB_IMPLEMENT
using AppsFlyerSDK; 
using GoogleMobileAds.Api;
#endif

#if FIREBASE_IMPLEMENT
using Firebase.Analytics;
using ConfigValue = Firebase.RemoteConfig.ConfigValue;
using UnityEngine.SceneManagement;
using System.Threading.Tasks; 
#else
using ConfigValue = DVAH.ConfigValue;
#endif

public class FireBaseManagerDemo : DVAH.Singleton<FireBaseManagerDemo>
{
    public bool IsFetchDone => FireBaseBridge.Instant.isFetchDOne;

    protected override void Awake()
    {
        base.Awake();
        var init = FireBaseBridge.Instant; 
        
    }
     

    public void LogButtonGame(string nameBtn)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log($"btn_{SceneManager.GetActiveScene().name}_{nameBtn}");
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            $"btn_{SceneManager.GetActiveScene().name}_{nameBtn}"
        );
#endif
#endif
    }

    public void LogSceneGame(string sceneName)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log($"Scene_{sceneName}");
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            $"Scene_{sceneName}"
        );
#endif
#endif
    }

    public void LogEvent(string sEvent)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log(sEvent);
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            sEvent
        );
#endif
#endif
    }

    public void LogEvent(string name, string parameterName, double parameterValue)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log($"{name}_{parameterName}_{parameterValue}");
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            name,
            parameterName,
            parameterValue
        );
#endif
#endif
    }

    public void LogEvent(string name, string parameterName, long parameterValue)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log($"{name}_{parameterName}_{parameterValue}");
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            name,
            parameterName,
            parameterValue
        );
#endif
#endif
    }

    public void LogEvent(string name, string parameterName, int parameterValue)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log($"{name}_{parameterName}_{parameterValue}");
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            name,
            parameterName,
            parameterValue
        );
#endif
#endif
    }

    public void LogEvent(string name, string parameterName, string parameterValue)
    {
#if MIRAI_FIREBASEANALYTIC
#if UNITY_EDITOR
        Debug.Log($"{name}_{parameterName}_{parameterValue}");
#else
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            name,
            parameterName,
            parameterValue
        );
#endif
#endif
    }
}
