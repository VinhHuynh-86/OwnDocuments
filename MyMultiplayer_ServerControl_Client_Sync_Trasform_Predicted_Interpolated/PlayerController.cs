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
        public Vector2 directionInput = Vector2.zero;
        public Vector2 rotationInput = Vector2.zero; 

        public HistoryStatePoint()
        {
        }

        public HistoryStatePoint(Vector2 _directionInput, Vector2 _rotationInput, float _deltaTime, Vector3 _deltaPos, Vector3 _deltaRot, Vector3 _velocity)
        {
            deltaTime = _deltaTime;
            deltaPos = _deltaPos;
            deltaRot = _deltaRot;
            velocity = _velocity;
            directionInput = _directionInput;
            rotationInput = _rotationInput;
        }
    }
    public class StatePoint
    {
        public Vector2 directionInput = Vector2.zero;
        public Vector2 rotationInput = Vector2.zero;
        public float timestamp = 0;
        public Vector3 pos = Vector3.zero;
        public Vector3 rot = Vector3.zero;
        public Vector3 velocity = Vector3.zero;

        public StatePoint()
        {
        }
        public StatePoint(Vector2 _directionInput, Vector2 _rotationInput, float _timestamp, Vector3 _pos, Vector3 _rot, Vector3 _velocity)
        {
            directionInput = _directionInput;
            rotationInput = _rotationInput;
            timestamp = _timestamp;
            pos = _pos;
            rot = _rot;
            velocity = _velocity;
        }
    }
////////////////////////////////////////////////////////////////////////////////////////////////////



public class PlayerController : GenericCharacter
{

    private const float k_Epsilon = 0.0001f;
        public float interpolationBackTime = 0.1f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        
        // We store twenty states with "playback" information
        State[] m_BufferedState = new State[20];
        // Keep track of what slots are used
	    int m_TimestampCount = 0;
        private float m_LastSendTime = 0f;
        public static float FixedSendsPerSecond = 20f;

        private Vector3 m_ServerVelocity = Vector3.zero;
        private List<HistoryStatePoint> m_ClientHistoryState = new List<HistoryStatePoint>();
        private StatePoint m_ServerState = new StatePoint();
        private StatePoint m_ClientState = new StatePoint();
        private StatePoint m_ClientPredictedState = new StatePoint();
        private Vector2[] m_ServerHistoryReceiveDirectionInput = new Vector2[20];
        private Vector2[] m_ClientHistoryGetDirectionInputBack = new Vector2[20];
        private Vector2 m_ClientDirectionInput = Vector2.zero;
		
		private Vector2[] m_ServerHistoryReceiveRotationInput = new Vector2[20];
		private Vector2[] m_ClientHistoryGetRotationInputBack = new Vector2[20];
		private Vector2 m_ClientRotationInput = Vector2.zero;
        private float m_Speed = 10.0f;
        private float m_RotSpeed = 5.0f;
        private float m_Latency = 0.025f; //1f / FixedSendsPerSecond; // = 0.025f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        private float m_ClientHistoryDuration = 0f;
        private float m_SnapDistance = 0.25f;
        private float m_VelocityTolerance = 0.1f;
        float CONVERGE_MULTIPLIER = 0.05f;
        
        // public float MinMeters = 0.15f;
        // /// <summary>
        // /// The min degrees to rotate before a send it sent
        // /// </summary>
        // public float MinDegrees = 1.5f;
        // private float m_LastServerSendTime = 0f;
        // private Vector3 m_LastSentPos = Vector3.zero;
        // private Quaternion m_LastSentRot = Quaternion.identity;
        // private bool m_Interpolate = true;
        // private float m_Speed = 10.0f;
        // private bool isSubmitted = true;
        

    public GameObject IsMineArrow;
    public TextMeshProUGUI playerName;

        protected void Awake()
        {
            for(int v = 0; v < m_BufferedState.Length; ++v)
            {
                m_BufferedState[v] = new State();
            }
            for(int v = 0; v < m_ServerHistoryReceiveDirectionInput.Length; ++v)
            {
                m_ServerHistoryReceiveDirectionInput[v] = Vector2.zero;
            }
            for(int v = 0; v < m_ClientHistoryGetDirectionInputBack.Length; ++v)
            {
                m_ClientHistoryGetDirectionInputBack[v] = Vector2.zero;
            }
            for(int v = 0; v < m_ServerHistoryReceiveRotationInput.Length; ++v)
            {
                m_ServerHistoryReceiveRotationInput[v] = Vector2.zero;
            }
            for(int v = 0; v < m_ClientHistoryGetRotationInputBack.Length; ++v)
            {
                m_ClientHistoryGetRotationInputBack[v] = Vector2.zero;
            }
            m_ClientHistoryState.Add(new HistoryStatePoint(Vector2.zero, Vector2.zero, 0, Vector3.zero, Vector3.zero, Vector3.zero));
        }

