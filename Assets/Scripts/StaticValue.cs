namespace HyperGame.Script
{
    public class StaticValue
    {
        private static string ApiKey  = "AIzaSyAu0GrVBRkSqPbu0497OFHGIZ4GmERZ9HA";
        private static string AIModel = "gemini-2.0-flash";
        public static  string Host    = $"https://generativelanguage.googleapis.com/v1beta/models/{AIModel}:generateContent?key={ApiKey}";
    }
}