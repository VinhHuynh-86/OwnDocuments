using UnityEngine;
using Unity.RemoteConfig;
using Doozy.Engine.Nody;
using Doozy.Engine.UI;
using gameoptions;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System;

public enum INIT_STATE
{
    NONE = 0,
    LOAD_AUDIO,
    INIT_LANGUAGE,
    INIT_TRACKING,
    AGE_GATE,
    INIT_DATA,
    INIT_DOOZY,
    UPDATE_NEW_VERSION,
    LOAD_STUDIO_BAND,
    STUDIO,
    TUTORIAL,
    FINISHED
}


public class ViewInit : MonoBehaviour
{

    bool isNeedToOverrideRemoteConfig = false;
    public struct UserAttributes {}
    public struct AppAttributes {}
    public INIT_STATE mState;
    public Image fillProgressBar;
    public AudioSource audioSource;
    [SerializeField] private GraphController MainGraph;
    private string forceUpdateVersion = "0.0.0";
    private bool isUpdateProgress;
    private float percentProgress;

    //const
    const float WAI_TIME = 5f;
    float TIME_DELAY = 1f;
    const string LOADING_AUDIO_PATH = "SFX/Loading/";
    string[] LOADING_SONG_NAME = new string[]{
        "AOBT_LOADING",
        "CLTC_LOADING",
        "IWTB_LOADING",
        "KIQU_LOADING",
        "KYAL_LOADING_01",
        "KYAL_LOADING_02",
        "RGGA_LOADING",
        "SSOR_LOADING",
        "UNPR_LOADING",
        "WWRY_LOADING"
    };

    //private
    float timeControl;
    Timer waitTime = new Timer();
    AudioClip loadingAudioClip;

    void Awake()
    {
        ConfigManager.FetchCompleted += ApplyRemoteSettings;
        ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
    }

    void Start()
    {
    }

    public void ApplyRemoteSettings (ConfigResponse configResponse) {
        if(configResponse.status == ConfigRequestStatus.Failed)
        {
            return;
        }
        Debug.Log(" ApplyRemoteSettings():: configResponse.status = " + configResponse.status);

        switch (configResponse.requestOrigin) {
            case ConfigOrigin.Default:
            case ConfigOrigin.Cached:
                Debug.Log ("No settings loaded this session; using default values.");
           
                break;
            case ConfigOrigin.Remote:
                Debug.Log ("New settings loaded this session; update values accordingly. m_IsInitData: "+Game.Instance.m_IsInitData);

                if(!Game.Instance.m_IsInitData)
                {
                    Debug.Log(" ApplyRemoteSettings():: isNeedToOverrideRemoteConfig = true ");

                    isNeedToOverrideRemoteConfig = true;
                }
                else
                {
                    Debug.Log(" ApplyRemoteSettings():: call OverrideAllRemoteConfig");

                    OverrideAllRemoteConfig();
                }

                break;
        }
    }

    void OnEnable()
    {
        Game.Instance.SetViewState(Define.VIEW.INIT);
        SetState(INIT_STATE.LOAD_AUDIO);
        Game.Instance.m_IsInitData = false;
        waitTime.SetDuration(WAI_TIME);
    }

    void Update()
    {
        switch (mState)
        {
            case INIT_STATE.INIT_TRACKING:
#if !UNITY_EDITOR
                // waitTime.Update(Time.deltaTime);
                // if (PublishingSDKManager.Instance.IsSDKReady() || waitTime.JustFinished())
                {
                    SetState(INIT_STATE.INIT_DOOZY);
                }
#else
                SetState(INIT_STATE.INIT_DOOZY);
#endif                
                break;

            case INIT_STATE.INIT_DOOZY:
                if (MainGraph != null && MainGraph.Initialized)
                    SetState(INIT_STATE.INIT_DATA);
                break;

            case INIT_STATE.INIT_DATA:                  
                timeControl += Time.deltaTime;
                if(timeControl < TIME_DELAY || Game.Instance.m_IsInitData)
                {
                    return;
                }

                if(!Game.Instance.m_IsInitData)
                {
                    // Generate ID, Call Init ID only once
                    // CostumeManager.Instance.InitID();
                    // SpriteManager.Instance.InitID();
                    // finished cloud Save load system, load from cloud to local.
                    ProfileMgr.Instance.Init();                    
                    Game.Instance.m_IsInitData = true;
                    if(isNeedToOverrideRemoteConfig)
                    {
                        Debug.Log(" Update():: call OverrideAllRemoteConfig");

                        OverrideAllRemoteConfig();
                        ScoreManager.Instance.Init();
                    }
                    SpriteManager.Instance.LoadRewardSprite();
                }
                {
                    if (TutorialManager.Instance.Tutorial == (int)Define.TUTORIAL.START)
                    {
                        SetState(INIT_STATE.TUTORIAL);
                    }
                    else
                    {
                        SetState(INIT_STATE.STUDIO);
                    }
                }
                break;

            case INIT_STATE.FINISHED:
                break;
        }
        UpdateBackKey();
        UpdateProgress();
    }

