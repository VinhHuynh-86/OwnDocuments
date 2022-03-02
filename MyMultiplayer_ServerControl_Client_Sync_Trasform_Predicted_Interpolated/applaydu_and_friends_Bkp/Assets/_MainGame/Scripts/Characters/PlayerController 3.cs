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
        public Vector2 directionInput = Vector2.zero;
        public Vector2 rotationInput = Vector2.zero;
        public Vector3 pos = Vector3.zero;
        public Vector3 rot = Vector3.zero;
        public Vector3 velocity = Vector3.zero;
        public Vector3 deltaPos = Vector3.zero;
        public Vector3 deltaRot = Vector3.zero;

        public HistoryStatePoint()
        {
        }

        public HistoryStatePoint(float _deltaTime, Vector2 _directionInput, Vector2 _rotationInput, Vector3 _pos, Vector3 _rot, Vector3 _velocity, Vector3 _deltaPos, Vector3 _deltaRot)
        {
            deltaTime = _deltaTime;
            directionInput = _directionInput;
            rotationInput = _rotationInput;
            pos = _pos;
            rot = _rot;
            velocity = _velocity;
            deltaPos = _deltaPos;
            deltaRot = _deltaRot;
        }
    }
    public class StatePoint
    {
        public float timestamp = 0;
        public Vector2 directionInput = Vector2.zero;
        public Vector2 rotationInput = Vector2.zero;
        public Vector3 pos = Vector3.zero;
        public Vector3 rot = Vector3.zero;
        public Vector3 velocity = Vector3.zero;

        public StatePoint()
        {
        }
        public StatePoint(float _timestamp, Vector2 _directionInput, Vector2 _rotationInput, Vector3 _pos, Vector3 _rot, Vector3 _velocity)
        {
            timestamp = _timestamp;
            directionInput = _directionInput;
            rotationInput = _rotationInput;
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
        public float m_SnapDistance = 0.1f;

        /// <summary>
        /// The distance before snaping to the position
        /// </summary>
        [Tooltip("The transform will snap if the velocity is greater than this velocity")]
        public float m_VelocityTolerance = 0.1f;

        /// <summary>
        /// Should the server interpolate
        /// </summary>
        public bool InterpolateServer = true;

        /// <summary>
        /// Enables extrapolation
        /// </summary>
        public bool ExtrapolatePosition = false;

        /// <summary>
        /// The maximum amount of expected send rates to extrapolate over when awaiting new packets.
        /// A higher value will result in continued extrapolation after an object has stopped moving
        /// </summary>
        public float MaxSendsToExtrapolate = 5;

        public bool EnableRange;

        /// <summary>
        /// Checks for missed sends without provocation. Provocation being a client inside it's normal SendRate
        /// </summary>
        public bool EnableNonProvokedResendChecks;
        public AnimationCurve DistanceSendrate = AnimationCurve.Constant(0, 500, 20);
        /// <summary>
        /// The base amount of sends per seconds to use when range is disabled
        /// </summary>
        public static float FixedSendsPerSecond = 20f;
        /// <summary>
        /// The min meters to move before a send is sent
        /// </summary>
        public float MinMeters = 0.15f;

        /// <summary>
        /// The min degrees to rotate before a send it sent
        /// </summary>
        public float MinDegrees = 1.5f;
        private readonly Dictionary<ulong, ClientSendInfo> m_ClientSendInfo = new Dictionary<ulong, ClientSendInfo>();
        private const float k_Epsilon = 0.0001f;
        private bool m_Interpolate = true;
        public float interpolationBackTime = 0.1f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        
        // Keep track of what slots are used
	    int m_TimestampCount = 0;
        private float m_LastSendTime = 0f;
        private float m_LastServerSendTime = 0f;
        private Vector3 m_LastSentPos = Vector3.zero;
        private Quaternion m_LastSentRot = Quaternion.identity;
        private Vector3 m_ServerVelocity = Vector3.zero;
        private List<HistoryStatePoint> m_ClientHistoryState = new List<HistoryStatePoint>();
        private StatePoint m_ServerState = new StatePoint();
        private StatePoint m_ClientState = new StatePoint();
        private StatePoint m_ClientPredictedState = new StatePoint();
        // We store twenty states with "playback" information
        private Vector2[] m_ServerHistoryReceiveDirectionInput = new Vector2[20];
        private Vector2[] m_ClientHistoryGetDirectionInputBack = new Vector2[20];
        private Vector2 m_ClientDirectionInput = Vector2.zero;
		
		private Vector2[] m_ServerHistoryReceiveRotationInput = new Vector2[20];
		private Vector2[] m_ClientHistoryGetRotationInputBack = new Vector2[20];
		private Vector2 m_ClientRotationInput = Vector2.zero;
        private float m_Speed = 10.0f;
        private float m_RotSpeed = 5.0f;
        private float m_Latency = 0.025f; //1.000f / FixedSendsPerSecond; // = 0.025f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        private float m_ClientHistoryDuration = 0f;
        float CONVERGE_MULTIPLIER = 0.05f;
        private bool isSubmitted = true;
        

        public GameObject IsMineArrow;
        public TextMeshProUGUI playerName;

        protected void OnValidate()
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

        protected override void Awake()
        {
            base.Awake();
            for(int v = 0; v < m_ServerHistoryReceiveDirectionInput.Length; ++v)
            {
                m_ServerHistoryReceiveDirectionInput[v] = new Vector2(0, 0);
            }
            for(int v = 0; v < m_ClientHistoryGetDirectionInputBack.Length; ++v)
            {
                m_ClientHistoryGetDirectionInputBack[v] = new Vector2(0, 0);
            }
            for(int v = 0; v < m_ServerHistoryReceiveRotationInput.Length; ++v)
            {
                m_ServerHistoryReceiveRotationInput[v] = new Vector2(0, 0);
            }
            for(int v = 0; v < m_ClientHistoryGetRotationInputBack.Length; ++v)
            {
                m_ClientHistoryGetRotationInputBack[v] = new Vector2(0, 0);
            }
            m_ClientHistoryState.Add(new HistoryStatePoint(0, Vector2.zero, Vector2.zero, this.transform.position, this.transform.eulerAngles, this.rb.velocity, Vector3.zero, Vector3.zero));
        }

        [ServerRpc]
        public void SubmitInputFromClientToServerRpc(float time, Vector2 directionInput, Vector2 rotationInput, ServerRpcParams rpcParams = default)
        {
            Debug.Log("SubmitInputFromClientToServerRpc");

            for (int v = m_ServerHistoryReceiveDirectionInput.Length - 1; v > 0; --v)
            {
                m_ServerHistoryReceiveDirectionInput[v] = m_ServerHistoryReceiveDirectionInput[v-1];
            }
            moveDir.x = directionInput.x;
            moveDir.z = directionInput.y;
            m_ServerHistoryReceiveDirectionInput[0] = directionInput;
            m_ServerHistoryReceiveRotationInput[0] = rotationInput;

            if (IsServer)
            {
                m_ServerVelocity = this.rb.velocity;
                SubmitTransformFromServerToClientRpc(GameNetManager.Instance.mNwManager.NetworkTime, m_ServerHistoryReceiveDirectionInput[0], m_ServerHistoryReceiveRotationInput[0], transform.position, transform.eulerAngles, m_ServerVelocity);
            }
        }

        [ClientRpc]
        public void SubmitTransformFromServerToClientRpc(float timestamp, Vector2 directionInput, Vector2 rotationInput, Vector3 position, Vector3 eulerAngles, Vector3 velocity, ClientRpcParams rpcParams = default)
        {
            Debug.Log("SubmitTransformFromServerToClientRpc");

            // if (IsOwner && IsClient)
            if(IsLocalPlayer)
            {
                ReceiveServerFrame(new StatePoint(timestamp, directionInput, rotationInput, position, eulerAngles, velocity));
            }         
        }

        // Check on server side
        protected void ServerCheckForMissedSends()
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

                        SubmitTransformFromServerToClientRpc(GameNetManager.Instance.mNwManager.NetworkTime, m_ServerHistoryReceiveDirectionInput[0], m_ServerHistoryReceiveRotationInput[0], info.LastMissedPosition.Value, info.LastMissedRotation.Value.eulerAngles, m_ServerVelocity,
                            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { NetworkManager.Singleton.ConnectedClientsList[i].ClientId } } });

                        info.LastMissedPosition = null;
                        info.LastMissedRotation = null;
                    }
                }
            }
        }

        protected float GetTimeForLerp(Vector3 pos1, Vector3 pos2)
        {
            return 1f / DistanceSendrate.Evaluate(Vector3.Distance(pos1, pos2));
        }

        public void ReceiveServerFrame(StatePoint serverFrame)
        {
            Debug.Log("ReceiveServerFrame");

            for (int v = m_ClientHistoryGetDirectionInputBack.Length - 1; v > 0; --v)
            {
                m_ClientHistoryGetDirectionInputBack[v] = m_ClientHistoryGetDirectionInputBack[v-1];
            }
            m_ClientHistoryGetDirectionInputBack[0] = serverFrame.directionInput;

            for (int v = m_ClientHistoryGetRotationInputBack.Length - 1; v > 0; --v)
            {
                m_ClientHistoryGetRotationInputBack[v] = m_ClientHistoryGetRotationInputBack[v-1];
            }
            m_ClientHistoryGetRotationInputBack[0] = serverFrame.rotationInput;

            // Remove frames from history until it's duration is equal to the latency.
            float dt = m_ClientHistoryDuration > m_Latency ? (m_ClientHistoryDuration - m_Latency) : 0.0f;
            m_ClientHistoryDuration -= dt;
            float t = 0;
            while (m_ClientHistoryState.Count > 0 && dt > 0)
            {
                if (dt >= m_ClientHistoryState[0].deltaTime)
                {
                    dt -= m_ClientHistoryState[0].deltaTime;
                    m_ClientHistoryState.RemoveAt(0);
                }
                else
                {
                    t = (1 - dt / m_ClientHistoryState[0].deltaTime);
                    m_ClientHistoryState[0].deltaTime -= dt;
                    m_ClientHistoryState[0].deltaPos *= t;
                    m_ClientHistoryState[0].deltaRot *= t;
                    break;
                }
            }

            // If predicted and server velocity difference exceeds the tolerance,
            // replay inputs. This is only needed if the
            // velocity for one frame depends on the velocity of the previous
            // frame. Depending on your game you may also need
            // to do this for angular velocity or other variables.
            // if((m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            // if ((m_ClientHistoryState[0].deltaPos.magnitude > m_SnapDistance) || (m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            if (m_ClientHistoryState[0].deltaPos.magnitude > m_SnapDistance)
            {
                // Debug.Log("ReceiveServerFrame::m_SnapDistance ->replay");
                Debug.Log("ReceiveServerFrame::replay : m_ClientHistoryState[] = " + m_ClientHistoryState[0].pos + " <= VS => serverFrame.pos" + serverFrame.pos);
                m_ClientPredictedState = serverFrame;

                foreach(HistoryStatePoint frame in m_ClientHistoryState)
                {
                    StatePoint newState = ReComputePosition(m_ClientPredictedState, frame);
                    frame.deltaPos = newState.pos - m_ClientPredictedState.pos;
                    frame.deltaRot = newState.rot - m_ClientPredictedState.rot;
                    frame.velocity = newState.velocity;
                    m_ClientPredictedState = newState;
                }
                // Debug.Log("ReceiveServerFrame::replay : 2 m_ClientPredictedState = " + m_ClientPredictedState.pos);
            }
            else
            {
                Debug.Log("ReceiveServerFrame::predicted state");

                // Add deltas from history to server state to get predicted state.
                m_ClientPredictedState = serverFrame;
                foreach(HistoryStatePoint frame in m_ClientHistoryState)
                {
                    m_ClientPredictedState.pos += frame.deltaPos;
                    m_ClientPredictedState.rot += frame.deltaRot;
                }
            }
        }

        // Called every client frame on Client side
        public void UpdateClientFrame(float deltaTime)
        {
            // m_ClientHistoryGetInputBack[0] = m_ClientInput;
            // Run player controller to get new prediction and add to m_ClientHistoryState
            StatePoint newState = UpdateTransform(m_ClientPredictedState, deltaTime);
            HistoryStatePoint frame = new HistoryStatePoint(deltaTime, m_ClientPredictedState.directionInput, m_ClientPredictedState.rotationInput, newState.pos, newState.rot, newState.velocity, newState.pos - m_ClientPredictedState.pos, newState.rot - m_ClientPredictedState.rot);
            m_ClientHistoryState.Add(frame);
            m_ClientHistoryDuration += deltaTime;
            m_ClientPredictedState = newState;
        }

        protected StatePoint ReComputePosition(StatePoint predictedStatePoint, HistoryStatePoint frame)
        {
            this.transform.position = predictedStatePoint.pos;
            this.transform.eulerAngles = predictedStatePoint.rot;
            this.rb.velocity = predictedStatePoint.velocity;
            StatePoint newState = new StatePoint();
            UpdateFaceDir(frame.directionInput);
            UpdateWalking(frame.directionInput);
            newState.pos = this.transform.position;
            newState.rot = this.transform.eulerAngles; //predictedStatePoint.rot + frame.deltaRot; // Need to compute rotation
            newState.velocity = this.rb.velocity; // predictedStatePoint.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + frame.deltaTime; // m_Latency;
            newState.directionInput = frame.directionInput;
            newState.rotationInput = frame.rotationInput;
           
            return newState;
        }

        protected StatePoint UpdateTransform(StatePoint predictedStatePoint, float deltaTime)
        {
            StatePoint newState = new StatePoint();
            UpdateFaceDir(predictedStatePoint.directionInput);
            UpdateWalking(predictedStatePoint.directionInput);
            newState.pos = this.transform.position;
            newState.rot = this.transform.eulerAngles; // Need to compute rotation
            newState.velocity = this.rb.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + deltaTime;
            newState.directionInput = predictedStatePoint.directionInput;
            newState.rotationInput = predictedStatePoint.rotationInput;

            return newState;
        }

        public void UpdateInput()
        {
            moveDir = GetMoveDirection();
            m_ClientDirectionInput.x = moveDir.x;
            m_ClientDirectionInput.y = moveDir.z;

            if (GameNetManager.Instance.mNwManager.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond))
            {
                if(m_ClientDirectionInput != Vector2.zero || m_ClientHistoryGetDirectionInputBack[0] != Vector2.zero)
                {
                    m_LastSendTime = GameNetManager.Instance.mNwManager.NetworkTime;
                    SubmitInputFromClientToServerRpc(GameNetManager.Instance.mNwManager.NetworkTime, m_ClientDirectionInput, m_ClientRotationInput);
                }
            }        
        }
        protected void UpdateFaceDir(Vector2 directionInput)
        {
            moveDir = new Vector3(directionInput.x, 0, directionInput.y);
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
        protected void UpdateWalking(Vector2 directionInput)
        {
            // Calculate how fast we should be moving
            moveDir = new Vector3(directionInput.x, 0, directionInput.y);
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

        protected void UpdateRotation(Vector2 predictedRotationInput)
        {
            if(predictedRotationInput.x > 0)
            {
                Quaternion rot = Quaternion.Euler(0, predictedRotationInput.x * 90 * m_RotSpeed * Time.fixedDeltaTime, 0);
                transform.rotation = rot * transform.rotation;
            }
            if(predictedRotationInput.y > 0)
            {
                Quaternion rot = Quaternion.Euler(0, predictedRotationInput.y * (-90) * m_RotSpeed * Time.fixedDeltaTime, 0);
                transform.rotation = rot * transform.rotation;
            }
        }

    public override void NetworkStart()
    {
        if (!IsServer && !IsLocalPlayer)
            enabled = false;
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (ragdollHelper && ragdollHelper.IsActive())
        {
            if (ragdollHelper.IsFalling())
            {
                DeadAndRespawn();
            }
            return;
        }
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
            
        if(canMove)
        {
            // if (IsOwner && IsClient)
            if(IsLocalPlayer)
            {
                moveDir = GetMoveDirection();
                m_ClientDirectionInput.x = moveDir.x;
                m_ClientDirectionInput.y = moveDir.z;
                if (GameNetManager.Instance.mNwManager.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond))
                {
                    if(m_ClientDirectionInput != Vector2.zero || m_ClientHistoryGetDirectionInputBack[0] != Vector2.zero)
                    {
                        m_LastSendTime = GameNetManager.Instance.mNwManager.NetworkTime;
                        SubmitInputFromClientToServerRpc(GameNetManager.Instance.mNwManager.NetworkTime, m_ClientDirectionInput, m_ClientRotationInput);
                    }
                }        
            }

            if(IsServer)
            {
                UpdateFaceDir(m_ServerHistoryReceiveDirectionInput[0]);
                UpdateWalking(m_ServerHistoryReceiveDirectionInput[0]);
            }

            // if (IsOwner && IsClient)
            if(IsLocalPlayer)
            {
                UpdateClientFrame(Time.fixedDeltaTime);
            }    

        }

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
                else if (CheckGround())
                {
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

    public Vector2 GetDirectionInput()
    {
        return m_ClientDirectionInput;
    }

    public Vector3 GetMoveDirection()
    {
        float deltaX = ActionPhraseManager.Instance.joystick.Horizontal;
        float deltaY = ActionPhraseManager.Instance.joystick.Vertical;
#if UNITY_EDITOR
        deltaX += Input.GetAxisRaw("Horizontal");
        deltaY += Input.GetAxisRaw("Vertical");
#endif

        if (Mathf.Abs(deltaX) >= 0.2f || Mathf.Abs(deltaY) >= 0.2f)
        {
            if (!mainCamera)
            {
                mainCamera = GameObject.FindGameObjectWithTag(TAG.MAIN_CAMERA);
            }

            Vector3 v2 = deltaY * mainCamera.transform.forward; //Vertical axis to which I want to move with respect to the camera
            Vector3 h2 = deltaX * mainCamera.transform.right; //Horizontal axis to which I want to move with respect to the camera

            return (v2 + h2).normalized; //Global position to which I want to move in magnitude 1
        }
        else
        {
            return Vector3.zero;
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
}
