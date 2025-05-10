namespace HyperGame.Script
{
    public class StaticValue
    {
        private static string ApiKey  = "AIzaSyAu0GrVBRkSqPbu0497OFHGIZ4GmERZ9HA";
        //private static string AIModel = "gemini-2.0-flash";
        private static string AIModel = "gemini-2.5-flash-preview-04-17";
        //private static string AIModel = "gemini-2.5-pro-exp-03-25";
        public static  string Host    = $"https://generativelanguage.googleapis.com/v1beta/models/{AIModel}:generateContent?key={ApiKey}";
    }
}