using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Defines;

public class Collectible : MonoBehaviour
{
    NavMeshPath mPath;
    CollectibleType mType;
    int mStyle;
    bool mIsCollected;

    void Awake()
    {
        mPath = new NavMeshPath();
    }

    void Update()
    {
        transform.eulerAngles += new Vector3(0, 5 * 60 * Time.deltaTime, 0);
    }

    public void SetInfo(CollectibleType type, int stype)
    {
        GameObject collectible = null;
        switch (type)
        {
            // case CollectibleType.HEAD:
            //     collectible = GameMgr.Instance.CreateHead((HeadStyle)stype, transform);
            //     break;

            case CollectibleType.BODY:
            case CollectibleType.FOOD:
                collectible = GameMgr.Instance.CreateBody((BodyStyle)stype, transform);
                break;
        }

        if (collectible != null)
        {
            for (int i = 0; i < collectible.transform.childCount; i++)
            {
                Transform child = collectible.transform.GetChild(i);
                child.tag = Tag.COLLECTIBLE;
                child.gameObject.layer = (int)CollisionType.COLLECTIBLE;
            }
        }

        mType = type;
        mStyle = stype;
        mIsCollected = false;
    }

    public void Place(Player player, int angle)
    {
        Vector3 target = player.GetHeadPositionWithOffset(2f, angle);
        if (NavMesh.CalculatePath(player.GetHeadPosition(), target, NavMesh.AllAreas, mPath))
        {
            transform.position = target;
            transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
        }
        else
        {
            int dir = angle > 0 ? 1 : -1;
            Place(player, angle + 5 * dir);
        }
    }

    public void Collect()
    {
        mIsCollected = true;
        GetComponent<AudioSource>().Play();
    }

    public bool IsCollected()
    {
        return mIsCollected;
    }

    public CollectibleType GetCollectibleType()
    {
        return mType;
    }

    public int GetCollectibleStyle()
    {
        return mStyle;
    }
}
