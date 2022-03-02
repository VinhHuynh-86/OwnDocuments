using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerfectText : MonoBehaviour
{
    public TextMeshProUGUI comboText;

    public void SetComboText(int combo)
    {
        if(combo > 1)
        {
            comboText.text = "x"+combo;
        }
        else
        {
            comboText.text = "";
        }
        GameUtils.Instance.UpdateTextUnderlay(comboText);
    }
}
