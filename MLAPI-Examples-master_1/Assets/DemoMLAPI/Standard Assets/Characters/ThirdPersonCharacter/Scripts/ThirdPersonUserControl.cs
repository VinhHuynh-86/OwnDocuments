using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkingManagerComponents.Binary;
using System.IO;
using UnityEngine;  
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : NetworkedBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;    
        private bool m_Crouch;                  // the world-relative desired move direction, calculated from the camForward and user input.

        private float lastSentTime = 0;
        public float PosUpdatesPerSecond = 20;
        float m_Horizontal = 0;
        float m_Vertical = 0;
        float m_Speed = 10;

        public override void NetworkStart()
        {
            if(isServer || isHost)
            {
                RegisterMessageHandler("OnPositionUpdate", OnPositionUpdate);
            }
            if(isClient || isHost)
            {
                RegisterMessageHandler("OnSetClientPosition", OnSetClientPosition);
            }

            transform.position = new Vector3(0, 0, -1f);
        }

        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }


        private void Update()
        {
            if(isClient || isHost)
            {
                if (!m_Jump)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }
                /*
                if(m_Jump)
                {
                    NetworkSceneManager.SwitchScene("PlayScene");
                }
                */
            }

            if(isClient || isHost)
            // if (!isLocalPlayer)
            //     return;

            {
                if(Time.time - lastSentTime > (1f / PosUpdatesPerSecond))
                {
                    m_Horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
                    m_Vertical = CrossPlatformInputManager.GetAxis("Vertical");
                    // if(horizontal == 0 && vertical == 0)
                    //     return;

                    m_Crouch = Input.GetKey(KeyCode.C);

                    using (BitWriter writer = new BitWriter())
                    {
                        writer.WriteFloat(m_Horizontal);
                        writer.WriteFloat(m_Vertical);
                        writer.WriteBool(m_Crouch);
                        writer.WriteBool(m_Jump);
                        
                        SendToServer("OnPositionUpdate", "ServerPositionChannel", writer.Finalize());
                    }
                    // ComputeMovement(horizontal, vertical, crouch, m_Jump);

                    lastSentTime = Time.time;
                }
            }

            // if (isServer || isHost)
            // {
            //     // Debug.Log("Update():: 1");

            //     if(Time.time - lastSentTime > (1f / PosUpdatesPerSecond))
            //     {
            //         // Debug.Log("Update():: 2");

            //         using (BitWriter writer = new BitWriter())
            //         {
            //             writer.WriteUInt(networkId);
            //             writer.WriteFloat(transform.position.x);
            //             writer.WriteFloat(transform.position.y);
            //             writer.WriteFloat(transform.position.z);
            //             // Debug.Log("Update():: call OnSetClientPosition()");
            //             SendToClientsTarget("OnSetClientPosition", "ClientPositionChannel", writer.Finalize());
            //         }
                    
            //         lastSentTime = Time.time;
            //     }
            // }
            
        }

        private void ComputeMovement()
        {
            // calculate move direction to pass to character
             if (m_Cam != null)
             {
                 // calculate camera relative direction to move:
                 m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                 m_Move = m_Speed * m_Vertical * m_CamForward + m_Speed * m_Horizontal * m_Cam.right;
             }
             else
             {
                 // we use world-relative directions in the case of no main camera
                 m_Move = m_Speed * m_Vertical * Vector3.forward + m_Speed * m_Horizontal * Vector3.right;
             }
             // pass all parameters to the character control script
             m_Character.Move(m_Move, m_Crouch, m_Jump);
             m_Jump = false;
        }

        private void FixedUpdate()
        {
            // if (isLocalPlayer)
            

            // if(isServer)
            // {
            //     Color color =  Random.ColorHSV();
            //     using (BitWriter writer = new BitWriter())
            //     {
            //         writer.WriteFloat(color.r);
            //         writer.WriteFloat(color.g);
            //         writer.WriteFloat(color.b);
            //         SendToClientsTarget("OnChangeColor", "ColorChannel", writer.Finalize());
            //     }
            // }

            // if(isClient)
            // {
            //     Debug.Log("FixedUpdate()2::isClient");

            //     Color color =  Random.ColorHSV();
            //     using (BitWriter writer = new BitWriter())
            //     {
            //         writer.WriteFloat(color.r);
            //         writer.WriteFloat(color.g);
            //         writer.WriteFloat(color.b);
            //         SendToServer("OnChangeColor", "ColorChannel", writer.Finalize());
            //     }
            // }
        }

        private void OnPositionUpdate(uint clientId, byte[] data)
        {
            // Debug.Log("OnPositionUpdate()::clientId=" + clientId + " ; ownerClientId=" + ownerClientId);

            //This makes it behave like a HLAPI Command. It's only invoked on the same object that called it.
            if (isLocalPlayer && clientId != ownerClientId)
                return;

            BitReader reader = new BitReader(data);
            m_Horizontal = reader.ReadFloat();
            m_Vertical = reader.ReadFloat();

            // if(horizontal == 0 && vertical == 0)
            //         return;

            m_Crouch = reader.ReadBool();
            m_Jump = reader.ReadBool();

            // // Debug.Log("OnPositionUpdate()::horizontal=" + horizontal + " ; vertical=" + vertical);

            // transform.position = new Vector3(transform.position.x + speed * horizontal * Time.deltaTime, transform.position.y, transform.position.z + speed * vertical * Time.deltaTime);
            ComputeMovement();

            using (BitWriter writer = new BitWriter())
            {
                writer.WriteUInt(networkId);
                writer.WriteFloat(m_Move.x);
                writer.WriteFloat(m_Move.y);
                writer.WriteFloat(m_Move.z);
                // Debug.Log("Update():: call OnSetClientPosition()");
                SendToClientsTarget("OnSetClientPosition", "ClientPositionChannel", writer.Finalize());
            }
        }

        //This gets called on all clients except the one the position update is about.
        void OnSetClientPosition(uint clientId, byte[] data)
        {
            // Debug.Log("OnSetClientPosition():: begin");
            BitReader reader = new BitReader(data);
            uint targetNetId = reader.ReadUInt();

            if (targetNetId != networkId)
                    return;

            m_Move.x = reader.ReadFloat();
            m_Move.y = reader.ReadFloat();
            m_Move.z = reader.ReadFloat();

            m_Character.Move(m_Move, m_Crouch, m_Jump);
            m_Jump = false;
            // GetNetworkedObject(targetNetId).transform.position = new Vector3(x, y, z);

            // Debug.Log("OnSetClientPosition():: end");
        }
        
    }
}
