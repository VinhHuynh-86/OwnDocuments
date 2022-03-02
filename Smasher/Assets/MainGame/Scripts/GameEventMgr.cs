using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine;
using Doozy.Engine.UI;

public class GameEventMgr : MonoBehaviour
{
	void Start()
	{

	}

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
        if (message == null) return;
        Debug.Log("Received the '" + message.EventName + "' game event.");

        if (message.Source != null)
		{
        	Debug.Log("'" + message.EventName + "' game event was sent by the [" + message.Source.name + "] GameObject.");
		}

		switch (message.EventName)
		{
			case "play":
				GameMgr.Instance.Play();
				break;

			case "watch_for_head":
				GameMgr.Instance.WatchForHead();
				break;

			case "watch_for_revive":
				GameMgr.Instance.WatchForRevive();
				break;

			case "give_up":
				GameMgr.Instance.GiveUp();
				break;
		}
    }
}
