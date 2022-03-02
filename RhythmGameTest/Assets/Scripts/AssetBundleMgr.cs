using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class AssetBundleMgr : MonoBehaviour
{
    #region Instance
    private static AssetBundleMgr instance;
    public static AssetBundleMgr Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AssetBundleMgr>();
                if (instance == null)
                {
                    instance = new AssetBundleMgr();
                }
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion
    //json file that store asset bundle info
    [System.Serializable]
    public class AssetBundleInfo
    {
        public string name;
        public long fileSize;
        public uint crc32;
        public bool isDownloaded;
    }
    [System.Serializable]
    public class AssetBundleData
    {
        public List<AssetBundleInfo> list;
    }
    ////////////////////////////////////////

    public static readonly string ASSET_BUNDLE_JSON_LINK = "https://game-portal.gameloft.com/2093/sites/queen_local/dlc/assetbundleinfo.json";
    private string ASSET_BUNDLE_JSON_LINK_LOCAL = "";
    public static readonly string ASSET_BUNDLE_DATA_LINK = "https://game-portal.gameloft.com/2093/sites/queen_local/dlc";

    AssetBundleData bundleData;
    List<AssetBundleInfo> needDownloadList = new List<AssetBundleInfo>();
    AssetBundle myLoadedMusicData;
    AssetBundle myLoadedStage;
    AssetBundle myLoadedCommonMaterials;
    string bundleLocalPath = "";
    bool isDownloading = false;
    int currentBundleDownloadIndex = 0;
    long fileSizeDownloading = 0;
    long fileSizeDownloadSucess = 0;
    float totalSizeDownload = 0f;



    void Start()
    {
    #if UNITY_ANDROID
        bundleLocalPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/Android");
    #elif UNITY_IOS
        bundleLocalPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles" + Path.DirectorySeparatorChar + "iOS");
    #endif
    //on PC always get StandaloneWindows64 path
    #if UNITY_STANDALONE
        bundleLocalPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/StandaloneWindows64");
    #endif
    #if UNITY_EDITOR
        bundleLocalPath = Path.Combine(Application.dataPath, "AssetBundles/StandaloneWindows64");
    #endif
        ASSET_BUNDLE_JSON_LINK_LOCAL = Path.Combine(Application.persistentDataPath, "assetbundleinfo.json");

        WriteABJson();
        bundleData = new AssetBundleData();
    }

    void WriteABJson()
    {
    //     //RUN ONCE. use this to create json file after data updated
    // #if UNITY_ANDROID
    //     bundleLocalPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/Android");
    // #elif UNITY_IOS
    //     bundleLocalPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles" + Path.DirectorySeparatorChar + "iOS");
    // #endif
    //     AssetBundleData savedData = new AssetBundleData();
    //     savedData.list = new List<AssetBundleInfo>();

    //     string myPath = "Assets/AssetBundles/WriteInfo";
    //     DirectoryInfo dir = new DirectoryInfo(myPath);
    //     FileInfo[] fileInfo = dir.GetFiles("*");
    //     foreach (FileInfo f in fileInfo)
    //     {
    //         if(f.Name.Contains(".meta")) continue;
    //         if(f.Name == "rainbow" || f.Name == "common_texture" || f.Name == "kyal" || f.Name == "ssor") continue;
    //         AssetBundleInfo info = new AssetBundleInfo();
    //         info.name = f.Name;
    //         info.fileSize = f.Length;
    //         uint crc;
    //         BuildPipeline.GetCRCForAssetBundle(Path.Combine(bundleLocalPath, f.Name), out crc);
    //         info.crc32 = crc;
    //         info.isDownloaded = false;
    //         savedData.list.Add(info);
    //     }
    //     Debug.Log("WriteABJson"+savedData.list.Count);
    //     string saveString = JsonUtility.ToJson(savedData);
    //     File.WriteAllText("Assets/assetbundleinfo.json", saveString);
    }

    public void LoadABJson(Action<float> callback)
    {
        StartCoroutine(DownloadABInfo(downloadSize => {callback(downloadSize);}));
    }
    public AssetBundleData LoadABJsonLocal()
    {
        string jsonData = File.ReadAllText(ASSET_BUNDLE_JSON_LINK_LOCAL);
        return JsonUtility.FromJson<AssetBundleData>(jsonData);
    }

    public IEnumerator DownloadABInfo(Action<float> callback)
    {
        UnityWebRequest uwr = new UnityWebRequest(ASSET_BUNDLE_JSON_LINK);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        yield return uwr.SendWebRequest();

        if(uwr.isNetworkError || uwr.isHttpError) {
            Debug.Log(uwr.error);
            callback(-1f);
        }
        else {
            // Debug.Log(uwr.downloadHandler.text);
            AssetBundleData serverBundleData = JsonUtility.FromJson<AssetBundleData>(uwr.downloadHandler.text);
            if(File.Exists(ASSET_BUNDLE_JSON_LINK_LOCAL))
            {
                bundleData = LoadABJsonLocal();
                CreateDownloadList(serverBundleData);
            }
            else
            {
                bundleData = serverBundleData;
                needDownloadList = bundleData.list;
                File.WriteAllText(ASSET_BUNDLE_JSON_LINK_LOCAL, uwr.downloadHandler.text);
            }

            float downloadSize = CalculateDownloadSize();
            totalSizeDownload = downloadSize;
            callback(downloadSize);
        }
    }

    private void CreateDownloadList(AssetBundleData serverBundleData)
    {
        needDownloadList.Clear();
        for(int i = 0; i < serverBundleData.list.Count; i++)
        {
            AssetBundleInfo abInfoServer = serverBundleData.list[i];
            AssetBundleInfo abInfoLocal = bundleData.list[i];
            if(abInfoServer.crc32 != abInfoLocal.crc32 ||
            !abInfoLocal.isDownloaded || i >= bundleData.list.Count || !File.Exists(Path.Combine(Application.persistentDataPath, abInfoLocal.name)))
            {
                needDownloadList.Add(abInfoServer);
            }
        }
        // DownloadBundle();
    }

    public float CalculateDownloadSize()
    {
        float totalSize = 0f;
        foreach(AssetBundleInfo abInfo in needDownloadList)
        {
            totalSize += abInfo.fileSize;
        }
        return totalSize / 1024f / 1024f;
    }

    public void DownloadBundle()
    {
        if(needDownloadList.Count == 0) return;

        isDownloading = true;
        currentBundleDownloadIndex = 0;
        fileSizeDownloadSucess = 0;
        fileSizeDownloading = 0;
        StartCoroutine(LoadFileFromServer(needDownloadList[currentBundleDownloadIndex]));
    }
    void DownloadNextFile()
    {
        //mark as download success and save to local bundle json
        for(int i = 0; i < bundleData.list.Count; i++)
        {
            if(bundleData.list[i].name == needDownloadList[currentBundleDownloadIndex].name)
            {
                bundleData.list[i] = needDownloadList[currentBundleDownloadIndex];
                bundleData.list[i].isDownloaded = true;
                fileSizeDownloadSucess += bundleData.list[i].fileSize;
                fileSizeDownloading = 0;
                string saveString = JsonUtility.ToJson(bundleData);
                File.WriteAllText(ASSET_BUNDLE_JSON_LINK_LOCAL, saveString);
                break;
            }
        }

        currentBundleDownloadIndex++;
        if(currentBundleDownloadIndex >= needDownloadList.Count)
        {
            //finish download
            isDownloading = false;
        }
        else
        {
            StartCoroutine(LoadFileFromServer(needDownloadList[currentBundleDownloadIndex]));
        }
    }

    public IEnumerator LoadFileFromServer(AssetBundleInfo abInfo)
    {
        if(!HasEnoughSpace(abInfo.fileSize))
        {
            isDownloading = false;
            Game.Instance.OnDLCError(Define.DLC_ERROR_TYPE.NOT_ENOUGH_SPACE);
            yield break;
        }
        string url = Path.Combine(ASSET_BUNDLE_DATA_LINK, abInfo.name);
        string savePath = Path.Combine(Application.persistentDataPath, abInfo.name);
        DownloadHandlerFile dh = new DownloadHandlerFile(savePath);
        dh.removeFileOnAbort = true;
        UnityWebRequest uwr = new UnityWebRequest(url);
        uwr.downloadHandler = dh;
        UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();
        while (!operation.isDone)
        {
            Debug.Log("LoadFileFromServer Loading progress: "+uwr.downloadProgress);
            fileSizeDownloading = (long)(uwr.downloadProgress * needDownloadList[currentBundleDownloadIndex].fileSize);
            if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                Game.Instance.OnDLCError(Define.DLC_ERROR_TYPE.NETWORK_ERROR);
                isDownloading = false;
                uwr.Abort();
            }
            yield return null;
        }
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Game.Instance.OnDLCError(Define.DLC_ERROR_TYPE.NETWORK_ERROR);
            isDownloading = false;
            Debug.Log("LoadFileFromServer error: "+uwr.error);
        }
        else
        {
            Debug.Log("LoadFileFromServer Done@!");
            DownloadNextFile();
        }
    }

    public AsyncOperation LoadVenueBundle(string venueName)
    {
        string venuePath = GetVenuePath(venueName);
        myLoadedCommonMaterials = AssetBundle.LoadFromFile(Path.Combine(bundleLocalPath, "common_texture"));
        myLoadedStage = AssetBundle.LoadFromFile(venuePath);
        if (myLoadedStage == null) {
            Debug.Log("Failed to load AssetBundle scenes!");
            return null;
        }
        string[] scenePaths = myLoadedStage.GetAllScenePaths();
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadAllBundle()
    {
        myLoadedCommonMaterials.Unload(true);
        myLoadedStage.Unload(true);
        myLoadedMusicData.Unload(true);
    }

    public float GetAssetBundleProgress()
    {
        return GetProgressInMB()/totalSizeDownload;
    }

    public float GetProgressInMB()
    {
        return ((fileSizeDownloading + fileSizeDownloadSucess)/1024f/1024f);
    }

    public bool IsDownloading()
    {
        return isDownloading;
    }
    private bool HasEnoughSpace(long fileSize)
    {
        return true;
        // float sizeNeed = fileSize / 1024f / 1024f;
        // int freeSpaceMB = SimpleDiskUtils.DiskUtils.CheckAvailableSpace();
        // return freeSpaceMB > (sizeNeed + 1);
    }

    private string GetSongPath(string songName)
    {
        string parentPath = Application.persistentDataPath;
        if(songName == "kyal" || songName == "ssor")
        {
        Debug.Log("bundleLocalPath: "+bundleLocalPath);
            parentPath = bundleLocalPath;
        }
        Debug.Log("parentPath: "+parentPath);
        return Path.Combine(parentPath, songName);
    }
    private string GetVenuePath(string venueName)
    {
        string parentPath = Application.persistentDataPath;
        if(venueName == "rainbow")
        {
            parentPath = bundleLocalPath;
        }
        return Path.Combine(parentPath, venueName);
    }
}
