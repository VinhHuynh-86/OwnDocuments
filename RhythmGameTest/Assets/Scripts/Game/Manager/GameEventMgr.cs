using UnityEngine;
using Doozy.Engine;
using Doozy.Engine.UI;
using System;

public class GameEventMgr : Singleton<GameEventMgr>
{
    public static void SendEvent(string eventName)
    {
        GameEventMessage.SendEvent(eventName);
    }

    private void OnEnable()
    {
        Message.AddListener<GameEventMessage>(OnMessage);
    }

    private void OnDisable()
    {
        Message.RemoveListener<GameEventMessage>(OnMessage);
    }

    private void OnMessage(GameEventMessage message)
    {
        // Debug.Log("GameEventMgr::OnMessage():: message = " + message.EventName);

        if (message == null) return;
        // Debug.Log("Received the '" + message.EventName + "' game event.");
        switch (message.EventName)
        {
            case "start_countdown":
                GameManager.Instance.SetBackToGameCountdown();
                break;
            case "GotoAP":
                GameManager.Instance.OnResumeGame();
                break;
        }
    }
}
