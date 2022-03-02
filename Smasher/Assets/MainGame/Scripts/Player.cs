using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Defines;

public class Player : MonoBehaviour
{
    [SerializeField] Head Head;
    [SerializeField] GameObject PrefabBody;
    [SerializeField] ParticleSystem ParticleQuestCompleted;
    [SerializeField] List<ParticleSystem> ParticleEmoji;

    public static Action OnDead;
    public static Action OnHurt;
    public static Action<int, int> OnFoodCollected;

    enum STATE
    {
        INIT,
        IDLE,
        MOVE,
        ATTACK,
        DEAD,
        WIN,
    }

    STATE mState;
    List<GameObject> mBodies = new List<GameObject>();
    Vector3 mDirection;
    Timer mTimerEmoji = new Timer();
    int mFoodCurrent = 0;
    int mFoodTotal = 0;
    float mAngle = 0;

    void Start()
    {
        VariableJoystick.OnData += OnJoystickData;
        Body.OnCollect += OnCollect;
        Body.OnExploded += OnBodyExploded;

        mTimerEmoji.SetDuration(5);
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.INIT:
                break;

            case STATE.IDLE:
                if (mDirection != Vector3.zero)
                {
                    SetState(STATE.MOVE);
                }
                break;

            case STATE.MOVE:
                if (mDirection != Vector3.zero)
                {
                    Head.Move(mDirection, 3f);
                }
                else
                {
                    SetState(STATE.ATTACK);
                }
                break;

            case STATE.ATTACK:
                break;

            case STATE.DEAD:
                break;
        }

        UpdateEmoji();
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.INIT:
                mFoodCurrent = 0;
                mFoodTotal = 2;
                if (OnFoodCollected != null)
                {
                    OnFoodCollected(mFoodCurrent, mFoodTotal);
                }

                Head.transform.position = new Vector3(0, 0, -0.25f);
                Head.SetRotation(Quaternion.Euler(new Vector3(0, 180, 0)));
                Head.SetStyle(HeadStyle.CLAW01, Tag.PLAYER);

                for (int i = 0; i < mBodies.Count; i++)
                {
                    GameObject.Destroy(mBodies[i]);
                }
                mBodies.Clear();

                // for (int i = 0; i < 2; i++)
                {
                    AddBody(BodyStyle.NORMAL);
                    // AddBody(BodyStyle.GUN);
                    // AddBody(BodyStyle.GUN_ELECTRIC);
                }
                break;

