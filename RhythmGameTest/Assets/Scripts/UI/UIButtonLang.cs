using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIButtonLang : MonoBehaviour
{
    public GameObject objectLangUnselected;
    public GameObject objectLangENUnselected;
    public GameObject objectLangSelected;
    public TextMeshProUGUI textLangUnselected;
    public Image imgLangNative;
    public Image imgLangNativeSelected;
    // public TextMeshProUGUI textLangSelected;

    public void SetTextLang(string text)
    {
        textLangUnselected.text = text;
    }

    public void SetSpriteLang(Sprite sprite)
    {
        imgLangNative.sprite = sprite;
        imgLangNativeSelected.sprite = sprite;
    }

    public void SetSelected()
    {
        objectLangUnselected.SetActive(false);
        objectLangENUnselected.SetActive(false);
        objectLangSelected.SetActive(true);
    }

    public void SetUnselected()
    {
        objectLangUnselected.SetActive(true);
        objectLangENUnselected.SetActive(false);
        objectLangSelected.SetActive(false);
    }

    public void SetUnselectedLangEN()
    {
        objectLangUnselected.SetActive(false);
        objectLangENUnselected.SetActive(true);
        objectLangSelected.SetActive(false);
    }
}
