// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("kTbfHfZiYTiSbvrgo+URqs3NzSiATg50ck5S162TEutKYTrKzfYwl2ZTu06EB1kJ55ilFj1yw+GRVXfqTk4pY4B8QRMTY/Y6PYGuXhcUnQMzgQIhMw4FCimFS4X0DgICAgYDAF5SxgfjRkPlirQQRI4hTUwdOAIh/VYUgP6RoAyOTJmlW4qYw7+zbVuBAgwDM4ECCQGBAgIDotkFGm3lvXssLWtXldiCkXvjvyj1I9Ih/ommGLsnZm6slS24q3hmlFLbp1aNGhzm/YzFsHLr6Oa/gPaLeL/mXJBLNWWzHpvZiAqovK7NB8mAonHiNsCbgCJQvditMCRXK8qbrbfs8BXK6JYf3XL6+u4vkGqTlcXaORT3nSJuuPh8PC/FX5ZNRAEAAgMC");
        private static int[] order = new int[] { 13,2,9,7,13,11,12,9,12,12,10,12,13,13,14 };
        private static int key = 3;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
