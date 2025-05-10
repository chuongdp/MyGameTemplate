namespace MiraiGame.Script.Services.Interface
{
    using System;
    using DVAH;

    public interface IAdServices
    {
        public void ShowAoa();

        public void ShowBanner();

        public void HideBanner();

        public void ShowInterstitial(string placement);

        public void ShowRewardedAd(string placement);

        public void ShowMrec();

        public void HideMrec();

        public void ShowNativeAd();

        public void HideNativeAd();
    }
}