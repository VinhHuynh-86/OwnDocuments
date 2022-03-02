#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(LevelScript))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelScript myTarget = (LevelScript)target;
        if (GUILayout.Button("SAVE"))
        {
            string path = Application.dataPath + "/Resources/CSV/" + myTarget.levelName + ".csv";

            if (File.Exists(path))
            {                
                string text = "";
                for (int i = myTarget.levelDesign.Length - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        text += myTarget.levelDesign[i];
                    }
                    else
                    {
                        text += myTarget.levelDesign[i] + "\n";
                    }
                }
                if (text.Length > 0)
                {
                    File.WriteAllText(path, text);
                    AssetDatabase.Refresh();
                }
            }
        }
        int minutes = Mathf.FloorToInt(myTarget.timer / 60F);
        float seconds = myTarget.timer - minutes * 60;
        string niceTime = string.Format("{0:0}:{1:00.000}", minutes, seconds);
        EditorGUILayout.LabelField("Current time: " + niceTime);
        for (int i = myTarget.startShow; i < myTarget.endShow; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", myTarget.GetTime(i), GUILayout.Width(100));
            for (int j = 1; j <= myTarget.numLane; j++)
            {
                string newValue = EditorGUILayout.TextField("", myTarget.GetNote(i, j), GUILayout.Width(25));
                if (newValue != myTarget.GetNote(i, j))
                {
                    myTarget.ChangeNote(i, j, newValue);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif