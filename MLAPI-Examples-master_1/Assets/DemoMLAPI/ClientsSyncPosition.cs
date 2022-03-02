using MLAPI.Attributes;
using MLAPI.MonoBehaviours.Core;
using MLAPI.NetworkingManagerComponents.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class ClientsSyncPosition : NetworkedBehaviour
{   
    private List<ThirdPersonUserControl> m_CharactersControl = new List<ThirdPersonUserControl>(); 
    
    public override void NetworkStart()
    {
        if(isClient)
        {
            RegisterMessageHandler("SetClientPosition", OnSetClientPosition);
        }
    }


    private void AddCharacter(ThirdPersonUserControl character)
    {
        m_CharactersControl.Add(character.GetComponent<ThirdPersonUserControl>());
    }

    void OnSetClientPosition(uint clientId, byte[] data)
    {
        BitReader reader = new BitReader(data);
        uint targetNetId = reader.ReadUInt();
        if (targetNetId != networkId)
                    return;

        int count = m_CharactersControl.Count;
        for(int v = 0; v < count; ++v)
        {
            float x = reader.ReadFloat();
            float y = reader.ReadFloat();
            float z = reader.ReadFloat();
            GetNetworkedObject(targetNetId).transform.position = new Vector3(x, y, z);
        }
    }
}
