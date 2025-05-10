using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DVAH
{
     public class AdUnitModuleNativeOverlay_Base : AdUnitModule
     {
          public override AdUnitModule Init(Init_ ModuleSDK, AD_TYPE aD_TYPE, List<string> unitIDs)
          {
               base.Init(ModuleSDK, aD_TYPE, unitIDs);

               foreach (string s in unitIDs)
               {
                    int ID = _unitIDs.IndexOf(s);
                    Load(ID);
               }

               return this;
          }

     #region LOAD/SHOW

          public override void Hide(int ID = 0)
          {
               throw new System.NotImplementedException();
          }

          public override bool IsLoaded(int ID = 0)
          {
               throw new System.NotImplementedException();
          }

          public override void Load(int ID = 0)
          {
               throw new System.NotImplementedException();
          }

     #endregion
     }
}