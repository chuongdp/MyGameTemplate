#if ADMOB_IMPLEMENT
using DVAH.Ad.Native;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DVAH
{
    [System.Serializable]
    public class DictNativeObject : SerializableDictionary<int, AdNativeObject>
    {

    }
    public class AdUnitModuleNative_Admob : AdUnitModule
    {
        [SerializeField]
        DictNativeObject _nativeobjects = new DictNativeObject();
        public DictNativeObject NativeObjects => _nativeobjects;
        Dictionary<int, NativeAd> _nativeAds = new Dictionary<int, NativeAd>();
        Dictionary<int, AdLoader> _nativeADLoader = new Dictionary<int, AdLoader>();
        Dictionary<int, float> _nativeRetryAttemps = new Dictionary<int, float>();

        GameObject nativeTemplate;

        Coroutine refreshCapping;

#if UNITY_EDITOR
        Dictionary<int, bool> _fakeEditor = new Dictionary<int, bool>();
#endif

        public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
        {
            base.Init(ModuleSDK, aD_TYPE, unitIDs);

            Camera camera = (new GameObject("CamNative")).AddComponent<Camera>();
            camera.transform.SetParent(this.transform);
            camera.transform.localPosition = new Vector3(-10000, -10000, -10500);
            camera.orthographic = true;
            camera.orthographicSize = 1800;
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.depth = -100;

            nativeTemplate = Resources.Load<GameObject>("AD/AdNative");
            for (int i = 0; i < _unitIDs.Count; i++)
            {
                var tmp = this._moduleSDK.DVAH_Data.adUnits[AD_TYPE.Native].AdUnitDatas[i];
                if (tmp.network != AD_NETWORK.Admob)
                    continue;
                var tmpPrefab = tmp.Prefab == null ? nativeTemplate : tmp.Prefab as GameObject;
                _nativeADLoader.Add(i, null);
                _nativeAds.Add(i, null);
                _nativeRetryAttemps.Add(i, 1);
#if UNITY_EDITOR
                _fakeEditor.Add(i, false);
#endif
                if (!_nativeobjects.ContainsKey(i) || _nativeobjects[i] == null)
                {
                    try
                    {
                      _nativeobjects[i]                      = Instantiate(tmpPrefab, this.transform).GetComponent<AdNativeObject>();
                        _nativeobjects[i].transform.position = new Vector3(-10000, -10000, -10000);
                        _nativeobjects[i].gameObject.SetActive(true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{CONSTANT.Prefix}===> Init Adnative object {i} error: {e}");
                    }
                }
                Load(i);
            }

            this.gameObject.AddComponent<ControlNativeTrick>();
         //   this.GetOrAddComponent<ControlNativeTrick>();
            return this;
        }

        #region Native handle
        IEnumerator waitReloadAd(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }

        public AdLoader RequestNativeAd(string AdID)
        {
            AdLoader adLoader = new AdLoader.Builder(AdID)
                .ForNativeAd()
                .Build();

            adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;
            adLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
            adLoader.OnNativeAdImpression += this.HandleNativeAdImpression;
            adLoader.OnNativeAdClicked += this.AdLoader_OnNativeAdClicked;

            adLoader.LoadAd(new AdRequest());
            return adLoader;
        }


        public bool CreateNativeAd(int adNativeID)
        {
            Debug.Log(CONSTANT.Prefix + $"===>set object native " + adNativeID + " <===");
#if UNITY_EDITOR

            //_nativeobjects[adNativeID].bodyGO.text = "<color=blue>" + this._unitIDs[adNativeID] + "</color>\n";
            _nativeobjects[adNativeID].setAdBG(new Texture2D[3].ToList());

            return true;
#endif

            GameObject tmp = null;
            List<Texture2D> imagetexture = this._nativeAds[adNativeID].GetImageTextures();
            if (imagetexture.Any())
            {
                List<GameObject> Bgs = _nativeobjects[adNativeID].setAdBG(imagetexture);

                this._nativeAds[adNativeID].RegisterImageGameObjects(Bgs);
            }


            Texture2D iconTexture = this._nativeAds[adNativeID].GetIconTexture();
            tmp = _nativeobjects[adNativeID].SetIconTexture(iconTexture);

            if (tmp != null && !this._nativeAds[adNativeID].RegisterIconImageGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register adIcon error <====");
                return false;
            }

            string headLineText = this._nativeAds[adNativeID].GetHeadlineText();
            tmp = _nativeobjects[adNativeID].SetHeadlineText(headLineText);
            if (tmp != null && !this._nativeAds[adNativeID].RegisterHeadlineTextGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register adHeadline error <====");
                return false;
            }

            Texture2D iconChoice = this._nativeAds[adNativeID].GetAdChoicesLogoTexture();
            tmp = _nativeobjects[adNativeID].SetAdChoicesLogoTexture(iconChoice);
            if (tmp != null && !this._nativeAds[adNativeID].RegisterAdChoicesLogoGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register adChoiceIcon error <====");

                return false;
            }

            string CTAText = this._nativeAds[adNativeID].GetCallToActionText();
            if (!string.IsNullOrEmpty(CTAText))
            {
                tmp = _nativeobjects[adNativeID].SetCallToActionText(CTAText);
                if (tmp != null && !this._nativeAds[adNativeID].RegisterCallToActionGameObject(tmp))
                {
                    Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register CTA error <====");
                    return false;
                }
            }

            string advertiseText = this._nativeAds[adNativeID].GetAdvertiserText();
            tmp = _nativeobjects[adNativeID].SetAdvertiserText(advertiseText);
            if (tmp != null && !this._nativeAds[adNativeID].RegisterAdvertiserTextGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register advertise text error!<====");
                return false;
            }

            string bodyText = this._nativeAds[adNativeID].GetBodyText();
            tmp = _nativeobjects[adNativeID].SetBodyText(bodyText);
            if (tmp != null && !this._nativeAds[adNativeID].RegisterBodyTextGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register body text error!<====");
                return false;
            }

            string priceText = this._nativeAds[adNativeID].GetPrice();
            tmp = _nativeobjects[adNativeID].SetPrice(priceText);
            if (tmp != null && !this._nativeAds[adNativeID].RegisterPriceGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register price text error!<====");
                return false;
            }

            string storeText = this._nativeAds[adNativeID].GetStore();
            tmp = _nativeobjects[adNativeID].SetStore(storeText);
            if (tmp != null && !this._nativeAds[adNativeID].RegisterStoreGameObject(tmp))
            {
                Debug.LogError(CONSTANT.Prefix + $"===> Native Ad register store text error!<====");
                return false;
            }

            return true;
        }

        public DictNativeObject CreateNativeAd(int adNativeID, AdNativeObject_UI prefab)
        {
            this._nativeobjects[adNativeID] = Instantiate(prefab, this.transform);
            if (this.IsLoaded(adNativeID))
                this.CreateNativeAd(adNativeID);

            return this.NativeObjects;
        }
        private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            int ID = _nativeADLoader.FirstOrDefault(x => x.Value == (AdLoader)sender).Key;
            Debug.LogError(CONSTANT.Prefix + $"===> NativeAd {ID} load Fail! error: " + e.LoadAdError.GetMessage());

            if (ID < 0)
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> HandleAdFailedToLoad cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
                return;
            }

            if (_nativeRetryAttemps[ID] >= 4)
                return;

            _nativeRetryAttemps[ID] *= 2;
            double retryDelay = _nativeRetryAttemps[ID];
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, _unitIDs[ID], null, _placement);
            StartCoroutine(waitReloadAd((float)retryDelay, () =>
            {
                this.Load(ID);
            }));
        }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        {
            Debug.Log(CONSTANT.Prefix + $"===> Native ad loaded.");

            int ID = getID(sender);
            if (ID < 0)
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> HandleAdLoaded cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
                return;
            }

            if (!_nativeADLoader[ID].AdUnitId.Equals(_unitIDs[ID]))
            {
                Debug.LogErrorFormat(CONSTANT.Prefix + "===> adloaderID {0} doesnt == senderID {1}", _nativeADLoader[ID].AdUnitId, ((AdLoader)sender).AdUnitId);
                return;
            }

            this._nativeAds[ID] = e.nativeAd;

            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, ID, null, _placement);

            _nativeRetryAttemps[ID] = 1;

            if (this.CreateNativeAd(ID))
            {
                Debug.Log(CONSTANT.Prefix + $"===> Native {_unitIDs[ID]} create object success!.");
            }
            else
            {
                Load(ID);
            }
        }

        private void HandleNativeAdImpression(object sender, EventArgs e)
        {
            Debug.Log(CONSTANT.Prefix + $"===> Handle ad native impression! ");
        }


        private void AdLoader_OnNativeAdClicked(object sender, EventArgs e)
        {
            Debug.Log(CONSTANT.Prefix + $"===> Handle ad native clicked! ");

            int ID = _nativeADLoader.FirstOrDefault(x => x.Value == (AdLoader)sender).Key;
            this.InvokeCallback(ID, AdUnitState.Click);
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, _unitIDs[ID], null, _placement);

        }

        int getID(object adLoader)
        {
            foreach (var item in _nativeADLoader)
            {
                if (item.Value.Equals(adLoader))
                    return item.Key;
            }

            Debug.LogError(CONSTANT.Prefix + $"Can not find Adloader");
            return -1;
        }


        #endregion

        #region LOAD/SHOW
        public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
        {
            base.Show(ID, callback);
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPreOpen, _unitIDs[ID], null, _placement);
            _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnShowed, _unitIDs[ID], null, _placement);

            this._nativeobjects[ID].gameObject.SetActive(true);
            this.InvokeCallback(ID, AdUnitState.Open);

            if (!this.IsLoaded(ID))
                return;
            if (!this._moduleSDK.AdManager.CappingTimes.ContainsKey(AD_TYPE.Native))
                return;
            if (!this._moduleSDK.AdManager.CappingTimes[AD_TYPE.Native].ContainsKey(ID))
                return;
            float cappingTime = (float)this._moduleSDK.AdManager.CappingTimes[AD_TYPE.Native][ID]?.Invoke();
            if (cappingTime <= 0)
                return;
            refreshCapping = StartCoroutine(waitReloadAd(cappingTime, () =>
            {
                this.Load(ID);
            }));
        }

        public override void Hide(int ID = 0)
        {
            if (!this._nativeobjects.ContainsKey(ID) || this._nativeobjects[ID] == null)
                return;
            this._nativeobjects[ID].transform.SetParent(this.transform);
            this._nativeobjects[ID].transform.position = new Vector3(-10000, -10000, -10000);
            if (refreshCapping == null)
                return;
            StopCoroutine(refreshCapping);
        }

        public override bool IsLoaded(int ID = 0)
        {
#if UNITY_EDITOR
            Debug.LogError("Check native loaded " + ID + "--" + this._fakeEditor[ID]);
            return this._fakeEditor[ID];
#endif
            return this._nativeAds[ID] != null;
        }

        public override void Load(int ID = 0)
        {
            Debug.Log(CONSTANT.Prefix + $"===>Start load Native {ID}_ " + this._unitIDs[ID] + " <====");
            this._nativeADLoader[ID] = this.RequestNativeAd(this._unitIDs[ID]);
            this._nativeAds[ID] = null;
#if UNITY_EDITOR
            this._fakeEditor[ID] = true;//Random.Range(0, 100) > 50 ? true : false;
            this.HandleNativeAdLoaded(this._nativeADLoader[ID], new NativeAdEventArgs());
#endif
        }


        #endregion
    }

}
#endif