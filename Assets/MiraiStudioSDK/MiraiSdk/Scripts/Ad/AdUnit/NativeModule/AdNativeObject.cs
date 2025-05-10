using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DVAH.Ad.Native
{
    public abstract class AdNativeObject : MonoBehaviour
    {

        public abstract List<GameObject> setAdBG(List<Texture2D> texs);
        public abstract GameObject SetIconTexture(Texture2D tex = null);

        public abstract GameObject SetHeadlineText(string headline = null);

        public abstract GameObject SetAdChoicesLogoTexture(Texture2D tex = null);

        public abstract GameObject SetCallToActionText(string ctaText = null);
        public abstract GameObject SetAdvertiserText(string advertText = null);

        public abstract GameObject SetBodyText(string bodyText = null);

        public abstract GameObject SetPrice(string price = null);

        public abstract GameObject SetStore(string store = null);

        public abstract void Show(int ID = -1, float MinX = 0, float MinY = 0, float MaxX = 1, float MaxY = 1);


    }
}