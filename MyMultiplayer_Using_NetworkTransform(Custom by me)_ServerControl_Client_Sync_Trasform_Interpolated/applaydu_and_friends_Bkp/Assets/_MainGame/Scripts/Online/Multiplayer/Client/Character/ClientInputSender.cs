using UnityEngine;
using MLAPI;
using System;
public class ClientInputSender : NetworkBehaviour
{
    //private
    PlayerController mPlayerController;
    ClientCharacterHandler mClientCharacterHandler;
    private float TIME_TO_SEND_SERVER = 0.05f;
    private float mLastSend = 0f;
    bool isCanSendIdle = false;

    //public
    public Action<string> SetAnim;


    //const
    const string idleTrigger = "Trigger_Idle";
    const string walkingTrigger = "Trigger_Walking";
    const string victoryTrigger = "Trigger_Victory";
    // Start is called before the first frame update
    void Start()
    {
        mPlayerController = GetComponent<PlayerController>();
        mClientCharacterHandler = GetComponent<ClientCharacterHandler>();

        if (!GameManager.Instance.mIsMulti)
        {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!IsLocalPlayer)
            return;
        switch(mClientCharacterHandler.STATE)
        {
            case PLAYER_STATE.IDLE:
            case PLAYER_STATE.WALK:
            case PLAYER_STATE.FALL:
                if (ActionPhraseManager.Instance == null || ActionPhraseManager.Instance.joystick == null)
                    return;

                var input = mPlayerController.GetInput();

                if(input.x != 0 || input.y != 0)
                {
                    isCanSendIdle = true;
                    if (Time.time - mLastSend > TIME_TO_SEND_SERVER)
                    {
                        mLastSend = Time.time;
                        SetAnim?.Invoke(walkingTrigger);
                        // mPlayerController.UpdateMoveDirByServerRpc(dir.x, dir.z);
                    }
                }
                else
                {
                    if (isCanSendIdle)
                    {
                        isCanSendIdle = false;
                        // mPlayerController.UpdateMoveDirByServerRpc(dir.x, dir.z);
                        SetAnim?.Invoke(idleTrigger);
                    }
                }
                break;
            case PLAYER_STATE.FINISH:
                break;
            default:
                break;
        }
    }

    public override void NetworkStart()
    {
        if(IsServer)
        {
            this.enabled = false;
            return;
        }
    }
}