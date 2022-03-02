using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using TMPro;

public class PopupGeneration : MonoBehaviour
{
    [SerializeField]protected TextMeshProUGUI textContent;
    public void SetTextContent(string value)
    {
        textContent.text = value;
    }
}
