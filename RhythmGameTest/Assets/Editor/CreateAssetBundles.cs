using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
    #if UNITY_IOS
        string assetBundleDirectory = Application.streamingAssetsPath + Path.DirectorySeparatorChar +
                                "AssetBundles" + Path.DirectorySeparatorChar + BuildTarget.Android;
        if(Directory.Exists(assetBundleDirectory))
        {
            Directory.Delete(assetBundleDirectory, true);
        }
        
        assetBundleDirectory = Application.streamingAssetsPath + Path.DirectorySeparatorChar +
                                "AssetBundles" + Path.DirectorySeparatorChar + BuildTarget.iOS;
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
    #else
        //make another bundle pack to run on PC
        string assetBundleDirectory = "Assets/AssetBundles" + Path.DirectorySeparatorChar + BuildTarget.StandaloneWindows64;
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

        assetBundleDirectory = Application.streamingAssetsPath + Path.DirectorySeparatorChar +
                                "AssetBundles" + Path.DirectorySeparatorChar + BuildTarget.Android;
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    #endif
    }
}