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


        private PlayerMovement m_Player;
        private Vector3 m_ServerVelocity = Vector3.zero;
        private Vector2 m_ClientInput = Vector3.zero;
        private Vector2[] m_ServerHistoryReceiveInput = new Vector2[20];
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        private float m_Speed = 20.0f;
        private float m_RotSpeed = 5.0f;
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

        /// <summary>
        /// Registers message handlers
        /// </summary>
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
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                // Debug.Log("FixedUpdate m_ServerHistoryReceiveInput[0]=" + m_ServerHistoryReceiveInput[0]);
                transform.position += new Vector3(m_ServerHistoryReceiveInput[0].x, 0, m_ServerHistoryReceiveInput[0].y) * Time.fixedDeltaTime * m_Speed;
                
                if(m_ServerHistoryReceiveInput[0].x > 0)
                {
                    Quaternion rot = Quaternion.Euler(0, 90 * m_RotSpeed * Time.fixedDeltaTime, 0);
                    transform.rotation = rot * transform.rotation;
                }
                else if(m_ServerHistoryReceiveInput[0].x < 0)
                {
                    Quaternion rot = Quaternion.Euler(0, -90 * m_RotSpeed * Time.fixedDeltaTime, 0);
                    transform.rotation = rot * transform.rotation;
                }
            }
            
            {
                if (!IsServer)
                {
                    m_ClientInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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

                        if(m_Player)
                        {
                            m_ServerVelocity = m_Player.m_Rigidbody.velocity;
                        }
                                                                                                                                        //ApplyTransformClientRpc(transform.position, transform.rotation.eulerAngles, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsList.Where(c => c.ClientId != OwnerClientId).Select(c => c.ClientId).ToArray() } });
                        SubmitTransformFromServerToClientRpc(gameObject.transform.position, gameObject.transform.eulerAngles, m_ServerVelocity, m_ServerHistoryReceiveInput[0]);
                    }
                }
            }
            {
                //If we are server and interpolation is turned on for server OR we are not server and interpolation is turned on
                if (!IsServer && InterpolatePosition)
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
            }

            if (IsServer && EnableRange && EnableNonProvokedResendChecks)
                ServerCheckForMissedSends();
        }


        private void Update()
        {
            
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////


    }
}
