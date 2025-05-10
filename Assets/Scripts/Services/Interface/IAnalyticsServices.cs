namespace MiraiGame.Script.Services.Interface
{
    using System;
    using System.Collections;

    public interface IAnalyticsServices
    {
        public void LogEventWithOneParam(string eventName);

        public void LogEventWithParameterAsync(string eventName);

        public void LogEventWithParameterAsync(string eventName, Hashtable hash);

        public void OnInitDone(Action callback);
    }
}