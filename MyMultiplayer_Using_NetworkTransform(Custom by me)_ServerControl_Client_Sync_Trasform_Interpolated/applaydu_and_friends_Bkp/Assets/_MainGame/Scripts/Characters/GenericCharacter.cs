using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using TMPro;

public enum DPAD : int
{
    NONE = 0,
    LEFT,
    UP,
    RIGHT,
    DOWN
}


public enum PLAYER_STATE : int
{
    INIT = 0,
    IDLE,
    WALK,
    RUN,
    JUMP,
    HIT,
    FALL,
    SLIDE,
    FINISH
}

[System.Serializable]
public class CollidedObject
{
    public GameObject obj = null;
    public Timer timer = new Timer();
    public bool isCheckpoint = false;
}

public abstract class GenericCharacter : NetworkBehaviour
{
    [SerializeField]
    protected RagdollHelper ragdollHelper;
    [HideInInspector]
    public Rigidbody rb;
    public int jumpForceBase;
    // public NetworkVariableTransform mNetworkVariableTransform;
    public NetworkVariableGeneric mNetworkVariableGeneric;
    public NetworkVariableAnimator mNetworkVariableAnimator;
    public CapsuleCollider col;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float orgSpeed = 10.0f;
    [HideInInspector]
    public float speed;
    public float airVelocity = 8f;
    public float maxVelocityChange = 10.0f;
    public float jumpHeight = 2.0f;
    public float maxFallSpeed = 20.0f;
    public float rotateSpeed = 25f; //Speed the player rotate
    public GameObject mainHead;
    public GameObject mainRig;
    public GameObject mascotRig;
    public ParticleSystem vfxMascot;
    public ParticleSystem vfxHitPlayer;
    public ParticleSystem vfxHitObstacle;
    public ParticleSystem vfxHitCheckpoint;
    public ParticleSystem vfxRun;
    public ParticleSystem vfxJump;
    public ParticleSystem vfxRespawn;
    public ParticleSystem vfxInRangeWarning;
    public ParticleSystem vfxDebuffSlow;
    public ParticleSystem[] vfxEmoteList;

    public ObstacleBoostSlow boostSlowPrefab;
    protected ObstacleBoostSlow boostSlow;
    public ObstacleBoostSpeed boostSpeedPrefab;
    protected ObstacleBoostSpeed boostSpeed;
    public ObstacleBoostBlast boostBlastPrefab;
    protected ObstacleBoostBlast boostBlast;
    public ObstacleBoostShield boostShieldPrefab;
    protected ObstacleBoostShield boostShield;

    [SerializeField]
    protected Vector3 moveDir;

    protected GameObject mainCamera;
    protected AffectedImpact anotherAffectedImpact;

    protected float verticalLook;
    protected Vector3 startPoint, checkPoint, fallingStartPos;
    protected float deadDistance = 20f;

    protected Timer timerControl = new Timer();
    protected bool canMove = false; //If player is not hitted
    protected bool isStuned = false;
    protected bool wasStuned = false; //If player was stunned before get stunned another time
    protected bool slide = false;
    protected float pushForce;
    protected Vector3 pushDir;
    protected float distToGround;

    [SerializeField]
    protected Animator animator;
    public PLAYER_STATE mState = PLAYER_STATE.IDLE;

    protected Timer fallingTimer = new Timer();
    protected Timer CountdownToDieTimer = new Timer();
    protected Timer boostTimer = new Timer();
    protected BOOST_TYPE boostType = BOOST_TYPE.NONE;
    protected Timer slowTimer = new Timer();
    protected bool isSlowing = false;
    protected Timer mascotTimer = new Timer();
    protected bool isMascot = false;
    protected Timer autoFreeTimer = new Timer();
    protected bool isConstraint = false;
    protected Vector3 orgVelocity = Vector3.zero;
    // protected LayerMask myLayer;

    // protected CheckPointInfo latestCheckpointPassed = new CheckPointInfo();

