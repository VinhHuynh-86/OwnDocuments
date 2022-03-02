using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

public class NetworkVariableGeneric : NetworkBehaviour
{
    NetworkVariable<PLAYER_STATE> NetworkPlayerState = new NetworkVariable<PLAYER_STATE>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
    NetworkVariableVector3 NetworkToggleRagdoll = new NetworkVariableVector3(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly });
    NetworkVariableInt NetworkHairID = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
    NetworkVariableInt NetworkEyesID = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
    NetworkVariableInt NetworkSkinID = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });

    NetworkVariableULong NetworkTimeStamp = new NetworkVariableULong();
    
    public NetworkVariable<PLAYER_STATE> PlayerState
    {
        get { return NetworkPlayerState; }
        set { NetworkPlayerState = value; }
    }

    public NetworkVariableVector3 ToggleRagdoll
    {
        get { return NetworkToggleRagdoll; }
        set { NetworkToggleRagdoll = value; }
    }

    public NetworkVariableInt HairID
    {
        get { return NetworkHairID; }
        set { NetworkHairID = value; }
    }
    public NetworkVariableInt EyesID
    {
        get { return NetworkEyesID; }
        set { NetworkEyesID = value; }
    }
    public NetworkVariableInt SkinID
    {
        get { return NetworkSkinID; }
        set { NetworkSkinID = value; }
    }

    public NetworkVariableULong TimeStamp
    {
        get { return NetworkTimeStamp; }
        set { NetworkTimeStamp = value; }
    }
}
