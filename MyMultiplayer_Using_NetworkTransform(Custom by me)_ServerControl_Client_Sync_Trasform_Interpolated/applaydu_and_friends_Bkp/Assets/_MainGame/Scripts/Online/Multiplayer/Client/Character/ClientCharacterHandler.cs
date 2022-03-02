using UnityEngine;
using MLAPI;
public class ClientCharacterHandler : NetworkBehaviour
{
    //private
    // private NetworkVariableTransform mNetworkVariableTransform;
    private NetworkVariableAnimator mNetworkVariableAnimator;
    private NetworkVariableGeneric mNetworkVariableGeneric;
    private Rigidbody mRB;
    private ClientInputSender mClientInputSender;
    private Animator mAnimator;  
    private PLAYER_STATE mState = PLAYER_STATE.IDLE;
    private Timer timerControl = new Timer();
    RagdollHelper ragdollHelper;
    //public
    public PLAYER_STATE STATE
    {
        get { return mState; }
        set { mState = value; }
    }
    //const
    const string idleTrigger = "Trigger_Idle";
    const string walkingTrigger = "Trigger_Walking";
    const string jumpingTrigger = "Trigger_Jumping";
    const string victoryTrigger = "Trigger_Victory";

    const float TIME_FINISH = 2f;

    Vector3 lerpStartPos;
    Quaternion lerpStartRot;
    Vector3 lerpEndPos;
    Quaternion lerpEndRot;
    float posLerpTime;
    float rotLerpTime;

    private void Awake()
    {
        // mNetworkVariableTransform = GetComponent<NetworkVariableTransform>();
        mNetworkVariableAnimator = GetComponent<NetworkVariableAnimator>();
        mNetworkVariableGeneric = GetComponent<NetworkVariableGeneric>();
        mClientInputSender = GetComponent<ClientInputSender>();
        mAnimator = GetComponent<Animator>();
        ragdollHelper = GetComponent<RagdollHelper>();
        if (!GameManager.Instance.mIsMulti)
        {
            enabled = false;
            return;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogFormat("isMulti {0}", GameManager.Instance.mIsMulti);     
        SetState(PLAYER_STATE.IDLE);
    }
    private void OnEnable()
    {
        mClientInputSender.SetAnim += SetAnim;
        // mNetworkVariableTransform.Position.OnValueChanged += OnPositionChanged;
        // mNetworkVariableTransform.Rotation.OnValueChanged += OnRotationChanged;
        mNetworkVariableAnimator.Animation.OnValueChanged += PlayAnim;
        mNetworkVariableAnimator.AnimationSpeed.OnValueChanged += AnimationSpeedChanged;
        mNetworkVariableGeneric.PlayerState.OnValueChanged += PlayerStateChanged;
        mNetworkVariableGeneric.ToggleRagdoll.OnValueChanged += OnToggleRagdoll;
    }

    private void OnDisable()
    {
        mClientInputSender.SetAnim -= SetAnim;
        // mNetworkVariableTransform.Position.OnValueChanged -= OnPositionChanged;
        // mNetworkVariableTransform.Rotation.OnValueChanged -= OnRotationChanged;
        mNetworkVariableAnimator.Animation.OnValueChanged -= PlayAnim;
        mNetworkVariableAnimator.AnimationSpeed.OnValueChanged -= AnimationSpeedChanged;
        mNetworkVariableGeneric.PlayerState.OnValueChanged -= PlayerStateChanged;
        mNetworkVariableGeneric.ToggleRagdoll.OnValueChanged -= OnToggleRagdoll;
    }
    

    // Update is called once per frame
    void Update()
    {
        switch (mState)
        {
            case PLAYER_STATE.IDLE:
            case PLAYER_STATE.WALK:
            case PLAYER_STATE.FALL:
                // this.Interpolate();
                break;
            case PLAYER_STATE.FINISH:
                timerControl.Update(Time.deltaTime);
                if(timerControl.JustFinished())
                {
                    GameManager.Instance.mClient.StopClient();
                    ActionPhraseManager.Instance.OnBack();
                    GameEventMgr.Instance.SetEvent(GAME_EVENT.GOTO_RESULT);
                }
                break;
            default:
                break;
        }
    }

    // void Interpolate()
    // {
    //     const float maxInterpolateTime = 0.1f; // todo config

    //     this.posLerpTime += Time.deltaTime;
    //     transform.position = Vector3.Lerp(this.lerpStartPos, this.lerpEndPos, this.posLerpTime / maxInterpolateTime);

    //     this.rotLerpTime += Time.deltaTime;
    //     transform.rotation = Quaternion.Slerp(this.lerpStartRot, this.lerpEndRot, this.rotLerpTime / maxInterpolateTime);
    // }

    
    public override void NetworkStart()
    {
        if(IsServer)
        {
            enabled = false;
            return;
        }
        else
        {
        }
    }

    void SetAnim(string anim)
    {
        mNetworkVariableAnimator.Animation.Value = anim;
    }

    void PlayAnim(string oldValue, string newValue)
    {
        string anim = newValue;
        //reset all trigger
        for(int i = 0; i < mAnimator.parameters.Length; i++)
        {
            mAnimator.ResetTrigger(mAnimator.parameters[i].name);
        }

        mAnimator.SetTrigger(anim);
    }

    void InitFinish()
    {
        SetState(PLAYER_STATE.FINISH);
        ActionPhraseManager.Instance.ActiveWinner();
        timerControl.SetDuration(TIME_FINISH);
        PlayAnim("",victoryTrigger);
    }

    // private void OnPositionChanged(Vector3 old, Vector3 latest)
    // {
    //     this.lerpEndPos = latest;
    //     this.lerpStartPos = transform.position;
    //     this.posLerpTime = 0;
    // }
    // private void OnRotationChanged(Quaternion old, Quaternion latest)
    // {
    //     this.lerpEndRot = latest;
    //     this.lerpStartRot = transform.rotation;
    //     this.rotLerpTime = 0;
    // }

    void PlayerStateChanged(PLAYER_STATE oldValue, PLAYER_STATE newValue)
    {
        switch(newValue)
        {
            case PLAYER_STATE.FINISH:
                InitFinish();
                break;
        }
    }

    void SetState(PLAYER_STATE nextStatus)
    {
        if (mState != PLAYER_STATE.FALL)
        {
            if (nextStatus == PLAYER_STATE.FALL)
            {
                mState = PLAYER_STATE.FALL;
                PlayAnim("",jumpingTrigger);
            }
            else
            {
                if (mState != nextStatus)
                {
                    mState = nextStatus;
                    PlayAnim("",mState == PLAYER_STATE.IDLE ? idleTrigger : walkingTrigger);
                }
            }
        }
        else
        {
            if (mState != nextStatus)
            {
                mState = nextStatus;
                PlayAnim("",mState == PLAYER_STATE.IDLE ? idleTrigger : walkingTrigger);
            }
        }

    }

    void AnimationSpeedChanged(float oldValue, float newValue)
    {
        mAnimator.speed = newValue;
    }

    void OnToggleRagdoll(Vector3 oldValue, Vector3 newValue)
    {
        Debug.Log("OnToggleRagdoll + " + newValue.ToString());
        if(ragdollHelper)
        {
            ragdollHelper.ToggleRagdoll(true);
            ragdollHelper.AddForce(newValue);
        }
    }
}
