using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;
#endif
using UnityEditor;

public class LoadAssetBundles : MonoBehaviour
{
    #region Instance
    private static LoadAssetBundles instance;
    public static LoadAssetBundles Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LoadAssetBundles>();
                if (instance == null)
                {
                    instance = new LoadAssetBundles();
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

    [SerializeField] bool _useVueneFromEditor = false;

    AssetBundle myLoadedMusicData;
    AssetBundle myLoadedStage;
    AssetBundle myLoadedCommonTextures;
    AssetBundle venueMaterials;
    string bundlePath = "";
    void Start()
    {
    #if UNITY_ANDROID
        bundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/Android");
    #elif UNITY_IOS
        bundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles" + Path.DirectorySeparatorChar + "iOS");
    #endif
    //on PC always get StandaloneWindows64 path
    #if UNITY_STANDALONE
        bundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/StandaloneWindows64");
    #endif
    #if UNITY_EDITOR
        bundlePath = Path.Combine(Application.dataPath, "AssetBundles/StandaloneWindows64");
    #endif
    }

    public IEnumerator LoadAudioAsync(string[] audiosOfSong, string curSong, List<AudioClip> audioLst)
    {
        AssetBundleCreateRequest bundleLoadRequest = null;
        Object[] musicList;
        if(!_useVueneFromEditor)
        {
            bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(bundlePath, curSong));

            while(bundleLoadRequest.progress < 1)
            {
                LoadingManager.Instance.SetAudiosProgress(bundleLoadRequest.progress / 2);
                yield return null;
            }

            LoadingManager.Instance.SetAudiosProgress(bundleLoadRequest.progress / 2);
            yield return new WaitForSeconds(Define.SECOND_PER_FRAME);

            myLoadedMusicData = bundleLoadRequest.assetBundle;

            AssetBundleRequest assetLoadRequest = myLoadedMusicData.LoadAllAssetsAsync();
            while (assetLoadRequest.progress < 1)
            {
                LoadingManager.Instance.SetAudiosProgress((bundleLoadRequest.progress + assetLoadRequest.progress) / 2);
                yield return null;
            }

            musicList = assetLoadRequest.allAssets;
        }
        else
        {
            ResourceRequest resource = null;
            List<Object> objLst = new List<Object>();
            for(int v = 0; v < audiosOfSong.Length; ++v)
            {      
                if(audiosOfSong[v] != null && audiosOfSong[v].ToLower() != "null")
                {
                    resource = Resources.LoadAsync("Music/" + curSong + "/" + audiosOfSong[v]);
                    objLst.Add(resource.asset);
                    LoadingManager.Instance.SetAudiosProgress(1.0f * v / audiosOfSong.Length);
                }
                yield return null;
            }
            musicList = objLst.ToArray();
            resource.allowSceneActivation = true;
            LoadingManager.Instance.SetAudiosProgress(1);

            // musicList = Resources.LoadAll("Music/" + curSong);
        }

        
        foreach (Object obj in musicList)
        {
            for(int j = 0; j < audiosOfSong.Length; j++)
            {
                if(audiosOfSong[j].ToLower().CompareTo(obj.name.ToLower()) == 0)
                {
                    audioLst[j] = (obj as AudioClip);
                    LoadingManager.Instance.SetAudiosProgress(1.0f * j / audiosOfSong.Length);
                }
            }
        }
        LoadingManager.Instance.SetAudiosProgress(1f);

        yield return new WaitForSeconds(Define.WAIT_FOR_SECOND);                                
        {
            LoadingManager.Instance.UpdateLoadingAudiosStatus(true);
        }
    }

    public AsyncOperation LoadVenueBundle(string venueName)
    {
        Debug.Log("LoadAudioAsync path = " + Path.Combine(bundlePath, venueName));

        myLoadedCommonTextures = AssetBundle.LoadFromFile(Path.Combine(bundlePath, "common_texture"));
        string filePath = Path.Combine(bundlePath, venueName + "_materials");
        var materialsAB = AssetBundle.LoadFromFile(filePath);

        #if UNITY_EDITOR
        Material[] materials = materialsAB.LoadAllAssets<Material>();
        foreach(Material m in materials)
            {
                var shaderName = m.shader.name;
                var newShader = Shader.Find(shaderName);
                if(newShader != null)
                {
                    m.shader = newShader;
                }
                else
                {
                    Debug.LogWarning("unable to refresh shader: "+shaderName+" in material "+m.name);
                }
            }
        #endif

// #if UNITY_EDITOR
        if(this._useVueneFromEditor)
        {
            // string scenePathInEditor = "Assets/Scenes/"
            //                             + venueName.Substring(0,1).ToUpper() + venueName.Substring(1, venueName.Length - 1)
            //                             + ".unity";
            // return EditorSceneManager.LoadSceneAsyncInPlayMode(scenePathInEditor, new LoadSceneParameters(LoadSceneMode.Additive));
            return SceneManager.LoadSceneAsync(venueName, LoadSceneMode.Additive);
        }
// #endif

        myLoadedStage = AssetBundle.LoadFromFile(Path.Combine(bundlePath, venueName));
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
        venueMaterials.Unload(true);
        myLoadedCommonTextures.Unload(true);

    #if UNITY_EDITOR
        if(!this._useVueneFromEditor)
    #endif
        if(myLoadedStage != null)
            myLoadedStage.Unload(true);

        if(myLoadedMusicData != null)
            myLoadedMusicData.Unload(true);
    }


    #if UNITY_EDITOR
    public static void FixShadersForEditor(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                ReplaceShaderForEditor(renderer.sharedMaterials);
            }

            var tmps = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                ReplaceShaderForEditor(tmp.material);
                ReplaceShaderForEditor(tmp.materialForRendering);
            }
            
            var spritesRenderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spritesRenderers)
            {
                ReplaceShaderForEditor(spriteRenderer.sharedMaterials);
            }

            var images = prefab.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                ReplaceShaderForEditor(image.material);
            }
            
            var particleSystemRenderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var particleSystemRenderer in particleSystemRenderers)
            {
                ReplaceShaderForEditor(particleSystemRenderer.sharedMaterials);
            }

            var particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particle in particles)
            {
                var renderer = particle.GetComponent<Renderer>();
                if (renderer != null) ReplaceShaderForEditor(renderer.sharedMaterials);
            }
        }

        public static void ReplaceShaderForEditor(Material[] materials)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                ReplaceShaderForEditor(materials[i]);
            }
        }

        public static void ReplaceShaderForEditor(Material material)
        {
            if (material == null) return;

            var shaderName = material.shader.name;
            var shader = Shader.Find(shaderName);

            if (shader != null) material.shader = shader;
        }
    #endif
}