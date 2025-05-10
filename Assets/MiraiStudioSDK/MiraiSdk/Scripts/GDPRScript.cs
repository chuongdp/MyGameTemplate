using System.Collections.Generic;
using System.Threading.Tasks;
#if ADMOB_IMPLEMENT
using GoogleMobileAds.Ump.Api;
#endif
using UnityEngine;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif

namespace DVAH
{
    public class GDPRScript : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Awake() { DontDestroyOnLoad(this.gameObject); }

        private void Start()
        {
            this.ShowATT();

            _ = this.WaitATT();
        }

        private async Task WaitATT()
        {
#if UNITY_IOS
            Debug.Log($"Miraigames: Waiting for ATT");
            while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                await Task.Delay(100);

            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED) return;
#endif
            await Task.Delay(100);

#if !UNITY_EDITOR && ADMOB_IMPLEMENT
            UMP();
#endif
        }

#if !UNITY_EDITOR && ADMOB_IMPLEMENT
        private ConsentForm _consentForm;

        private void UMP()
        {
            var debugSettings = new ConsentDebugSettings
            {
                // Geography appears as in EEA for debug devices.
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = new List<string>
                {
                    "F6F1E271D1D9104C4D69D6A14D7C34B4"
                }
            };

            // Here false means users are not under age.
            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings    = debugSettings
            };

            // Check the current consent information status.
            ConsentInformation.Update(request, this.OnConsentInfoUpdated);
        }

        private void OnConsentInfoUpdated(FormError error)
        {
            if (error != null)
            {
                // Handle the error.
                Debug.LogError(CONSTANT.Prefix + "==>GDPR OnConsentInfoUpdated Error: " + error);

                return;
            }

            if (ConsentInformation.IsConsentFormAvailable())
            {
                this.LoadConsentForm();
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
        }

        private void LoadConsentForm()
        {
            Debug.Log(CONSTANT.Prefix + "==>Call load consen");
            // Loads a consent form.
            ConsentForm.Load(this.OnLoadConsentForm);
        }

        private void OnLoadConsentForm(ConsentForm consentForm, FormError error)
        {
            if (error != null)
            {
                // Handle the error.
                Debug.LogError(CONSTANT.Prefix + "==>GDPR OnLoadConsentForm Error: " + error);

                return;
            }

            // The consent form was loaded.
            // Save the consent form for future requests.
            this._consentForm = consentForm;

            // You are now ready to show the form.
            if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
            {
                this._consentForm.Show(this.OnShowForm);
            }
        }

        private void OnShowForm(FormError error)
        {
            if (error == null) return;
            // Handle the error.
            Debug.LogError(CONSTANT.Prefix + "==> GDPR OnShowForm Error: " + error);
        }
#endif

        private void ShowATT()
        {
#if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) return;

            Debug.Log($"Miraigames: Requesting ATT");
            ATTrackingStatusBinding.RequestAuthorizationTracking();
#endif
        }
    }
}