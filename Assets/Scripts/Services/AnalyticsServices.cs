namespace MiraiGame.Script.Services
{
    using System.Collections;
    using DVAH;
    using GameFoundation.DI;
    using MiraiGame.Script.Services.Interface;
    using UnityEngine;

    public class AnalyticsServices : IAnalyticsServices, IInitializable
    {
        public void Initialize() { Debug.Log($"Init Firebase Analytics: {FireBaseBridge.Instant}"); }

        public void LogEventWithOneParam(string eventName) { }

        public void LogEventWithParameterAsync(string eventName) { }

        public void LogEventWithParameterAsync(string eventName, Hashtable hash) { }

        public void OnInitDone(System.Action callback) { callback?.Invoke(); }
    }
}