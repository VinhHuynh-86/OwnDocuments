using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Defines;

public class Body : MonoBehaviour
{
    [SerializeField] private Transform Pivot;
    [SerializeField] GameObject Model;
    [SerializeField] ParticleSystem PSGlow;
    [SerializeField] ParticleSystem PSExplode;
    [SerializeField] ParticleSystem PSSplash;
    [SerializeField] ParticleSystem PSHit;
    [SerializeField] ParticleSystem PSRangeGun;
    [SerializeField] AudioSource AudioHitSword;
    [SerializeField] AudioSource AudioPop;

    public static Action<GameObject> OnCollect;
    public static Action<Transform, Transform, string, BulletType> OnShoot;
    public static Action<string, BodyStyle> OnExploded;

    enum STATE
    {
        INIT,
        IDLE,
        MOVE,
        ATTACK,
        DEAD,
    }

    STATE mState;
    Animator mAnimator;
    CharacterCollision mAttackCollision;
    Transform mTarget;
    BodyStyle mStyle;
    Vector3 mVelocity;
    Timer mTimerShoot = new Timer();
    Timer mTimerSlash = new Timer();
    int mHp;
    string mAnimationName;
    bool mIsHurt = false;
    bool mIsFirst = false;

    void Start()
    {
        mTimerShoot.SetDuration(1.5f);
        mTimerSlash.SetDuration(1.5f);
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.IDLE:
            case STATE.MOVE:
                Vector3 direction = mTarget.position - transform.position;
                Vector3 target = mTarget.position - direction.normalized * (mIsFirst ? 0f : Config.BODY_DISTANCE);

                if(direction != Vector3.zero)
                {
                    Pivot.rotation = Quaternion.LookRotation(direction);
                }
                transform.position = Vector3.SmoothDamp(transform.position, target, ref mVelocity, 0.0f);
                // UpdateShoot();
                break;

            case STATE.ATTACK:
                break;

            case STATE.DEAD:
                break;
        }
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.IDLE:
                mAnimator.SetTrigger("TriggerIdle");
                break;

            case STATE.MOVE:
                mAnimator.SetTrigger("TriggerRun");
                break;

            case STATE.ATTACK:
                mAnimator.SetTrigger("TriggerAttack");
                PSSplash.Play();
                AudioHitSword.Play();
                mAttackCollision.SetActiveCollider(true);
                DOTween.To(x => {}, 0, 1, .3f).OnComplete(() => {
                    mAttackCollision.SetActiveCollider(false);
                    SetState(STATE.IDLE);
                });
                break;

