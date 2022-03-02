using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Doozy.Engine.UI;
using UnityEngine.Audio;
using System;
// #if UNITY_IOS
//     using Unity.Notifications.iOS;
// #endif

public class Game : MonoBehaviour
{
    #region Instance
    private static Game instance;
    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Game>();
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    [HideInInspector]
    public string[] levelDesign;
    [HideInInspector]
    public string namefile;
    public float timeMusicStart = 0f;
    public float speedNote = 3f;    
    public bool m_IsInitData = false;
    public GameObject gamePlay;
    public GameObject gameView;
    public DisableTouch disableTouch;
    public ViewMainMenu viewMainMenu;
    // public ViewDownloadDLC viewDownloadDLC;

    public Camera gameCamera;
    public AudioMixer masterMixer;
    public AudioMixer masterDoozyMixer;
    public ItemSprites spritesVenue;
    public bool isCheckShowRatingPopup = false;
    public bool isShowRatingPopup = false;
    public float fadeDuration;
    public GameObject objInit;

    //private variables
    //private SongInfo curSong = null;
    private int currentLevel = 0;
    private Define.GAME_MODE gameMode = Define.GAME_MODE.EASY;
    private int mCurStadiumIndex = -1;
    private int mCurSongDiffIndex = -1;
    private int mSongIdxInStadium;
    private string venueID;
    private string mCurSongDiffID = "SD001";
    private int mSongIndex = 0;
    SongInfo songInfo ;
    [SerializeField]
    private Define.VIEW viewState = Define.VIEW.NONE;
    [SerializeField]
    private Define.VIEW preViewState = Define.VIEW.NONE;

    private Timer timerInitSDK = new Timer();
    private Timer timerCanShowAds = new Timer();

    bool firstEnterStudio = true;

    private string archiveFactID;

    private Define.INGAME_MODE ingameMode = Define.INGAME_MODE.FREE_VERSION;

    private Define.TUTORIAL_TYPE isDemoTutorial = Define.TUTORIAL_TYPE.NORMAL_TUTORIAL;
    public Define.TUTORIAL_TYPE IsDemoTutorial
    {
        get { return isDemoTutorial; }
        set { isDemoTutorial = value; }
    }
    bool isFirstlaunch = false;
    public bool IsFirstlaunch
    {
        get { return isFirstlaunch; }
        set { isFirstlaunch = value; }
    }
    private Define.VIEW redirectionView = Define.VIEW.NONE;
    public Define.VIEW RedirectionView
    {
        get { return redirectionView; }
        set { redirectionView = value; }
    }
    // static List<string> AgeLimit = new List<string> { "IT", "CN", "RU" };
    static List<string> Age13Limit = new List<string> { "US", "AU", "NZ", "CX", "CC", "NF", "CK", "NU", "TK", "AS", "GU", "MP", "PR", "VI", "UM"};
    static List<string> Age16Limit = new List<string> { 
        "EU", "BE", "BG", "CZ", "DK", "DE", "EE", "IE", "EL", "ES", "FR", "HR", "IT", "CY", "LV", "LT", "LU", "HU", "MT", "NL", "AT", "PL", "PT", "RO", "SI", "SK", "FI", "SE", "GR", "BM", "GI",
        "AX", "CP", "GF", "PF", "TF", "GP", "MQ", "YT", "NC", "RE", "BL", "MF", "PM", "VF", "LI", "AW", "BQ", "CW", "SX", "NO", "SJ", "IB", "IC", "EA", "GB",
        "AI", "BM", "AQ", "IO", "VG", "KI", "DG", "FK", "IS", "GG", "IM", "JE", "MS", "PN", "SH", "GS", "TC", "WF", "KY", "*N/A"
        };
    static List<string> countryCOPPA_CCPA = new List<string> { 
        "US", "AS", "UM", "GU", "MP", "PR", "VI"
        };
    static List<string> countryOFT_16 = new List<string> { 
        "CX", "CC", "NF", "CK", "NU", "TK"
        };
    static List<string> countryOFT_13 = new List<string> { 
        "AU", "NZ"
        };
    static List<string> countryGDPR = new List<string> { 
        "BE", "BG", "CZ", "DK", "DE", "EE", "IE", "ES", "FR", "HR", "IT", "CY", "LV", "LT", "LU", "HU", "MT", "NL", "AT", "PL", "PT", "RO", "SI", "SK", "FI", "SE", "GR", "BM", "GI",
        "AX", "CP", "GF", "PF", "TF", "GP", "MQ", "YT", "NC", "RE", "BL", "MF", "PM", "LI", "AW", "BQ", "CW", "SX", "NO", "SJ", "IB", "IC", "EA", "GB",
        "AI", "BM", "AQ", "IO", "VG", "DG", "FK", "IS", "GG", "IM", "JE", "MS", "PN", "SH", "GS", "TC", "WF", "KY", "*N/A"
        };

#if UNITY_IPHONE
    [DllImport("__Internal")]
    extern static public string IOSgetPhoneCountryCode();
#endif

