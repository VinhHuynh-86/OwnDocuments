using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ViewMainMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] textTitle;
    [SerializeField] private UIButton cheatLeftTopBtn;
    [SerializeField] private GameObject btnUnlockGame;
    [SerializeField] private Animator yearAnim;
    [SerializeField] private TopBarUI topBarUI;

    private int tapCount = 0;
    private int curYear = 0;

    void Awake()
    {
        #if ENABLE_CHEAT
        #else
        if(this.cheatLeftTopBtn != null)
            Destroy(this.cheatLeftTopBtn.gameObject);
        #endif
    }
    void Start()
    {
        if (Define.ENABLE_CHEAT)
        {
            RegisterCheatButton();
        }
    }

    void Update()
    {
        UpdateBackKey();        
    }

    void OnEnable()
    {
        if(!Game.Instance.m_IsInitData) return;
        if (Game.Instance)
        {
            Game.Instance.SetViewState(Define.VIEW.MAIN_MENU);
        }

        if (BaseCamera.Instance)
        {
            BaseCamera.Instance.ActiveBaseCamera();
        }

        if (TutorialManager.Instance)
        {
            TutorialManager.Instance.UpdateState();
            
            {
                btnUnlockGame.SetActive(!ProfileMgr.Instance.IsGameUnlocked);
            }
        }
        

        int nextYear = int.Parse(Database.GetLastYearVenueUnlocked());
        if(curYear == 0 || curYear == nextYear)
        {
            UpdateYearText();
        }
        else
        {
            yearAnim.Play("Year_Change", 0);
            SFXManager.Instance.Play(Define.SFX.UI_YEAR_CHANGE);
        }

        if((TutorialManager.Instance.Tutorial >= Define.TUTORIAL.DONE))
        {
            InvisibleAllBtn();
            FadeInAllBtn(Game.Instance.fadeDuration);
            Game.Instance.fadeDuration = 0;
        }
        Game.Instance.DestroyObjInit();
        SFXManager.Instance.UnloadMusic(); // unload music in credits
    }

    void OnDisable()
    {
        if (BaseCamera.Instance) BaseCamera.Instance.DeactiveBaseCamera();
        
    }

    public void UpdateInfoGameObject()
    {
        topBarUI.UpdateResources();
        if (TutorialManager.Instance)
        {
            TutorialManager.Instance.UpdateState();
            {
                btnUnlockGame.SetActive(!ProfileMgr.Instance.IsGameUnlocked);
            }
            
        }
        
        UpdateYearText();

        if((TutorialManager.Instance.Tutorial >= Define.TUTORIAL.DONE))
        {
            InvisibleAllBtn();
            FadeInAllBtn(Game.Instance.fadeDuration);
            Game.Instance.fadeDuration = 0;
        }
    }

    private void RegisterCheatButton()
    {
        cheatLeftTopBtn.OnClick.OnTrigger.Event.AddListener(() =>
        {
            tapCount++;
            if (tapCount == 3)
            {
                tapCount = 0;
                GameEventMgr.SendEvent("open_cheat");
            }
        });
    }

    public void UpdateBackKey()
    {
        if (GameUtils.Instance.IsActiveBackKey())
        {
            if (Game.Instance.IsDisablingTouch())
            {
                AndroidToast.ShowCannotBack();
            }
            else
            {
                ShowPopupExit();
                SFXManager.Instance.Play(Define.SFX.UI_MENU_BACK);
            }
        }
    }

    public void ShowPopupExit()
    {
        UIPopup popup = UIPopup.GetPopup(Define.POPUP_YES_NO);
        popup.Show();
        SFXManager.Instance.Play(Define.SFX.UI_MESSAGE_POP_UP);
        PopupYesNo popupYesNo = popup.GetComponent<PopupYesNo>();
        popupYesNo.SetTextContent("STR_EXIT_GAME");
        popupYesNo.SetCallBackYes(PopupExitYes);
    }

    public void PopupExitYes()
    {        
        Application.Quit();
    }

    public void UpdateYearText()
    {
        for(int i = 0; i < textTitle.Length; i++)
        {
            textTitle[i].text = Database.GetLastYearVenueUnlocked();
            curYear = int.Parse(Database.GetLastYearVenueUnlocked());
        }
        // textTitle.GetComponent<MultiUnderlay>().UpdateText();
    }


    public void FadeInAllBtn(float duration)
    {
        GameUtils.Instance.FadeInObj(btnUnlockGame.gameObject, duration);
    }

    public void FadeOutAllBtn(float duration)
    {
        GameUtils.Instance.FadeOutObj(btnUnlockGame.gameObject, duration);
    }

    public void InvisibleAllBtn()
    {
        GameUtils.Instance.SetInvisibleObject(btnUnlockGame.gameObject);
	}
}