    protected List<CollidedObject> collidedObjectList = new List<CollidedObject>();//store the list gameobject that collied with player

    protected const float TIME_FINISH = 2f;
    protected const float TIME_COLLIDE = 0.5f;//time between 2 collisions with the same object
    protected const string idleTrigger = "Trigger_Idle";
    protected const string walkingTrigger = "Trigger_Walking";
    protected const string jumpingTrigger = "Trigger_Jumping";
    protected const string victoryTrigger = "Trigger_Victory";
    protected const float TIME_FALLING = 1.5f;
    protected bool isInHit = false; // to check thie player who is forced by blast or shield boost
    protected bool isInWarningRange = false;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        // mNetworkVariableTransform = GetComponent<NetworkVariableTransform>();
        mNetworkVariableGeneric = GetComponent<NetworkVariableGeneric>();
        mNetworkVariableAnimator = GetComponent<NetworkVariableAnimator>();
        
        startPoint = transform.position;
        checkPoint = startPoint;

        mState = PLAYER_STATE.IDLE;
        if(GameManager.Instance.mIsMulti)
            mNetworkVariableGeneric.PlayerState.Value = PLAYER_STATE.IDLE;
        PlayAnim(idleTrigger);
        isSlowing = false;
        speed = orgSpeed;
        orgVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);

    }

    protected virtual void Start()
    {
    }

    public virtual void StartMovement()
    {
        canMove = true;
    }

    protected virtual bool CheckGround()
    {
        if (!IsGrounded())
        {
            if (slide && HasFloorUnder())
            {
                SetState(PLAYER_STATE.SLIDE);
            }
            else
            {
                SetState(PLAYER_STATE.FALL);
            }
            return false;
        }
        return true;
    }

    protected virtual void UpdateFaceDir()
    {
        Vector3 targetDir = moveDir; //Direction of the character

        targetDir.y = 0;
        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }
        Quaternion tr = Quaternion.LookRotation(targetDir); //Rotation of the character to where it moves
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * rotateSpeed); //Rotate the character little by little
        transform.rotation = targetRotation;
    }
    protected virtual void FixedUpdate()
    {
        UpdatePlaying();
    }
    protected virtual void Update()
    {
        // if(GameManager.Instance.mIsMulti)
        {
            if (isInWarningRange)
            {
                Debug.Log("Check is in warning range");
                bool isInRange = false;
                Collider[] colliders = Physics.OverlapSphere(transform.position, 1);
                foreach (Collider hit in colliders)
                {
                    if (hit.gameObject.tag == TAG.BOOST_BLAST)
                    {
                        isInRange = true;
                    }
                }
                if (!isInRange)
                {
                    isInWarningRange = false;
                    StopVFXInRange();
                }
            }
        }
    }


    protected virtual bool IsGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y - 0.1f, col.bounds.center.z), col.radius * 0.75f, groundLayer);
    }

    protected virtual bool HasFloorUnder()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y - Mathf.Abs(deadDistance), col.bounds.center.z), col.radius * 0.75f, groundLayer);
    }


    protected virtual void UpdatePlaying()
    {
        {
            if (isSlowing && !slowTimer.IsDone())
            {
                //Debug.Log("isSlowing " + isSlowing + " slowTimer " + slowTimer);
                slowTimer.Update(Time.deltaTime);
                if (slowTimer.JustFinished())
                {
                    isSlowing = false;
                    speed = orgSpeed;
				    if (GameManager.Instance.mIsMulti)
					{
						StopSlowVFXClientRpc();
						mNetworkVariableAnimator.AnimationSpeed.Value = 1;
					}
					else
					{						
						vfxDebuffSlow.Stop();
						animator.speed = 1;
					}
				}
            }

            if (!boostTimer.IsDone())
            {
                boostTimer.Update(Time.deltaTime);
                if (boostTimer.JustFinished())
                {
                    if(boostType == BOOST_TYPE.BLAST_GATE)
                    {
                        boostBlast.BlastExplode();
                    }
                    ResetCurrentBoost();
                }
            }

            if (isMascot && !mascotTimer.IsDone())
            {
                mascotTimer.Update(Time.deltaTime);
                if (mascotTimer.JustFinished())
                {
                    StopMascot();
                }
            }

            // For Shield boost
            if (!autoFreeTimer.IsDone())
            {
                autoFreeTimer.Update(Time.deltaTime);
                if (autoFreeTimer.JustFinished())
                {
                    if (!GameManager.Instance.mIsMulti)
                    {
                        Free();
                        StopVFXInRange();
                    }
                }
            }

            for (int i = 0; i < collidedObjectList.Count; i++)
            {
                CollidedObject colObj = collidedObjectList[i];
                if (!colObj.isCheckpoint)
                {
                    colObj.timer.Update(Time.deltaTime);
                    if (colObj.timer.JustFinished())
                    {
                        collidedObjectList.Remove(colObj);
                    }
                }
            }
        }
    }

    public virtual void DeadAndRespawn()
    {
        ToggleRagdoll(Vector3.zero);
        ResetCurrentBoost();
        transform.position = checkPoint;
        // if(latestCheckpointPassed)
        // {
        //     CameraManager.Instance.UpdateCamTriggerPassed(latestCheckpointPassed);
        // }
        PlayAnim(idleTrigger);
        if (vfxRespawn)
        {
            if (GameManager.Instance.mIsMulti)
                PlayVFXClientRpc(TAG.OBSTACLES_BOUNCER);
            else
              PlayVfx(TAG.RESPAWN);
        }

    }

    public virtual void CountdownToDie()
    {
        if (CountdownToDieTimer.IsDone())
        {
            CountdownToDieTimer.SetDuration(1);
        }
    }

    protected virtual void SetState(PLAYER_STATE nextStatus)
    {
        if (mState != nextStatus)
        {
            switch (nextStatus)
            {
                case PLAYER_STATE.IDLE:
                    rb.velocity = Vector3.zero;
                    PlayAnim(idleTrigger);
                    break;
                case PLAYER_STATE.WALK:
                    PlayAnim(walkingTrigger);
                    break;
                case PLAYER_STATE.SLIDE:
                    PlayAnim(jumpingTrigger);
                    break;
                case PLAYER_STATE.FALL:
                    fallingStartPos = transform.position;
                    PlayAnim(jumpingTrigger);
                    CountdownToDie();
                    break;
            }
            mState = nextStatus;
            if(GameManager.Instance.mIsMulti)
                mNetworkVariableGeneric.PlayerState.Value = mState;
        }
    }

    public virtual void Jump(int jumpForce = 0)
    {
        // if (IsGrounded())
        {
            {
                if (jumpForce == 0)
                {
                    jumpForce = jumpForceBase;
                }
                rb.AddForce((moveDir + Vector3.up) * jumpForce, ForceMode.Impulse);
            }
        }
    }

    protected virtual void AddToCollidedList(GameObject obj)
    {
        CollidedObject newObj = new CollidedObject();
        newObj.obj = obj;
        newObj.isCheckpoint = (obj.tag == TAG.CHECK_POINT) ? true : false;
        newObj.timer.SetDuration(TIME_COLLIDE);
        collidedObjectList.Add(newObj);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        switch (collision.collider.tag)
        {
            case TAG.PLAYER:
            case TAG.BOT:
                foreach (CollidedObject colObj in collidedObjectList)
                {
                    if (collision.gameObject == colObj.obj)
                        return;
                }
                if (GameManager.Instance.mIsMulti)
                    PlayVFXClientRpc(TAG.PLAYER);
                else
                    PlayVfx(TAG.PLAYER);
                AddToCollidedList(collision.gameObject);
                break;
            case TAG.OBSTACLES_BOUNCER:
                foreach (CollidedObject colObj in collidedObjectList)
                {
                    if (collision.gameObject == colObj.obj)
                        return;
                }
                if (GameManager.Instance.mIsMulti)
                    PlayVFXClientRpc(TAG.OBSTACLES_BOUNCER);
                else
                    PlayVfx(TAG.OBSTACLES_BOUNCER);
                AddToCollidedList(collision.gameObject);
                break;
        }
    }

    protected virtual void SetCheckPoint(Vector3 pos)
    {
        checkPoint = pos;
    }

    public virtual void HitPlayer(Vector3 velocityF, float time)
    {
        Debug.Log("HitPlayer");

        if (gameObject.tag == TAG.BOT)
        {
            Lock();
        }

        SoundManager.Instance.Play(SFX.MC_IMPACT, false, transform.position);
        if (ragdollHelper && ragdollHelper.IsActive())
        {
        }
        else
        {
            Debug.Log(velocityF);
            rb.velocity = velocityF;

            pushForce = velocityF.magnitude;
            pushDir = Vector3.Normalize(velocityF);
            StartCoroutine(Decrease(velocityF.magnitude, time));
        }
        if (autoFreeTimer.IsDone())
        {
            autoFreeTimer.SetDuration(2);
        }
    }

    protected virtual IEnumerator Decrease(float value, float duration)
    {
        Debug.Log("Decrease");

        if (isStuned)
            wasStuned = true;
        isStuned = true;
        canMove = false;

        float delta = 0;
        delta = value / duration;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            yield return null;
            if (!slide) //Reduce the force if the ground isnt slide
            {
                pushForce = pushForce - Time.deltaTime * delta;
                pushForce = pushForce < 0 ? 0 : pushForce;
                //Debug.Log(pushForce);
            }
            rb.AddForce(new Vector3(0, Define.GRAVITY * GetComponent<Rigidbody>().mass, 0)); //Add gravity
        }
        rb.velocity = pushDir * pushForce;

        if (wasStuned)
        {
            wasStuned = false;
        }
        else
        {
            isStuned = false;
            canMove = true;
        }
    }

    protected virtual float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * -Define.GRAVITY);
    }

    protected virtual void PlayAnim(string triggerName)
    {
        if (!GameManager.Instance.mIsMulti)
        {
            PlayActAnimation(triggerName);
        }
    }

    protected virtual void PlayActAnimation(string triggerName)
    {
        if (isMascot)
        {
            Animator mascotAnim = mascotRig.GetComponent<Animator>();
            //reset all triggers
            for (int v = 0; v < mascotAnim.parameters.Length; ++v)
            {
                mascotAnim.ResetTrigger(mascotAnim.parameters[v].name);
            }
            mascotAnim.SetTrigger(triggerName);
            //return;
        }

        if (animator != null)
        {
            //reset all triggers
            for (int v = 0; v < animator.parameters.Length; ++v)
            {
                animator.ResetTrigger(animator.parameters[v].name);
            }

            animator.SetTrigger(triggerName);
        }
    }

    public virtual bool IsBoostActivating()
    {
        // if (!boostTimer.IsDone())
        //     return false;
        // else
        //     return true;
        return boostType != BOOST_TYPE.NONE;
    }

    public virtual bool ActiveBoost(BOOST_TYPE bType)
    {
        if (!boostTimer.IsDone()) return false;
        switch (bType)
        {
            case BOOST_TYPE.SPEED_GATE:
                {
                    ActiveSpeedBoost();
                    SetSpeedBoost(boostSpeed.effectSpeed, boostSpeed.effectTiming);
                    if(GameManager.Instance.mIsMulti)
                        ActiveSpeedBoostClientRpc();
                }
                break;


            case BOOST_TYPE.SLOW_GATE:
                {
                    ActiveSlowBoost();
                    SetSlowBoost(boostSlow.effectTiming);
                    if (GameManager.Instance.mIsMulti)
                        ActiveSlowBoostClientRpc();
                }
                break;


            case BOOST_TYPE.BLAST_GATE:
                {
                    ActiveBlastBoost();
                    SetBlastBoost(boostBlast.effectTiming);
                    if (GameManager.Instance.mIsMulti)
                        ActiveBlastBoostClientRpc();
                }
                break;

            case BOOST_TYPE.SHIELD_GATE:
                {
                    ActiveShieldBoost();
                    SetShieldBoost(boostShield.effectTiming);
                    if (GameManager.Instance.mIsMulti)
                        ActiveShieldBoostClientRpc();
                }
                break;
        }


        return true;
    }
    protected virtual void ResetCurrentBoost()
    {
        if (boostType == BOOST_TYPE.NONE)
            return;
        switch (boostType)
        {
            case BOOST_TYPE.SPEED_GATE:
                if (GameManager.Instance.mIsMulti)
                    mNetworkVariableAnimator.AnimationSpeed.Value = 1f;
                else
                   animator.speed = 1f;
                speed = orgSpeed;
                if (boostSpeed)
                {
                    boostSpeed.MyDestroy();
                    if (GameManager.Instance.mIsMulti)
                        StopSpeedByBoostClientRpc();
                }
                break;

            case BOOST_TYPE.SLOW_GATE:
                if (GameManager.Instance.mIsMulti)
                    mNetworkVariableAnimator.AnimationSpeed.Value = 1f;
                else
                    animator.speed = 1f;
                speed = orgSpeed;
                if (boostSlow)
                {
                    boostSlow.MyDestroy();
                    if (GameManager.Instance.mIsMulti)
                        StopSlowByBoostClientRpc();
                }
                break;

            case BOOST_TYPE.BLAST_GATE:
                if (boostBlast)
                    boostBlast.MyDestroy(); ;
                break;

            case BOOST_TYPE.SHIELD_GATE:
                if (boostShield)
                    boostShield.MyDestroy();
                break;
        }
        boostTimer.SetTimerDone();
        boostType = BOOST_TYPE.NONE;
    }
    public virtual void ChangeMascot(float timing, float effectSpeed)
    {
        Debug.Log("Generic boostType " + boostType);
        if (boostType == BOOST_TYPE.SHIELD_GATE)
            return;
        Debug.Log("Generic ChangeMascot");
        isMascot = true;
        mascotTimer.SetDuration(timing);
        if (GameManager.Instance.mIsMulti)
        {
            ChangeMascotClientRpc();
            PlayVFXClientRpc(TAG.RESPAWN);
            PlayVFXClientRpc(TAG.TRAP_MASCOT);
        }
        else
        {
            mainRig.SetActive(false);
            mascotRig.SetActive(true);
            vfxRespawn.Play();
            vfxMascot.Play();
        }
        speed = effectSpeed;        
        if (mState == PLAYER_STATE.IDLE)
        {
            PlayAnim(idleTrigger);
        }
        else if (mState == PLAYER_STATE.WALK)
        {
            PlayAnim(walkingTrigger);
        }
        SoundManager.Instance.Play(SFX.MC_CHICKEN_ON, false, transform.position);
    }

    public virtual void SetSlowByBoost(float timing, float slowSpeed)
    {
        if (boostType == BOOST_TYPE.SHIELD_GATE)
            return;

        if (isSlowing) return;

        Debug.Log("SetSlow");
        isSlowing = true;
        slowTimer.SetDuration(timing);
        speed = slowSpeed;

        if (GameManager.Instance.mIsMulti)
        {
            PlayBoostVFXClientRpc(BOOST_TYPE.SLOW_GATE);
            mNetworkVariableAnimator.AnimationSpeed.Value = 0.5f;
        }
        else
        {
            vfxDebuffSlow.Play();
            animator.speed = 0.5f;
        }
    }

    public virtual void SetSlowByTrap(float timing, float slowSpeed)
    {
        if (boostType == BOOST_TYPE.SHIELD_GATE)
            return;

        if (isSlowing) return;

        Debug.Log("SetSlow");
        isSlowing = true;
        slowTimer.SetDuration(timing);
        speed = slowSpeed;

        if (GameManager.Instance.mIsMulti)
        {
            PlayTrapVFXClientRpc(TRAP_TYPE.FROST);
            mNetworkVariableAnimator.AnimationSpeed.Value = 0.5f;
        }
        else
        {
            vfxDebuffSlow.Play();
            animator.speed = 0.5f;
        }
    }

    public virtual void StopSlow()
    {
        if (boostType == BOOST_TYPE.SHIELD_GATE)
            return;
        Debug.Log("StopSlow");
        isSlowing = false;
        slowTimer.SetTimerDone();
        speed = orgSpeed;
        if (GameManager.Instance.mIsMulti)
        {
            mNetworkVariableAnimator.AnimationSpeed.Value = 1;
            StopSlowVFXClientRpc();
        }
        else
        {
            animator.speed = 1;
            vfxDebuffSlow.Stop();
        }
    }
    public virtual void Twisting(Vector3 blackHolePos, float blackHoleMass)
    {
        Vector3 direction = blackHolePos - transform.position;

        if (direction.sqrMagnitude > Define.MIN_DISTANCE)
        {
            rb.velocity = Vector3.zero;

            float rotationSpeed = 0.005f * blackHoleMass / direction.sqrMagnitude;
            transform.RotateAround(blackHolePos, Vector3.up, rotationSpeed * Time.deltaTime);

            // float centripetalSpeed = 10f;
            // float gForce = blackHoleMass / direction.sqrMagnitude;
            // rb.AddForce(direction.normalized * gForce * centripetalSpeed * Time.deltaTime);

            float centripetalSpeed = 4f;
            direction = blackHolePos - transform.position;
            direction.Normalize();
            transform.Translate(direction * centripetalSpeed * Time.deltaTime, Space.World);
        }
    }
    public virtual void UniversalGravitation(Vector3 blackHolePos, float blackHoleMass)
    {
        if (boostType == BOOST_TYPE.SHIELD_GATE)
            return;

        Vector3 direction = blackHolePos - transform.position;
        direction.y = 0;
        float centripetalSpeed = 2f;
        float gForce = blackHoleMass / direction.sqrMagnitude;
        rb.AddForce(direction.normalized * gForce * centripetalSpeed * Time.deltaTime);
    }

    public virtual void UniversalGravitation2(Vector3 blackHolePos, float blackHoleMass)
    {
        if (boostType == BOOST_TYPE.SHIELD_GATE)
            return;

        Lock();
        Vector3 direction = blackHolePos - transform.position;
        direction.y = 0;
        float centripetalSpeed = 1f;
        rb.AddForce(direction.normalized * blackHoleMass * centripetalSpeed * Time.deltaTime);

        // rb.AddForce(forced, ForceMode.Force);
    }

    public virtual void Lock()
    {
        canMove = false;
        isConstraint = true;
        rb.velocity = Vector3.zero;

        Debug.Log("GenericCharacter::Lock");
    }

    // public virtual void ResetLayerMask()
    // {
    //     gameObject.layer = myLayer;
    // }

    public virtual void PlayEmoteIndex(int index)
    {
        if (!GameManager.Instance.mIsMulti)
        {
            PlayEmote(index);
        }
    }
    public virtual void PlayEmote(int index)
    {
        if (vfxEmoteList[index])
        {
            vfxEmoteList[index].Play();
        }
    }

    public virtual void PlayVFXInRange()
    {
        // if(!vfxInRangeWarning.isPlaying)
        {
            if (GameManager.Instance.mIsMulti)
                PlayVFXInRangeClientRpc();
            else
               vfxInRangeWarning.Play();
        }
    }

    public virtual void StopVFXInRange()
    {
        // if(vfxInRangeWarning.isPlaying)
        {
            if (GameManager.Instance.mIsMulti)
                StopVFXInRangClientRpc();
            else
               vfxInRangeWarning.Stop();
        }
    }

    public virtual void Free()
    {
        isInHit = false;
        canMove = true;
        isConstraint = false;
        rb.velocity = Vector3.zero;
        Debug.Log("GenericCharacter::Lock");
    }

    public virtual bool IsFree()
    {
        return canMove;
    }

    public virtual void ToggleRagdoll(Vector3 force)
    {
        ApplyToggleRagdoll(force);
    }

    public virtual void ApplyToggleRagdoll(Vector3 force)
    {
        SetState(PLAYER_STATE.IDLE);
        ragdollHelper.ToggleRagdoll(force.magnitude != 0);
        ragdollHelper.AddForce(force);
        if (GameManager.Instance.mIsMulti)
        {
            mNetworkVariableGeneric.ToggleRagdoll.Value = force;
        }
    }


    //play footsteps
    SFX curFootStep = SFX.FOOTSTEPS_01;
    protected virtual void TriggerFootstepEvent()
    {
        SoundManager.Instance.Play(SFX.FOOTSTEPS_01, false, transform.position);
        // SoundManager.Instance.Play(curFootStep);
        // curFootStep = curFootStep + 1;
        // if (curFootStep >= SFX.MC_CHICKEN_ON) curFootStep = SFX.FOOTSTEPS_01;
    }

    public virtual void PlayVfx(string tag)
    {
        switch (tag)
        {
            case TAG.OBSTACLES_BOUNCER:
                vfxHitObstacle.Play();
                break;
            case TAG.PLAYER:
            case TAG.BOT:
                vfxHitPlayer.Play();
                break;
            case TAG.RESPAWN:
                vfxRespawn.Play();
                break;
            case TAG.CHECK_POINT:
                vfxHitCheckpoint.Play();
                break;
        }
    }
    public virtual void PlayBoostVfx(BOOST_TYPE type)
    {
        switch (type)
        {
            case BOOST_TYPE.SLOW_GATE:
                vfxDebuffSlow.Play();
                break;
        }
    }
    public virtual void PlayTrapVfx(TRAP_TYPE type)
    {
        switch (type)
        {
            case TRAP_TYPE.MASCOT:
                vfxMascot.Play();
                break;

            case TRAP_TYPE.FROST:
                vfxDebuffSlow.Play();
                break;
        }
    }

    public virtual bool HasBoost(BOOST_TYPE _type)
    {
        return boostType == _type;
    }
    protected virtual void StopMascot()
    {
        isMascot = false;
        if (GameManager.Instance.mIsMulti)
        {
            StopMascotClientRpc();
            PlayVFXClientRpc(TAG.RESPAWN);
        }
        else
        {
            mainRig.SetActive(true);
            mascotRig.SetActive(false);
            vfxRespawn.Play();
        }
        speed = orgSpeed;
        SoundManager.Instance.Play(SFX.MC_CHICKEN_OFF, false, transform.position);
    }


    protected void SetSpeedBoost(float _speed, float _timing)
    {
        if (GameManager.Instance.mIsMulti)
            mNetworkVariableAnimator.AnimationSpeed.Value = 1.5f;
        else
        animator.speed = 1.5f;
        speed = _speed;
        boostType = BOOST_TYPE.SPEED_GATE;
        boostTimer.SetDuration(_timing);
    }
    protected void ActiveSpeedBoost()
    {
        Debug.Log("ActiveSpeedBoost");
        boostSpeed = GameObject.Instantiate<ObstacleBoostSpeed>(boostSpeedPrefab, this.gameObject.transform);
        boostSpeed.StartBoost(this.gameObject);       
        SoundManager.Instance.Play(SFX.BOOSTGATE_SPEED);
    }


    protected  void SetSlowBoost(float timing)
    {
        boostType = BOOST_TYPE.SLOW_GATE;
        boostTimer.SetDuration(timing);
    }
    protected void ActiveSlowBoost()
    {
        boostSlow = GameObject.Instantiate<ObstacleBoostSlow>(boostSlowPrefab, this.gameObject.transform);
        boostSlow.StartBoost(this.gameObject);
        SoundManager.Instance.Play(SFX.BOOSTGATE_SLOW);
    }

    protected void SetBlastBoost(float timing)
    {
        boostType = BOOST_TYPE.BLAST_GATE;
        boostTimer.SetDuration(timing);
    }
    protected virtual void ActiveBlastBoost()
    {
        boostBlast = GameObject.Instantiate<ObstacleBoostBlast>(boostBlastPrefab, this.gameObject.transform);
        boostBlast.StartBoost(this.gameObject);
        SoundManager.Instance.Play(SFX.BOOSTGATE_BOMB, false, transform.position);
    }

    protected void SetShieldBoost(float timing)
    {
        boostType = BOOST_TYPE.SHIELD_GATE;
        boostTimer.SetDuration(timing);
    }
    protected virtual void ActiveShieldBoost()
    {
        boostShield = GameObject.Instantiate<ObstacleBoostShield>(boostShieldPrefab, this.gameObject.transform);
        boostShield.StartBoost(this.gameObject);
        SoundManager.Instance.Play(SFX.BOOSTGATE_SHIELD, false, transform.position);   
    }
    public virtual void SetForce(Vector3 force)
    {
        if (!isInHit)
        {
            isInHit = true;
        }
        else
        {
            return;
        }
        if (autoFreeTimer.IsDone())
        {
            rb.AddForce(force);
            autoFreeTimer.SetDuration(1);
        }
    }