    bool isInterrupted = false;
    public void InterruptGame(bool value)
    {
        isInterrupted = value;
    }
    public bool IsInterruptGame()
    {
        return isInterrupted;
    }
    void Awake()
    {
    #if ENABLE_CHEAT
        Debug.unityLogger.logEnabled = true;
    #else
        Debug.unityLogger.logEnabled = false;
    #endif
        timerInitSDK.SetDuration(10f);
        timerCanShowAds.SetDuration(10 * 60);//set limit 10 mins for Ads
        timerCanShowAds.SetTimerDone();
    }
    void Start()
    {
        EnableTouch();
        DeactiveGamePlay();

        songInfo = new SongInfo();
        mCurStadiumIndex = 0;
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IPHONE
        if(!IsNoInternet())
        {
            timerInitSDK.Update(Time.deltaTime);
            if(timerInitSDK.JustFinished())
            {
                // SetPublishingSDKLanguage();
                timerInitSDK.Reset();
            }
        }
#endif
        timerCanShowAds.Update(Time.deltaTime);
    }

    public string VenueID
    {
        get { return venueID; }
        set { venueID = value; }
    }

    public int SongIndex
    {
        get { return mSongIndex; }
        set { mSongIndex = value; }
    }

    public void DeactiveGamePlay()
    {
        gamePlay.SetActive(false);
    }

