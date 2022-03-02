using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MLAPI.NetworkVariable;
using MLAPI;
using MLAPI.Messaging;
using System.Linq;


////////////////////////////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class State
    {
        public float timestamp = -1;
        public Vector3 pos = Vector3.zero;
        public Quaternion rot = Quaternion.identity;
        public State()
        {
        }

        public State(float time, Vector3 _pos, Quaternion _rot)
        {
            timestamp = time;
            pos = _pos;
            rot = _rot;
        }
    }

    public class HistoryStatePoint
    {
        public float deltaTime = 0;
        public Vector3 deltaPos = Vector3.zero;
        public Vector3 deltaRot = Vector3.zero;
        public Vector3 velocity = Vector3.zero;
        public Vector2 input = Vector2.zero;

        public HistoryStatePoint()
        {
        }

        public HistoryStatePoint(float _deltaTime, Vector3 _deltaPos, Vector3 _deltaRot, Vector3 _velocity, Vector2 _input)
        {
            deltaTime = _deltaTime;
            deltaPos = _deltaPos;
            deltaRot = _deltaRot;
            velocity = _velocity;
            input = _input;
        }
    }
    public class StatePoint
    {
        public Vector2 input = Vector2.zero;
        public float timestamp = 0;
        public Vector3 pos = Vector3.zero;
        public Vector3 rot = Vector3.zero;
        public Vector3 velocity = Vector3.zero;

        public StatePoint()
        {
        }
        public StatePoint(Vector2 _input, float _timestamp, Vector3 _pos, Vector3 _rot, Vector3 _velocity)
        {
            input = _input;
            timestamp = _timestamp;
            pos = _pos;
            rot = _rot;
            velocity = _velocity;
        }
    }

////////////////////////////////////////////////////////////////////////////////////////////////////



public class PlayerController : GenericCharacter
{

    internal class ClientSendInfo
        {
            public float LastSent;
            public Vector3? LastMissedPosition;
            public Quaternion? LastMissedRotation;
        }

        internal class ServerSendInfo
        {
            public float LastSent;
            public Vector3? LastMissedPosition;
            public Quaternion? LastMissedRotation;
        }

        /// <summary>
        /// The base amount of sends per seconds to use when range is disabled
        /// </summary>
        [Range(0, 120)]
        public float FixedSendsPerSecond = 20f;

        /// <summary>
        /// Is the sends per second assumed to be the same across all instances
        /// </summary>
        [Tooltip("This assumes that the SendsPerSecond is synced across clients")]
        public bool AssumeSyncedSends = true;

        /// <summary>
        /// Enable interpolation
        /// </summary>
        [Tooltip("This requires AssumeSyncedSends to be true")]
        public bool InterpolatePosition = true;

        /// <summary>
        /// The distance before snaping to the position
        /// </summary>
        [Tooltip("The transform will snap if the distance is greater than this distance")]
        public float SnapDistance = 10f;

        /// <summary>
        /// Should the server interpolate
        /// </summary>
        public bool InterpolateServer = true;

        /// <summary>
        /// The min meters to move before a send is sent
        /// </summary>
        public float MinMeters = 0.15f;

        /// <summary>
        /// The min degrees to rotate before a send it sent
        /// </summary>
        public float MinDegrees = 1.5f;

        /// <summary>
        /// Enables extrapolation
        /// </summary>
        public bool ExtrapolatePosition = false;

        /// <summary>
        /// The maximum amount of expected send rates to extrapolate over when awaiting new packets.
        /// A higher value will result in continued extrapolation after an object has stopped moving
        /// </summary>
        public float MaxSendsToExtrapolate = 5;

        /// <summary>
        /// The channel to send the data on
        /// </summary>
        [Tooltip("The channel to send the data on. Uses the default channel if left unspecified")]
        public string Channel = null;

        private float m_LerpTime;
        private Vector3 m_LerpStartPos;
        private Quaternion m_LerpStartRot;
        private Vector3 m_LerpEndPos;
        private Quaternion m_LerpEndRot;

        private float m_LastSendTime;
        private Vector3 m_LastSentPos;
        private Quaternion m_LastSentRot;

        private float m_LastReceiveTime;

        /// <summary>
        /// Enables range based send rate
        /// </summary>
        public bool EnableRange;

        /// <summary>
        /// Checks for missed sends without provocation. Provocation being a client inside it's normal SendRate
        /// </summary>
        public bool EnableNonProvokedResendChecks;

        /// <summary>
        /// The curve to use to calculate the send rate
        /// </summary>
        public AnimationCurve DistanceSendrate = AnimationCurve.Constant(0, 500, 20);

