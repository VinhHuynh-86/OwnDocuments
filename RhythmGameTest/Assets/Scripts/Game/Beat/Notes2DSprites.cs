using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MyDictionaryEntry2D
{
    public Define.COLORS key;
    public int currentValue;
    public Sprite[] value;
}

public class Notes2DSprites : MonoBehaviour
{
    [SerializeField]
    private List<MyDictionaryEntry2D> sprites;
    private Dictionary<Define.COLORS, Sprite[]> dictionary;
    private SpriteRenderer objRenderer;

    void Awake()
    {
        objRenderer = GetComponent<SpriteRenderer>();
        // Fixed: Unitiy dictionary can not visible in UI inspector
        dictionary = new Dictionary<Define.COLORS, Sprite[]>();
        foreach (MyDictionaryEntry2D entry in sprites)
        {
            dictionary.Add(entry.key, entry.value);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
