using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using Doozy.Engine.Progress;
using TMPro;

public class ViewPauseMenu : MonoBehaviour
{
    public TextMeshProUGUI textSubTitle;
    [SerializeField] private UIButton cheatLeftTopBtn;
    [SerializeField] private Progressor ProgressCountdown;
    [SerializeField] private GameObject rehearsalBtn;
    [SerializeField] private GameObject songSelectionBtn;
    [SerializeField] private GameObject restartBtn;
    [SerializeField] private GameObject back_IAP_Btn;
    [SerializeField] private GameObject homeBtn;
    [SerializeField] private GameObject viewContainer;
    [SerializeField] private Animator countdownAnimator;
    private int tapCount = 0;
    Timer mTimerContinue = new Timer();
    const float COUNTDOWN_TIMER = 3;

    bool isInterrupted = false;
    void Start()
    {
        if(Define.ENABLE_CHEAT)
        {
            RegisterCheatButton();
        }
        else
        {
            if(this.cheatLeftTopBtn != null)
                Destroy(this.cheatLeftTopBtn.gameObject);
        }
        ProgressCountdown.SetMax(COUNTDOWN_TIMER);
    }

    void OnEnable()
    {
        if(Game.Instance == null || SFXManager.Instance == null || viewContainer == null || textSubTitle == null)
            return;

        textSubTitle.text = Game.Instance.GetTextSongName() + " - " + Game.Instance.GetTextGameMode();
        Game.Instance.SetViewState(Define.VIEW.PAUSE);
        ProgressCountdown.gameObject.SetActive(false);

        // Debug.Log("ViewPauseMenu::OnEnable():: viewContainer.SetActive(true) ");

        viewContainer.SetActive(true);
        if(SFXManager.Instance)
            SFXManager.Instance.PauseAllSFX();
        
        if(Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.BASIC_TUTORIAL_REPLAY
            || Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.ADVANCED_TUTORIAL_REPLAY
        )
        {
            SetActiveBtnForTutorialSetting();
        }
        else
        {
            SetActiveGroupBtnForNormal();
        }

        
    }

    void OnDisable()
    {
        if(SFXManager.Instance)
            SFXManager.Instance.ResumeAllSFX();        
    }

    void Update()
    {
        UpdateBackKey();
    }

    void SetActiveGroupBtnForRehearsal()
    {
        songSelectionBtn.SetActive(false);
        homeBtn.SetActive(true);
        rehearsalBtn.SetActive(true);
        restartBtn.SetActive(true);
    }

    void SetActiveGroupBtnForIAP()
    {
        songSelectionBtn.SetActive(false);
        restartBtn.SetActive(false);
        homeBtn.SetActive(false);
        rehearsalBtn.SetActive(false);
    }

    void SetActiveGroupBtnForNormal()
    {
        songSelectionBtn.SetActive(true);
        restartBtn.SetActive(true);
        homeBtn.SetActive(true);
        rehearsalBtn.SetActive(false);
    }

    void SetActiveBtnForTutorialSetting()
    {
        songSelectionBtn.SetActive(false);
        restartBtn.SetActive(false);
        homeBtn.SetActive(true);
        rehearsalBtn.SetActive(false);
    }

    private void RegisterCheatButton()
    {
        cheatLeftTopBtn.OnClick.OnTrigger.Event.AddListener(() =>
        {
            tapCount++;
            if(tapCount == 3)
            {
                tapCount = 0;
                GameEventMgr.SendEvent("open_cheat");
            }
        });
    }

    public void OnClickResume()
    {
        isInterrupted = false;
        countdownAnimator.Play("Countdown");
        viewContainer.SetActive(false);
    }

