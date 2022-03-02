using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MyDictionaryEntry
{
    public Define.COLORS key;
    public Material[] value;
}

[RequireComponent(typeof(Renderer))]
public class MaterialSwitch : MonoBehaviour
{
    [SerializeField]
    private List<MyDictionaryEntry> materials;
    public Dictionary<Define.COLORS, Material[]> dictionary;
    private Renderer objRenderer;

    void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        // Fixed: Unitiy dictionary can not visible in UI inspector
        dictionary = new Dictionary<Define.COLORS, Material[]>();
        foreach (MyDictionaryEntry entry in materials)
        {
            dictionary.Add(entry.key, entry.value);
        }
    }
}
