using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defines;

public class CollectibleMgr : MonoBehaviour
{
    [SerializeField] private GameObject PrefabCollectible;
    List<GameObject> mCollectibles = new List<GameObject>();

    int mPlaceOffset = 0;

    void Start()
    {

    }

    void Update()
    {

    }

    GameObject GetInactiveCollectible()
    {
        List<GameObject> collectibles = mCollectibles.FindAll(collectible => collectible.transform.childCount == 0);
        if (collectibles.Count > 0)
        {
            return collectibles[0];
        }
        return null;
    }
    public void Add(CollectibleType collectibleType, int type, Player player, int index)
    {
        GameObject collectible = GetInactiveCollectible();
        if (collectible == null)
        {
            collectible = Instantiate(PrefabCollectible, transform);
            mCollectibles.Add(collectible);
        }

        if (index == 0)
        {
            mPlaceOffset = 0;
        }

        int dir = index % 2 == 0 ? 1 : -1;
        int angle = (index - mPlaceOffset) * dir * 15;
        if (index % 2 == 1)
        {
            mPlaceOffset++;
        }

        collectible.GetComponent<Collectible>().Place(player, angle);
        collectible.GetComponent<Collectible>().SetInfo(collectibleType, type);
    }

    public bool HasAttackItem()
    {
        bool hasAttackItem = false;
        for (int i = 0; i < mCollectibles.Count; i++)
        {
            Collectible collectible = mCollectibles[i].GetComponent<Collectible>();
            if (!collectible.IsCollected())
            {
                CollectibleType type = collectible.GetCollectibleType();
                int style = collectible.GetCollectibleStyle();

                if (type == CollectibleType.HEAD)
                {
                    hasAttackItem = true;
                    break;
                }
                else if (type == CollectibleType.BODY)
                {
                    if (style > (int)BodyStyle.NORMAL && style < (int)BodyStyle.FOOD)
                    {
                        hasAttackItem = true;
                        break;
                    }
                }
            }
        }
        return hasAttackItem;
    }

    public void ClearAll()
    {
        for (int i = 0; i < mCollectibles.Count; i++)
        {
            GameObject.Destroy(mCollectibles[i]);
        }
        mCollectibles.Clear();
    }
}
