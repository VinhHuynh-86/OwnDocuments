using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defines;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject ObjectContainer;
    [SerializeField] private GameObject ObjectBulletNormal;
    [SerializeField] private GameObject ObjectBulletElectric;
    [SerializeField] private AudioSource shootAudio;
    [SerializeField] private ParticleSystem ParticleHit;
    [SerializeField] private ParticleSystem ParticleObjElectric;
    [SerializeField] private ParticleSystem ParticleObjElectricExplode;

    enum STATE
    {
        FIRE,
        HIT,
        EXPLODE,
        DEAD
    }

    STATE mState;
    Timer mTimerLife    = new Timer();
    Timer mTimerHit     = new Timer();
    Timer mTimerExplode = new Timer();
    float mSpeed;

    BulletType mType = BulletType.NORMAL;
    Vector3 mTarget = Vector3.zero;
    const float BULLET_SPEED = 8f;

    void Start()
    {
        mTimerLife.SetDuration(1f);
        mTimerHit.SetDuration(0.5f);
        mTimerExplode.SetDuration(2f);
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.FIRE:
                transform.Translate(mSpeed * Vector3.forward * Time.deltaTime);
                if(mType == BulletType.NORMAL)
                {
                    mTimerLife.Update(Time.deltaTime);
                    if (mTimerLife.IsDone())
                    {
                        Dead();
                    }
                }
                else
                {
                    if (Vector3.Distance (transform.position, mTarget) <= mSpeed*Time.deltaTime)
                    {
                        Explode();
                    }
                }
                break;

            case STATE.HIT:
                mTimerHit.Update(Time.deltaTime);
                if (mTimerHit.IsDone())
                {
                    Dead();
                }
                break;

            case STATE.EXPLODE:
                mTimerExplode.Update(Time.deltaTime);
                if (mTimerExplode.JustFinished())
                {
                    Dead();
                }
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
            case STATE.FIRE:
                mTimerLife.Reset();
                ObjectContainer.SetActive(true);
                gameObject.SetActive(true);
                GetComponent<BoxCollider>().enabled = true;
                break;

            case STATE.HIT:
                mTimerHit.Reset();
                ParticleHit.Play();
                ParticleHit.GetComponent<AudioSource>().Play();
                ObjectContainer.SetActive(false);
                GetComponent<BoxCollider>().enabled = false;
                break;

            case STATE.EXPLODE:
                //for electric bullet
                mTimerExplode.Reset();
                ObjectContainer.SetActive(false);
                GetComponent<BoxCollider>().enabled = false;
                ParticleObjElectricExplode.Play();
                ParticleObjElectricExplode.GetComponent<AudioSource>().Play();

                int layerMask = 1 << (int)CollisionType.BODY;//check with other body only
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, layerMask);
                foreach (Collider hitCollider in hitColliders)
                {
                    Body collideBody = hitCollider.gameObject.GetComponent<Body>();
                    if(hitCollider.gameObject.tag == Tag.ENEMY && !collideBody.IsDead())
                    {
                        collideBody.Hurt(1);
                    }
                }
                break;

            case STATE.DEAD:
                gameObject.SetActive(false);
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (transform.tag == Tag.PLAYER && other.gameObject.layer == (int)CollisionType.CHOCOLATE)
        {
            Hit();
            other.GetComponentInParent<Chocolate>().Hurt(1);
        }
    }

    public BulletType GetBulletType()
    {
        return mType;
    }

    public void SetType(BulletType type)
    {
        mType = type;
        if(mType == BulletType.ELECTRIC)
        {
            ObjectBulletNormal.SetActive(false);
            ObjectBulletElectric.SetActive(true);
            ParticleObjElectric.Play();
        }
        else
        {
            ObjectBulletNormal.SetActive(true);
            ObjectBulletElectric.SetActive(false);
            ParticleObjElectric.Stop();
        }
    }

    public void Fire(Vector3 targetPos)
    {
        mSpeed = BULLET_SPEED;
        mTarget = targetPos;
        SetState(STATE.FIRE);
        shootAudio.Play();
    }

    public void Hit()
    {
        SetState(STATE.HIT);
    }

    public void Dead()
    {
        SetState(STATE.DEAD);
    }

    public void Explode()
    {
        SetState(STATE.EXPLODE);
    }
}
