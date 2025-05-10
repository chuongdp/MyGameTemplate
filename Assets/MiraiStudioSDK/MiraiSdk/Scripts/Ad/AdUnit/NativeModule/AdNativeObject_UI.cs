using System.Collections;
using System.Collections.Generic;
using DVAH.Lib;
using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

namespace DVAH.Ad.Native
{
    using GameFoundation.Scripts.Utilities.Extension;

    [System.Serializable]
    public class AdNativeObject_UI : AdNativeObject
    {
        public RawImage adIcon, adChoice;
        public GameObject adBGFitter;

        public Transform adBGManager => adBGFitter.transform.parent;

        public Text callToAction, advertiser, headLine, body, price, store;

        public RectTransform rectTransform;

        public int AdID;

        void Awake()
        {
            rectTransform = this.GetComponent<RectTransform>();
        }

        private void CheckCanvas()
        {
            Canvas c = this.GetComponentInParent<Canvas>();
            if (c == null || c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                string error = CONSTANT.Prefix + $"====>If native object doesnt on canvas OR canvas using RenderMode.ScreenSpaceOverlay, " +
                    "then native AD not clickable which make your impression not record eventhought you saw native AD show up on editor/device!!! <====";
                Debug.LogError(error);
                callToAction.text = error;
                advertiser.text = error;
                headLine.text = error;
                body.text = error;

                callToAction.color = Color.red;
                advertiser.color = Color.red;
                headLine.color = Color.red;
                body.color = Color.red;
            }
        }

        public override List<GameObject> setAdBG(List<Texture2D> texs)
        {
            List<GameObject> BGs = new List<GameObject>();
#if UNITY_EDITOR
            for (int i = 0; i < 3; i++)
            {

                GameObject bg;
                if (i >= this.adBGManager.childCount)
                {
                    bg = Instantiate(this.adBGFitter, this.adBGFitter.transform.position,
                       Quaternion.identity, this.adBGManager);
                }
                else
                {
                    bg = this.adBGManager.GetChild(i).gameObject;
                }
                float aspect = 1;

                bg.GetComponentInChildren<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                bg.GetComponentInChildren<AspectRatioFitter>().aspectRatio = aspect;
                bg.GetComponentInChildren<RawImage>().texture = null;
                bg.SetActive(true);

                BGs.Add(bg.transform.GetChild(0).gameObject);
            }
#else
            for (int i = 0; i< texs.Count; i++) {

                Texture2D tex = texs[i];
                GameObject bg;
                if (i >= this.adBGManager.childCount) {
                    bg = Instantiate(this.adBGFitter, this.adBGFitter.transform.position,
                       Quaternion.identity, this.adBGManager);
                } else {
                    bg = this.adBGManager.GetChild(i).gameObject;
                }
                float aspect = texs[i].width / texs[i].height;

                bg.GetComponentInChildren<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                bg.GetComponentInChildren<AspectRatioFitter>().aspectRatio = aspect; 

                bg.GetComponentInChildren<RawImage>().texture = tex;
                bg.SetActive(true);

                BGs.Add(bg.transform.GetChild(0).gameObject);
            }
#endif 

            return BGs;
        }

        public override void Show(int ID = -1, float MinX = 0, float MinY = 0, float MaxX = 1, float MaxY = 1)
        {
            Transform parent = this.transform.parent;
            this.AdID = ID;
            if (!this.rectTransform)
                this.rectTransform = this.GetComponent<RectTransform>();
            Debug.Log($"{CONSTANT.Prefix}==> Set parent native " + parent.name);
            this.transform.SetParent(parent);
            this.transform.parent.GetOrAddComponent<DestroyListener>().Init(ID);
            this.rectTransform.anchorMax = new Vector2(MaxX, MaxY);
            this.rectTransform.anchorMin = new Vector2(MinX, MinY);
            this.transform.localScale = Vector3.one;
            this.transform.localPosition = Vector3.zero;
            this.transform.localEulerAngles = Vector3.zero;
            this.rectTransform.sizeDelta = Vector2.zero;
            this.rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0); // Bottom
            this.rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0); // Top

