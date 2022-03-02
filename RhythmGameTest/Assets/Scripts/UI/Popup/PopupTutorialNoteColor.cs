using Doozy.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTutorialNoteColor : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textContent;
    [SerializeField] TextMeshProUGUI textBtnContinue;
    [SerializeField] UIButton btnPrev;
    [SerializeField] GameObject noteColorActive;
    [SerializeField] GameObject noteColorDeactive;

    void OnEnable()
    {
    }

    public void OnOk()
    {
        try
        {
            TutorialManager.Instance.OnOkPopup();
            UIPopup.HidePopup("PopupTutorialNoteColor");
        }
        catch (Exception) { }
    }

    public void OnPrev()
    {
        try
        {
        TutorialManager.Instance.OnPrevPopup();
        UIPopup.HidePopup("PopupTutorialNoteColor");
        }
        catch (Exception) { }
    }
    public void SetTextContent(string value)
    {
        textContent.text = value;
    }

    public void SetTextButton(string value)
    {
        textBtnContinue.text = value;
    }

    public void ShowBtnPrev(bool show)
    {
        btnPrev.gameObject.SetActive(show);
    }

    public void SetNoteColorActive(bool active)
    {
        if(active)
        {
            noteColorActive.SetActive(true);
            noteColorDeactive.SetActive(false);
        }
        else
        {
            noteColorActive.SetActive(false);
            noteColorDeactive.SetActive(true);
        }
    }
}
