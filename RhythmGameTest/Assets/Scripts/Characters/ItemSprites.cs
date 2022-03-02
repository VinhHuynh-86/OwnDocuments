using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSprites", menuName = "ItemSprites")]
public class ItemSprites : ScriptableObject
{
    public List<ItemSprite> listItemSprite;
    public Sprite GetSprite(string ID)
    {
        ItemSprite item = listItemSprite.Find(x => x.ID.Equals(ID));
        if(item != null)
        {
            if(item.sprite != null)
                return item.sprite;

            return Resources.Load<Sprite>(item.spritePath);
        }
        return null;
    }

    public void SetID(int index, string ID)
    {
        listItemSprite[index].ID = ID;
    }
}

[System.Serializable]
public class ItemSprite
{
    public string ID;
    public Sprite sprite;
    public string spritePath;
}
