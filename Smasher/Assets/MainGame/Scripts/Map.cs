using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Defines;

public class Map : MonoBehaviour
{
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private List<BoxCollider> ZoneColliders;

    public static Action OnMapFinish;

    enum STATE
    {
        NONE,
        INIT,
        ATTACK,
        FINISH,
    }

    STATE mState;
    List<GameObject> mEnemies = new List<GameObject>();
    List<HeadStyle> mHeadStyles = new List<HeadStyle>();
    List<BodyStyle> mBodyStyles = new List<BodyStyle>();
    Player mPlayer;
    int mCurrentZone = 0;
    int mBodyLengthMin;
    int mBodyLengthMax;

    void Start()
    {
        mPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        QuestMgr.OnCompleted += OnQuestCompleted;
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.INIT:
                break;

            case STATE.ATTACK:
                if (IsClear())
                {
                    SetState(STATE.FINISH);
                }
                break;

            case STATE.FINISH:
                break;
        }
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.INIT:
                Spawn();
                break;

            case STATE.ATTACK:
                for (int i = 0; i < mEnemies.Count; i++)
                {
                    mEnemies[i].GetComponent<Enemy>().Attack();
                }

                QuestMgr.Instance.Reset();
                QuestMgr.Instance.RandomQuest();
                break;

            case STATE.FINISH:
                if (OnMapFinish != null)
                {
                    OnMapFinish();
                }
                break;
        }
    }

    bool IsClear()
    {
        List<GameObject> enemies = mEnemies.FindAll(enemy => !enemy.GetComponent<Enemy>().IsDead());
        return enemies.Count == 0;
    }

    void SpawnEnemyInZone(int zone)
    {
        Vector3 spawnPosition = GetSpawnPosition(zone);
        while (Vector3.Distance(spawnPosition, mPlayer.GetHeadPosition()) < 5f)
        {
            spawnPosition = GetSpawnPosition(zone);
        }

        GameObject objectEnemy = GetInActiveEnemy();
        if (objectEnemy == null)
        {
            objectEnemy = Instantiate(EnemyPrefab, transform);
            mEnemies.Add(objectEnemy);
        }
        objectEnemy.SetActive(true);

        Enemy enemy = objectEnemy.GetComponent<Enemy>();
        HeadStyle headStyle = mHeadStyles[UnityEngine.Random.Range(0, mHeadStyles.Count)];

        GameObject head = enemy.AddHead(headStyle);
        head.transform.position = spawnPosition;
        head.transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));

        int bodyLength = UnityEngine.Random.Range(mBodyLengthMin, mBodyLengthMax + 1);
        for (int j = 0; j < bodyLength; j++)
        {
            BodyStyle bodyStyle = mBodyStyles[UnityEngine.Random.Range(0, mBodyStyles.Count)];
            enemy.AddBody(bodyStyle);
        }

        if (mState == STATE.ATTACK)
        {
            enemy.Attack();
        }
        else
        {
            enemy.Init();
        }
    }

    Vector3 GetSpawnPosition(int zone)
    {
        BoxCollider collider = ZoneColliders[zone];
        float left = collider.center.x - collider.size.x / 2;
        float right = -left;
        float bottom = collider.center.z - collider.size.z / 2;
        float top = bottom + collider.size.z;

        return new Vector3(UnityEngine.Random.Range(left, right), 0, UnityEngine.Random.Range(bottom, top));
    }

    GameObject GetInActiveEnemy()
    {
        List<GameObject> enemies = mEnemies.FindAll(enemy => !enemy.activeSelf);
        if (enemies.Count > 0)
        {
            return enemies[0];
        }

        return null;
    }

    void OnQuestCompleted(int zone)
    {
        mPlayer.Bravo();
    }

    public void Init()
    {
        SetState(STATE.INIT);
    }

    public void Play()
    {
        SetState(STATE.ATTACK);
    }

    public void Spawn()
    {
        int playerZone = 0;
        List<int> maxEnemyInZone = new List<int>() { 5, 6, 6, 6, 7 };

        mHeadStyles.Clear();
        mBodyStyles.Clear();
        switch (playerZone)
        {
            case 0:
                mHeadStyles.Add(HeadStyle.CLAW01);
                mBodyStyles.Add(BodyStyle.NORMAL);
                mBodyLengthMin = 2;
                mBodyLengthMax = 4;
                break;

            // case 1:
            //     mHeadStyles.Add(HeadStyle.CLAW01);
            //     mBodyStyles.Add(BodyStyle.NORMAL);
            //     mBodyStyles.Add(BodyStyle.SPIKE_01);
            //     mBodyLengthMin = 3;
            //     mBodyLengthMax = 6;
            //     break;

            // case 2:
            //     mHeadStyles.Add(HeadStyle.CLAW01);
            //     mHeadStyles.Add(HeadStyle.CLAW02);
            //     mBodyStyles.Add(BodyStyle.NORMAL);
            //     mBodyStyles.Add(BodyStyle.SPIKE_01);
            //     mBodyStyles.Add(BodyStyle.GUN);
            //     mBodyLengthMin = 4;
            //     mBodyLengthMax = 8;
            //     break;

            // case 3:
            //     mHeadStyles.Add(HeadStyle.CLAW02);
            //     mHeadStyles.Add(HeadStyle.CLAW03);
            //     mHeadStyles.Add(HeadStyle.GUN);
            //     mBodyStyles.Add(BodyStyle.NORMAL);
            //     mBodyStyles.Add(BodyStyle.SPIKE_01);
            //     mBodyStyles.Add(BodyStyle.GUN);
            //     mBodyStyles.Add(BodyStyle.SPIKE_02);
            //     mBodyLengthMin = 5;
            //     mBodyLengthMax = 10;
            //     break;

            // case 4:
            //     mHeadStyles.Add(HeadStyle.CLAW03);
            //     mHeadStyles.Add(HeadStyle.CLAW04);
            //     mBodyStyles.Add(BodyStyle.SPIKE_01);
            //     mBodyStyles.Add(BodyStyle.GUN);
            //     mBodyStyles.Add(BodyStyle.SPIKE_02);
            //     mBodyLengthMin = 8;
            //     mBodyLengthMax = 20;
            //     break;
        }

        List<GameObject> enemies = mEnemies.FindAll(
            delegate (GameObject enemyObject)
            {
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                return !enemy.IsDead();
            }
        );

        int count = maxEnemyInZone[playerZone] - enemies.Count;
        for (int i = 0; i < count; i++)
        {
            SpawnEnemyInZone(playerZone);
        }
    }
}
