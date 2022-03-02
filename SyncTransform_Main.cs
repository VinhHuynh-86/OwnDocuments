using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VH_SYNC
{
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

    /// <summary>
    /// A component for syncing variables
    /// Initial goal: allow an FPS-style snapshot
    /// with variables updating at specific place in the frame
    /// </summary>
    [AddComponentMenu("MLAPI/SyncTransform")]
    // todo: check inheriting from NetworkBehaviour. Currently needed for IsOwner, to synchronize position
    public class SyncTransform : NetworkBehaviour
    {
        private const float k_Epsilon = 0.0001f;

        private bool m_Interpolate = true;
        public float interpolationBackTime = 0.1f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        
        // We store twenty states with "playback" information
        State[] m_BufferedState = new State[20];
        // Keep track of what slots are used
	    int m_TimestampCount = 0;
        private float m_LastSendTime = 0f;
        private float m_LastServerSendTime = 0f;
        private Vector3 m_LastSentPos = Vector3.zero;
        private Quaternion m_LastSentRot = Quaternion.identity;
        public static float FixedSendsPerSecond = 20f;
        public float MinMeters = 0.15f;

        /// <summary>
        /// The min degrees to rotate before a send it sent
        /// </summary>
        public float MinDegrees = 1.5f;


        private PlayerMovement m_Player;
        private Vector3 m_ServerVelocity = Vector3.zero;
        private List<HistoryStatePoint> m_ClientHistoryState = new List<HistoryStatePoint>();
        private StatePoint m_ServerState = new StatePoint();
        private StatePoint m_ClientState = new StatePoint();
        private StatePoint m_ClientPredictedState = new StatePoint();
        private Vector2[] m_ServerHistoryReceiveInput = new Vector2[20];
        private Vector2[] m_ClientHistoryGetInputBack = new Vector2[20];
        private Vector2 m_ClientInput = new Vector2();
        private float m_Speed = 10.0f;

        private float m_Latency = 0.02f; //1f / FixedSendsPerSecond; // = 0.025f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        private float m_ClientHistoryDuration = 0f;
        private float m_VelocityTolerance = 0.1f;
        private float m_SnapDistance = 0.25f;
        float CONVERGE_MULTIPLIER = 0.05f;
        private bool isSubmitted = true;

        private void Awake() {
            m_Player = GetComponent<PlayerMovement>();
            for(int v = 0; v < m_BufferedState.Length; ++v)
            {
                m_BufferedState[v] = new State();
            }
            for(int v = 0; v < m_ServerHistoryReceiveInput.Length; ++v)
            {
                m_ServerHistoryReceiveInput[v] = Vector2.zero;
            }
            for(int v = 0; v < m_ClientHistoryGetInputBack.Length; ++v)
            {
                m_ClientHistoryGetInputBack[v] = Vector2.zero;
            }
            m_ClientHistoryState.Add(new HistoryStatePoint(0, Vector3.zero, Vector3.zero, Vector3.zero, Vector2.zero));
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

            if (IsServer)
            {
                // transform.position = new Vector3(transform.position.x + m_ServerHistoryReceiveInput[0].x * Time.fixedDeltaTime * m_Speed, transform.position.y, transform.position.z + m_ServerHistoryReceiveInput[0].y * Time.fixedDeltaTime * m_Speed);
                if(m_Player)
                {
                    m_ServerVelocity = m_Player.m_Rigidbody.velocity;
                }
                SubmitTransformFromServerToClientRpc(gameObject.transform.position, gameObject.transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveInput[0], NetworkManager.Singleton.NetworkTime, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
            }

        }

        //[VINH] Send transform Server to Client
        // this function in Client side and it is called by server
        [ClientRpc]
        private void SubmitTransformFromServerToClientRpc(Vector3 position, Vector3 eulerAngles, Vector3 velocity, Vector2 input, float timestamp, ClientRpcParams rpcParams = default)
        {
            if (!enabled)
                return;

            
            // if (rpcParams.Equals(null) || rpcParams.Send.Equals(null) || rpcParams.Send.TargetClientIds == null)
            //     return;

            if (IsOwner && IsClient)
            {
                // Apply transform for Client (interpolate)
                // ApplyTransformInternal(position, Quaternion.Euler(eulerAngles), timestamp);
                ReceiveServerFrame(new StatePoint(input, timestamp, position, eulerAngles, velocity));
            }         
        }

        void FixedUpdate()
        {
            //Server side send tranform to client
            // if (IsServer)
            // {
            //     // if (NetworkManager.Singleton.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond) && (Vector3.Distance(transform.position, m_LastSentPos) > MinMeters || Quaternion.Angle(transform.rotation, m_LastSentRot) > MinDegrees))
            //     if (NetworkManager.Singleton.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond) && isSubmitted)
            //     {
            //         if(m_Player)
            //         {
            //             m_ServerVelocity = m_Player.m_Rigidbody.velocity;
            //         }
            //         m_LastSendTime = NetworkManager.Singleton.NetworkTime;
            //         m_LastSentPos = transform.position;
            //         m_LastSentRot = transform.rotation;
            //         // SubmitTransformFromServerToClientRpc(gameObject.transform.position, gameObject.transform.eulerAngles, m_ServerVelocity, NetworkManager.Singleton.NetworkTime, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
            //         SubmitTransformFromServerToClientRpc(gameObject.transform.position, gameObject.transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveInput[0], NetworkManager.Singleton.NetworkTime, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
            //         m_LastServerSendTime = NetworkManager.Singleton.NetworkTime;
            //         if(isSubmitted && m_ServerHistoryReceiveInput[0] == Vector2.zero)
            //         {
            //             isSubmitted = false;
            //         }
            //     }
            // }

            //Client side send input to server
            if (IsOwner && IsClient)
            {
                if (NetworkManager.Singleton.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond))
                {
                    m_ClientInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                    if(m_ClientInput != Vector2.zero || m_ClientHistoryGetInputBack[0] != Vector2.zero)
                    {
                        m_LastSendTime = NetworkManager.Singleton.NetworkTime;
                        SubmitInputFromClientToServerRpc(m_ClientInput, NetworkManager.Singleton.NetworkTime);
                    }
                }
                
            }

            if(IsServer)
            {
                transform.position = new Vector3(transform.position.x + m_ServerHistoryReceiveInput[0].x * Time.fixedDeltaTime * m_Speed, transform.position.y, transform.position.z + m_ServerHistoryReceiveInput[0].y * Time.fixedDeltaTime * m_Speed);
            }

            if (IsOwner && IsClient && m_ClientHistoryGetInputBack[0] != Vector2.zero)
            {
                // if this.gameObject is local let's send its position
                {
                    // Interpolate();
                    UpdateClientFrame(Time.fixedDeltaTime);
                }
            }            
        }



        // Called when we receive a player state update from the server. 
        // Server send state to client and Client receive it on this function
        public void ReceiveServerFrame(StatePoint serverFrame)
        {
            for (int v = m_ClientHistoryGetInputBack.Length - 1; v > 0; --v)
            {
                m_ClientHistoryGetInputBack[v] = m_ClientHistoryGetInputBack[v-1];
            }
            m_ClientHistoryGetInputBack[0] = serverFrame.input;

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
            if (m_ClientHistoryState[0].deltaPos.magnitude > m_SnapDistance)
            // if((m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            {
                // Debug.Log("ReceiveServerFrame::m_SnapDistance ->replay");
                Debug.Log("ReceiveServerFrame::replay : 1 m_ClientPredictedState = " + m_ClientPredictedState.pos);
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
            // m_ClientHistoryGetInputBack[0] = m_ClientInput;
            // Run player controller to get new prediction and add to m_ClientHistoryState
            StatePoint newState = UpdatePosition(m_ClientPredictedState, deltaTime);
            HistoryStatePoint frame = new HistoryStatePoint(deltaTime, newState.pos - m_ClientPredictedState.pos, newState.rot - m_ClientPredictedState.rot, newState.velocity, m_ClientPredictedState.input);
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
            newState.pos = new Vector3(predictedStatePoint.pos.x + frame.input.x * frame.deltaTime * m_Speed, predictedStatePoint.pos.y, predictedStatePoint.pos.z + frame.input.y * frame.deltaTime * m_Speed);
            m_Player.transform.position = newState.pos;
            newState.rot = m_Player.transform.eulerAngles; //predictedStatePoint.rot + frame.deltaRot; // Need to compute rotation
            newState.velocity = m_Player.m_Rigidbody.velocity; // predictedStatePoint.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + frame.deltaTime; // m_Latency;
            newState.input = frame.input;
           
            return newState;
        }

        private StatePoint UpdatePosition(StatePoint predictedStatePoint, float deltaTime)
        {
            StatePoint newState = new StatePoint();
            transform.position = new Vector3(predictedStatePoint.pos.x + predictedStatePoint.input.x * deltaTime * m_Speed, predictedStatePoint.pos.y, predictedStatePoint.pos.z + predictedStatePoint.input.y * deltaTime * m_Speed);
            newState.pos = transform.position;
            newState.rot = transform.eulerAngles; // Need to compute rotation
            newState.velocity = m_Player.m_Rigidbody.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + deltaTime;
            newState.input = predictedStatePoint.input;

            return newState;
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

        private void ApplyTransformInternal(Vector3 position, Quaternion rotation, float timestamp)
        {
            if (!enabled)
                return;

            {
                if(m_Interpolate)
                {
                    // Receive latest state information
                    Vector3 pos = Vector3.zero;
                    Quaternion rot = Quaternion.identity;
                    
                    // Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
                    for (int v = m_BufferedState.Length - 1; v > 0; --v)
                    {
                        m_BufferedState[v] = m_BufferedState[v-1];
                    }
                    
                    // Save currect received state as 0 in the buffer, safe to overwrite after shifting
                    State state = new State();
                    state.timestamp = timestamp;
                    state.pos = position;
                    state.rot = rotation;
                    m_BufferedState[0] = state;
                    
                    // Increment state count but never exceed buffer size
                    m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

                    // Check integrity, lowest numbered state in the buffer is newest and so on
                    for (int v = 0; v < m_TimestampCount - 1; v++)
                    {
                        if (m_BufferedState[v].timestamp < m_BufferedState[v+1].timestamp)
                            Debug.Log("State inconsistent");
                    }

                    Interpolate();
                }
                else
                {
                    gameObject.transform.position = position;
                    gameObject.transform.rotation = rotation;
                }
            }
        }

    }
}
