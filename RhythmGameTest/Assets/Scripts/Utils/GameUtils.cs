using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Text;
using Doozy.Engine.UI;
using UnityEngine.UI;

public class GameUtils : Singleton<GameUtils>
{
    [HideInInspector]
    public bool isShowCannotBack = false;
    [HideInInspector]
    public Timer timerControl = new Timer();
    private float timeBackKeyActive;
    const float TIME_BACK_KEY_ACTIVE = 0.5f;
    const float TITLE_SCALE_4_3 = 0.85f;

    void Update()
    {
        if(timeBackKeyActive < TIME_BACK_KEY_ACTIVE)
        {
            timeBackKeyActive += Time.unscaledDeltaTime;
        }
        if(UIPopup.AnyPopupVisible)
            timeBackKeyActive = 0;
        if(isShowCannotBack)
        {
            timerControl.Update(Time.unscaledDeltaTime);
            if(timerControl.JustFinished())
            {
                isShowCannotBack = false;
            }
        }
    }    

    public void UpdateTextUnderlay(TextMeshProUGUI text)
    {
        MultiUnderlay underlay = text.gameObject.GetComponent<MultiUnderlay>();
        if(underlay != null)
        {
            underlay.UpdateText();
        }
    }

    public void UpdateAddValue(TextMeshProUGUI text, int value, float time)
    {
        int startValue = int.Parse(text.text);
        int endValue = startValue + value;        
        DOTween.To(() => startValue, x => startValue = x, endValue, time).OnUpdate(() =>
        {
            text.text = "" + startValue;
        });
    }

    public string ReplaceString(string s, string oldString, string newString)
    {
        StringBuilder builder = new StringBuilder(s);
        builder.Replace(oldString, newString);
        return builder.ToString();
    }

    public void SetColorText(TextMeshProUGUI textMeshProUGUI, float value)
    {
        Color c = textMeshProUGUI.color;
        c.a = value;
        textMeshProUGUI.color = c;
        textMeshProUGUI.GetComponent<MultiUnderlay>().UpdateAlpha();
    }

    public bool IsActiveBackKey()
    {
        return Input.GetKeyDown(KeyCode.Escape) && timeBackKeyActive > TIME_BACK_KEY_ACTIVE;
    }

    public bool IsScreenRatio4_3()
    {
        if((float)Screen.width/(float)Screen.height <= 4f/3f)
        {
            return true;
        }
        return false;
    }  

    public void ScaleTitle(Transform transTitle)
    {
        ScaleTitle(transTitle, TITLE_SCALE_4_3);
    }

    public void ScaleTitle(Transform transTitle, float scale)
    {
        if(GameUtils.Instance.IsScreenRatio4_3())
        {
            transTitle.localScale = Vector3.one*scale;
        }
        else
        {
            transTitle.localScale = Vector3.one;
        }
    }

    public void SetInvisibleObject(GameObject obj)
    {
        Image img = obj.GetComponent<Image>();
        Color c = img.color;
        c.a = 0;
        img.color = c;
        Image[] imgChildren = obj.GetComponentsInChildren<Image>();
        for(int i = 0; i  < imgChildren.Length; i++)
        {
            c = imgChildren[i].color;
            c.a = 0;
            imgChildren[i].color = c;
        }

        TextMeshProUGUI[] txtChildren = obj.GetComponentsInChildren<TextMeshProUGUI>();
        for(int i = 0; i  < txtChildren.Length; i++)
        {
            c = txtChildren[i].color;
            c.a = 0;
            txtChildren[i].color = c;
        }
    }

    public void FadeOutObj(GameObject obj, float duration)
    {
        FadeObj(obj, 0, duration);
    }

    public void FadeInObj(GameObject obj, float duration)
    {
        FadeObj(obj, 1, duration);
    }

    public void FadeObj(GameObject obj, float value, float duration)
    {
        Image img = obj.GetComponent<Image>();
        img.DOFade(0, duration);
        Image[] imgChildren = obj.GetComponentsInChildren<Image>();
        for(int i = 0; i  < imgChildren.Length; i++)
        {
            imgChildren[i].DOFade(value, duration);
        }

        TextMeshProUGUI[] txtChildren = obj.GetComponentsInChildren<TextMeshProUGUI>();
        for(int i = 0; i  < txtChildren.Length; i++)
        {
            txtChildren[i].DOFade(value, duration);
        }
    }
}
