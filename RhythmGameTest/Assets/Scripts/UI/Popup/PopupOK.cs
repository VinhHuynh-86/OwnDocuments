using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupOK : MonoBehaviour
{
    public TextMeshProUGUI textContent;

    public void SetTextContent(string value)
    {
        string text = Localization.Instance.GetString(value);
        textContent.text = text;
    }
}
