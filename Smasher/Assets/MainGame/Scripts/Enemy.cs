using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Defines;


public class Enemy : MonoBehaviour
{
    [SerializeField] Head Head;
    [SerializeField] GameObject PrefabBody;

    public static Action<List<int>> OnDead;

    enum STATE
    {
        INIT,
        APPEAR,
        IDLE,
        TURN,
        MOVE,
        DEAD,
        HIDE,
    }

    STATE mState;
    NavMeshPath mPath;
    List<GameObject> mBodies = new List<GameObject>();
    List<int> mHeadStyles = new List<int>();
    List<int> mBodyStyles = new List<int>();
    Timer mTimerAI = new Timer();
    Quaternion mTargetRotation;
    int mBodyIndex = 0;

    void Start()
    {
        mPath = new NavMeshPath();
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.INIT:
                break;

            case STATE.APPEAR:
                mTimerAI.Update(Time.deltaTime);
                if (mTimerAI.IsDone())
                {
                    mBodies[mBodyIndex].SetActive(true);
                    mBodies[mBodyIndex++].GetComponent<Body>().Flash();
                    if (mBodyIndex == mBodies.Count)
                    {
                        SetState(STATE.IDLE);
                    }
                    else
                    {
                        mTimerAI.Reset();
                    }
                }
                break;

            case STATE.IDLE:
                mTimerAI.Update(Time.deltaTime);
                if (mTimerAI.IsDone())
                {
                    SetState(STATE.MOVE);
                }
                if (IsDead())
                {
                    SetState(STATE.DEAD);
                }

                break;

            case STATE.TURN:
                Head.transform.rotation = (Quaternion.RotateTowards(Head.transform.rotation, mTargetRotation, 120 * Time.deltaTime));
                if (Math.Abs(Quaternion.Dot(Head.GetRotation(), mTargetRotation)) > 0.99f)
                {
                    SetState(STATE.MOVE);
                }
                if (IsDead())
                {
                    SetState(STATE.DEAD);
                }

                break;

            case STATE.MOVE:
                mTimerAI.Update(Time.deltaTime);
                if (Head.HasArrived() || mTimerAI.IsDone())
                {
                    if (UnityEngine.Random.Range(0, 100) < 5)
                    {
                        SetState(STATE.TURN);
                    }
                    else
                    {
                        mTimerAI.SetDuration(UnityEngine.Random.Range(3f, 5f));
                        TryToMoveRandom(Head.transform.eulerAngles, 3);
                    }
                }
                if (IsDead())
                {
                    SetState(STATE.DEAD);
                }

                break;

            case STATE.DEAD:
                mTimerAI.Update(Time.deltaTime);
                if (mTimerAI.IsDone())
                {
                    gameObject.SetActive(false);
                    for (int i = 0; i < mBodies.Count; i++)
                    {
                        mBodies[i].GetComponent<Body>().Dead();
                    }
                    SetState(STATE.HIDE);
                }
                break;
        }
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.INIT:
                Head.gameObject.SetActive(false);
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].SetActive(false);
                }
                break;

            case STATE.APPEAR:
                mBodyIndex = 0;
                mTimerAI.SetDuration(0.25f);
                Head.gameObject.SetActive(true);
                // Head.Flash();
                break;

            case STATE.IDLE:
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].GetComponent<Body>().Idle();
                }
                mTimerAI.SetDuration(UnityEngine.Random.Range(1f, 1.5f));
                break;

            case STATE.TURN:
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].GetComponent<Body>().Idle();
                }

                int dir = UnityEngine.Random.Range(0, 100) < 50 ? 1 : -1;
                mTargetRotation = Quaternion.Euler(0, Head.transform.eulerAngles.y + UnityEngine.Random.Range(60, 120) * dir, 0);
                break;

            case STATE.MOVE:
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].GetComponent<Body>().Move();
                }
                mTimerAI.SetDuration(UnityEngine.Random.Range(3f, 5f));
                TryToMoveRandom(Head.transform.eulerAngles, 3);
                break;

            case STATE.DEAD:
                mTimerAI.SetDuration(.2f);
                // Head.MoveToTarget(Head.transform.position);
                if (OnDead != null)
                {
                    List<int> bodyStyles = mBodyStyles.FindAll(style => style != (int)BodyStyle.NORMAL);
                    OnDead(bodyStyles);
                }
                break;

            case STATE.HIDE:
                mHeadStyles.Clear();
                mBodyStyles.Clear();
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].SetActive(false);
                }
                break;
        }
    }

    void TryToMoveRandom(Vector3 eulerAngles, int retry)
    {
        Vector3 target = Head.transform.position + Quaternion.Euler(eulerAngles) * Vector3.forward;
        if (NavMesh.CalculatePath(Head.transform.position, target, NavMesh.AllAreas, mPath))
        {
            Head.MoveToTarget(target);
        }
        else if (retry > 0)
        {
            eulerAngles += new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            TryToMoveRandom(eulerAngles, --retry);
        }
        else
        {
            SetState(STATE.IDLE);
        }
    }

    GameObject GetInactiveBody()
    {
        List<GameObject> bodies = mBodies.FindAll(body => !body.activeSelf);
        if (bodies.Count > 0)
        {
            return bodies[0];
        }

        return null;
    }

    public GameObject AddHead(HeadStyle style)
    {
        mHeadStyles.Add((int)style);
        Head.SetStyle(style, Tag.ENEMY);

        return Head.gameObject;
    }

    public void AddBody(BodyStyle style)
    {
        GameObject objectBody = GetInactiveBody();
        if (objectBody == null)
        {
            objectBody = Instantiate(PrefabBody, transform);
            mBodies.Add(objectBody);
        }

        Body body = objectBody.GetComponent<Body>();
        body.SetStyle(style, Tag.ENEMY);

        if (mBodyStyles.Count == 0)
        {
            body.SetTarget(Head.transform, Head.GetRotation(), true);
        }
        else
        {
            body.SetTarget(mBodies[mBodyStyles.Count - 1].transform, mBodies[mBodyStyles.Count - 1].GetComponent<Body>().GetRotation());
        }
        objectBody.SetActive(true);
        mBodyStyles.Add((int)style);
    }

    public bool IsDead()
    {
        List<GameObject> bodies = new List<GameObject>();
        List<GameObject> bodiesDie = new List<GameObject>();
        for(int i = 0; i < mBodies.Count; i++)
        {
            Body body = mBodies[i].GetComponent<Body>();
            if(!body.IsDead())
            {
                bodies.Add(body.gameObject);
            }
            else
            {
                bodiesDie.Add(body.gameObject);
            }
        }
        if(bodiesDie.Count > 0)
        {
            RealignBodyTarget(bodiesDie);
        }
        return bodies.Count == 0;
    }

    void RealignBodyTarget(List<GameObject> bodiesDie)
    {
        //1 body exploded. Need to realign
        for(int i = 0; i < bodiesDie.Count; i++)
        {
            mBodies.Remove(bodiesDie[i]);
        }
        for(int i = 0; i < mBodies.Count; i++)
        {
            if (i == 0)
            {
                mBodies[i].GetComponent<Body>().SetTarget(Head.transform, Head.GetRotation(), true);
            }
            else
            {
                mBodies[i].GetComponent<Body>().SetTarget(mBodies[i - 1].transform, mBodies[i - 1].GetComponent<Body>().GetRotation());
            }
        }
    }

    public void Init()
    {
        SetState(STATE.INIT);
    }

    public void Attack()
    {
        SetState(STATE.APPEAR);
    }

    public Vector3 GetHeadPosition()
    {
        return Head.transform.position;
    }

    public int GetBodyLength()
    {
        return mBodyStyles.Count;
    }

    public bool IsDeadBodyGreaterThanLive()
    {
        return false;
    }
}
