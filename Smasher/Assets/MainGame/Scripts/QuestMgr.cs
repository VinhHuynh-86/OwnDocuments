using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defines;

public class QuestMgr : Singleton<QuestMgr>
{
    [SerializeField] private GameObject ObjectHint;
    [SerializeField] private List<GameObject> Heads;
    [SerializeField] private List<GameObject> Bodies;
    [SerializeField] private AudioSource AudioCompleted;

    public static Action<QuestInfo> OnStart;
    public static Action<QuestInfo, int> OnCheck;
    public static Action<int> OnCompleted;

    QuestInfo mQuest;
    int mCount;
    int mZoneIndex = 0;
    bool mIsInProgress = false;

    void Start()
    {
        Head.OnCollect += OnHeadCollect;
        Body.OnExploded += OnBodyExploded;
    }

    void Update()
    {
        ObjectHint.transform.Rotate(new Vector3(0, 5, 0));
    }

    void Check()
    {
        if (OnCheck != null)
        {
            OnCheck(mQuest, mCount);
        }

        if (mIsInProgress && mCount == mQuest.Count)
        {
            if (OnCompleted != null)
            {
                AudioCompleted.Play();
                OnCompleted(mZoneIndex++);
                mIsInProgress = false;
            }
        }
    }

    void OnHeadCollect(GameObject collectible)
    {
        // int style = collectible.GetComponentInParent<Collectible>().GetCollectibleStyle();
        // if (mQuest.Type == QuestType.COLLECT_HEAD && mQuest.Head == (HeadStyle)style)
        // {
        //     mCount++;
        // }
        // else if (mQuest.Type == QuestType.COLLECT_BODY && mQuest.Body == (BodyStyle)style)
        // {
        //     mCount++;
        // }
        // Check();
    }

    void OnBodyExploded(string tag, BodyStyle style)
    {
        if (tag == Tag.ENEMY)
        {
            if (mQuest.Type == QuestType.DESTROY_BODY && mQuest.Body == style)
            {
                mCount++;
            }
            Check();
        }
    }

    public void RandomQuest()
    {
        if (mIsInProgress)
        {
            return;
        }

        List<QuestInfo> quests = new List<QuestInfo>();
        switch (mZoneIndex)
        {
            case 0:
                // quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.FOOD, Description = "Collect %d apple", Count = 15 });
                // quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.CLAW01, Description = "Destroy %d ememy head", Count = 5 });
                quests.Add(new QuestInfo() { Type = QuestType.DESTROY_BODY, Body = BodyStyle.NORMAL, Description = "Destroy %d ememy body", Count = 15 });
                break;

            // case 1:
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.FOOD, Description = "Collect %d apple", Count = 15 });
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.SPIKE_01, Description = "Collect %d enemy body", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.CLAW01, Description = "Destroy %d ememy head", Count = 5 });
            //     break;

            // case 2:
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.SPIKE_01, Description = "Collect %d enemy body", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.CLAW02, Description = "Destroy %d ememy head", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_BODY, Body = BodyStyle.SPIKE_01, Description = "Destroy %d enemy body", Count = 15 });
            //     break;

            // case 3:
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.SPIKE_01, Description = "Collect %d enemy body", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.GUN, Description = "Collect %d enemy body", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.CLAW02, Description = "Destroy %d ememy head", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.GUN, Description = "Destroy %d ememy head", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_BODY, Body = BodyStyle.GUN, Description = "Collect %d enemy body", Count = 15 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_BODY, Body = BodyStyle.SPIKE_02, Description = "Collect %d enemy body", Count = 15 });
            //     break;

            // default:
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.GUN, Description = "Collect %d enemy body", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.COLLECT_BODY, Body = BodyStyle.SPIKE_02, Description = "Collect %d enemy body", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.CLAW03, Description = "Destroy %d ememy head", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_HEAD, Head = HeadStyle.CLAW04, Description = "Destroy %d ememy head", Count = 5 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_BODY, Body = BodyStyle.GUN, Description = "Collect %d enemy body", Count = 15 });
            //     quests.Add(new QuestInfo() { Type = QuestType.DESTROY_BODY, Body = BodyStyle.SPIKE_02, Description = "Collect %d enemy body", Count = 15 });
            //     break;

        }

        if (quests.Count > 0)
        {
            mQuest = quests[UnityEngine.Random.Range(0, quests.Count)];

            for (int i = 0; i < Heads.Count; i++)
            {
                Heads[i].SetActive(false);
            }

            for (int i = 0; i < Bodies.Count; i++)
            {
                Bodies[i].SetActive(false);
            }

            // if (mQuest.Type == QuestType.COLLECT_HEAD || mQuest.Type == QuestType.DESTROY_HEAD)
            // {
            //     for (int i = 0; i < Heads.Count; i++)
            //     {
            //         Heads[i].SetActive(i == (int)mQuest.Head);
            //     }
            // }
            // else if (mQuest.Type == QuestType.COLLECT_BODY || mQuest.Type == QuestType.DESTROY_BODY)
            // {
            //     for (int i = 0; i < Bodies.Count; i++)
            //     {
            //         Bodies[i].SetActive(i == (int)mQuest.Body);
            //     }
            // }
            if (mQuest.Type == QuestType.DESTROY_BODY)
            {
                for (int i = 0; i < Bodies.Count; i++)
                {
                    Bodies[i].SetActive(i == (int)mQuest.Body);
                }
            }

            if (OnStart != null)
            {
                OnStart(mQuest);
            }

            mCount = 0;
            mIsInProgress = true;
        }
    }

    public void Reset()
    {
        mZoneIndex = 0;
        mIsInProgress = false;
    }
}
