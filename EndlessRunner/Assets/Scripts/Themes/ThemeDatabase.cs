using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

// Handles loading data from the Asset Bundle to handle different themes for the game
public class ThemeDatabase
{
    static protected Dictionary<string, ErThemeData> themeDataList;
    static public Dictionary<string, ErThemeData> dictionnary { get { return themeDataList; } }

    static protected bool m_Loaded = false;
    static public bool loaded { get { return m_Loaded; } }

    static public ErThemeData GetThemeData(string type)
    {
        ErThemeData list;
        if (themeDataList == null || !themeDataList.TryGetValue(type, out list))
            return null;

        return list;
    }

    static public IEnumerator LoadDatabase()
    {
        // If not null the dictionary was already loaded.
        if (themeDataList == null)
        {
            themeDataList = new Dictionary<string, ErThemeData>();

            Debug.Log("[VH]::LoadDatabase::themeData");

            yield return Addressables.LoadAssetsAsync<ErThemeData>("themeDatas", op =>
            {
                if (op != null)
                {
                    UnityEngine.Debug.Log("[VH]::LoadDatabase::themeData::1");

                    if(!themeDataList.ContainsKey(op.themeName))
                    {
                        themeDataList.Add(op.themeName, op);
                        UnityEngine.Debug.Log("[VH]::LoadDatabase::themeData::2");
                    }
                }
            });

            m_Loaded = true;
        }

    }
}