    public void StartGamePlay()
    {
        BaseCamera.Instance.ActiveBaseCamera();
        BaseCamera.Instance.InsertGameViewcameraInStack(gameCamera);
        LoadGameData();
        if(TutorialManager.Instance.Tutorial == Define.TUTORIAL.START)
        {
            GameManager.Instance.SetState(Define.GAME_STATE.SCENARIO);
            TutorialManager.Instance.UpdateState();
        }
        else
        {
            GameManager.Instance.SetState(Define.GAME_STATE.INIT);
        }
        LoadingManager.Instance.HideLoading();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void SetGameInfo()
    {
        SongDifficulty songDiff = Database.GetSongDifficultyByID(GetSongDifficultyID());
        SongsInVenues songsInVenue = Database.GetSongByID(songDiff.songID);        
        songInfo.songName = songsInVenue.songName;
        songInfo.difficultFile = songDiff.levelDesign;
        songInfo.audios = songsInVenue.audios.ToArray();
        songInfo.colors = songsInVenue.colors.ToArray();
        
        SetGameMode(songDiff.difficult);
        GameData.SetInfo(songInfo);
    }

    public void LoadGameData()
    {
        GameData.LoadLevel();
    }

    public void LoadNextGameData(int index, bool isLoadSong = true)
    {
        mCurSongDiffIndex = index;
        SongDifficulty songDiff = Database.GetSongDifficulty()[index];
        mCurSongDiffID = songDiff.ID;
        SongsInVenues songsInVenue = Database.GetSongByID(songDiff.songID);
        if(isLoadSong)
        {
            songInfo.songName = songsInVenue.songName;
            songInfo.difficultFile = songDiff.levelDesign;
            songInfo.audios = songsInVenue.audios.ToArray();
            songInfo.colors = songsInVenue.colors.ToArray();
            SetGameMode(songDiff.difficult);
            GameData.SetInfo(songInfo);
        }
        else
        {
            GameData.GetInfo().beats.Clear();
            GameData.GetInfo().difficultFile = songDiff.levelDesign;
            SetGameMode(songDiff.difficult);
        }
        GameData.LoadLevel(isLoadSong);
    }

    public void SetSelectedSongDifficultyInfo(string ID, int songIdxInStadium, Define.GAME_MODE _gameMode)
    {
        mCurSongDiffID = ID;
        List<SongDifficulty> songDiff = Database.GetSongDifficulty();
        int count = songDiff.Count;
        for(int i = 0; i < count; ++i)
        {
            if(songDiff[i].ID == ID)
            {
                mCurSongDiffIndex = i;
            }
        }
        mSongIdxInStadium = songIdxInStadium;
        gameMode = _gameMode;
    }

    public string GetSongDifficultyID()
    {
        return mCurSongDiffID;
    }

    public int GetSongDifficultyIndex()
    {
        return mCurSongDiffIndex;
    }

    public int GetSongIndexInStadium()
    {
        return mSongIdxInStadium;
    }

    public string GetCurrentSongName()
    {
        return songInfo.songName;
    }

    public void SetGameMode(Define.GAME_MODE _gameMode)
    {
        gameMode = _gameMode;
    }

    public Define.GAME_MODE GetGameMode()
    {
        return gameMode;
    }

    public void SetStadiumIndex(int stadiumIndex)
    {
        mCurStadiumIndex = stadiumIndex;
        venueID = Database.GetConcertVenues()[stadiumIndex].ID;
    }

    public int GetCurrStadiumIndex()
    {
        return mCurStadiumIndex;
    }

    public void OnClickUnlockGame()
    {
        // IAPManager.Instance.BuyUnlockGame();
    }

    public void UnlockGameSucess()
    {
        GameEventMgr.SendEvent("iap_back_main_menu");
        ProfileMgr.Instance.IsGameUnlocked = true;
        ProfileMgr.Instance.Save();
        // if(IAPManager.IsBuying)
        // {
        //     IAPManager.IsBuying = false;
        //     SFXManager.Instance.Play(Define.SFX.UI_MESSAGE_POP_UP);
        // }
    #if UNITY_IOS
        // else
        {
            UIPopup popup = UIPopup.GetPopup(Define.POPUP_OK);
            popup.Show();
            PopupOK popupOK = popup.GetComponent<PopupOK>();
            popupOK.SetTextContent("STR_RESTORE_PURCHASE_SUCCESS");
        }
    #endif
    }

    public void UnlockGameFail()
    {
        // if(IAPManager.IsBuying)
        // {
        //     IAPManager.IsBuying = false;
        //     UIPopup popup = UIPopup.GetPopup(Define.POPUP_OK);
        //     popup.Show();
        //     PopupOK popupOK = popup.GetComponent<PopupOK>();
        //     popupOK.SetTextContent("STR_IAP_ERROR");
        // }
    #if UNITY_IOS
        // else
        {
            UIPopup popup = UIPopup.GetPopup(Define.POPUP_OK);
            popup.Show();
            PopupOK popupOK = popup.GetComponent<PopupOK>();
            popupOK.SetTextContent("STR_RESTORE_PURCHASE_FAIL");
        }
    #endif
    }

    public bool IsBuyingIAP()
    {
        return true;
        // return IAPManager.IsBuying;
    }

    public void SetViewState(Define.VIEW state)
    {
        preViewState = viewState;
        viewState = state;
    }

    public Define.VIEW GetViewState()
    {
        return viewState;
    }

    public Define.VIEW GetPreViewState()
    {
        return preViewState;
    }

    public void BackPreViewState()
    {
        Define.VIEW tmp = viewState;
        viewState = preViewState;
        preViewState = tmp;
    }

    public string GetTextGameMode()
    {
        string text = "";
        switch(gameMode)
        {
            case Define.GAME_MODE.EASY:
                text = Localization.Instance.GetString("STR_EASY");
            break;
            case Define.GAME_MODE.MEDIUM:
                text = Localization.Instance.GetString("STR_MEDIUM");
            break;
            case Define.GAME_MODE.HARD:
                text = Localization.Instance.GetString("STR_HARD");
            break;
        }
        return text;
    }

    public string GetTextSongName()
    {
        if(songInfo != null && songInfo.songName != null)
        {
            return Localization.Instance.GetString(songInfo.songName);
        }
        return "";
    }

    public bool IsApplyKidRule(int age)
    {
        return IsApplyKidRuleCOPPA_CCPA(age)
            || IsApplyKidRuleOFT(age)
            || IsApplyKidRuleGDPR(age)
            || IsApplyKidRule_RotW(age);
    }

    public bool IsApplyKidRuleCOPPA_CCPA(int age)
    {
        string countryCode = GetCountryCode();
        return age < 13 && (countryCOPPA_CCPA.Contains(countryCode));
    }

    public bool IsApplyKidRuleOFT(int age)
    {
        string countryCode = GetCountryCode();
        return (age < 13 && (countryOFT_13.Contains(countryCode))) || (age < 16 && (countryOFT_16.Contains(countryCode)));
    }

    public bool IsApplyKidRuleGDPR(int age)
    {
        string countryCode = GetCountryCode();
        return age < 16 && (countryGDPR.Contains(countryCode));
    }

    public bool IsApplyKidRuleRussian(int age)
    {
        string countryCode = GetCountryCode();
        return age < 18 && (countryCode == "RU");
    }

    public bool IsApplyKidRule_RotW(int age)
    {
        string countryCode = GetCountryCode();
        return age < 13
         && !countryCOPPA_CCPA.Contains(countryCode)
         && !countryOFT_13.Contains(countryCode)
         && !countryGDPR.Contains(countryCode);
    }

    public bool CheckCOPPA(int age)
    {
        string countryCode = GetCountryCode();
        // if (AgeLimit.Contains(countryCode))
        // {
        //     return true;
        // }
        // else 
        if (age < 13 && Age13Limit.Contains(countryCode))
        {
            return true;
        }
        else if (age < 16 && Age16Limit.Contains(countryCode))
        {
            return true;
        }
        return false;
    }

    public string GetCountryCode()
    {
        string result = "US";
#if UNITY_EDITOR
#elif UNITY_ANDROID       
        using(AndroidJavaClass cls = new AndroidJavaClass("java.util.Locale"))
        {
            if (cls != null)
            {
                using (AndroidJavaObject locale = cls.CallStatic<AndroidJavaObject>("getDefault"))
                {
                    if (locale != null)
                    {
                        result = locale.Call<string>("getCountry");
                    }
                    else
                    {
                        Debug.Log("locale null");
                    }
                }
            }
            else
            {
                Debug.Log("cls null");
            }
        }
#elif UNITY_IPHONE
        result = IOSgetPhoneCountryCode();
#endif
        return result.ToUpper();
    }

    public void OnDLCError(Define.DLC_ERROR_TYPE errorType)
    {
        // viewDownloadDLC.OnDLCError(errorType);
    }

    public bool IsFirstEnterStudio()
    {
        return firstEnterStudio;
    }
    public void UpdateFirstEnterStudio(bool value)
    {
        firstEnterStudio = value;
    }

    public void SetActiveGamView(bool isActive)
    {
        if(!gameView.activeSelf && isActive)
        {
            gameView.SetActive(true);
        }

        if(gameView.activeSelf && !isActive)
        {
            gameView.SetActive(false);
        }
    }

    public void InitSoundSetting()
    {
        if(masterMixer != null)
        {
            masterMixer.SetFloat("MasterVolume", ProfileMgr.Instance.MusicVolume*80 - 80);
            masterDoozyMixer.SetFloat("MasterDoozyVolume", ProfileMgr.Instance.SoundEffects*80 - 80);
        }
    }

    public void DisableTouch(bool hasLoadingIcon = false)
    {
        disableTouch.SetEnable(true, hasLoadingIcon);
    }
    public void EnableTouch()
    {
        disableTouch.SetEnable(false);
    }
    public bool HasAnyTouchOnBlank()
    {
        if(disableTouch != null)
        {
            return disableTouch.HasAnyTouch();
        }
        return false;
    }

    public bool IsDisablingTouch()
    {
        return disableTouch.IsEnable();
    }

    public void SetIngameMode(Define.INGAME_MODE mode)
    {
        ingameMode = mode;
    }

    public Define.INGAME_MODE GetIngameMode()
    {
        return ingameMode;
    }


    public bool IsNoInternet()
    {
        return Application.internetReachability == NetworkReachability.NotReachable;
    }

    public string ArchiveFactID
    {
        get { return archiveFactID; }
        set { archiveFactID = value; }
    }

    public void ResetIngameMode()
    {
        ingameMode = Define.INGAME_MODE.NONE;
    }


    // IEnumerator RequestAuthorization()
    // {
    // #if UNITY_IOS
    //     using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
    //     {
    //         while (!req.IsFinished)
    //         {
    //             yield return null;
    //         };

    //         // string res = "\n RequestAuthorization: \n";
    //         // res += "\n finished: " + req.IsFinished;
    //         // res += "\n granted :  " + req.Granted;
    //         // res += "\n error:  " + req.Error;
    //         // res += "\n deviceToken:  " + req.DeviceToken;
    //         // Debug.Log(res);
    //     }
    // #else
    //     yield return null;
    // #endif
    // }

    public void DestroyObjInit()
    {
        if(objInit != null)
        {
            // Unload Splash sprite
            Resources.UnloadAsset(objInit.transform.GetChild(0).GetComponent<Image>().sprite);
            Destroy(objInit);
        }
    }


    public void UpdateInfoMainMenu()
    {
        viewMainMenu.UpdateInfoGameObject();
    }
}
