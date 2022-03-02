using MLAPI.Attributes;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkingManagerComponents.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class ServerSyncPosition : NetworkedBehaviour
{
    private float lastSentTime;
    private float PosUpdatesPerSecond = 20;
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    public List<ThirdPersonUserControl> m_CharactersControl = new List<ThirdPersonUserControl>(); // A reference to the ThirdPersonUserControl on the object
    public List<ThirdPersonCharacter> m_Characters = new List<ThirdPersonCharacter>(); // A reference to the ThirdPersonCharacter on the object
    private void Start()
    {
        if(isServer)
        {
            RegisterMessageHandler("PositionUpdate", OnRecievePositionUpdate);
        }

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
    }
    // public override void NetworkStart()
    // {
    //     if(isServer)
    //     {
    //         RegisterMessageHandler("PositionUpdate", OnRecievePositionUpdate);
    //     }
    // }

    // private void Update()
    // {
    //     if (isServer && NetworkingManager.singleton.IsClientConnected)
    //     {
    //         if(Time.time - lastSentTime > (1f / PosUpdatesPerSecond))
    //         {
    //             using (BitWriter writer = new BitWriter())
    //             {
    //                 writer.WriteUInt(networkId);
    //                 for(int v = 0; v < m_CharactersControl.Count; ++v)
    //                 {
    //                     writer.WriteUInt(m_CharactersControl[v].ownerClientId);
    //                     writer.WriteFloat(transform.position.x);
    //                     writer.WriteFloat(transform.position.y);
    //                     writer.WriteFloat(transform.position.z);
    //                 }
    //                 SendToNonLocalClients("OnSetClientPosition", "SetClientPosition", writer.Finalize());
    //             }
    //             lastSentTime = Time.time;
    //         }
    //     }

    // }

    private void Update() {
        UpdateCharacter();
    }

    private void UpdateCharacter()
    {
        ThirdPersonCharacter[] list = FindObjectsOfType<ThirdPersonCharacter>();
        if(m_Characters.Count == 0 || m_Characters.Count != list.Length)
        {
            m_Characters.Clear();
            m_Characters.AddRange(list);
            for(int v = 0; v < m_Characters.Count; ++v)
            {
                m_CharactersControl.Add(m_Characters[v].GetComponent<ThirdPersonUserControl>());
            }
        }
    }

    private void OnRecievePositionUpdate(uint clientId, byte[] data)
    {
        //This makes it behave like a HLAPI Command. It's only invoked on the same object that called it.
        int idx = -1;
        for(int v = 0; v < m_CharactersControl.Count; ++v)
        {
            if(m_CharactersControl[v].ownerClientId == clientId)
            {
                idx = v;
                break;
            }
        }
        if (idx == -1)
            return;

        BitReader reader = new BitReader(data);
        float _horizontal = reader.ReadFloat();
        float _vertical = reader.ReadFloat();
        bool _crouch = reader.ReadBool();
        bool _jump = reader.ReadBool();
        // calculate move direction to pass to character
        if (m_Cam != null)
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = _vertical*m_CamForward + _horizontal*m_Cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            m_Move = _vertical*Vector3.forward + _horizontal*Vector3.right;
        }
        // pass all parameters to the character control script
        m_Characters[idx].Move(m_Move, _crouch, _jump);
        _jump = false;
    }
}