        private readonly Dictionary<ulong, ClientSendInfo> m_ClientSendInfo = new Dictionary<ulong, ClientSendInfo>();
        private readonly ServerSendInfo m_ServerSendInfo = new ServerSendInfo();

        /// <summary>
        /// The delegate used to check if a move is valid
        /// </summary>
        /// <param name="clientId">The client id the move is being validated for</param>
        /// <param name="oldPos">The previous position</param>
        /// <param name="newPos">The new requested position</param>
        /// <returns>Returns Whether or not the move is valid</returns>
        public delegate bool MoveValidationDelegate(ulong clientId, Vector3 oldPos, Vector3 newPos);

        /// <summary>
        /// If set, moves will only be accepted if the custom delegate returns true
        /// </summary>
        public MoveValidationDelegate IsMoveValidDelegate = null;


        // private PlayerMovement m_Player;
        private Vector3 m_ServerVelocity = Vector3.zero;
        private Vector2 m_ClientInput = Vector3.zero;
        private Vector2[] m_ServerHistoryReceiveInput = new Vector2[20];
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        private float m_Speed = 20.0f;
        private float m_RotSpeed = 5.0f;
        

        public GameObject IsMineArrow;
        public TextMeshProUGUI playerName;

        private void OnValidate()
        {
            if (!AssumeSyncedSends && InterpolatePosition)
                InterpolatePosition = false;
            if (InterpolateServer && !InterpolatePosition)
                InterpolateServer = false;
            if (MinDegrees < 0)
                MinDegrees = 0;
            if (MinMeters < 0)
                MinMeters = 0;
            if (EnableNonProvokedResendChecks && !EnableRange)
                EnableNonProvokedResendChecks = false;
        }