            case STATE.DEAD:
                mAnimator.SetTrigger("TriggerDie");
                break;
        }
    }

    private void PlayAnim(string triggerName)
    {
        //reset all triggers
        for (int v = 0; v < mAnimator.parameters.Length; ++v)
        {
            mAnimator.ResetTrigger(mAnimator.parameters[v].name);
        }

        mAnimator.SetTrigger(triggerName);
    }

    GameObject FindNearestObjByTag(Collider[] objList, string tag)
    {
        GameObject nearestObj = null;
        float nearestDistance = float.MaxValue;
        float distance;
        
        foreach(Collider obj in objList)
        {
            distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < nearestDistance && tag == obj.tag)
            {
                nearestDistance = distance;
                nearestObj = obj.gameObject;
            }
        }
        return nearestObj;
    }

    void UpdateShoot()
    {
        if(!Model.activeSelf || transform.tag != Tag.PLAYER) return;
        switch(mStyle)
        {
            case BodyStyle.NORMAL:
                
                mTimerSlash.Update(Time.deltaTime);
                if (mTimerSlash.IsDone())
                {
                    int layerMask = 1 << (int)CollisionType.BODY;//check with other body only
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, layerMask);
                    foreach (Collider hitCollider in hitColliders)
                    {
                        Body collideBody = hitCollider.gameObject.GetComponent<Body>();
                        if(hitCollider.gameObject.tag == Tag.ENEMY && !collideBody.IsDead())
                        {
                            PSSplash.Play();
                            collideBody.Hurt(1);
                            mTimerSlash.Reset();
                            AudioHitSword.Play();
                            PlayAnim("TriggerAttack");
                        }
                    }
                }
            break;
            case BodyStyle.GUN:
            case BodyStyle.GUN_ELECTRIC:
                mTimerShoot.Update(Time.deltaTime);
                if (mTimerShoot.IsDone())
                {
                    int layerMask = 1 << (int)CollisionType.BODY;//check with other body only
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f, layerMask);
                    GameObject shotTarget = FindNearestObjByTag(hitColliders, Tag.ENEMY);

                    if(shotTarget)
                    {
                        Body collideBody = shotTarget.GetComponent<Body>();
                        if(shotTarget.tag == Tag.ENEMY && !collideBody.IsDead())
                        {
                            if (OnShoot != null)
                            {
                                BulletType bType = (mStyle == BodyStyle.GUN_ELECTRIC) ? BulletType.ELECTRIC : BulletType.NORMAL;
                                Debug.Log(bType);
                                OnShoot(Pivot, shotTarget.transform, transform.tag, bType);
                            }
                            mTimerShoot.Reset();
                        }
                    }
                }
            break;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(IsDead() || transform.tag == other.tag) return;

        CollisionType type = (CollisionType)other.gameObject.layer;
        switch (type)
        {
            case CollisionType.CLAW:
                Hurt(1);
                break;
        }

        if (transform.tag == Tag.PLAYER && other.tag == Tag.COLLECTIBLE)
        {
            GameObject objectCollectible = other.transform.gameObject;
            if (OnCollect != null)
            {
                OnCollect(objectCollectible);
            }
            objectCollectible.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        
        if (IsDead() || transform.tag == other.tag || other.tag == Tag.COLLECTIBLE)
        {
            return;
        }

        CollisionType type = (CollisionType)other.gameObject.layer;
        switch (type)
        {
            case CollisionType.CLAW:
                // Hurt(1);
                break;

            case CollisionType.BULLET:
                Bullet bullet = other.GetComponent<Bullet>();
                if(bullet.GetBulletType() == BulletType.NORMAL)
                {
                    Hurt(1);
                    bullet.Hit();
                }
                else
                {
                    bullet.Explode();
                }
                break;

            case CollisionType.CHOCOLATE:
                if (transform.tag == Tag.PLAYER)
                {
                    other.GetComponentInParent<Chocolate>().Hurt(1);
                }
                break;
        }
    }

    public void Hurt(int damage)
    {
        if (!mIsHurt)
        {
            DOTween.To(x => {}, 0, 1, .2f).OnComplete(() => {
                mHp -= damage;
                if (mHp <= 0)
                {
                    Explode();
                }
                else
                {
                    
                    PlayAnim("TriggerHit" + (UnityEngine.Random.Range(0,100) < 50 ? "1" : "2"));
                    PSHit.Play();
                }
                mIsHurt = false;
            });        
            mIsHurt = true;
        }
    }

    void Explode()
    {
        Model.SetActive(false);
        PSExplode.Play();
        AudioPop.Play();
        Dead();

        if (OnExploded != null)
        {
            OnExploded(transform.tag, mStyle);
        }
    }

    public void SetTarget(Transform target, Quaternion rotation, bool isFirst = false)
    {
        mIsFirst = isFirst;
        float distance = mIsFirst ? 0f : Config.BODY_DISTANCE;
        Vector3 nextTarget = target.position + rotation * Vector3.back * distance;

        Pivot.rotation = rotation;
        transform.position = nextTarget;

        mTarget = target;
    }

    public void SetStyle(BodyStyle style, string tag)
    {
        for (int i = 0; i < Model.transform.childCount; i++)
        {
            Model.transform.GetChild(i).gameObject.SetActive(false);
        }

        GameObject body = GameMgr.Instance.CreateBody(style, Model.transform);

        mStyle = style;
        mHp = 2;
        mIsHurt = false;

        Model.SetActive(true);
        if(tag == Tag.PLAYER)
        {
            PSGlow.gameObject.SetActive(true);
            PSRangeGun.gameObject.SetActive(mStyle!=BodyStyle.NORMAL);
        }
        else
        {
            PSGlow.gameObject.SetActive(false);
            PSRangeGun.gameObject.SetActive(false);
        }
        mAnimator = body.GetComponent<Animator>();
        mAttackCollision = body.GetComponent<CharacterCollision>();

        transform.tag = tag;
        mAttackCollision.SetTag(tag);
    }

    public void Heal()
    {
        mHp = 2;
    }

    public void Flash()
    {
        Material m = Model.GetComponentInChildren<SkinnedMeshRenderer>().material;
        m.SetColor("_EmissionColor", Color.white);
        m.DOKill(true);
        m.DOColor(new Color(0, 0, 1 / 255), "_EmissionColor", 0.25f).SetEase(Ease.Flash);

        transform.DOKill(true);
        transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.25f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);
    }

    public BodyStyle GetStyle()
    {
        return mStyle;
    }

    public void SetRotation(Quaternion rotation)
    {
        Pivot.rotation = rotation;
    }

    public Quaternion GetRotation()
    {
        return Pivot.rotation;
    }

    public void Idle()
    {
        SetState(STATE.IDLE);
    }

    public void Move()
    {
        SetState(STATE.MOVE);
    }

    public void Attack()
    {
        SetState(STATE.ATTACK);
    }

    public void Dead()
    {
        SetState(STATE.DEAD);
    }

    public bool IsDead()
    {
        return (mState == STATE.DEAD);
    }

    public bool IsLowHp()
    {
        return mHp == 1;
    }
}