            if (!this.gameObject.activeInHierarchy)
                Debug.LogError("You must active parent BEFORE call show ad native!");
            StartCoroutine(wait(parent.gameObject.activeSelf));
        }



        IEnumerator wait(bool isParentActive)
        {
            yield return new WaitForEndOfFrame();
            if (!isParentActive)
                this.transform.parent.gameObject.SetActive(true);

            adIcon.rectTransform.anchoredPosition = Vector2.zero;
            adIcon.rectTransform.sizeDelta = Vector2.zero;
            adIcon.GetComponent<BoxCollider2D>().size = adIcon.rectTransform.rect.size;

            adChoice.rectTransform.anchoredPosition = Vector2.zero;
            adChoice.rectTransform.sizeDelta = Vector2.zero;
            adChoice.GetComponent<BoxCollider2D>().size = adChoice.rectTransform.rect.size;

            callToAction.rectTransform.anchoredPosition = Vector2.zero;
            callToAction.rectTransform.sizeDelta = Vector2.zero;
            callToAction.GetComponent<BoxCollider2D>().size = callToAction.rectTransform.rect.size;

            advertiser.rectTransform.anchoredPosition = Vector2.zero;
            advertiser.rectTransform.sizeDelta = Vector2.zero;
            advertiser.GetComponent<BoxCollider2D>().size = advertiser.rectTransform.rect.size;

            headLine.rectTransform.anchoredPosition = Vector2.zero;
            headLine.rectTransform.sizeDelta = Vector2.zero;
            headLine.GetComponent<BoxCollider2D>().size = headLine.rectTransform.rect.size;

            body.rectTransform.anchoredPosition = Vector2.zero;
            body.rectTransform.sizeDelta = Vector2.zero;
            body.GetComponent<BoxCollider2D>().size = body.rectTransform.rect.size;

            price.rectTransform.anchoredPosition = Vector2.zero;
            price.rectTransform.sizeDelta = Vector2.zero;
            price.GetComponent<BoxCollider2D>().size = price.rectTransform.rect.size;

            store.rectTransform.anchoredPosition = Vector2.zero;
            store.rectTransform.sizeDelta = Vector2.zero;
            store.GetComponent<BoxCollider2D>().size = store.rectTransform.rect.size;

            for (int i = 0; i < adBGManager.childCount; i++)
            {
                var tmp = adBGManager.GetChild(i).GetChild(0);
                var rect = tmp.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                tmp.GetComponent<BoxCollider2D>().size = rect.rect.size;
            }

            //CheckCanvas();

            if (!isParentActive)
                this.transform.parent.gameObject.SetActive(false);
        }

        public override GameObject SetIconTexture(Texture2D tex)
        {
            this.adIcon.texture = tex;
            return this.adIcon.gameObject;
        }

        public override GameObject SetHeadlineText(string headline)
        {
            headLine.text = headline;
            return headLine.gameObject;
        }

        public override GameObject SetAdChoicesLogoTexture(Texture2D iconChoice)
        {
            if (iconChoice == null)
            {
                adChoice.gameObject.SetActive(false);
                return null;
            }
            adChoice.texture = iconChoice;
            return adChoice.gameObject;
        }

        public override GameObject SetCallToActionText(string CTAText)
        {
            if (CTAText == null)
            {
                callToAction.gameObject.SetActive(false);
                return null;
            }
            callToAction.text = CTAText;
            return callToAction.gameObject;
        }

        public override GameObject SetAdvertiserText(string advertiseText)
        {
            if (advertiseText == null)
            {
                advertiser.gameObject.SetActive(false);
                return null;
            }
            advertiser.text = advertiseText;
            return advertiser.gameObject;
        }

        public override GameObject SetBodyText(string bodyText)
        {
            if (bodyText == null)
            {
                body.gameObject.SetActive(false);
                return null;
            }
            body.text = bodyText;
            return body.gameObject;
        }

        public override GameObject SetPrice(string priceText)
        {
            if (priceText == null)
            {
                price.gameObject.SetActive(false);
                return null;
            }
            price.text = priceText;
            return price.gameObject;
        }

        public override GameObject SetStore(string storeText)
        {
            if (storeText == null)
            {
                store.gameObject.SetActive(false);
                return null;
            }
            store.text = storeText;
            return store.gameObject;
        }

        public void Close()
        {
            AdBridge.Instant.HideAd(AD_TYPE.Native, this.AdID);
        }


    }
}