using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Doozy.Engine.UI;

public class TutorialManager : MonoBehaviour
{
    // public value
    public static TutorialManager Instance = null;
    public TextMeshProUGUI textIndicator;
    public GameObject handBG;
    public GameObject lightBG;
    [HideInInspector]
    public List<float> listTimeState = new List<float>();

    // Main Menu
    public Button btnStudioShop;
    public Transform handStudioShop;
    public Button btnStudioPlay;
    public Transform handStudioPlay;
    public Button btnStudioSetting;
    public Button btnStudioAchievement;
    public Button btnStudioHistory;
    public Button btnStudioUnlockGame;
    public bool isGoingStudio = true;


    // Venue    
    [HideInInspector]
    public GameObject venueTarget;

    //Song Selection
    [HideInInspector]
    public List<GameObject> listBtnSongMode = new List<GameObject>();
    [HideInInspector]
    public GameObject btnSongModeEasy;
    [HideInInspector]
    public Transform handSongModeEasy;
    public Button btnSongArrow;
    public Transform handSongArrow;

    //InGame
    public Animator handLeft;
    public Animator handRight;
    public GameObject rockBar;
    public UnityEvent onRockBarFull;
    public UnityEvent onRockBarInit;
    public GameObject scenario;
    public Image imgScenario;
    public TextMeshProUGUI contentScenario;

    //Result
    public Button btnResultHome;
    public Transform handResultHome;
    public Button btnResultNext;
    public Transform handResultNext;

    // private value
    public Define.TUTORIAL tutorial;
    public Define.TUTORIAL preTutorial;
    public Define.TUTORIAL preTutorial2;
    private int originalOrderLayer;
    private Canvas canvasObj = null;
    private Transform handControl;
    private Vector3 originalPosition;
    private Timer timerControl = new Timer();
    private int handDirection;

    //const
    const float TIME_HAND_FADE = 1f;
    const float DISTANCE_SHOW_HAND_MAX = 64;
    const float DISTANCE_SHOW_HAND_MIN = 53f;
    const float HAND_ANIM_DEFAULT_SPEED = 1f;
    const float HAND_ANIM_SLOW_SPEED = 0.5f;
    const float HAND_ANIM_FAST_SPEED = 2f;

    const float TIME_SHOW_TEXT = 4f;
    const float TIME_DELAY_TEXT = 1f;
    const float TIME_DELAY_TEXT_2 = 2f;

    const int LEFT = 0;
    const int RIGHT = 1;
    const int UP = 2;
    const int DOWN = 3;
    const int HEAD = 0;
    const int BODY = 1;
    const int LEG = 2;