////////////////////////////////////////////////////////////////////////////////////////////////////
// For Multiplayer //
    // For Multiplayer //


    public override void NetworkStart()
    {
        Debug.Log("IsServer " + IsServer);
        if (!IsServer && GameManager.Instance.mIsMulti)
            enabled = false;
    }

    [ClientRpc]
    public void PlayVFXClientRpc(string tag)
    {
        PlayVfx(tag);
    }
    [ClientRpc]
    public void PlayBoostVFXClientRpc(BOOST_TYPE type)
    {
        PlayBoostVfx(type);
    }
    [ClientRpc]
    public void PlayTrapVFXClientRpc(TRAP_TYPE type)
    {
        PlayTrapVfx(type);
    }
    [ClientRpc]
    public void PlayVFXInRangeClientRpc()
    {
        vfxInRangeWarning.Play();
    }
    [ClientRpc]
    public void StopVFXInRangClientRpc()
    {
        vfxInRangeWarning.Stop();
    }
    [ClientRpc]
    public void ActiveSpeedBoostClientRpc()
    {
        ActiveSpeedBoost();
    }
    [ClientRpc]
    public void StopSpeedByBoostClientRpc()
    {
        if (boostSpeed)
            boostSpeed.MyDestroy();
    }
    [ClientRpc]
    public void ActiveSlowBoostClientRpc()
    {
        ActiveSlowBoost();
    }
    [ClientRpc]
    public void StopSlowByBoostClientRpc()
    {
        if (boostSlow)
            boostSlow.MyDestroy();
    }
    [ClientRpc]
    public void StopSlowVFXClientRpc() // For trap or boost
    {
        vfxDebuffSlow.Stop();
    }
    [ClientRpc]
    public void ActiveBlastBoostClientRpc()
    {
        ActiveBlastBoost();
    }
    [ClientRpc]
    public void ActiveShieldBoostClientRpc()
    {
        ActiveShieldBoost();
    }
    [ClientRpc]
    public void ChangeMascotClientRpc()
    {
        mainRig.SetActive(false);
        mascotRig.SetActive(true);
    }
    [ClientRpc]
    public void StopMascotClientRpc()
    {
        mainRig.SetActive(true);
        mascotRig.SetActive(false);
    }
}
