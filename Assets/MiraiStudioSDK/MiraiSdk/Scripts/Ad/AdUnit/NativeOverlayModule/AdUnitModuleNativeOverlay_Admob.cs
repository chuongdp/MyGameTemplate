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
          public class AdUnitModuleNativeOverlay_Admob : AdUnitModule
          {
               Dictionary<int, NativeOverlayAd> _nativeOverlayAds = new Dictionary<int, NativeOverlayAd>();

               Dictionary<int, AdLoader> _nativeADLoader     = new Dictionary<int, AdLoader>();
               Dictionary<int, float>    _nativeRetryAttemps = new Dictionary<int, float>();

               GameObject nativeTemplate;

               int _isShowSuccess = -1;

               public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
               {
                    base.Init(ModuleSDK, aD_TYPE, unitIDs);

                    nativeTemplate = Resources.Load<GameObject>("AD/AdNative");
                    for (int i = 0; i < _unitIDs.Count; i++)
                    {
                         if (!unitIDs.Contains(_unitIDs[i]))
                              continue;

                         _nativeRetryAttemps.Add(i, 0);
                         _nativeOverlayAds.Add(i, null);

                         Load(i);
                    }

                    return this;
               }

          #region Native handle

               IEnumerator waitReloadAd(float delay, Action callback)
               {
                    yield return new WaitForSeconds(delay);

                    callback?.Invoke();
               }


               // private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
               // {
               //     Debug.Log(CONSTANT.Prefix + $"===> Native ad loaded.");


               //     int ID = getID(sender);
               //     if (ID < 0)
               //     {
               //         Debug.LogErrorFormat(CONSTANT.Prefix + "===> HandleAdLoaded cant find ID _{0}_ from sender", ((AdLoader)sender).AdUnitId);
               //         return;
               //     }

               //     if (!_nativeADLoader[ID].AdUnitId.Equals(_unitIDs[ID]))
               //     {
               //         Debug.LogErrorFormat(CONSTANT.Prefix + "===> adloaderID {0} doesnt == senderID {1}", _nativeADLoader[ID].AdUnitId, ((AdLoader)sender).AdUnitId);
               //         return;
               //     }

               //     this._nativeAds[ID] = e.nativeAd;
               //     _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, ID,null,_placement);

               //     if (this.CreateNativeAd(ID))
               //     {
               //        Debug.Log(CONSTANT.Prefix + $"===> Native {_unitIDs[ID]} create object success!.");
               //     }
               //     else
               //     {
               //         Load(ID);
               //     } 
               // }

               // private void HandleNativeAdImpression(object sender, EventArgs e)
               // {
               //     Debug.Log(CONSTANT.Prefix + $"===> Handle ad native impression! ");
               // }


               // private void AdLoader_OnNativeAdClicked(object sender, EventArgs e)
               // {
               //     Debug.Log(CONSTANT.Prefix + $"===> Handle ad native clicked! ");

               //     int ID = _unitIDs.IndexOf(((AdLoader)sender).AdUnitId);
               //     this.InvokeCallback(ID, AdUnitState.Click);
               //      _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClick, _unitIDs[ID],null,_placement);
               // }


               public void RenderAd(int ID = 0)
               {
                    if (_nativeOverlayAds[ID] == null)
                         return;
                    Debug.Log("Rendering Native Overlay ad.");

                    // Define a native template style with a custom style.
                    NativeTemplateStyle style = new NativeTemplateStyle
                    {
                         TemplateId          = "medium",
                         MainBackgroundColor = Color.white,
                         CallToActionText = new NativeTemplateTextStyle
                         {
                              BackgroundColor = Color.green,
                              TextColor       = Color.white,
                              FontSize        = 9,
                              Style           = NativeTemplateFontStyle.Bold,
                         },
                    };

                    // Renders a native overlay ad at the default size
                    // and anchored to the bottom of the screne.
                    _nativeOverlayAds[ID].RenderTemplate(style, new AdSize(Screen.width, Screen.height / 4), AdPosition.Bottom);
               }

          #endregion

               // void OnGUI()
               // {
               //      if (GUI.Button(new Rect(0, Screen.height / 4, Screen.width / 10, Screen.width / 20), "Close Ad"))
               //           Debug.Log("CCCCCCC");
               // }

          #region LOAD/SHOW

               public override void Show(int ID = 0, Action<int, AdUnitState> callback = null, string placement = null)
               {
                    base.Show(ID, callback);
                    _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnPreOpen, _unitIDs[ID], null, _placement);

                    RenderAd(ID);
                    _nativeOverlayAds[ID].Show();
                    _nativeOverlayAds[ID].SetTemplatePosition(AdPosition.Bottom);
               }

               public override void Hide(int ID = 0)
               {
                    if (!CheckID(ID))
                         return;

                    _nativeOverlayAds[ID].Hide();
               }

               public override bool IsLoaded(int ID = 0)
               {
                    return _nativeOverlayAds[ID] != null;
               }

               public override void Load(int ID = 0)
               {
                    // Clean up the old ad before loading a new one.
                    if (_nativeOverlayAds[ID] != null)
                    {
                         _nativeOverlayAds[ID].Destroy();
                    }

                    Debug.Log("Loading native overlay ad.");

                    // Create a request used to load the ad.
                    AdRequest adRequest = new AdRequest();

                    // Optional: Define native ad options.
                    NativeAdOptions options = new NativeAdOptions()
                    {
                         AdChoicesPlacement = AdChoicesPlacement.BottomLeftCorner,
                         MediaAspectRatio   = MediaAspectRatio.Any,
                    };


                    string _adUnitId = _unitIDs[ID];
                    // Send the request to load the ad.
                    NativeOverlayAd.Load(_adUnitId, adRequest, options, (NativeOverlayAd ad, LoadAdError error) =>
                    {
                         if (error != null)
                         {
                              Debug.LogError("Native Overlay ad failed to load an ad " + " with error: " + error);
                              Debug.LogError(CONSTANT.Prefix + $"===> NativeAd {_adUnitId} load Fail! error: " + error);
                              _nativeRetryAttemps[ID]++;
                              double retryDelay = Math.Pow(2, Math.Min(6, _nativeRetryAttemps[ID]));
                              _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoadFailed, _unitIDs[ID], null, _placement);
                              StartCoroutine(waitReloadAd((float)retryDelay, () => { Load(ID); }));
                              return;
                         }

                         // The ad should always be non-null if the error is null, but
                         // double-check to avoid a crash.
                         if (ad == null)
                         {
                              Debug.LogError("Unexpected error: Native Overlay ad load event "
                                           + " fired with null ad and null error.");
                              return;
                         }

                         // The operation completed successfully.
                         Debug.Log("Native Overlay ad loaded with response : " + ad.GetResponseInfo());
                         Debug.Log(CONSTANT.Prefix                             + $"===> Native {_adUnitId} ad loaded.");
                         _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnLoaded, ID, null, _placement);
                         _nativeRetryAttemps[ID] = 0;

                         ad.OnAdFullScreenContentOpened += () => { _isShowSuccess = ID; };

                         ad.OnAdFullScreenContentClosed += () =>
                         {
                              Debug.Log(CONSTANT.Prefix + $"===> Native {_adUnitId} ad Close.");
                              _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnClosed, ID, null, _placement);
                              Load(ID);
                              _isShowSuccess = -1;
                         };

                         ad.OnAdImpressionRecorded += () =>
                         {
                              Debug.Log(CONSTANT.Prefix + $"===> Native {_adUnitId} ad Impression.");
                              _moduleSDK.EventCallback(adFomart, AdUnitEvent.OnImpression, ID, null, _placement);
                              InvokeCallback(ID, AdUnitState.Impression);
                         };


                         _nativeOverlayAds[ID] = ad;
                    });
               }

          #endregion
          }
     }
#endif