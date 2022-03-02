using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
#endif

public class iOSBuildPostprocessor
{
	[PostProcessBuildAttribute(900)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		if (target != BuildTarget.iOS)
			return;
		DoPostProcess(pathToBuiltProject);
	}

	public static void DoPostProcess(string pathToBuiltProject)
	{
#if UNITY_EDITOR_OSX
		string projPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");
		PBXProject proj = new PBXProject();
		proj.ReadFromFile(projPath);

		string mainTargetGuid;
		string unityFrameworkTargetGuid;

		var unityMainTargetGuidMethod = proj.GetType().GetMethod("GetUnityMainTargetGuid");
		var unityFrameworkTargetGuidMethod = proj.GetType().GetMethod("GetUnityFrameworkTargetGuid");

		if (unityMainTargetGuidMethod != null && unityFrameworkTargetGuidMethod != null)
		{
			mainTargetGuid = (string)unityMainTargetGuidMethod.Invoke(proj, null);
			unityFrameworkTargetGuid = (string)unityFrameworkTargetGuidMethod.Invoke(proj, null);
		}
		else
		{
			mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
			unityFrameworkTargetGuid = mainTargetGuid;
		}

		if (mainTargetGuid != unityFrameworkTargetGuid)
		{
			proj.SetBuildProperty(unityFrameworkTargetGuid, "ENABLE_BITCODE", "false");
		}

		proj.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "false");
		proj.AddBuildProperty(mainTargetGuid, "OTHER_LDFLAGS", "-ObjC");

		proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", "iPhone Developer: Andrei Dabija (J2B3YGXMHG)");
		proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Developer");
		proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", "GAMELOFT317");
		proj.SetTeamId(mainTargetGuid, "A4QBZ46HAP");

		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "WebKit.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "StoreKit.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "MobileCoreServices.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "MultipeerConnectivity.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "GLKit.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "MessageUI.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "CoreMotion.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "CoreTelephony.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "Accelerate.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "AdSupport.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "MediaPlayer.framework", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "Metal.framework", true);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "iAd.framework", true);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "Photos.framework", true);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "AssetsLibrary.framework", true);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "libxml2.tbd", false);
		proj.AddFrameworkToProject(unityFrameworkTargetGuid, "libz.tbd", false);

		proj.WriteToFile(projPath);

		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(pathToBuiltProject + "/Info.plist"));
		plist.root.SetString("AppLovinSdkKey", "ZTwMEyV5uBrsFT8m-QPTDxKQFt7tesSCONZdK4iCFT3V0BF_cfTrauGrmNiOE9bdW9FsEz2TW5-WHDWvt4lSz_");
		plist.root.SetString("GADApplicationIdentifier", "ca-app-pub-5607002036640398~9779416206");
		File.WriteAllText(pathToBuiltProject + "/Info.plist", plist.WriteToString());
#endif
	}
}
