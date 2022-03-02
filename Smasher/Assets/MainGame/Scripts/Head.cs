using System;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using Defines;

public class Head : MonoBehaviour
{
    [SerializeField] Transform Pivot;
    [SerializeField] GameObject Model;
    [SerializeField] ParticleSystem PSGlow;
    [SerializeField] ParticleSystem PSExplode;
    [SerializeField] ParticleSystem PSShield;
    [SerializeField] AudioSource AudioBite;
    [SerializeField] AudioSource AudioPop;

    public static Action<Transform, string> OnShoot;
    // public static Action<string, HeadStyle> OnExploded;
    public static Action<GameObject> OnCollect;

    enum STATE
    {
        IDLE,
        MOVE,
        WIN,
        DEAD,
    }

    STATE mState;
    NavMeshAgent mAgent;
    HeadStyle mStyle;
    Timer mTimerShoot = new Timer();
    Timer mTimerShield = new Timer();
    Vector3 mDirection;
    int mHp;
    float mSpeed;
    bool mIsHurt = false;

    void Start()
    {
        mTimerShoot.SetDuration(0.5f);
        mAgent = GetComponent<NavMeshAgent>();
        SetState(STATE.IDLE);
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.IDLE:
                mAgent.enabled = true;
                break;

            case STATE.MOVE:
                Pivot.rotation = Quaternion.LookRotation(mDirection);
                transform.Translate(mDirection * mSpeed * Time.deltaTime);
                break;
        }

        mTimerShield.Update(Time.deltaTime);
        if (mTimerShield.JustFinished())
        {
            PSShield.gameObject.SetActive(false);
        }
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.IDLE:
                break;

            case STATE.MOVE:
                break;
        }
    }

    // void UpdateShoot()
    // {
    //     if (mStyle == HeadStyle.GUN && Model.activeSelf)
    //     {
    //         mTimerShoot.Update(Time.deltaTime);
    //         if (mTimerShoot.IsDone())
    //         {
    //             if (OnShoot != null)
    //             {
    //                 OnShoot(Pivot, transform.tag);
    //             }
    //             mTimerShoot.Reset();
    //         }
    //     }
    // }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (transform.tag == Tag.PLAYER && other.tag == Tag.COLLECTIBLE)
    //     {
    //         GameObject objectCollectible = other.transform.parent.gameObject;
    //         if (OnCollect != null)
    //         {
    //             OnCollect(objectCollectible);
    //         }
    //         objectCollectible.SetActive(false);
    //     }
    // }

    // void OnTriggerStay(Collider other)
    // {
    //     if (transform.tag == other.tag || other.tag == Tag.COLLECTIBLE)
    //     {
    //         return;
    //     }

    //     CollisionType type = (CollisionType)other.gameObject.layer;
    //     switch (type)
    //     {
    //         case CollisionType.CLAW:
    //             if (mTimerShield.IsDone())
    //             {
    //                 Hurt(1);
    //             }
    //             break;

    //         case CollisionType.BULLET:
    //             if (mTimerShield.IsDone())
    //             {
    //                 Hurt(1);
    //                 other.GetComponent<Bullet>().Hit();
    //             }
    //             break;

    //         case CollisionType.CHOCOLATE:
    //             if (transform.tag == Tag.PLAYER)
    //             {
    //                 other.GetComponentInParent<Chocolate>().Hurt(1);
    //             }
    //             break;
    //     }
    // }

    // void Hurt(int damage)
    // {
    //     if (Model.activeSelf && !mIsHurt)
    //     {
    //         Material m = Model.GetComponentInChildren<MeshRenderer>().material;
    //         m.DOKill(true);
    //         m.DOColor(Color.white, "_EmissionColor", 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
    //         {
    //             mHp -= damage;
    //             if (mHp <= 0)
    //             {
    //                 Explode();
    //             }
    //             mIsHurt = false;
    //         });

    //         AudioBite.Play();
    //         mIsHurt = true;
    //     }
    // }

    // void Explode()
    // {
    //     Model.SetActive(false);
    //     PSExplode.Play();
    //     AudioPop.Play();

    //     if (OnExploded != null)
    //     {
    //         OnExploded(transform.tag, mStyle);
    //     }
    // }

    public void SetStyle(HeadStyle type, string tag)
    {
        return;
        // for (int i = 0; i < Model.transform.childCount; i++)
        // {
        //     Model.transform.GetChild(i).gameObject.SetActive(false);
        // }

        // GameObject head = GameMgr.Instance.CreateHead(type, Model.transform);
        // for (int i = 0; i < head.transform.childCount; i++)
        // {
        //     Transform child = head.transform.GetChild(i);
        //     child.tag = tag;
        //     if (child.name == "Body")
        //     {
        //         child.gameObject.layer = (int)CollisionType.BODY;
        //     }
        //     else if (child.name == "Claw")
        //     {
        //         child.gameObject.layer = (int)CollisionType.CLAW;
        //     }
        // }

        // head.tag = tag;
        // Pivot.tag = tag;
        // transform.tag = tag;

        // mStyle = type;
        // mHp = 2;
        // mIsHurt = false;

        // Model.SetActive(true);
        // PSGlow.gameObject.SetActive(tag == Tag.PLAYER);
    }

    public void Idle()
    {
        SetState(STATE.IDLE);
    }

    public void Move(Vector3 direction, float speed)
    {
        mDirection = direction;
        mSpeed = speed;

        SetState(STATE.MOVE);
    }

    public void Win()
    {
        mAgent.enabled = false;
        SetState(STATE.WIN);
    }

    public void Dead()
    {
        mAgent.enabled = false;
        SetState(STATE.DEAD);
    }

    public void Revive()
    {
        PSShield.gameObject.SetActive(true);
        mTimerShield.SetDuration(3f);
        SetState(STATE.IDLE);
    }

    public void Flash()
    {
        return;
        // Material m = Model.GetComponentInChildren<SkinnedMeshRenderer>().material;
        // m.SetColor("_EmissionColor", Color.white);
        // m.DOKill(true);
        // m.DOColor(new Color(0, 0, 1 / 255), "_EmissionColor", 0.25f).SetEase(Ease.Flash);

        // transform.DOKill(true);
        // transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.25f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);
    }

    public void MoveToTarget(Vector3 target)
    {
        mAgent.SetDestination(target);
    }

    public void SetRotation(Quaternion rotation)
    {
        Pivot.rotation = rotation;
    }

    public Quaternion GetRotation()
    {
        return Pivot.rotation;
    }

    public Vector3 GetEulerAngles()
    {
        return Pivot.eulerAngles;
    }

    public bool HasArrived()
    {
        float remainingDistance = mAgent.remainingDistance;
        if (mAgent.pathPending)
        {
            remainingDistance = float.PositiveInfinity;
        }
        return remainingDistance <= 0.5f;
    }

    public bool IsDead()
    {
        return false;
    }
}
