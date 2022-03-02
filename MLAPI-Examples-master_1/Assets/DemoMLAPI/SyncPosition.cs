using MLAPI.MonoBehaviours.Core;
using System.IO;
using UnityEngine;

public class SyncPosition : NetworkedBehaviour
{
    private float lastSentTime;
    public float PosUpdatesPerSecond = 20;

    float speed = 10;
    float horizontal = 0;
    float vertical = 0;
    private void Awake()
    {
        if(isServer)
        {
           RegisterMessageHandler("OnRecievePositionUpdate", OnRecievePositionUpdate);
        }
        if(isClient)
        {
           RegisterMessageHandler("OnSetClientPosition", OnSetClientPosition);
        }

        transform.position = new Vector3(0, 0, -1f);
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if(Time.time - lastSentTime > (1f / PosUpdatesPerSecond))
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            if(horizontal == 0 && vertical == 0)
                return;

            

            // Debug.Log("OnPositionUpdate()::_horizontal=" + horizontal + " ; _vertical=" + vertical);

            using(MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(horizontal);
                    writer.Write(vertical);
                }
                SendToServer("OnRecievePositionUpdate", "ServerPositionUpdatesChannel", stream.ToArray());
                
            }
            lastSentTime = Time.time;
        }
    }

    //This gets called on all clients except the one the position update is about.
    void OnSetClientPosition(uint clientId, byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                uint targetNetId = reader.ReadUInt32();
                // Debug.Log("OnSetClientPosition()::networkId=" + networkId + "; targetNetId=" + targetNetId);
                if (targetNetId != networkId)
                    return;

                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();

                // Debug.Log("OnSetClientPosition()::x=" + x + " ; z=" + z);

                GetNetworkedObject(targetNetId).transform.position = new Vector3(x, y, z);
                // transform.position = new Vector3(x, y, z);
            }
        }
    }

    //This gets called on the server when a client sends it's position.
    void OnRecievePositionUpdate(uint clientId, byte[] data)
    {
        // Debug.Log("OnRecievePositionUpdate()::clientId=" + clientId + "; ownerClientId=" + ownerClientId);

        //This makes it behave like a HLAPI Command. It's only invoked on the same object that called it.
        if (clientId != ownerClientId)
            return;

        
        using (MemoryStream readStream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(readStream))
            {
                Debug.Log("OnRecievePositionUpdate()::transform.position=" + transform.position);

                horizontal = reader.ReadSingle();
                vertical = reader.ReadSingle();
                if(horizontal == 0 && vertical == 0)
                    return;

                // if(!once)
                //     return;

                // if(Mathf.Abs(transform.position.x) >= 0.1f || Mathf.Abs(transform.position.z) >= 0.1f)
                //     once = false;


                // Debug.Log("OnRecievePositionUpdate()::horizontal=" + horizontal + " ; vertical=" + vertical);
                transform.position = new Vector3(transform.position.x + speed * horizontal * Time.deltaTime, transform.position.y, transform.position.z + speed * vertical * Time.deltaTime);
                // transform.Translate(new Vector3(horizontal * Time.deltaTime, 0, vertical * Time.deltaTime));
                // transform.position = new Vector3(transform.position.x + horizontal * Time.deltaTime, transform.position.y, transform.position.z + vertical * Time.deltaTime);
                // transform.position.Set(transform.position.x + horizontal * Time.deltaTime, transform.position.y, transform.position.z + vertical * Time.deltaTime);

                // GetNetworkedObject(networkId).transform.position = transform.position;
            }
        }

        using (MemoryStream writeStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(writeStream))
            {
                writer.Write(networkId);
                writer.Write(transform.position.x);
                writer.Write(transform.position.y);
                writer.Write(transform.position.z);
            }
            
            //Sends the position to all clients except the one who requested it. Similar to a Rpc with a if(isLocalPlayer) return;
            // SendToNonLocalClients("OnSetClientPosition", "ClientPositionUpdatesChannel", writeStream.ToArray());
            SendToClientsTarget("OnSetClientPosition", "ClientPositionUpdatesChannel", writeStream.ToArray());
        }
    }
}
