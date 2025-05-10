using System.Collections;
using UnityEngine;

namespace DVAH
{
    public class ControlNativeTrick : MonoBehaviour
    {
        #if FIREBASE_IMPLEMENT
        void Start()
        {
            StartCoroutine(waitConfigTrickRate());
        }

        IEnumerator waitConfigTrickRate()
        {
            yield return new WaitUntil(() => FireBaseBridge.Instant.isFetchDOne && AdBridge.Instant.isInitDone());
            yield return new WaitForSeconds(2);
            var nativeObject = AdBridge.Instant.getNativeObject();
            foreach (var item in nativeObject)
            {
                if (item.Value.GetComponentInParent<AdBridge>() == null)
                {
                    continue;
                }
                if (Random.Range(0, 100) >= AdBridge.Instant.DVAH_Data.NativeTrickRate)
                {
                    continue;
                }
                item.Value.transform.position = Vector2.zero;
            }

        }
        #endif
    }
}