using Doozy.Engine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTutorial : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textContent;
    [SerializeField] TextMeshProUGUI textBtnContinue;
    [SerializeField] UIButton btnPrev;
    [SerializeField] Transform container;
    [SerializeField] Transform notesActive;
    [SerializeField] Transform notesDeactive;
    Vector3 posContainer = Vector3.zero;

    void OnEnable()
    {
        posContainer = container.localPosition;
        notesActive.gameObject.SetActive(false);
        notesDeactive.gameObject.SetActive(false);
        if(Game.Instance.GetViewState() != Define.VIEW.SONG)
        {
            Time.timeScale = 0;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AndroidToast.ShowCannotBack();
        }
    }

    public void OnOk()
    {
        try
        {
            TutorialManager.Instance.OnOkPopup();
            if(TutorialManager.Instance.Tutorial != Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR
                && TutorialManager.Instance.Tutorial != Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2
            )
            {
                Time.timeScale = 1;
            }
            UIPopup.HidePopup("PopupTutorial");
        }
        catch (Exception) { }
    }

    public void OnPrev()
    {
        try
        {
            TutorialManager.Instance.OnPrevPopup();
            if(TutorialManager.Instance.Tutorial != Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR
                && TutorialManager.Instance.Tutorial != Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2
            )
            {
                Time.timeScale = 1;
            }
            UIPopup.HidePopup("PopupTutorial");
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

    public Vector3 GetPos()
    {
        return container.localPosition;
    }

    public void SetPos(Vector3 pos)
    {
        posContainer = pos;
    }

    void UpdatePos()
    {
        if(container.localPosition != posContainer)
        {
            container.localPosition = posContainer;
            
        }
    }

    public void ShowNotesActive()
    {
        notesActive.gameObject.SetActive(true);
    }

    public void ShowNotesDeactive()
    {
        notesDeactive.gameObject.SetActive(true);
    }

    public void Finished()
    {
        UpdatePos();
    }
}