    const string HAND_IDLE = "Idle";
    const string HAND_TAP_LEFT = "Tap_Left";
    const string HAND_TAP_MID = "Tap";
    const string HAND_TAP_RIGHT = "Tap_Right";
    const string HAND_HOLD_START_LEFT = "Hold_Start_Left";
    const string HAND_HOLD_START_MID = "Hold";
    const string HAND_HOLD_START_RIGHT = "Hold_Start_Right";
    const string HAND_HOLD_END_LEFT = "Hold_End_Left";
    const string HAND_HOLD_END_MID = "Hold_End_Mid";
    const string HAND_HOLD_END_RIGHT = "Hold_End_Right";
    const string HAND_MEGA_SWIPE_LEFT = "Megaswipe_Left";
    const string HAND_MEGA_SWIPE_RIGHT = "Megaswipe_Right";
    const string HAND_SWIPE_LEFT_TO_MID = "Swipe_LeftToMid";
    const string HAND_SWIPE_MID_TO_LEFT = "Swipe_MidToLeft";
    const string HAND_SWIPE_MID_TO_RIGHT = "Swipe_MidToRight";
    const string HAND_SWIPE_RIGHT_TO_MID = "Swipe_RightToMid";
    const string HAND_DRAG_LEFT_TO_MID = "Hold_Drag_LeftToMid";
    const string HAND_DRAG_MID_TO_LEFT = "Hold_Drag_MidToLeft";
    const string HAND_DRAG_MID_TO_RIGHT = "Hold_Drag_MidToRight";
    const string HAND_DRAG_RIGHT_TO_MID = "Hold_Drag_RightToMid";
    const string HAND_FADE_IN_LEFT = "FadeIn_Left";
    const string HAND_FADE_IN_MID = "FadeIn_Mid";
    const string HAND_FADE_IN_RIGHT = "FadeIn_Right";
    const string HAND_FADE_OUT = "FadeOut";
    const string ARROW_POINT_DOWN = "Point_Down";
    const string ARROW_POINT_UP = "Point_Up";
    const string ARROW_POINT_LEFT = "Point_Left";
    const string ARROW_POINT_RIGHT = "Point_Right";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        tutorial = Define.TUTORIAL.DONE;
    }

    void Start()
    {
        // FadeOutHandLeft();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTutorial();
    }

    void UpdateTutorial()
    {
        switch (tutorial)
        {
            case Define.TUTORIAL.INGAME_SCENARIO_INTRO:
                timerControl.Update(Time.deltaTime);
                if (timerControl.JustFinished())
                {
                    SFXManager.Instance.PlayBackGround(Define.SFX.CROWD_BED); 
                    InitState(Define.TUTORIAL.INGAME_SCENARIO_FREE);
                }
                break;

            case Define.TUTORIAL.INGAME_SCENARIO_FREE:
                timerControl.Update(Time.deltaTime);
                if (timerControl.JustFinished())
                {
                    InitState(Define.TUTORIAL.INGAME_POPUP_INTRO);
                }
                break;

            case Define.TUTORIAL.INGAME_TAP:
                if (IsNextState())
                {
                    InitState(Define.TUTORIAL.INGAME_USER_TAP);
                }
                break;
            case Define.TUTORIAL.INGAME_USER_TAP:
                if (IsNextState())
                {
                    InitState(Define.TUTORIAL.INGAME_TAP_HOLD);
                }
                break;
            case Define.TUTORIAL.INGAME_TAP_HOLD:
                if (IsNextState())
                {
                    InitState(Define.TUTORIAL.INGAME_USER_TAP_HOLD);
                }
                break;
            case Define.TUTORIAL.INGAME_USER_TAP_HOLD:
                if (IsNextState())
                {
                    InitState(Define.TUTORIAL.INGAME_MIGHTY_SWIPE);
                }
                break;
            case Define.TUTORIAL.INGAME_MIGHTY_SWIPE:
                if (IsNextState())
                {
                    InitState(Define.TUTORIAL.INGAME_USER_MIGHTY_SWIPE);
                }
                break;
            case Define.TUTORIAL.INGAME_USER_MIGHTY_SWIPE:
                if (IsNextState())
                {
                    InitState(Define.TUTORIAL.INGAME_PRE_POPUP_NOTE_COLOR);
                }
                break;

            case Define.TUTORIAL.INGAME_PRE_POPUP_NOTE_COLOR:
                timerControl.Update(Time.deltaTime);
                if (timerControl.JustFinished())
                {
                    InitState(Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR);
                }
                break;
        }
    }

    public void UpdateState()
    {
        switch (tutorial)
        {
            case Define.TUTORIAL.START:
                InitState(Define.TUTORIAL.INGAME_SCENARIO_INTRO);
                break;

            case Define.TUTORIAL.INGAME_SCENARIO_INTRO:
                InitState(Define.TUTORIAL.INGAME_POPUP_INTRO);
                break;

            case Define.TUTORIAL.BASIC_LOADING:
                InitState(Define.TUTORIAL.INGAME_TAP);
            break;

            case Define.TUTORIAL.INGAME_SCENARIO_END:
                if (Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.BASIC_TUTORIAL_REPLAY)
                {
                    InitState(Define.TUTORIAL.DONE);
                    Game.Instance.IsDemoTutorial = Define.TUTORIAL_TYPE.NORMAL_TUTORIAL;
                }
                else
                {
                    InitState(Define.TUTORIAL.DONE);
                }
                break;

            case Define.TUTORIAL.DONE:
                break;
        }
    }

    public void OnOkPopup()
    {
        switch (tutorial)
        {
            case Define.TUTORIAL.INGAME_POPUP_INTRO:
                InitState(Define.TUTORIAL.INGAME_TAP);
                break;

            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR:
                InitState(Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2);
                // InitStateDelay(Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2);
                break;

            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2:
                InitState(Define.TUTORIAL.INGAME_SCENARIO_END);
                break;
        }
    }

    public void OnPrevPopup()
    {
        switch (tutorial)
        {
            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2:
                InitState(Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR);
                // InitStateDelay(Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR);
                break;
        }
    }

    public void InitState(Define.TUTORIAL state)
    {
        preTutorial = tutorial;
        tutorial = state;
        HideHand();
        if (canvasObj != null)
        {
            canvasObj.sortingOrder = originalOrderLayer;
        }
        switch (state)
        {
            case Define.TUTORIAL.START:
                break;

            case Define.TUTORIAL.INGAME_SCENARIO_INTRO:
                rockBar.SetActive(false);
                string content = Localization.Instance.GetString("STR_TEXAS") + ", 1977";
                ShowScenario(content, true);
                timerControl.SetDuration(2.5f);
                
                break;

            case Define.TUTORIAL.INGAME_SCENARIO_FREE:
                HideScenario(true); 
                SFXManager.Instance.FadeInBG(Define.SFX.CROWD_BED, 1f, true);
                SFXManager.Instance.FadeInCrowdCheer(1f);
                timerControl.SetDuration(3.5f);
                break;

            case Define.TUTORIAL.INGAME_POPUP_INTRO:
                // SFXManager.Instance.Stop();
                ShowPopup("STR_TUT_INTRO");
                break;
            case Define.TUTORIAL.INGAME_TAP:
                UpdateHandMotionTap();
                if(Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.NORMAL_TUTORIAL)
                {
                    GameManager.Instance.SetState(Define.GAME_STATE.INIT);
                }
                ShowTextDelay("STR_TUT_TAP", TIME_DELAY_TEXT_2);
                break;
            case Define.TUTORIAL.INGAME_USER_TAP:
                ShowTextDelay("STR_TUT_KEEP_GOING");
                HideTextDelay(TIME_SHOW_TEXT);
                break;
            case Define.TUTORIAL.INGAME_TAP_HOLD:
                UpdateHandMotionTapHold();
                ShowTextDelay("STR_TUT_TAP_HOLD", TIME_DELAY_TEXT_2);
                break;
            case Define.TUTORIAL.INGAME_USER_TAP_HOLD:
                ShowTextDelay("STR_TUT_KEEP_GOING");
                HideTextDelay(TIME_SHOW_TEXT);
                break;
            case Define.TUTORIAL.INGAME_MIGHTY_SWIPE:
                UpdateHandMotionMegaSwipe();
                ShowTextDelay("STR_TUT_SWIPE");
                break;
            case Define.TUTORIAL.INGAME_USER_MIGHTY_SWIPE:
                ShowTextDelay("STR_TUT_KEEP_GOING");
                HideTextDelay(TIME_SHOW_TEXT);
                break;
            case Define.TUTORIAL.INGAME_PRE_POPUP_NOTE_COLOR:
                SetHandLeftIdle();
                SetHandRightIdle();
                FadeOutAudio(0);
                SFXManager.Instance.PlayCrowdEnd();
                GameManager.Instance.SetStateNone();
                timerControl.SetDuration(3f);
                SFXManager.Instance.FadeOutBG();
                SFXManager.Instance.LoadClipMusic("SFX/lic_mm_studio/m_lic_sfx_London_1974");
                break;
            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR:
                SFXManager.Instance.StopAll();
                FadeInHandLeft();
                FadeInHandRight();
                ShowPopup("STR_TUT_COLOR_01", "STR_NEXT");
                if (!GameManager.Instance.IsGamePaused())
                {
                    GameManager.Instance.OnPauseGameOnly();
                }
                break;
            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2:
                ShowPopup("STR_TUT_COLOR_02", "STR_NEXT", true);
                break;
            case Define.TUTORIAL.INGAME_SCENARIO_END:
                GameManager.Instance.OnResumeGame();
                GameManager.Instance.SetStateNone();
                if (Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.NORMAL_TUTORIAL)
                {
                    string content2 = Localization.Instance.GetString("STR_LONDON") + ", 1974\n" + Localization.Instance.GetString("STR_TUT_FLASHBACK3");
                    ShowScenario(content2, true);
                    Invoke("BackMainMenu", 2f);
                    // Play London 1974
                    SFXManager.Instance.PlayMusic();
                }
                else if (Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.BASIC_TUTORIAL_REPLAY)
                {                    
                    Invoke("BackSetting", 1f);
                }
                break;

            case Define.TUTORIAL.DONE:
                SetHandLeftIdle();
                SetHandRightIdle();
                ProfileMgr.Instance.Tutorial = (int)state;
                ProfileMgr.Instance.Save();
                // Game.Instance.RequestLocalPushPermission();
                break;
        }
    }

    public void UpdateHandMotionTap()
    {
        Invoke("HandLeftFadeInMid", 3f);        
        Invoke("HandLeftTapMid", 3.25f);        
        Invoke("HandLeftTapMid", 5f);
    }

    public void HandLeftFadeInMid()
    {
        handLeft.enabled = true;
        handLeft.Play(HAND_FADE_IN_MID);
    }

    public void HandLeftTapMid()
    {
        handLeft.enabled = true;
        handLeft.speed = HAND_ANIM_SLOW_SPEED;
        handLeft.Play(HAND_TAP_MID, -1, 0f);
    }

    public void UpdateHandMotionTapHold()
    {
        Invoke("HandRightFadeInMid", 0.5f);
        Invoke("HandRightTapHoldMid", 0.75f);
        Invoke("HandRightHoldEndMid", 2f);
    }

    public void HandRightFadeInMid()
    {
        handRight.enabled = true;
        handRight.Play(HAND_FADE_IN_MID);
    }

    public void HandRightTapHoldMid()
    {
        handRight.enabled = true;
        handRight.speed = HAND_ANIM_SLOW_SPEED;
        handRight.Play(HAND_HOLD_START_MID, -1, 0f);
    }

    public void HandRightHoldEndMid()
    {
        handRight.enabled = true;
        handRight.Play(HAND_HOLD_END_MID, -1, 0f);
    }

    public void UpdateHandMotionMegaSwipe()
    {
        HandLeftFadeInLeft();
        HandRightFadeInRight();
        Invoke("HandLeftMegaSwipe", 0.25f);
        Invoke("HandRightMegaSwipe", 0.25f);
    }

    public void HandLeftFadeInLeft()
    {
        handLeft.enabled = true;
        handLeft.Play(HAND_FADE_IN_LEFT);
    }

    public void HandRightFadeInRight()
    {
        handRight.enabled = true;
        handRight.Play(HAND_FADE_IN_RIGHT);
    }

    public void HandLeftMegaSwipe()
    {
        handLeft.enabled = true;
        handLeft.speed = HAND_ANIM_SLOW_SPEED;
        handLeft.Play(HAND_MEGA_SWIPE_RIGHT, -1, 0f);
    }

    public void HandRightMegaSwipe()
    {
        handRight.enabled = true;
        handRight.speed = HAND_ANIM_SLOW_SPEED;
        handRight.Play(HAND_MEGA_SWIPE_LEFT, -1, 0f);
    }

    public void UpdateHandMotionSwipe()
    {
        Invoke("HandRightFadeInMid", 3f);        
        Invoke("HandRightSwipe", 3.15f);        
        Invoke("HandRightSwipe", 4.75f);
    }

    public void HandRightSwipe()
    {
        handRight.enabled = true;
        handRight.speed = HAND_ANIM_SLOW_SPEED;
        handRight.Play(HAND_SWIPE_MID_TO_LEFT, -1, 0);
    }

    public void UpdateHandMotionDrag()
    {
        HandLeftFadeInRight();        
        Invoke("HandLeftTapHoldRight", 0.1f);
        Invoke("HandLeftDragRightToMid", 0.8f);
        Invoke("HandLeftHoldEndMid", 1.5f);
    }

    public void HandLeftFadeInRight()
    {
        handLeft.enabled = true;
        handLeft.Play(HAND_FADE_IN_RIGHT);
    }

    public void HandLeftTapHoldRight()
    {
        handLeft.speed = HAND_ANIM_SLOW_SPEED;
        handLeft.Play(HAND_HOLD_START_RIGHT, -1, 0f);
    }

    public void HandLeftDragRightToMid()
    {
        handLeft.Play(HAND_DRAG_RIGHT_TO_MID, -1, 0);
    }

    public void HandLeftHoldEndMid()
    {
        handLeft.Play(HAND_HOLD_END_MID, -1, 0f);
    }

    public void UpdateHandTemp(Note note)
    {
        switch (note.type)
        {
            case Define.NOTE_TYPE.SHORT:
                HandTap(note, HAND_ANIM_SLOW_SPEED);
                break;
            case Define.NOTE_TYPE.LONG:
                HandHoldStart(note, HAND_ANIM_SLOW_SPEED);
                break;

            case Define.NOTE_TYPE.MIGHTY_SWIPE:
                HandMegaSwipe(note, HAND_ANIM_SLOW_SPEED);
                break;

            case Define.NOTE_TYPE.SWIPE:
                HandSwipe(note, HAND_ANIM_SLOW_SPEED);
                break;

            case Define.NOTE_TYPE.DRAG:
                HandHoldStart(note, HAND_ANIM_SLOW_SPEED);
                break;
        }
    }

    public void UpdateHand(Note note)
    {
        if (!IsAutoCollect() && !IsKeepGoingState())
        {
            return;
        }
        switch (note.type)
        {
            case Define.NOTE_TYPE.SHORT:
                HandTap(note);
                break;

            case Define.NOTE_TYPE.LONG:
                if (note.isDragHeadNote)
                {
                    HandHoldStart(note);
                }
                else if (note.isDragTailNote)
                {
                    HandHoldEnd(note);
                }
                break;

            case Define.NOTE_TYPE.MIGHTY_SWIPE:
                HandMegaSwipe(note);
                break;

            case Define.NOTE_TYPE.SWIPE:
                HandSwipe(note);
                break;

            case Define.NOTE_TYPE.DRAG:
                if (note.isDragTailNote)
                {
                    HandHoldEnd(note);
                }
                else
                {
                    if (note.isDragHeadNote && !note.wasDragHeadNotePressed)
                    {
                        HandHoldStart(note);
                    }

                    if (note.DragRequire != Define.DRAG_DIRECTION.DRAG_UP)
                    {
                        HandDrag(note);
                    }
                }
                break;
        }
    }


    public void HandTap(Note note, float speed = HAND_ANIM_FAST_SPEED)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                handLeft.enabled = true;
                handLeft.speed = speed;
                handLeft.Play(HAND_TAP_LEFT, -1, 0f);
                break;
            case Define.LANE.ID_2:
                handLeft.enabled = true;
                handLeft.speed = speed;
                handLeft.Play(HAND_TAP_MID, -1, 0f);
                break;
            case Define.LANE.ID_3:
                handLeft.enabled = true;
                handLeft.speed = speed;
                handLeft.Play(HAND_TAP_RIGHT, -1, 0f);
                break;
            case Define.LANE.ID_4:
                handRight.enabled = true;
                handRight.speed = speed;
                handRight.Play(HAND_TAP_LEFT, -1, 0f);
                break;
            case Define.LANE.ID_5:
                handRight.enabled = true;
                handRight.speed = speed;
                handRight.Play(HAND_TAP_MID, -1, 0f);
                break;
            case Define.LANE.ID_6:
                handRight.enabled = true;
                handRight.speed = speed;
                handRight.Play(HAND_TAP_RIGHT, -1, 0f);
                break;
        }
    }

    void HandHoldStart(Note note, float speed = HAND_ANIM_FAST_SPEED)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                handLeft.enabled = true;
                handLeft.speed = speed;
                handLeft.Play(HAND_HOLD_START_LEFT, -1, 0f);
                break;
            case Define.LANE.ID_2:
                handLeft.enabled = true;
                handLeft.speed = speed;
                handLeft.Play(HAND_HOLD_START_MID, -1, 0f);
                break;
            case Define.LANE.ID_3:
                handLeft.enabled = true;
                handLeft.speed = speed;
                handLeft.Play(HAND_HOLD_START_RIGHT, -1, 0f);
                break;
            case Define.LANE.ID_4:
                handRight.enabled = true;
                handRight.speed = speed;
                handRight.Play(HAND_HOLD_START_LEFT, -1, 0f);
                break;
            case Define.LANE.ID_5:
                handRight.enabled = true;
                handRight.speed = speed;
                handRight.Play(HAND_HOLD_START_MID, -1, 0f);
                break;
            case Define.LANE.ID_6:
                handRight.enabled = true;
                handRight.speed = speed;
                handRight.Play(HAND_HOLD_START_RIGHT, -1, 0f);
                break;
        }
    }

    void HandHoldEnd(Note note)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                handLeft.Play(HAND_HOLD_END_LEFT, -1, 0f);
                break;
            case Define.LANE.ID_2:
                handLeft.Play(HAND_HOLD_END_MID, -1, 0f);
                break;
            case Define.LANE.ID_3:
                handLeft.Play(HAND_HOLD_END_RIGHT, -1, 0f);
                break;
            case Define.LANE.ID_4:
                handRight.Play(HAND_HOLD_END_LEFT, -1, 0f);
                break;
            case Define.LANE.ID_5:
                handRight.Play(HAND_HOLD_END_MID, -1, 0f);
                break;
            case Define.LANE.ID_6:
                handRight.Play(HAND_HOLD_END_RIGHT, -1, 0f);
                break;
        }
    }

    void HandMegaSwipe(Note note, float speed = HAND_ANIM_FAST_SPEED)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                {
                    handLeft.speed = speed;
                    handLeft.enabled = true;
                    handLeft.Play(HAND_MEGA_SWIPE_RIGHT, -1, 0f);
                }
                break;

            case Define.LANE.ID_3:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT)
                {
                    handLeft.speed = speed;
                    handLeft.enabled = true;
                    handLeft.Play(HAND_MEGA_SWIPE_LEFT, -1, 0f);
                }
                break;

            case Define.LANE.ID_4:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                {
                    handRight.speed = speed;
                    handRight.enabled = true;
                    handRight.Play(HAND_MEGA_SWIPE_RIGHT, -1, 0f);
                }
                break;

            case Define.LANE.ID_6:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT)
                {
                    handRight.speed = speed;
                    handRight.enabled = true;
                    handRight.Play(HAND_MEGA_SWIPE_LEFT, -1, 0f);
                }
                break;
        }
    }

    void HandSwipe(Note note, float speed = HAND_ANIM_FAST_SPEED)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                {
                    handLeft.speed = speed;
                    handLeft.enabled = true;
                    handLeft.Play(HAND_SWIPE_LEFT_TO_MID, -1, 0);
                }
                break;
            case Define.LANE.ID_2:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT)
                {
                    handLeft.speed = speed;
                    handLeft.enabled = true;
                    handLeft.Play(HAND_SWIPE_MID_TO_LEFT, -1, 0);
                }
                else if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                {
                    handLeft.speed = speed;
                    handLeft.enabled = true;
                    handLeft.Play(HAND_SWIPE_MID_TO_RIGHT, -1, 0);
                }
                break;
            case Define.LANE.ID_3:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT)
                {
                    handLeft.speed = speed;
                    handLeft.enabled = true;
                    handLeft.Play(HAND_SWIPE_RIGHT_TO_MID, -1, 0);
                }
                break;
            case Define.LANE.ID_4:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                {
                    handRight.speed = speed;
                    handRight.enabled = true;
                    handRight.Play(HAND_SWIPE_LEFT_TO_MID, -1, 0);
                }
                break;
            case Define.LANE.ID_5:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT)
                {
                    handRight.speed = speed;
                    handRight.enabled = true;
                    handRight.Play(HAND_SWIPE_MID_TO_LEFT, -1, 0);
                }
                else if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                {
                    handRight.speed = speed;
                    handRight.enabled = true;
                    handRight.Play(HAND_SWIPE_MID_TO_RIGHT, -1, 0);
                }
                break;
            case Define.LANE.ID_6:
                if (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT)
                {
                    handRight.speed = speed;
                    handRight.enabled = true;
                    handRight.Play(HAND_SWIPE_RIGHT_TO_MID, -1, 0);
                }
                break;
        }
    }

    void HandDrag(Note note)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
                {
                    handLeft.Play(HAND_DRAG_LEFT_TO_MID, -1, 0);
                }
                break;
            case Define.LANE.ID_2:
                if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_LEFT)
                {
                    handLeft.Play(HAND_DRAG_MID_TO_LEFT, -1, 0);
                }
                else if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
                {
                    handLeft.Play(HAND_DRAG_MID_TO_RIGHT, -1, 0);
                }
                break;
            case Define.LANE.ID_3:
                if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_LEFT)
                {
                    handLeft.Play(HAND_DRAG_RIGHT_TO_MID, -1, 0);
                }
                break;
            case Define.LANE.ID_4:
                if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
                {
                    handRight.Play(HAND_DRAG_LEFT_TO_MID, -1, 0);
                }
                break;
            case Define.LANE.ID_5:
                if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_LEFT)
                {
                    handRight.Play(HAND_DRAG_MID_TO_LEFT, -1, 0);
                }
                else if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
                {
                    handRight.Play(HAND_DRAG_MID_TO_RIGHT, -1, 0);
                }
                break;
            case Define.LANE.ID_6:
                if (note.DragRequire == Define.DRAG_DIRECTION.DRAG_LEFT)
                {
                    handRight.Play(HAND_DRAG_RIGHT_TO_MID);
                }
                break;
        }
    }

    public void FadeInHand(Note note)
    {
        switch ((Define.LANE)note.indexCSV)
        {
            case Define.LANE.ID_1:
                handLeft.enabled = true;
                handLeft.Play(HAND_FADE_IN_LEFT);
                break;
            case Define.LANE.ID_2:
                handLeft.enabled = true;
                handLeft.Play(HAND_FADE_IN_MID);
                break;
            case Define.LANE.ID_3:
                handLeft.enabled = true;
                handLeft.Play(HAND_FADE_IN_RIGHT);
                break;
            case Define.LANE.ID_4:
                handRight.enabled = true;
                handRight.Play(HAND_FADE_IN_LEFT);
                break;
            case Define.LANE.ID_5:
                handRight.enabled = true;
                handRight.Play(HAND_FADE_IN_MID);
                break;
            case Define.LANE.ID_6:
                handRight.enabled = true;
                handRight.Play(HAND_FADE_IN_RIGHT);
                break;
        }
    }

    public void SetHandLeftIdle()
    {
        handLeft.enabled = true;
        handLeft.Play(HAND_IDLE);        
    }

    public void SetHandRightIdle()
    {
        handRight.enabled = true;
        handRight.Play(HAND_IDLE);
    }

    public void FadeInHandLeft()
    {
        SpriteRenderer spriteRenderer = handLeft.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            handLeft.enabled = false;
            spriteRenderer.DOFade(1, TIME_HAND_FADE);
        }
    }

    public void FadeInHandRight()
    {
        SpriteRenderer spriteRenderer = handRight.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            handRight.enabled = false;
            spriteRenderer.DOFade(1, TIME_HAND_FADE);
        }
    }

    public void FadeOutHandLeft()
    {
        SpriteRenderer spriteRenderer = handLeft.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            handLeft.enabled = false;
            spriteRenderer.DOFade(0, TIME_HAND_FADE);
        }
    }

    public void FadeOutHandRight()
    {
        SpriteRenderer spriteRenderer = handRight.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            handRight.enabled = false;
            spriteRenderer.DOFade(0, TIME_HAND_FADE);
        }
    }

    public void ShowText(string text)
    {
        textIndicator.text = Localization.Instance.GetString(text);
        ShowText();
    }

    public void ShowTextDelay(string text, float delayTime = TIME_DELAY_TEXT)
    {
        HideText();
        textIndicator.text = Localization.Instance.GetString(text);
        Invoke("ShowText", delayTime);
    }

    public void ShowText()
    {
        textIndicator.gameObject.SetActive(true);
        GameUtils.Instance.UpdateTextUnderlay(textIndicator);
    }

    public void HideText()
    {
        textIndicator.gameObject.SetActive(false);
    }

    public void HideTextDelay(float delayTime)
    {
        Invoke("HideText", delayTime);
    }

    public Define.TUTORIAL Tutorial
    {
        get { return tutorial; }
        set { tutorial = value; }
    }

    public bool IsAutoCollect()
    {
        return (tutorial == Define.TUTORIAL.INGAME_TAP
            || tutorial == Define.TUTORIAL.INGAME_TAP_HOLD
            || tutorial == Define.TUTORIAL.INGAME_MIGHTY_SWIPE
        );
    }

    public void FadeOutAudio(float timeDelay)
    {
        AudioSource[] audioSources = GameManager.Instance.GetArrayAudioSrouce();
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null && audioSources[i].clip != null)
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[i], timeDelay, 1f, 0f));
            }
        }
    }

    public void FadeInAudio(float timeDelay)
    {
        AudioSource[] audioSources = GameManager.Instance.GetArrayAudioSrouce();
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null && audioSources[i].clip != null)
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[i], timeDelay, 0f, 1f));
            }
        }
    }

    public void PauseAudio(float delayTime)
    {
        Invoke("PauseAudio", delayTime);
    }

    public void PauseAudio()
    {
        AudioSource[] audioSources = GameManager.Instance.GetArrayAudioSrouce();
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null && audioSources[i].clip != null)
            {
                audioSources[i].Pause();
            }
        }
    }

    public void DelayFadeInAudio(float delayTime)
    {
        Invoke("FadeInAudio", delayTime);
    }

    public void ResumeAudio(float delayTime)
    {
        Invoke("ResumeAudio", delayTime);
    }

    public void ResumeAudio()
    {
        AudioSource[] audioSources = GameManager.Instance.GetArrayAudioSrouce();
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null && audioSources[i].clip != null)
            {
                audioSources[i].UnPause();
            }
        }
    }

    bool IsNextState()
    {
        if (GameManager.Instance.IsGamePaused() || GameManager.Instance.GetState() != Define.GAME_STATE.INGAME)
        {
            return false;
        }
        float time = listTimeState[listTimeState.Count - 1];
        if (listTimeState.Count > 0 && GameManager.Instance.GetTimeAudio((int)Define.AUDIOS_INDEX.VOCAL) > time)
        {
            listTimeState.RemoveAt(listTimeState.Count - 1);
            return true;
        }
        return false;
    }

    public bool IsKeepGoingState()
    {
        return (tutorial == Define.TUTORIAL.INGAME_USER_TAP
            || tutorial == Define.TUTORIAL.INGAME_USER_TAP_HOLD
            || tutorial == Define.TUTORIAL.INGAME_USER_MIGHTY_SWIPE
        );
    }

    public void UpdateHandUI(Button btn, Transform trans, int direction)
    {
        UpdateHandObjectUI(btn.gameObject, trans, direction);
    }

    public void UpdateHandObjectUI(GameObject obj, Transform trans, int direction)
    {
        SetOrderObject(obj);
        handControl = trans;
        originalPosition = trans.localPosition;
        handDirection = direction;
        ShowHand();
        Invoke("PlayHandAnim", 0.1f);
    }

    public void SetHighlightObject(GameObject obj)
    {
        SetOrderObject(obj);
        if (!lightBG.activeSelf)
        {
            lightBG.SetActive(true);
        }
    }

    void SetOrderObject(GameObject obj)
    {
        canvasObj = obj.GetComponent<Canvas>();
        if (canvasObj == null)
        {
            obj.AddComponent<Canvas>();
            obj.AddComponent<GraphicRaycaster>();
            canvasObj = obj.GetComponent<Canvas>();
            canvasObj.overrideSorting = true;
            canvasObj.sortingOrder = 1;
        }
        originalOrderLayer = canvasObj.sortingOrder;
        canvasObj.sortingOrder = handBG.GetComponent<Canvas>().sortingOrder + 1;
    }

    void HighlightSongSelect()
    {
        for (int i = 0; i < listBtnSongMode.Count; i++)
        {
            SetHighlightObject(listBtnSongMode[i]);
        }
    }

    public void UnSetHighlightObject(GameObject obj)
    {
        obj.GetComponent<Canvas>().sortingOrder = originalOrderLayer;
        if (lightBG.activeSelf)
        {
            lightBG.SetActive(false);
        }
    }

    public void SetObjectWithHand(GameObject obj, Transform trans)
    {
        canvasObj = obj.GetComponent<Canvas>();
        originalOrderLayer = canvasObj.sortingOrder;
        handControl = trans;
        originalPosition = trans.localPosition;
    }

    public void UpdateHandObjectUIVenue()
    {
        UpdateHandObjectUI(canvasObj.gameObject, handControl, LEFT);
    }

    public bool IsInGamePart1()
    {
        return ((tutorial < Define.TUTORIAL.INGAME_SCENARIO_END));
    }

    public bool IsNeedPause()
    {
        return (
            tutorial == Define.TUTORIAL.INGAME_SCENARIO_INTRO
            || tutorial == Define.TUTORIAL.INGAME_SCENARIO_END
            || tutorial == Define.TUTORIAL.INGAME_POPUP_INTRO
            || tutorial == Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR
            || tutorial == Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2
        );
    }

    void ShowPopup(string text, string textButton = "STR_NEXT", bool showBtnPrev = false)
    {
        UIPopup popupTutorial = UIPopup.GetPopup("PopupTutorial");
        popupTutorial.Show();
        SFXManager.Instance.Play(Define.SFX.UI_MESSAGE_POP_UP);
        popupTutorial.GetComponent<PopupTutorial>().SetTextContent(Localization.Instance.GetString(text));
        popupTutorial.GetComponent<PopupTutorial>().SetTextButton(Localization.Instance.GetString(textButton));
        popupTutorial.GetComponent<PopupTutorial>().ShowBtnPrev(showBtnPrev);

        switch (tutorial)
        {
            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR:
                popupTutorial.GetComponent<PopupTutorial>().ShowNotesActive();
                break;

            case Define.TUTORIAL.INGAME_POPUP_NOTE_COLOR_2:
                popupTutorial.GetComponent<PopupTutorial>().ShowNotesDeactive();
                break;
        }
    }

    public void PlayHandAnim()
    {
        Animator animator = handControl.GetComponent<Animator>();
        GameObject arrow = animator.transform.GetChild(0).gameObject;

        switch (handDirection)
        {
            case LEFT:
                animator.Play(ARROW_POINT_LEFT, -1, 0);
                break;

            case RIGHT:
                animator.Play(ARROW_POINT_RIGHT, -1, 0);
                break;

            case UP:
                animator.Play(ARROW_POINT_UP, -1, 0);
                break;

            case DOWN:
                animator.Play(ARROW_POINT_LEFT, -1, 0);
                break;
        }
    }

    void ActiveHand()
    {
        handBG.SetActive(true);
        handControl.gameObject.SetActive(true);
    }

    void ShowHand(float delay = 0.1f)
    {
        Invoke("ActiveHand", delay);
    }

    void HideHand()
    {
        if (handControl != null)
        {
            handBG.SetActive(false);
            DOTween.Kill(handControl);
            handControl.localPosition = originalPosition;
            handControl.gameObject.SetActive(false);
        }
    }

    void DeactiveHandBG()
    {
        handBG.SetActive(false);
    }

    void HideHandBG(float delay = 0.11f)
    {
        Invoke("DeactiveHandBG", delay);
    }


    public void SetButtonSongEasy(GameObject btnSongMode, Transform handSongMode)
    {
        btnSongModeEasy = btnSongMode;
        handSongModeEasy = handSongMode;
    }

    public void ShowScenario(string text, bool animText = false)
    {
        scenario.SetActive(true);
        Color c = imgScenario.color;
        c.a = 1;
        imgScenario.color = c;
        contentScenario.text = text;
        contentScenario.GetComponent<MultiUnderlay>().UpdateText();
        if (animText)
        {
            FadeInText();
        }
    }

    public void FadeInText()
    {
        GameUtils.Instance.SetColorText(contentScenario, 0);
        contentScenario.DOFade(1, 2f).OnUpdate(() => contentScenario.GetComponent<MultiUnderlay>().UpdateAlpha()).SetDelay(1f);
    }

    public void HideScenario(bool fadeOut = false)
    {
        if(fadeOut)
        {
            contentScenario.DOFade(0, 1f).OnUpdate(() => contentScenario.GetComponent<MultiUnderlay>().UpdateAlpha());
            imgScenario.DOFade(0, 1.5f).OnComplete(()=> scenario.SetActive(false));
        }
        else
        {
            scenario.SetActive(false);
        }
    }

    public void BackMainMenu()
    {
        GameEventMgr.SendEvent("ingame_to_waiting");
        LoadingManager.Instance.StartLoadingToExitAP(Define.VIEW.MAIN_MENU);        
    }

    public void BackSetting()
    {
        GameEventMgr.SendEvent("ingame_to_waiting");
        LoadingManager.Instance.StartLoadingToExitAP(Define.VIEW.SETTING);        
    }

    public void SkipTutorial()
    {
        Tutorial = Define.TUTORIAL.DONE;
        SetHandLeftIdle();
        SetHandRightIdle();
        HideText();
        HideScenario();
        try
        {
            Time.timeScale = 1;
            UIPopup.HidePopup("PopupTutorial");
        }
        catch (Exception) { }
        rockBar.SetActive(true);
        SFXManager.Instance.StopAll();
        ProfileMgr.Instance.Tutorial = (int)Define.TUTORIAL.DONE;
        ProfileMgr.Instance.Save(); 
    }

    public void SkipTutorialFromPause()
    {
        Game.Instance.IsDemoTutorial = Define.TUTORIAL_TYPE.NORMAL_TUTORIAL;
        Tutorial = Define.TUTORIAL.DONE;
        SetHandLeftIdle();
        SetHandRightIdle();
        HideText();
    }

    public void InitStateDelay(Define.TUTORIAL state, float duration = 0.2f)
    {
        int tmp = 0;
        DOTween.To(() => tmp, x => tmp = x, 10, duration).SetUpdate(true).OnComplete(() => { InitState(state); });
    }
}
