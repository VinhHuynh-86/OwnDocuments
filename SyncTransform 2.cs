using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VH_SYNC
{
    [System.Serializable]
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

        public HistoryStatePoint(float _deltaTime, Vector3 _deltaPos, Vector3 _deltaRot, Vector3 _velocity, Vector2 _directionInput, Vector2 _rotationInput)
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

    /// <summary>
    /// A component for syncing variables
    /// Initial goal: allow an FPS-style snapshot
    /// with variables updating at specific place in the frame
    /// </summary>
    [AddComponentMenu("MLAPI/SyncTransform")]
    // todo: check inheriting from NetworkBehaviour. Currently needed for IsOwner, to synchronize position
    public class SyncTransform : NetworkBehaviour
    {
        private float m_LastSendTime = 0f;
        public static float FixedSendsPerSecond = 20f;
        public float m_SnapDistance = 0.1f; // 10
        private PlayerMovement m_Player;
        private Vector3 m_ServerVelocity = Vector3.zero;
        private List<HistoryStatePoint> m_ClientHistoryState = new List<HistoryStatePoint>();
        private StatePoint m_ServerState = new StatePoint();
        private StatePoint m_ClientState = new StatePoint();
        private StatePoint m_ClientPredictedState = new StatePoint();
        private Vector2 m_ServerHistoryReceiveDirectionInput = Vector2.zero;
        private Vector2 m_ServerHistoryReceiveRotationInput = Vector2.zero;
        private Vector2 m_ClientHistoryGetDirectionInputBack = Vector2.zero;
        private Vector2 m_ClientHistoryGetRotationInputBack = Vector2.zero;
        private Vector2 m_ClientDirectionInput = Vector2.zero;
        private Vector2 m_ClientRotationInput = Vector2.zero;
        private float m_Speed = 10.0f;
        private float m_RotSpeed = 5.0f;

        private float m_Latency = 0.025f; //1f / FixedSendsPerSecond; // = 0.025f; // should be RTT. Round trip time (RTT) latency: the time for input to go from client to game server and the result goes back to client.
        // can find by ping to server and receive the result
        private float m_ClientHistoryDuration = 0f;
        private float m_VelocityTolerance = 0.1f;
        float CONVERGE_MULTIPLIER = 0.05f;

        // int needToResponseClient = -1;
        private void Awake() {
            m_Player = GetComponent<PlayerMovement>();
            m_ClientHistoryState.Add(new HistoryStatePoint(0, Vector3.zero, Vector3.zero, Vector3.zero, Vector2.zero, Vector2.zero));
        }
        
        //[VINH] Send transform Client to Server
        // this function on Server side and it is called by Client
        [ServerRpc]
        private void SubmitInputFromClientToServerRpc(Vector2 directionInput, Vector2 rotationInput, float time, ServerRpcParams rpcParams = default)
        {
            m_ServerHistoryReceiveDirectionInput = directionInput;
            m_ServerHistoryReceiveRotationInput = rotationInput;
            // needToResponseClient++;
            if(m_Player)
            {
                m_ServerVelocity = m_Player.m_Rigidbody.velocity;
            }

            Debug.Log("Send position to client::ServerPosition=" + transform.position);
            SubmitTransformFromServerToClientRpc(transform.position, transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveDirectionInput, m_ServerHistoryReceiveRotationInput, NetworkManager.Singleton.NetworkTime, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
        }

        //[VINH] Send transform Server to Client
        // this function in Client side and it is called by server
        [ClientRpc]
        private void SubmitTransformFromServerToClientRpc(Vector3 position, Vector3 eulerAngles, Vector3 velocity, Vector2 directionInput, Vector2 rotationInput, float timestamp, ClientRpcParams rpcParams = default)
        {
            if (!enabled)
                return;


            if (IsOwner && IsClient)
            {
                ReceiveServerFrame(new StatePoint(directionInput, rotationInput, timestamp, position, eulerAngles, velocity));
            }         
        }

        void ServerFixedUpdate()
        {
            if(IsServer)
            {
                transform.position = new Vector3(transform.position.x + m_ServerHistoryReceiveDirectionInput.x * Time.fixedDeltaTime * m_Speed, transform.position.y, transform.position.z + m_ServerHistoryReceiveDirectionInput.y * Time.fixedDeltaTime * m_Speed);
                if (m_ServerHistoryReceiveRotationInput.x == 1) // left
                {
                    Quaternion rot = Quaternion.Euler(0, 90 * m_RotSpeed * Time.fixedDeltaTime, 0);
                    transform.rotation = rot * transform.rotation;
                }

                if (m_ServerHistoryReceiveRotationInput.y == 1) // right
                {
                    Quaternion rot = Quaternion.Euler(0, -90 * m_RotSpeed * Time.fixedDeltaTime, 0);
                    transform.rotation = rot * transform.rotation;
                }

                // if (needToResponseClient > 0)
                // {
                //     if(m_Player)
                //     {
                //         m_ServerVelocity = m_Player.m_Rigidbody.velocity;
                //     }

                //     Debug.Log("Send position to client::ServerPosition=" + transform.position);
                //     needToResponseClient--;
                //     SubmitTransformFromServerToClientRpc(transform.position, transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveDirectionInput, NetworkManager.Singleton.NetworkTime, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
                // }
            }
        }
        void ClientFixedUpdate()
        {
            //Client side send input to server
            if (IsOwner && IsClient)
            {
                if (NetworkManager.Singleton.NetworkTime - m_LastSendTime >= (1f / FixedSendsPerSecond))
                {
                    m_ClientDirectionInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                    m_ClientRotationInput = new Vector2(Input.GetKey(KeyCode.Keypad1) == true ? 1 : 0, Input.GetKey(KeyCode.Keypad3) == true ? 1 : 0);
                    if(m_ClientDirectionInput != Vector2.zero || m_ClientHistoryGetDirectionInputBack != Vector2.zero  || m_ClientRotationInput != Vector2.zero || m_ClientHistoryGetRotationInputBack != Vector2.zero)
                    {
                        m_LastSendTime = NetworkManager.Singleton.NetworkTime;
                        SubmitInputFromClientToServerRpc(m_ClientDirectionInput, m_ClientRotationInput, NetworkManager.Singleton.NetworkTime);
                    }
                }
                
            }

            // if (IsOwner && IsClient && m_ClientHistoryGetDirectionInputBack != Vector2.zero)
            if (IsOwner && IsClient)
            {
                UpdateClientFrame(Time.fixedDeltaTime);
            }          
        }
        void FixedUpdate()
        {
            ClientFixedUpdate();
            ServerFixedUpdate();
        }



        // Called when we receive a player state update from the server. 
        // Server send state to client and Client receive it on this function
        public void ReceiveServerFrame(StatePoint serverFrame)
        {
            m_ClientHistoryGetDirectionInputBack = serverFrame.directionInput;
            m_ClientHistoryGetRotationInputBack = serverFrame.rotationInput;

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
            
            Debug.Log("ReceiveServerFrame::serverFrame.pos=" + serverFrame.pos + "; clientPosition=" + transform.position);
            Debug.Log("ReceiveServerFrame::m_ClientHistoryState[0].deltaPos = " + m_ClientHistoryState[0].deltaPos);
            if ((m_ClientHistoryState[0].deltaPos.magnitude > m_SnapDistance) || (m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            // if((m_ServerState.velocity - m_ClientHistoryState[0].velocity).magnitude > m_VelocityTolerance)
            {
                // Debug.Log("ReceiveServerFrame::m_SnapDistance ->replay");
                Debug.Log("ReceiveServerFrame::ReComputePosition");
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
            // Run player controller to get new prediction and add to m_ClientHistoryState
            StatePoint newState = UpdatePosition(m_ClientPredictedState, deltaTime);
            HistoryStatePoint frame = new HistoryStatePoint(deltaTime, newState.pos - m_ClientPredictedState.pos, newState.rot - m_ClientPredictedState.rot, newState.velocity, m_ClientPredictedState.directionInput,  m_ClientPredictedState.rotationInput);
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
            newState.pos = new Vector3(predictedStatePoint.pos.x + frame.directionInput.x * frame.deltaTime * m_Speed, predictedStatePoint.pos.y, predictedStatePoint.pos.z + frame.directionInput.y * frame.deltaTime * m_Speed);
            m_Player.transform.position = newState.pos;
            newState.rot = m_Player.transform.eulerAngles; //predictedStatePoint.rot + frame.deltaRot; // Need to compute rotation
            newState.velocity = m_Player.m_Rigidbody.velocity; // predictedStatePoint.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + frame.deltaTime; // m_Latency;
            newState.directionInput = frame.directionInput;
            newState.rotationInput = frame.rotationInput;
           
            return newState;
        }

        private StatePoint UpdatePosition(StatePoint predictedStatePoint, float deltaTime)
        {
            StatePoint newState = new StatePoint();
            transform.position = new Vector3(transform.position.x + predictedStatePoint.directionInput.x * deltaTime * m_Speed, predictedStatePoint.pos.y, transform.position.z + predictedStatePoint.directionInput.y * deltaTime * m_Speed);
            // transform.eulerAngles = new Vector3(transform.eulerAngles.x + predictedStatePoint.rotationInput.x * deltaTime * m_RotSpeed, predictedStatePoint.rot.y, transform.eulerAngles.z + predictedStatePoint.rotationInput.y * deltaTime * m_RotSpeed);
            if(predictedStatePoint.rotationInput.x > 0)
            {
                Quaternion rot = Quaternion.Euler(0, predictedStatePoint.rotationInput.x * 90 * m_RotSpeed * Time.fixedDeltaTime, 0);
                transform.rotation = rot * transform.rotation;
            }
            if(predictedStatePoint.rotationInput.y > 0)
            {
                Quaternion rot = Quaternion.Euler(0, predictedStatePoint.rotationInput.y * (-90) * m_RotSpeed * Time.fixedDeltaTime, 0);
                transform.rotation = rot * transform.rotation;
            }

            newState.pos = transform.position;
            newState.rot = transform.eulerAngles; // Need to compute rotation
            newState.velocity = m_Player.m_Rigidbody.velocity; // Need to compute velocity
            newState.timestamp = predictedStatePoint.timestamp + deltaTime;
            newState.directionInput = predictedStatePoint.directionInput;
            newState.rotationInput = predictedStatePoint.rotationInput;

            return newState;
        }

    }
}
