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
    public class AdNativeObject_GO : AdNativeObject
    {
        public MeshRenderer adIcon, adChoice;
        public GameObject adBGFitter;

        public Transform adBGManager => adBGFitter.transform.parent;

        [SerializeField] protected Text callToAction, advertiser, headLine, body, price, store; 
 
          public int AdID;
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
                
                bg.GetComponentInChildren<MeshRenderer>().material.mainTexture = texs[0];
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
                //float aspect = texs[i].width / texs[i].height;
 
                bg.GetComponentInChildren<MeshRenderer>().material.mainTexture = tex;
                bg.SetActive(true);

                BGs.Add(bg.transform.GetChild(0).gameObject);
            }
#endif 

            return BGs;
        }

        public override GameObject SetIconTexture(Texture2D tex = null)
        {
            this.adIcon.material.mainTexture = tex;
            return this.adIcon.gameObject;
        }

  public override void Show(int ID = -1,float MinX = 1, float MinY = 1, float MaxX = 1, float MaxY = 1){
            Transform parent = this.transform.parent;
            this.AdID = ID;
            
            Debug.Log($"{CONSTANT.Prefix}==> Set parent native "+parent.name);
            this.transform.SetParent(parent);
            this.transform.parent.GetOrAddComponent<DestroyListener>().Init(ID);
            this.transform.localScale = Vector3.one;
            this.transform.localPosition = Vector3.zero;
            this.transform.localEulerAngles = Vector3.zero;  

            if(!this.gameObject.activeInHierarchy)
                Debug.LogError("You must active parent BEFORE call show ad native!");
            StartCoroutine(wait(parent.gameObject.activeSelf));
        }

        

        IEnumerator wait(bool isParentActive)
        {
            yield return new WaitForEndOfFrame();
            if(!isParentActive)
                this.transform.parent.gameObject.SetActive(true); 
            //adIcon.GetComponent<BoxCollider>().size = adIcon.bounds.size/2;
 
            //adChoice.GetComponent<BoxCollider>().size = adChoice.bounds.size/2;

            // callToAction.rectTransform.anchoredPosition = Vector2.zero;
            // callToAction.rectTransform.sizeDelta = Vector2.zero;
            // callToAction.GetComponent<BoxCollider>().size = callToAction.rectTransform.rect.size;

            // advertiser.rectTransform.anchoredPosition = Vector2.zero;
            // advertiser.rectTransform.sizeDelta = Vector2.zero;
            // advertiser.GetComponent<BoxCollider>().size = advertiser.rectTransform.rect.size;

            // headLine.rectTransform.anchoredPosition = Vector2.zero;
            // headLine.rectTransform.sizeDelta = Vector2.zero;
            // headLine.GetComponent<BoxCollider>().size = headLine.rectTransform.rect.size;

            // body.rectTransform.anchoredPosition = Vector2.zero;
            // body.rectTransform.sizeDelta = Vector2.zero;
            // body.GetComponent<BoxCollider>().size = body.rectTransform.rect.size;

            // price.rectTransform.anchoredPosition = Vector2.zero;
            // price.rectTransform.sizeDelta = Vector2.zero;
            // price.GetComponent<BoxCollider>().size = price.rectTransform.rect.size;

            // store.rectTransform.anchoredPosition = Vector2.zero;
            // store.rectTransform.sizeDelta = Vector2.zero;
            // store.GetComponent<BoxCollider>().size = store.rectTransform.rect.size;

            for (int i = 0; i < adBGManager.childCount; i++)
            {
                var tmp = adBGManager.GetChild(i).GetChild(0);
                //var rect = tmp.GetComponent<MeshRenderer>(); 
                //tmp.GetComponent<BoxCollider>().size = rect.bounds.size/2;
            }

            //CheckCanvas();

              if(!isParentActive)
                this.transform.parent.gameObject.SetActive(false);
        }


        public override GameObject SetHeadlineText(string headline = null)
        {
            throw new System.NotImplementedException();
        }

        public override GameObject SetAdChoicesLogoTexture(Texture2D tex = null)
        {
           this.adChoice.material.mainTexture = tex;
            return this.adChoice.gameObject;
        }

         public override GameObject SetCallToActionText(string CTAText)
        {
            if(CTAText == null){
                callToAction.gameObject.SetActive(false);
                return null;
            }
           callToAction.text = CTAText;
           return callToAction.gameObject;
        }

        public override GameObject SetAdvertiserText(string advertiseText)
        {
            if(advertiseText == null){
                advertiser.gameObject.SetActive(false);
                return null;
            }
           advertiser.text = advertiseText;
           return advertiser.gameObject;
        }

        public override GameObject SetBodyText(string bodyText)
        {
            if(bodyText == null){
                body.gameObject.SetActive(false);
                return null;
            }
            body.text = bodyText;
            return body.gameObject;
        }

        public override GameObject SetPrice(string priceText)
        {
            if(priceText == null){
                price.gameObject.SetActive(false);
                return null;
            }
            price.text = priceText;
            return price.gameObject;
        }

        public override GameObject SetStore(string storeText) {
            if(storeText == null){
                store.gameObject.SetActive(false);
                return null;
            }
           store.text = storeText;
           return store.gameObject;
        }
    }
}