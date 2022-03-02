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
    public override void NetworkStart()
    {
        if(IsServer)
        {
            this.enabled = false;
            return;
        }
    }
    private void FixedUpdate()
    {
        if (!IsLocalPlayer)
            return;
        switch(mPlayerController.mState)
        {
            case PLAYER_STATE.IDLE:
            case PLAYER_STATE.WALK:
            case PLAYER_STATE.FALL:
                if (ActionPhraseManager.Instance == null || ActionPhraseManager.Instance.joystick == null)
                    return;
                var moveDirInput = mPlayerController.GetDirectionInput();

                if(moveDirInput.x != 0 || moveDirInput.y != 0)
                {
                    isCanSendIdle = true;
                    if (Time.time - mLastSend > TIME_TO_SEND_SERVER)
                    {
                        mLastSend = Time.time;
                        SetAnim?.Invoke(walkingTrigger);
                    }
                }
                else
                {
                    if (isCanSendIdle)
                    {
                        isCanSendIdle = false;
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
}