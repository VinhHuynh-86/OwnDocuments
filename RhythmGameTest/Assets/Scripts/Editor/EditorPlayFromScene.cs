using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif


#if UNITY_EDITOR
[ExecuteInEditMode]
public class EditorPlayFromScene : EditorWindow
{
    [SerializeField] string lastScene = "";
    [SerializeField] int targetScene = 0;
    [SerializeField] string waitScene = null;
    [SerializeField] bool hasPlayed = false;

    [MenuItem("Tools/Play From Scene")]
    public static void Run()
    {
        EditorWindow.GetWindow<EditorPlayFromScene>();
    }
    static string[] sceneNames;
    static EditorBuildSettingsScene[] scenes;

    void OnEnable()
    {
        scenes = EditorBuildSettings.scenes;
        sceneNames = scenes.Select(x => AsSpacedCamelCase(Path.GetFileNameWithoutExtension(x.path))).ToArray();
    }

    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            if (null == waitScene && !string.IsNullOrEmpty(lastScene))
            {
                EditorApplication.OpenScene(lastScene);
                lastScene = null;
            }
        }
    }

    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            if (EditorApplication.currentScene == waitScene)
            {
                waitScene = null;
            }
            return;
        }

        if (null == sceneNames) return;
        GUILayout.Space(50);
        targetScene = EditorGUILayout.Popup(targetScene, sceneNames);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play", GUILayout.Height(60)))
        {
            lastScene = EditorApplication.currentScene;
            waitScene = scenes[targetScene].path;
            EditorApplication.SaveCurrentSceneIfUserWantsTo();
            EditorApplication.OpenScene(waitScene);
            EditorApplication.isPlaying = true;
        }
        GUILayout.Space(5);
        if (GUILayout.Button("Open", GUILayout.Height(60)))
        {
            EditorApplication.SaveCurrentSceneIfUserWantsTo();
            EditorApplication.OpenScene(scenes[targetScene].path);
        }
                GUILayout.Space(5);
        if (GUILayout.Button("Select", GUILayout.Height(60)))
        {
            var scene = AssetDatabase.LoadAssetAtPath(scenes[targetScene].path, typeof(Object));
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(scene);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        if (GUILayout.Button("Open Build Settings", GUILayout.Height(60)))
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));

        }
         
    }

    public string AsSpacedCamelCase(string text)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length * 2);
        sb.Append(char.ToUpper(text[0]));
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                sb.Append(' ');
            sb.Append(text[i]);
        }
        return sb.ToString();
    }
}

#endif