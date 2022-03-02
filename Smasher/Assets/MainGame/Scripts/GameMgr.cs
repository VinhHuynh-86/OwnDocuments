using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using Doozy.Engine.UI.Animation;
using Defines;

public class GameMgr : Singleton<GameMgr>
{
    [SerializeField] private Animator CameraDriven;
    [SerializeField] private Player Player;
    [SerializeField] private CollectibleMgr CollectibleMgr;
    [SerializeField] private BulletMgr BulletMgr;
    [SerializeField] private List<GameObject> PrefabHeads;
    [SerializeField] private List<GameObject> PrefabBodies;

    List<GameObject> mHeads = new List<GameObject>();
    List<GameObject> mBodies = new List<GameObject>();

    Map mMap;
    int mCoin = 0;

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        Body.OnShoot += OnShoot;
        Chocolate.OnCollect += OnChocolateCollect;
        Enemy.OnDead += OnEnemyDead;

        Loading.OnLoaded += OnGameLoaded;
        Map.OnMapFinish += OnGameWin;
        Player.OnHurt += OnPlayerHurt;

        UnityAds.OnRewardVideoWatched += OnRewardVideoWatched;
        UnityAds.OnRewardVideoFailed += OnRewardVideoFailed;

        UIPopup.OnUIPopupAction += OnPopupAction;
    }

    void Update()
    {

    }

    void DestroyArray(List<GameObject> array)
    {
        for (int i = 0; i < array.Count; i++)
        {
            GameObject.Destroy(array[i]);
        }
        array.Clear();
    }

    void OnShoot(Transform owner, Transform target, string tag, BulletType type)
    {
        owner.tag = tag;
        BulletMgr.TargetShot(owner, target, type);
    }

    void OnChocolateCollect()
    {
        OnEnemyDead(
        new List<int>()
        {
            (int)BodyStyle.GUN,
        });
    }

    void OnEnemyDead(List<int> bodyStyles)
    {
        int index = 0;

        for (int i = 0; i < bodyStyles.Count; i++)
        {
            int bodyPercent = UnityEngine.Random.Range(0, 100);
            if (bodyPercent < 25)
            {
                CollectibleMgr.Add(CollectibleType.BODY, bodyStyles[UnityEngine.Random.Range(0, bodyStyles.Count)], Player, index++);
            }
        }

        // int healPercent = UnityEngine.Random.Range(0, 100);
        // if (healPercent < 50)
        // {
        //     CollectibleMgr.Add(CollectibleType.BODY, (int)BodyStyle.NORMAL, Player, index++);
        // }

        int foodCount = UnityEngine.Random.Range(1, 5);
        for (int i = 0; i <= foodCount; i++)
        {
            CollectibleMgr.Add(CollectibleType.BODY, (int)BodyStyle.FOOD, Player, index++);
        }

        mMap.Spawn();
    }

    void OnGameLoaded(GameObject map)
    {
        mCoin = 0;
        mMap = map.GetComponent<Map>();

        DestroyArray(mHeads);
        DestroyArray(mBodies);
        CollectibleMgr.ClearAll();

        Player.Init();
        mMap.Init();
        GameEventMgr.SendEvent("loading_completed");
    }

    void OnGameWin()
    {
        Player.Win();
        GameEventMgr.SendEvent("win");
    }

    void OnPlayerHurt()
    {
        if (Player.IsDead())
        {
            GiveUp();
        }
        else if ((!CollectibleMgr.HasAttackItem() && !Player.CanAttack()))
        {
            if (UnityAds.Instance.RewardVideoAvailable)
            {
                if (!UIPopup.AnyPopupVisible)
                {
                    UIPopup.GetPopup("Revive").Show();
                }
            }
            else
            {
                GiveUp();
            }
        }
    }

    void OnPopupAction(UIPopup popup, AnimationType type)
    {
        if (type == AnimationType.Show)
        {
            GamePause();
        }
        else if (type == AnimationType.Hide)
        {
            GameResume();
        }
    }

    void GamePause()
    {
        Time.timeScale = 0;
    }

    void GameResume()
    {
        Time.timeScale  = 1;
    }

    void RewardHead()
    {
        int percent = UnityEngine.Random.Range(0, 100);
        HeadStyle style = HeadStyle.GUN;

        if (percent > 95)
        {
            style = HeadStyle.CLAW04;
        }
        else if (percent > 90)
        {
            style = HeadStyle.CLAW03;
        }
        else if (percent > 50)
        {
            style = HeadStyle.CLAW02;
        }

        Player.SetHeadStyle(style);
        Player.Bravo();
    }

    void OnRewardVideoWatched(AdsRewardType type)
    {
        switch (type)
        {
            case AdsRewardType.Head:
                RewardHead();
                break;

            case AdsRewardType.Revive:
                RewardHead();
                Player.Revive();
                break;
        }
        GameResume();
    }

    void OnRewardVideoFailed(AdsRewardType type)
    {
        GameResume();
        UIPopup.GetPopup("NoReward").Show();
    }

    GameObject GetInactiveHeadByType(HeadStyle style)
    {
        List<GameObject> heads = mHeads.FindAll(head => !head.activeSelf && head.GetComponent<Style>().HeadStyle == style);
        if (heads.Count > 0)
        {
            return heads[0];
        }
        return null;
    }

    GameObject GetInactiveBodyByType(BodyStyle style)
    {
        List<GameObject> bodies = mBodies.FindAll(body => !body.activeSelf && body.GetComponent<Style>().BodyStyle == style);
        if (bodies.Count > 0)
        {
            return bodies[0];
        }
        return null;
    }

    public void Play()
    {
        mMap.Play();
        Player.Play();
    }

    public void GiveUp()
    {
        Player.Dead();
        GameEventMgr.SendEvent("lose");
    }

    public void WatchForHead()
    {
        UnityAds.Instance.ShowRewardVideo(AdsRewardType.Head);
    }

    public void WatchForRevive()
    {
        GamePause();
        UnityAds.Instance.ShowRewardVideo(AdsRewardType.Revive);
    }

    public GameObject CreateHead(HeadStyle style, Transform parent)
    {
        GameObject head = GetInactiveHeadByType(style);
        if (head == null)
        {
            head = Instantiate(PrefabHeads[(int)style]);
            mHeads.Add(head);
        }

        head.GetComponent<Style>().HeadStyle = style;
        head.transform.parent = parent;
        head.transform.localPosition = Vector3.zero;
        head.transform.localEulerAngles = Vector3.zero;
        head.transform.localScale = Vector3.one;
        head.SetActive(true);

        return head;
    }

    public GameObject CreateBody(BodyStyle style, Transform parent)
    {
        GameObject body = GetInactiveBodyByType(style);
        if (body == null)
        {
            body = Instantiate(PrefabBodies[(int)style], parent);
            mBodies.Add(body);
        }

        body.GetComponent<Style>().BodyStyle = style;
        body.transform.parent = parent;
        body.transform.localPosition = Vector3.zero;
        body.transform.localEulerAngles = Vector3.zero;
        body.transform.localScale = Vector3.one;
        body.SetActive(true);
        // body.tag = Tag.COLLECTIBLE;
        // body.gameObject.layer = (int)CollisionType.COLLECTIBLE;

        return body;
    }

    public void SetCameraDriven(string state)
    {
        CameraDriven.SetTrigger(state);
    }

    public int GetCurrentCoin()
    {
        return mCoin;
    }
}