        [ServerRpc]
        public void SubmitInputFromClientToServerRpc(Vector2 directionInput, Vector2 rotationInput, float time, ServerRpcParams rpcParams = default)
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
                // SubmitTransformFromServerToClientRpc(transform.position, transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveDirectionInput[0], GameNetManager.Instance.mNwManager.NetworkTime, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = GameNetManager.Instance.mNwManager.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
                SubmitTransformFromServerToClientRpc(m_ServerHistoryReceiveDirectionInput[0], m_ServerHistoryReceiveRotationInput[0], GameNetManager.Instance.mNwManager.NetworkTime, transform.position, transform.eulerAngles, m_ServerVelocity);
            }
        }

        [ClientRpc]
        public void SubmitTransformFromServerToClientRpc(Vector2 directionInput, Vector2 rotationInput, float timestamp, Vector3 position, Vector3 eulerAngles, Vector3 velocity, ClientRpcParams rpcParams = default)
        {
            Debug.Log("SubmitTransformFromServerToClientRpc");

            if (IsOwner && IsClient)
            {
                ReceiveServerFrame(new StatePoint(directionInput, rotationInput, timestamp, position, eulerAngles, velocity));
            }         
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

            m_ServerState = serverFrame;

            // If predicted and server velocity difference exceeds the tolerance,
            // replay inputs. This is only needed if the
            // velocity for one frame depends on the velocity of the previous
            // frame. Depending on your game you may also need
            // to do this for angular velocity or other variables.
            // if((m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            if ((m_ClientHistoryState[0].deltaPos.magnitude > m_SnapDistance) || (m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            {
                // Debug.Log("ReceiveServerFrame::m_SnapDistance ->replay");
                Debug.Log("ReceiveServerFrame::ReComputePosition : 1 m_ClientPredictedState = " + m_ClientPredictedState.pos);
                m_ClientPredictedState = m_ServerState;
                foreach(HistoryStatePoint frame in m_ClientHistoryState)
                {
                    StatePoint newState = ReComputePosition(m_ClientPredictedState, frame);
                    frame.deltaPos = newState.pos - m_ClientPredictedState.pos;
                    frame.deltaRot = newState.rot - m_ClientPredictedState.rot;
                    frame.velocity = newState.velocity;
                    m_ClientPredictedState = newState;
                }
                Debug.Log("ReceiveServerFrame::replay : 2 m_ClientPredictedState = " + m_ClientPredictedState.pos);
            }
            else
            {
                Debug.Log("ReceiveServerFrame::predicted state");

                // Add deltas from history to server state to get predicted state.
                m_ClientPredictedState = m_ServerState;
                foreach(HistoryStatePoint frame in m_ClientHistoryState)
                {
                    m_ClientPredictedState.pos += frame.deltaPos;
                    m_ClientPredictedState.rot += frame.deltaRot;
                }
            }
        }

        // Called every client frame on Client side
        private void UpdateClientFrame(float deltaTime)
        {
            Debug.Log("UpdateClientFrame");

            // m_ClientHistoryGetDirectionInputBack[0] = m_ClientDirectionInput;
            // Run player controller to get new prediction and add to m_ClientHistoryState
            StatePoint newState = UpdateTransform(m_ClientPredictedState, deltaTime);
            HistoryStatePoint frame = new HistoryStatePoint(m_ClientPredictedState.directionInput, m_ClientPredictedState.rotationInput, deltaTime, newState.pos - m_ClientPredictedState.pos, newState.rot - m_ClientPredictedState.rot, newState.velocity);
            m_ClientHistoryState.Add(frame);
            m_ClientHistoryDuration += deltaTime;

            // Extrapolate predicted position
            // CONVERGE_MULTIPLIER is a constant. Lower values make the client converge with the server more aggressively.
            // We chose 0.05.
            Vector3 rotationalVelocity = (newState.rot - m_ClientPredictedState.rot) / deltaTime;
            Vector3 extrapolatedPosition = m_ClientPredictedState.pos + newState.velocity * m_Latency * CONVERGE_MULTIPLIER;
            Vector3 extrapolatedRotation = m_ClientPredictedState.rot + rotationalVelocity * m_Latency * CONVERGE_MULTIPLIER;

            // Interpolate client position towards extrapolated position
            float t = (deltaTime / (m_Latency * (1 + CONVERGE_MULTIPLIER)));
            m_ClientState.pos = (extrapolatedPosition - m_ClientState.pos) * t;
            m_ClientState.rot = (extrapolatedRotation - m_ClientState.rot) * t;

            m_ClientPredictedState = newState;
        }

        private StatePoint ReComputePosition(StatePoint predictedStatePoint, HistoryStatePoint frame)
        {
            StatePoint newState = new StatePoint();
            moveDir = new Vector3(frame.directionInput.x, 0, frame.directionInput.y );
            // UpdateRotation(frame.rotationInput);
            UpdateMovement(frame.directionInput);
            newState.pos = this.transform.position;
            newState.rot = this.transform.eulerAngles; //predictedStatePoint.rot + frame.deltaRot; // Need to compute rotation
            newState.velocity = this.rb.velocity; // predictedStatePoint.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + frame.deltaTime; // m_Latency;
            newState.directionInput = frame.directionInput;
            newState.rotationInput = frame.rotationInput;
           
            return newState;
        }

        private StatePoint UpdateTransform(StatePoint predictedStatePoint, float deltaTime)
        {
            StatePoint newState = new StatePoint();
            moveDir = new Vector3(predictedStatePoint.directionInput.x, 0, predictedStatePoint.directionInput.y);
            // UpdateRotation(predictedStatePoint.rotationInput);
            UpdateMovement(predictedStatePoint.directionInput);
            newState.pos = this.transform.position;
            newState.rot = this.transform.eulerAngles; // Need to compute rotation
            newState.velocity = this.rb.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + deltaTime;
            newState.directionInput = predictedStatePoint.directionInput;
            newState.rotationInput = predictedStatePoint.rotationInput;

            return newState;
        }

        private void UpdateMovement(Vector2 predictedDirectionInput)
        {
            // Calculate how fast we should be moving
                        moveDir = new Vector3(predictedDirectionInput.x, 0, predictedDirectionInput.y);
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

        private void UpdateRotation(Vector2 predictedRotationInput)
        {
            // if(predictedRotationInput.x > 0)
            // {
            //     Quaternion rot = Quaternion.Euler(0, predictedRotationInput.x * 90 * m_RotSpeed * Time.fixedDeltaTime, 0);
            //     transform.rotation = rot * transform.rotation;
            // }
            // if(predictedRotationInput.y > 0)
            // {
            //     Quaternion rot = Quaternion.Euler(0, predictedRotationInput.y * (-90) * m_RotSpeed * Time.fixedDeltaTime, 0);
            //     transform.rotation = rot * transform.rotation;
            // }
        }


        private void Interpolate()
        {
            float currentTime = NetworkManager.NetworkTime;
            float interpolationTime = currentTime - interpolationBackTime;
            // We have a window of interpolationBackTime where we basically play 
            // By having interpolationBackTime the average ping, you will usually use interpolation.
            // And only if no more data arrives we will use extrapolation
            
            // Use interpolation
            // Check if latest state exceeds interpolation time, if this is the case then
            // it is too old and extrapolation should be used
            if (m_BufferedState[0].timestamp > interpolationTime)
            {
                for (int v = 0; v < m_TimestampCount; v++)
                {
                    // Find the state which matches the interpolation time (time+0.1) or use last state
                    if (m_BufferedState[v].timestamp <= interpolationTime || v == m_TimestampCount-1)
                    {
                        // The state one slot newer (<100ms) than the best playback state
                        State rhs = m_BufferedState[Mathf.Max(v-1, 0)];
                        // The best playback state (closest to 100 ms old (default time))
                        State lhs = m_BufferedState[v];
                        
                        // Use the time between the two slots to determine if interpolation is necessary
                        float length = rhs.timestamp - lhs.timestamp;
                        float t = 0.0F;
                        // As the time difference gets closer to 100 ms t gets closer to 1 in 
                        // which case rhs is only used
                        if (length > k_Epsilon)
                        {
                            Debug.Log("Interpolate");
                            t = ((interpolationTime - lhs.timestamp) / length);
                        }
                        
                        // if t=0 => lhs is used directly
                        transform.position = Vector3.Lerp(lhs.pos, rhs.pos, t);
                        transform.rotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);

                        
                        return;
                    }
                }
            }
            // Use extrapolation. Here we do something really simple and just repeat the last
            // received state. You can do clever stuff with predicting what should happen.
            else
            {
                
                Debug.Log("extrapolation");
            }
        }


    protected override void Start()
    {
        base.Start();
        if (IsServer)
        {
            GetComponent<AudioListener>().enabled = false;
#if UNITY_EDITOR
            AudioListener[] audios = GameObject.FindObjectsOfType<AudioListener>();
            for(int i = 0; i < audios.Length; i++)
            {
                audios[i].enabled = true;
                break;
            }
#endif
        }
        else
        {
            // mRB = GetComponent<Rigidbody>();
            // Destroy(mRB);
            // GetComponent<Collider>().enabled = false;
            if(IsLocalPlayer)
            {
                ActionPhraseManager.Instance.mainCharacter = this.gameObject.GetComponent<PlayerController>();
            }

            playerName.gameObject.SetActive(true);
            IsMineArrow.SetActive(true);
            GetComponent<AudioListener>().enabled = true;
            playerName.text = ProfileMgr.Instance.UserName;
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
        if(canMove)
        {
            if (IsOwner && IsClient)
            {
                if (GameNetManager.Instance.mNwManager.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond))
                {
                    moveDir = GetMoveDirection();
                    m_ClientDirectionInput.x = moveDir.x;
                    m_ClientDirectionInput.y = moveDir.z;
                    m_ClientRotationInput = new Vector2(Input.GetKey(KeyCode.Keypad1) == true ? 1 : 0, Input.GetKey(KeyCode.Keypad3) == true ? 1 : 0);

                    if(m_ClientDirectionInput != Vector2.zero || m_ClientHistoryGetDirectionInputBack[0] != Vector2.zero)
                    {
                        m_LastSendTime = GameNetManager.Instance.mNwManager.NetworkTime;
                        SubmitInputFromClientToServerRpc(m_ClientDirectionInput, m_ClientRotationInput, GameNetManager.Instance.mNwManager.NetworkTime);
                    }
                }        
            }
        }

        base.FixedUpdate();

        UpdateInputPlaying();
        // if(GameManager.Instance.mIsMulti)
        //     UpdatePositionForClients();
        
        if (ragdollHelper && ragdollHelper.IsActive())
        {
            if (ragdollHelper.IsFalling())
            {
                {
                    if (!GameManager.Instance.mIsMulti)
                    {
                        DeadAndRespawn();
                    }
                    else
                    {
                        DeadAndRespawn();
                    }
                }
            }
            return;
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
                    UpdateFaceDir();

                    if(IsServer)
                    {
                        // UpdateRotation(m_ServerHistoryReceiveRotationInput[0]);
                        Vector2 newMove = new Vector2(moveDir.x, moveDir.z);
                        UpdateMovement(newMove);
                    }

                    if (IsOwner && IsClient && m_ClientHistoryGetDirectionInputBack[0] != Vector2.zero)
                    {
                        UpdateClientFrame(Time.fixedDeltaTime);
                    }          
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
                        {
                            if (!GameManager.Instance.mIsMulti)
                            {
                                DeadAndRespawn();
                            }
                            else
                            {
                                DeadAndRespawn();
                            }
                        }
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

    void UpdateInputPlaying()
    {
        if (!GameManager.Instance.mIsMulti)
        { 
            this.moveDir = this.GetMoveDirection();
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

    }

    protected void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case TAG.FINISH:
            
            if(!GameManager.Instance.mIsMulti)
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
                if(!GameManager.Instance.mIsMulti)
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

    public override void NetworkStart()
    {
        // if (!IsServer && GameManager.Instance.mIsMulti)
        //     enabled = false;
    }
    // public void UpdatePositionForClients()
    // {
    //     mNetworkVariableTransform.Position.Value = transform.position;
    //     mNetworkVariableTransform.Rotation.Value = transform.rotation;
    // }

    [ServerRpc]
    public void UpdateMoveDirByServerRpc(float x, float z)
    {
        this.moveDir.x = x;
        this.moveDir.z = z;
    }
}