            case STATE.IDLE:
            case STATE.WIN:
                if (mState == STATE.IDLE)
                {
                    Head.Idle();
                }
                else
                {
                    Head.Win();
                }
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].GetComponent<Body>().Idle();
                }
                mDirection = Vector3.zero;
                break;

            case STATE.MOVE:
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].GetComponent<Body>().Move();
                }
                break;

            case STATE.ATTACK:
                for (int i = 0; i < mBodies.Count; i++)
                {
                    mBodies[i].GetComponent<Body>().Attack();
                }
                SetState(STATE.IDLE);
                break;

            case STATE.DEAD:
                Head.Dead();
                if (OnDead != null)
                {
                    EmojiPoop(true);
                    OnDead();
                }
                break;
        }
    }

    void UpdateEmoji()
    {
        mTimerEmoji.Update(Time.deltaTime);
        if (mTimerEmoji.IsDone())
        {
            List<GameObject> enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i].GetComponent<Enemy>();
                if (enemy != null && !enemy.IsDead())
                {
                    float distance = Vector3.Distance(Head.transform.position, enemy.GetHeadPosition());
                    if (distance < 3)
                    {
                        if (enemy.IsDeadBodyGreaterThanLive())
                        {
                            EmojiTroll();
                        }
                        else if (enemy.GetBodyLength() < Mathf.FloorToInt(mBodies.Count / 2) + 1)
                        {
                            EmojiTroll();
                        }
                        else if (enemy.GetBodyLength() > mBodies.Count * 2 - 1)
                        {
                            EmojiScare();
                        }
                    }
                }
            }
        }
    }

    void AddBody(BodyStyle style, bool isSplash = false)
    {
        GameObject objectBody = Instantiate(PrefabBody, transform);
        Body body = objectBody.GetComponent<Body>();

        if (mBodies.Count == 0)
        {
            body.SetTarget(Head.transform, Head.GetRotation(), true);
        }
        else
        {
            body.SetTarget(mBodies[mBodies.Count - 1].transform, mBodies[mBodies.Count - 1].GetComponent<Body>().GetRotation());
        }
        body.SetStyle(style, Tag.PLAYER);

        if (isSplash)
        {
            body.Flash();
        }

        if (mState == STATE.IDLE)
        {
            body.Idle();
        }
        else if (mState == STATE.MOVE)
        {
            body.Move();
        }

        mBodies.Add(objectBody);
    }

    void CollectBody(BodyStyle style)
    {
        GameObject bodyObject = null;
        switch (style)
        {
            case BodyStyle.NORMAL:
                List<GameObject> bodies = mBodies.FindAll(body => body.GetComponent<Body>().IsDead());
                if (bodies.Count > 0)
                {
                    bodyObject = bodies[0];
                }
                else
                {
                    bodies = mBodies.FindAll(body => body.GetComponent<Body>().IsLowHp());
                    if (bodies.Count > 0)
                    {
                        bodies[0].GetComponent<Body>().Heal();
                        EmojiFun();
                    }
                }
                break;

            case BodyStyle.FOOD:
                mFoodCurrent++;
                if (mFoodCurrent == mFoodTotal)
                {
                    mFoodCurrent = 0;
                    mFoodTotal *= 2;
                    AddBody(BodyStyle.NORMAL, true);
                }
                if (OnFoodCollected != null)
                {
                    OnFoodCollected(mFoodCurrent, mFoodTotal);
                }
                break;

            default:
                bodies = mBodies.FindAll(body => !body.GetComponent<Body>().IsDead() && body.GetComponent<Body>().GetStyle() == BodyStyle.NORMAL);
                if (bodies.Count > 0)
                {
                    bodyObject = bodies[0];
                }
                else
                {
                    bodies = mBodies.FindAll(body => !body.GetComponent<Body>().IsDead() && body.GetComponent<Body>().GetStyle() < style);
                    if (bodies.Count > 0)
                    {
                        bodyObject = bodies[0];
                    }
                }
                break;
        }

        if (bodyObject != null)
        {
            Body body = bodyObject.GetComponent<Body>();
            body.SetStyle(style, Tag.PLAYER);
            if (mState == STATE.IDLE)
            {
                body.Idle();
            }
            else if (mState == STATE.MOVE)
            {
                body.Move();
            }
            EmojiFun();
        }
    }

    void OnCollect(GameObject collectible)
    {
        Collectible item = collectible.GetComponentInParent<Collectible>();
        CollectibleType type = item.GetCollectibleType();

        int style = item.GetCollectibleStyle();
        switch (type)
        {
            // case CollectibleType.HEAD:
            //     Head.SetStyle((HeadStyle)style, Tag.PLAYER);
            //     break;

            case CollectibleType.BODY:
                CollectBody((BodyStyle)style);
                break;
        }
        item.Collect();
    }

    void OnBodyExploded(string tag, BodyStyle style)
    {
        if (tag == Tag.PLAYER)
        {
            EmojiSad();
            if (OnHurt != null)
            {
                OnHurt();
            }
        }
    }

    void OnJoystickData(float horizontal, float vertical)
    {
        mDirection = horizontal * Vector3.right + vertical * Vector3.forward;
    }

    void EmojiFun()
    {
        if (mTimerEmoji.IsDone())
        {
            List<EmojiType> emoji = new List<EmojiType>()
            {
                EmojiType.COOL,
                EmojiType.TEETH,
            };

            ParticleEmoji[(int)emoji[UnityEngine.Random.Range(0, emoji.Count)]].Play();
            mTimerEmoji.Reset();
        }
    }

    void EmojiSad()
    {
        if (mTimerEmoji.IsDone())
        {
            List<EmojiType> emoji = new List<EmojiType>()
            {
                EmojiType.ANGRY,
                EmojiType.CRY,
                EmojiType.DISAPPOINTED,
                EmojiType.PUKE
            };

            ParticleEmoji[(int)emoji[UnityEngine.Random.Range(0, emoji.Count)]].Play();
            mTimerEmoji.Reset();
        }
    }

    void EmojiTroll()
    {
        if (mTimerEmoji.IsDone())
        {
            List<EmojiType> emoji = new List<EmojiType>()
            {
                EmojiType.DROOL,
                EmojiType.LAUNCH_CRY,
                EmojiType.KISS,
                EmojiType.EVIL_LAUNCH,
            };

            ParticleEmoji[(int)emoji[UnityEngine.Random.Range(0, emoji.Count)]].Play();
            mTimerEmoji.Reset();
        }
    }

    void EmojiPoop(bool force = false)
    {
        if (mTimerEmoji.IsDone() || force)
        {
            List<EmojiType> emoji = new List<EmojiType>()
            {
                EmojiType.POOP
            };

            ParticleEmoji[(int)emoji[UnityEngine.Random.Range(0, emoji.Count)]].Play();
            mTimerEmoji.Reset();
        }
    }

    void EmojiScare()
    {
        if (mTimerEmoji.IsDone())
        {
            List<EmojiType> emoji = new List<EmojiType>()
            {
                EmojiType.SCARE
            };

            ParticleEmoji[(int)emoji[UnityEngine.Random.Range(0, emoji.Count)]].Play();
            mTimerEmoji.Reset();
        }
    }

    public void Init()
    {
        SetState(STATE.INIT);
    }

    public void Play()
    {
        SetState(STATE.IDLE);
    }

    public void Win()
    {
        SetState(STATE.WIN);
    }

    public void Dead()
    {
        SetState(STATE.DEAD);
    }

    public bool IsDead()
    {
        List<GameObject> bodies = mBodies.FindAll(body => !body.GetComponent<Body>().IsDead());
        return bodies.Count == 0;
    }

    public void Revive()
    {
        Head.Revive();
        SetState(STATE.IDLE);
    }

    public void SetHeadStyle(HeadStyle style)
    {
        Head.SetStyle(style, Tag.PLAYER);
    }

    public void Bravo()
    {
        ParticleQuestCompleted.transform.position = Head.transform.position;
        ParticleQuestCompleted.Play();
    }

    public bool CanAttack()
    {
        bool canAttack = false;
        for (int i = 0; i < mBodies.Count; i++)
        {
            Body body = mBodies[i].GetComponent<Body>();
            if (!body.IsDead() && body.GetStyle() > BodyStyle.NORMAL && body.GetStyle() < BodyStyle.FOOD)
            {
                canAttack = true;
                break;
            }
        }
        return !Head.IsDead() || canAttack;
    }

    public Vector3 GetHeadPosition()
    {
        return Head.transform.position;
    }

    public Vector3 GetHeadPositionWithOffset(float offset, float angle)
    {
        Vector3 eulerAngles = Head.GetEulerAngles() + new Vector3(0, angle, 0);
        Vector3 headDirection = Quaternion.Euler(eulerAngles) * Vector3.forward;

        return Head.transform.position + headDirection.normalized * offset;
    }
}