        private float GetTimeForLerp(Vector3 pos1, Vector3 pos2)
        {
            return 1f / DistanceSendrate.Evaluate(Vector3.Distance(pos1, pos2));
        }

        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
            if (IsServer)
            {
                GetComponent<AudioListener>().enabled = false;
    #if UNITY_EDITOR
                AudioListener[] audios = GameObject.FindObjectsOfType<AudioListener>(); //cheat to avoid warning "more 1 audio listener at server side"
                for (int i = 0; i < audios.Length; i++)
                {
                    audios[i].enabled = true;
                    break;
                }
    #endif
            }
            else
            {
                if(IsLocalPlayer)
                {
                    if(!rb)
                    {
                        rb = GetComponent<Rigidbody>();
                    }
                    rb.isKinematic = true;
                    ActionPhraseManager.Instance.mainCharacter = this.gameObject.GetComponent<PlayerController>();
                }

    
                if(!GameManager.Instance.mIsMulti)
                {
                    
                    playerName.gameObject.SetActive(true);
                    IsMineArrow.SetActive(true);
                    GetComponent<AudioListener>().enabled = true;
                    playerName.text = ProfileMgr.Instance.UserName;
                    playerName.color = new Color32(248, 32, 190, 255);
                }
            }
            GetComponent<CharUI>().UpdateCharCustom();
            if (ProfileMgr.Instance.IsFirstPlayGame)
            {
                ProfileMgr.Instance.IsFirstPlayGame = false;
                ProfileMgr.Instance.SaveProfile();
            }
        }
        public override void NetworkStart()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            var temp = transform.position;
            temp.y = 0.5f;
            transform.position = temp;

            if (m_Rigidbody)
            {
                // Only the owner should ever move an object
                // If we don't set the non-local-player object as kinematic,
                // the local physics would apply and result in unwanted position
                // updates being sent up
                m_Rigidbody.isKinematic = IsLocalPlayer;
            }

            m_LastSentRot = transform.rotation;
            m_LastSentPos = transform.position;

            m_LerpStartPos = transform.position;
            m_LerpStartRot = transform.rotation;

            m_LerpEndPos = transform.position;
            m_LerpEndRot = transform.rotation;

            for(int v = 0; v < m_ServerHistoryReceiveInput.Length; ++v)
            {
                m_ServerHistoryReceiveInput[v] = Vector2.zero;
            }

            if (!IsServer && !IsLocalPlayer)
                enabled = false;
        }

        //[VINH] Send transform Client to Server
        // this function on Server side and it is called by Client
        [ServerRpc]
        private void SubmitInputFromClientToServerRpc(Vector2 input, float time, ServerRpcParams rpcParams = default)
        {
            for (int v = m_ServerHistoryReceiveInput.Length - 1; v > 0; --v)
            {
                m_ServerHistoryReceiveInput[v] = m_ServerHistoryReceiveInput[v-1];
            }
            m_ServerHistoryReceiveInput[0] = input;
            // Debug.Log("Server receive m_ServerHistoryReceiveInput[0]=" + m_ServerHistoryReceiveInput[0]);
        }

        // Check on server side
        private void ServerCheckForMissedSends()
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
            {
                if (!m_ClientSendInfo.ContainsKey(NetworkManager.Singleton.ConnectedClientsList[i].ClientId))
                {
                    m_ClientSendInfo.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId, new ClientSendInfo()
                    {
                        LastMissedPosition = null,
                        LastMissedRotation = null,
                        LastSent = 0
                    });
                }

                ClientSendInfo info = m_ClientSendInfo[NetworkManager.Singleton.ConnectedClientsList[i].ClientId];
                Vector3? receiverPosition = NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject == null ? null : new Vector3?(NetworkManager.Singleton.ConnectedClientsList[i].PlayerObject.transform.position);
                Vector3? senderPosition = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject == null ? null : new Vector3?(NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.transform.position);

                if ((receiverPosition == null || senderPosition == null && NetworkManager.Singleton.NetworkTime - info.LastSent >= (1f / FixedSendsPerSecond)) || NetworkManager.Singleton.NetworkTime - info.LastSent >= GetTimeForLerp(receiverPosition.Value, senderPosition.Value))
                {
                    /* why is this??? ->*/
                    Vector3? pos = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject == null ? null : new Vector3?(NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.transform.position);
                    /* why is this??? ->*/
                    Vector3? rot = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject == null ? null : new Vector3?(NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.transform.rotation.eulerAngles);

                    if (info.LastMissedPosition != null && info.LastMissedRotation != null)
                    {
                        info.LastSent = NetworkManager.Singleton.NetworkTime;

                        SubmitTransformFromServerToClientRpc(info.LastMissedPosition.Value, info.LastMissedRotation.Value.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveInput[0], 
                            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { NetworkManager.Singleton.ConnectedClientsList[i].ClientId } } });

                        info.LastMissedPosition = null;
                        info.LastMissedRotation = null;
                    }
                }
            }
        }

        [ClientRpc]
        private void SubmitTransformFromServerToClientRpc(Vector3 position, Vector3 eulerAngles, Vector3 velocity, Vector2 input, ClientRpcParams rpcParams = default)
        {
            if (enabled)
            {
                ApplyTransformInternal(position, Quaternion.Euler(eulerAngles));
            }
        }

        private void ApplyTransformInternal(Vector3 position, Quaternion rotation)
        {
            if (!enabled)
                return;

            if (InterpolatePosition && (!IsServer || InterpolateServer))
            {
                m_LastReceiveTime = Time.unscaledTime;
                m_LerpStartPos = transform.position;
                m_LerpStartRot = transform.rotation;
                m_LerpEndPos = position;
                m_LerpEndRot = rotation;
                m_LerpTime = 0;
            }
            else
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }

    protected override void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround + 0.1f))
        {
            if (hit.transform.tag == TAG.OBSTACLES_SLIDING)
            {
                slide = true;
            }
            else
            {
                slide = false;
            }
        }

        if (ragdollHelper && ragdollHelper.IsActive())
        {
            if (ragdollHelper.IsFalling())
            {
                DeadAndRespawn();
            }
            return;
        }

        
        if(canMove)
        {
            if (IsServer)
            {
                moveDir = GetMoveDirection(m_ServerHistoryReceiveInput[0]);
                if (moveDir.magnitude != 0)
                {
                    UpdateFaceDir();
                    UpdateWalking();
                }
            }
            //If we are server and interpolation is turned on for server OR we are not server and interpolation is turned on
            else if (InterpolatePosition)
            {
                if (Vector3.Distance(transform.position, m_LerpEndPos) > SnapDistance)
                {
                    //Snap, set T to 1 (100% of the lerp)
                    m_LerpTime = 1f;
                }

                float sendDelay = (IsServer || !EnableRange || !AssumeSyncedSends || NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject == null) ? (1f / FixedSendsPerSecond) : GetTimeForLerp(transform.position, NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform.position);
                m_LerpTime += Time.unscaledDeltaTime / sendDelay;

                if (ExtrapolatePosition && Time.unscaledTime - m_LastReceiveTime < sendDelay * MaxSendsToExtrapolate)
                    transform.position = Vector3.LerpUnclamped(m_LerpStartPos, m_LerpEndPos, m_LerpTime);
                else
                    transform.position = Vector3.Lerp(m_LerpStartPos, m_LerpEndPos, m_LerpTime);

                if (ExtrapolatePosition && Time.unscaledTime - m_LastReceiveTime < sendDelay * MaxSendsToExtrapolate)
                    transform.rotation = Quaternion.SlerpUnclamped(m_LerpStartRot, m_LerpEndRot, m_LerpTime);
                else
                    transform.rotation = Quaternion.Slerp(m_LerpStartRot, m_LerpEndRot, m_LerpTime);
            }
            
            {
                if (!IsServer)
                {
                    m_ClientInput.x = ActionPhraseManager.Instance.joystick.Horizontal;
                    m_ClientInput.y = ActionPhraseManager.Instance.joystick.Vertical;
                    #if UNITY_EDITOR
                    m_ClientInput.x += Input.GetAxisRaw("Horizontal");
                    m_ClientInput.y += Input.GetAxisRaw("Vertical");
                    #endif
                }

                if (NetworkManager.Singleton.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond))
                {
                    if (!IsServer)
                    {
                        m_LastSendTime = NetworkManager.Singleton.NetworkTime;
                        m_LastSentPos = transform.position;
                        m_LastSentRot = transform.rotation;

                        SubmitInputFromClientToServerRpc(m_ClientInput, NetworkManager.Singleton.NetworkTime);
                    }
                    else if((Vector3.Distance(transform.position, m_LastSentPos) > MinMeters || Quaternion.Angle(transform.rotation, m_LastSentRot) > MinDegrees))
                    {
                        m_LastSendTime = NetworkManager.Singleton.NetworkTime;
                        m_LastSentPos = transform.position;
                        m_LastSentRot = transform.rotation;

                        m_ServerVelocity = this.rb.velocity;                                                                          //ApplyTransformClientRpc(transform.position, transform.rotation.eulerAngles, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
                        SubmitTransformFromServerToClientRpc(gameObject.transform.position, gameObject.transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveInput[0]);
                    }
                }
            }

            if (IsServer && EnableRange && EnableNonProvokedResendChecks)
                ServerCheckForMissedSends();

        }

        if (IsServer)
        {
            base.FixedUpdate();

            switch (mState)
            {
                case PLAYER_STATE.IDLE:
                    if (canMove && moveDir.magnitude > 0)
                    {
                        SetState(PLAYER_STATE.WALK);
                    }
                    else
                    {
                        CheckGround();
                    }
                    break;
                case PLAYER_STATE.WALK:
                    if (!canMove || moveDir.magnitude == 0)
                    {
                        SetState(PLAYER_STATE.IDLE);
                    }
                    break;
                case PLAYER_STATE.SLIDE:
                    rb.AddForce(new Vector3(0, Define.GRAVITY * GetComponent<Rigidbody>().mass, 0));
                    if (slide && HasFloorUnder())
                    {
                        if (Mathf.Abs(rb.velocity.magnitude) < speed * 1.0f)
                        {
                            rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
                        }
                    }
                    else
                    {
                        SetState(PLAYER_STATE.IDLE);
                    }
                    break;
                case PLAYER_STATE.FALL:
                    rb.AddForce(new Vector3(0, Define.GRAVITY * GetComponent<Rigidbody>().mass, 0));
                    if (IsGrounded())
                    {
                        CountdownToDieTimer.Reset();
                        SetState(PLAYER_STATE.IDLE);
                    }
                    else
                    {
                        if (HasFloorUnder())
                        {
                            CountdownToDieTimer.Reset();

                            Vector3 targetVelocity = new Vector3(moveDir.x * airVelocity, rb.velocity.y, moveDir.z * airVelocity);
                            Vector3 velocity = rb.velocity;
                            Vector3 velocityChange = (targetVelocity - velocity);
                            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                            rb.AddForce(velocityChange, ForceMode.VelocityChange);
                            if (velocity.y < -maxFallSpeed)
                            {
                                rb.velocity = new Vector3(velocity.x, -maxFallSpeed, velocity.z);
                            }
                        }
                    }
                    if (!CountdownToDieTimer.IsDone())
                    {
                        CountdownToDieTimer.Update(Time.deltaTime);
                        if (CountdownToDieTimer.JustFinished())
                        {
                            DeadAndRespawn();
                        }
                    }
                    // if (fallingStartPos.y - transform.position.y > 10f)
                    // {
                    //     DeadAndRespawn();
                    // }
                    break;

                case PLAYER_STATE.FINISH:

                    timerControl.Update(Time.deltaTime);
                    if (timerControl.JustFinished())
                    {
                        ActionPhraseManager.Instance.EndGame();
                        GameEventMgr.Instance.SetEvent(GAME_EVENT.GOTO_RESULT);
                    }
                    break;
            }
        }
    }

    public Vector2 GetInput()
    {
        return m_ClientInput;
    }

    public Vector3 GetMoveDirection(Vector3 input)
    {
//         float deltaX = ActionPhraseManager.Instance.joystick.Horizontal;
//         float deltaY = ActionPhraseManager.Instance.joystick.Vertical;
// #if UNITY_EDITOR
//         deltaX += Input.GetAxisRaw("Horizontal");
//         deltaY += Input.GetAxisRaw("Vertical");
// #endif

        if (Mathf.Abs(input.x) >= 0.2f || Mathf.Abs(input.y) >= 0.2f)
        {
            if (!mainCamera)
            {
                mainCamera = GameObject.FindGameObjectWithTag(TAG.MAIN_CAMERA);
            }

            Vector3 v2 = input.y * mainCamera.transform.forward; //Vertical axis to which I want to move with respect to the camera
            Vector3 h2 = input.x * mainCamera.transform.right; //Horizontal axis to which I want to move with respect to the camera

            return (v2 + h2).normalized; //Global position to which I want to move in magnitude 1
        }
        else
        {
            return Vector3.zero;
        }
    }

    private void UpdateWalking()
    {
        // Calculate how fast we should be moving
        Vector3 targetVelocity = moveDir;
        targetVelocity *= speed;

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rb.velocity;
        if (targetVelocity.magnitude < velocity.magnitude) //If I'm slowing down the character
        {
            targetVelocity = velocity;
            rb.velocity /= 1.1f;
        }
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        if (!slide)
        {
            if (Mathf.Abs(rb.velocity.magnitude) < speed * 1.0f)
                rb.AddForce(velocityChange, ForceMode.VelocityChange);

        }
        else if (Mathf.Abs(rb.velocity.magnitude) < speed * 1.0f)
        {
            rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
            //Debug.Log(rb.velocity.magnitude);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case TAG.FINISH:

                if (!GameManager.Instance.mIsMulti)
                {
                    InitFinish();
                }
                break;

            case TAG.CHECK_POINT:
                foreach (CollidedObject colObj in collidedObjectList)
                {
                    if (other.gameObject == colObj.obj)
                        return;
                }
                AddToCollidedList(other.gameObject);

                if (vfxHitCheckpoint)
                {
                    if (GameManager.Instance.mIsMulti)
                        PlayVFXClientRpc(TAG.OBSTACLES_BOUNCER);
                    else
                        PlayVfx(TAG.CHECK_POINT);
                }

                // latestCheckpointPassed = other.gameObject.GetComponent<CheckPointInfo>();
                SetCheckPoint(other.transform.position);
                break;

            case TAG.TRIGGER_CAMERA:
                if (!GameManager.Instance.mIsMulti)
                {
                    CameraManager.Instance.TriggerCamera(other.gameObject);
                }
                break;
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case TAG.OBSTACLES_ROTATOR:
                if (other.isTrigger)
                {
                    ObsMovingTube rotatorObj = other.gameObject.GetComponent<ObsMovingTube>();
                    switch (rotatorObj.GetDirectionRotator())
                    {
                        case DIRECTION.LEFT:
                            transform.position += Vector3.left * rotatorObj.GetRotateSpeed() * Time.deltaTime;
                            break;
                        case DIRECTION.RIGHT:
                            transform.position += Vector3.right * rotatorObj.GetRotateSpeed() * Time.deltaTime;
                            break;
                    }
                }
                break;
        }
    }

    void InitFinish()
    {
        mState = PLAYER_STATE.FINISH;
        ActionPhraseManager.Instance.ActiveWinner();
        timerControl.SetDuration(TIME_FINISH);
        PlayAnim(victoryTrigger);
    }

    protected override void TriggerFootstepEvent()
    {
        base.TriggerFootstepEvent();
    }

    
    // public void UpdatePositionForClients()
    // {
    //     mNetworkVariableTransform.Position.Value = transform.position;
    //     mNetworkVariableTransform.Rotation.Value = transform.rotation;
    // }

    // [ServerRpc]
    // public void UpdateMoveDirByServerRpc(float x, float z)
    // {
    //     this.moveDir.x = x;
    //     this.moveDir.z = z;
    // }
}
