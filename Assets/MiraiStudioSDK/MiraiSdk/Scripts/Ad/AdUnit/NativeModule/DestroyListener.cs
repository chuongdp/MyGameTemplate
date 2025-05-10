using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DVAH.Lib
{
     public class DestroyListener : MonoBehaviour
     {
          int _ID = -1;

          public void Init(int ID)
          {
               _ID = ID;
          }

          void OnDestroy()
          {
               if (_ID == -1)
                    return;
               AdBridge.Instant.HideAd(AD_TYPE.Native, _ID);
          }
     }
}