    void SetState(INIT_STATE state)
    {
        mState = state;
        switch (mState)
        {
            case INIT_STATE.LOAD_AUDIO:
                audioSource.volume = 1f;
                int randAudioIndex = UnityEngine.Random.Range(0, LOADING_SONG_NAME.Length);
                loadingAudioClip = Resources.Load<AudioClip>(LOADING_AUDIO_PATH + LOADING_SONG_NAME[randAudioIndex]);
                SetState(INIT_STATE.INIT_LANGUAGE);
                break;
            case INIT_STATE.INIT_LANGUAGE:
                ProfileMgr.Instance.InitSettings();

                Localization.Instance.Init();
                Game.Instance.InitSoundSetting();
                SetState(INIT_STATE.INIT_DATA);
                break;
                
            case INIT_STATE.INIT_DATA:
                timeControl = 0;
                if (!Game.Instance.m_IsInitData)
                {
                    Database.LoadData();
                    GameOptions.LoadProfiles();
                }
                audioSource.PlayOneShot(loadingAudioClip);
                break;

            case INIT_STATE.INIT_DOOZY:
                break;

            case INIT_STATE.STUDIO:
                percentProgress = 1;
                GameEventMgr.SendEvent("goto_mainmenu");
                SetState(INIT_STATE.FINISHED);
                break;

            case INIT_STATE.TUTORIAL:                
                Game.Instance.SetStadiumIndex(4); // Stadium Submit
                Game.Instance.SetSelectedSongDifficultyInfo("SD000", 0, Define.GAME_MODE.EASY);
                LoadingManager.Instance.StartLoadingToEnterAP();
                UnloadInit();
                break;

            case INIT_STATE.FINISHED:
                UnloadInit();
                break;
        }
    }

    void UnloadInit()
    {
        audioSource.DOFade(0f, 0.4f).OnComplete(() => {
            Resources.UnloadAsset(loadingAudioClip);
            Destroy(audioSource.gameObject);
        });
    }

    void OverrideAllRemoteConfig()
    {
        string concertVenueRemoteConfig = ConfigManager.appConfig.GetString(Define.CONCERTVENUE_SHEET);
        // Debug.Log ("ApplyRemoteSettings concertVenueRemoteConfig: "+concertVenueRemoteConfig);
        Database.OverrideDataConcertVenue(concertVenueRemoteConfig);

        string songDifficultRemoteConfig = ConfigManager.appConfig.GetString(Define.SONGDIFFICULTY_CONFIG_SHEET_1);
        Database.OverrideDataSongDifficulty(songDifficultRemoteConfig, Define.SONGDIFFICULTY_CONFIG_SHEET_1);
        string songDifficultRemoteConfig_2 = ConfigManager.appConfig.GetString(Define.SONGDIFFICULTY_CONFIG_SHEET_2);
        Database.OverrideDataSongDifficulty(songDifficultRemoteConfig_2, Define.SONGDIFFICULTY_CONFIG_SHEET_2);

        string rockMeterConfig = ConfigManager.appConfig.GetString(Define.ROCKMETER_SHEET);
        // Debug.Log ("ApplyRemoteSettings rockMeterConfig: "+rockMeterConfig);
        ScoreManager.Instance.SetRockMeterConfig(rockMeterConfig);

        forceUpdateVersion = ConfigManager.appConfig.GetString("ForceUpdateVersion");
    }

    public void UpdateBackKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AndroidToast.ShowCannotBack();
        }
    }

    public bool IsUpdateNewVersion()
    {
        string currentVersion = Application.version;
        currentVersion = currentVersion.Replace(".", "");
        forceUpdateVersion = forceUpdateVersion.Replace(".", "");
        if(forceUpdateVersion == "")
        {
            return false;
        }
        
        try
        {
            int version = int.Parse(currentVersion);
            int newVersion = int.Parse(forceUpdateVersion);
            if(version < newVersion)
            {
                UIPopup popup = UIPopup.GetPopup(Define.POPUP_UPDATE);
                popup.Show();
                return true;
            }
        }catch (Exception) { return false; }

        return false;
    }

    public void UpdateProgress()
    {
        if(!isUpdateProgress)
        {
            return;
        }

        percentProgress += 0.02f;
        fillProgressBar.fillAmount = percentProgress;
	}
}
