using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Doozy.Engine.UI;

public class PopupYesNo : MonoBehaviour
{
    public TextMeshProUGUI textContent;
    private Action callbackYes;

    public void SetTextContent(string value)
    {
        string text = Localization.Instance.GetString(value);
        textContent.text = text;
    }
    
    public void OnYes()
    {
        callbackYes.Invoke();
    }

    public void SetCallBackYes(Action callback)
    {
        callbackYes = callback;
    }
}
