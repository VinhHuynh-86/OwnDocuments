using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupCannotBack : MonoBehaviour
{
    public RectTransform rectTransBG;
    public Sprite spriteBG;
    public Sprite spriteBGShort;

    void OnEnable()
    {
        rectTransBG.sizeDelta = new Vector2(GetWidth(), rectTransBG.sizeDelta.y);
    }

    public float GetWidth()
    {
        float width = 800f;
        Image imgBG = rectTransBG.GetComponent<Image>();
        imgBG.sprite = spriteBG;
        switch(Localization.Instance.GetCurrentLanguage())
        {
            case Language.English:
            case Language.Turkish:
                width = 800f;
            break;

            case Language.Indonesian:
                width = 1000f;
            break;

            case Language.French:
                width = 1100f;
            break;
            
            case Language.German:
                width = 1200f;
            break;

            case Language.Italian:
                width = 950f;
            break;

            case Language.Japanese:
                width = 550;
                imgBG.sprite = spriteBGShort;
            break;

            case Language.Korean:
                width = 600;
            break;
            case Language.Portuguese:
                width = 700;
            break;

            case Language.Russian:
                width = 850f;
            break;

            case Language.ChineseSimplified:
            case Language.ChineseTraditional:
                width = 400f;
                imgBG.sprite = spriteBGShort;
            break;

            case Language.Spanish:
                width = 900f;
            break;

            case Language.Thai:
                width = 700f;
            break;
        }

        return width;
    }
}
