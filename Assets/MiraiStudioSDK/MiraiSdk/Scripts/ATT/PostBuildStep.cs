#if UNITY_EDITOR && UNITY_IOS

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using Utilities;
using System;
using System.Diagnostics;

namespace DVAH{
public class PostBuildStep
{
    // Set the IDFA request description:
    const string k_TrackingDescription = "We try to show ads for apps and products that will be most interesting to you based on the apps you use, the device you are on, and the country you are in.";

    [PostProcessBuild(0)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPListValues(pathToXcode);
        }
    }

    // Implement a function to read and write values to the plist file:
    static void AddPListValues(string pathToXcode)
    {
        // Retrieve the plist file from the Xcode project directory:
        string plistPath = pathToXcode + "/Info.plist";
        PlistDocument plistObj = new PlistDocument();


        // Read the values from the plist file:
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // Set values from the root object:
        PlistElementDict plistRoot = plistObj.root;

        // Set the description key-value in the plist:
        plistRoot.SetString("NSUserTrackingUsageDescription", k_TrackingDescription);
        plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption",false);

#if IRONSOURCE_IMPLEMENT
        
        var dict = plistRoot.CreateDict("NSAppTransportSecurity");
        dict.SetBoolean("NSAllowsArbitraryLoads",true);

         PlistElementArray SKAdNetworkItems = null;
        if (plistRoot.values.ContainsKey("SKAdNetworkItems"))
        {
            try
            {
                SKAdNetworkItems = plistRoot.values["SKAdNetworkItems"] as PlistElementArray;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(string.Format("Could not obtain SKAdNetworkItems PlistElementArray: {0}", e.Message));
            }
        }

        //Add IronSource's SKAdNetwork ID
        if (SKAdNetworkItems == null)
        {
            SKAdNetworkItems = plistRoot.CreateArray("SKAdNetworkItems");
        }

        string plistContent = File.ReadAllText(plistPath);
        if (!plistContent.Contains(IronSourceConstants.IRONSOURCE_SKAN_ID_KEY))
        {
            PlistElementDict SKAdNetworkIdentifierDict = SKAdNetworkItems.AddDict();
            SKAdNetworkIdentifierDict.SetString("SKAdNetworkIdentifier", IronSourceConstants.IRONSOURCE_SKAN_ID_KEY);
        }
#endif
        // Save changes to the plist:
        File.WriteAllText(plistPath, plistObj.WriteToString());

    }

}
}
#endif