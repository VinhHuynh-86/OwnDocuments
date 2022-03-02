using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Underlay
{
    public TextMeshProUGUI textUnderlay;
    public Color color;
    public Vector2 offset;


    public void UpdateUnderlay(TextMeshProUGUI textMain)
    {
        if(textUnderlay == null)
            return;
            
        UpdateText(textMain);
        textUnderlay.name = textMain.name;
        textUnderlay.alignment = textMain.alignment;
        textUnderlay.fontSize = textMain.fontSize;
        textUnderlay.color = color;
        textUnderlay.GetComponent<RectTransform>().offsetMin = offset;
        textUnderlay.GetComponent<RectTransform>().offsetMax = offset;
        textUnderlay.font = textMain.font;
        textUnderlay.gameObject.SetActive(true);
        LocalizationText locMain = textMain.gameObject.GetComponent<LocalizationText>();
        LocalizationText locUnderlay = textUnderlay.gameObject.GetComponent<LocalizationText>();
        if(locMain != null && locUnderlay != null)
        {
            locUnderlay.isUpdateLoc = locMain.isUpdateLoc;
        }
        UpdateOrder(textMain);
    }

    public void UpdateText(TextMeshProUGUI textMain)
    {
        if(textUnderlay != null && textMain != null)
        {
            textUnderlay.fontSize = textMain.fontSize;
            textUnderlay.text = textMain.text;
        }
    }

    public void SetAlpha(float alpha)
    {
        Color c = textUnderlay.color;
        c.a = alpha;
        textUnderlay.color = c;
    }

    public void UpdateOrder(TextMeshProUGUI textMain)
    {
        Canvas canvasMain = textMain.gameObject.GetComponent<Canvas>();
        Canvas canvasUnderlay = textUnderlay.gameObject.GetComponent<Canvas>();
        if(canvasMain != null && canvasUnderlay != null)
        {
            canvasUnderlay.sortingOrder = canvasMain.sortingOrder - 1;
        }
    }
}

public class MultiUnderlay : MonoBehaviour
{
    private TextMeshProUGUI textMain;
    public List<Underlay> underlays = new List<Underlay>();
    const float TIME_DELAY = 0.0005f;
 
    void OnEnable()
    {
        Invoke("UpdateUnderlay", TIME_DELAY);
    }

    void UpdateUnderlay()
    {
        textMain = GetComponent<TextMeshProUGUI>();
        for(int i = 0; i < underlays.Count; i++)
        {
            underlays[i].UpdateUnderlay(textMain);
        }
    }

    public void UpdateText()
    {
        Invoke("UpdateTextImmediate", TIME_DELAY);
    }

    public void UpdateTextImmediate()
    {
        for(int i = 0; i < underlays.Count; i++)
        {
            underlays[i].UpdateText(textMain);
        }
    }

    public void SetAlpha(float alpha)
    {
        for(int i = 0; i < underlays.Count; i++)
        {
            underlays[i].SetAlpha(alpha);
        }
    }

    public void UpdateAlpha()
    {
        Invoke("UpdateAlphaImmediate", TIME_DELAY);
    }

    public void UpdateAlphaImmediate()
    {
        if(textMain == null)
        {
            return;
        }
        float alpha = textMain.color.a;
        for(int i = 0; i < underlays.Count; i++)
        {
            underlays[i].SetAlpha(alpha);
        }
    }

    public void UpdateOrder()
    {
        Invoke("UpdateOrderImmediate", TIME_DELAY);
    }

    public void UpdateOrderImmediate()
    {
        for(int i = 0; i < underlays.Count; i++)
        {
            underlays[i].UpdateOrder(textMain);
        }
    }

    public void SetActive(bool isActive)
    {
        for(int i = 0; i < underlays.Count; i++)
        {
            underlays[i].textUnderlay.gameObject.SetActive(isActive);
        }
    }
}