    public void OnHome()
    {
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_YES_NO);
        popup.Show();
        PopupYesNo popupYesNo = popup.GetComponent<PopupYesNo>();
        popupYesNo.SetTextContent("STR_RETURN_SONG_SELECTION");
        popupYesNo.SetCallBackYes(OnHomeYes);
    }

    public void OnHomeYes()
    {
        GameEventMgr.SendEvent("Back to Waiting");
        GameManager.Instance.OnBackToMainMenu();
        if(Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.BASIC_TUTORIAL_REPLAY
            || Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.ADVANCED_TUTORIAL_REPLAY
        )
        {
            TutorialManager.Instance.SkipTutorialFromPause();
        }
    }

    public void OnSongSelection()
    {
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_YES_NO);
        popup.Show();
        PopupYesNo popupYesNo = popup.GetComponent<PopupYesNo>();
        popupYesNo.SetTextContent("STR_RETURN_SONG_SELECTION");
        popupYesNo.SetCallBackYes(OnSongSelectionYes);
    }

    public void OnSongSelectionYes()
    {
        GameManager.Instance.OnSelectSong();
        GameEventMgr.SendEvent("Back to Waiting");
    }

    public void OnRestart()
    {
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_YES_NO);
        popup.Show();
        PopupYesNo popupYesNo = popup.GetComponent<PopupYesNo>();
        popupYesNo.SetTextContent("STR_RESTART_GAME");
        popupYesNo.SetCallBackYes(OnRestartYes);
    }

    public void OnRestartYes()
    {
        GameManager.Instance.OnReplay();
        GameEventMgr.SendEvent(Define.GotoAP);
    }

    public void ResumeGame()
    {
        GameEventMgr.SendEvent(Define.GotoAP);
    }

    public void OnClickBackIAP()
    {
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_YES_NO);
        popup.Show();
        PopupYesNo popupYesNo = popup.GetComponent<PopupYesNo>();
        popupYesNo.SetTextContent("STR_RETURN_SONG_SELECTION");
        popupYesNo.SetCallBackYes(OnClickIAPYes);        
    }

    public void OnClickIAPYes()
    {
        GameManager.Instance.OnResumeGame();
        GameManager.Instance.SetStateNone();
        GameEventMgr.SendEvent("Back to Waiting");
    }

    public void OnClickRehearsal()
    {
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_YES_NO);
        popup.Show();
        PopupYesNo popupYesNo = popup.GetComponent<PopupYesNo>();
        popupYesNo.SetTextContent("STR_RETURN_SONG_SELECTION");
        popupYesNo.SetCallBackYes(OnRehearsalYes);
        
    }

    public void OnRehearsalYes()
    {
        GameManager.Instance.OnResumeGame();
        GameManager.Instance.SetStateNone();
        GameEventMgr.SendEvent("Back to Waiting");
    }

    public void UpdateBackKey()
    {
        if (GameUtils.Instance.IsActiveBackKey())
        { 
            if(viewContainer.activeSelf)
            {
                // Debug.Log("ViewPauseMenu::UpdateBackKey():: viewContainer.SetActive(false) ");
                isInterrupted = false;
                countdownAnimator.Play("Countdown");
                viewContainer.SetActive(false);
            }
            else
            {
                // Debug.Log("ViewPauseMenu::UpdateBackKey():: viewContainer.SetActive(true) ");

                countdownAnimator.Play("Idle");
                viewContainer.SetActive(true);
            }
        }
    }

    public bool IsIntterupted()
    {
        return isInterrupted;
    }
    private void OnApplicationFocus(bool focusStatus)
    {
        // Debug.Log("ViewPauseMenu::OnApplicationFocus():: focusStatus = " + focusStatus);
        if(focusStatus && isInterrupted)
        {
            // Debug.Log("ViewPauseMenu::OnApplicationFocus():: focusStatus = true && isInterrupted = true");
            // isInterrupted = false;
            // countdownAnimator.Play("Idle");
            // GameManager.Instance.OnResumeGame();
            // GameManager.Instance.OnPauseGameOnly();
            // viewContainer.SetActive(true);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && Game.Instance.m_IsInitData)
        {
            isInterrupted = true;

            if(!viewContainer.activeSelf)
            {
                countdownAnimator.Play("Idle");
                viewContainer.SetActive(true);
            }
        }
    } 
}
