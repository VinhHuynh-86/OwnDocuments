using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using Doozy.Engine.UI;

public class ViewSetting : MonoBehaviour
{
    public AudioMixer masterMixer;
    public AudioMixer masterDoozyMixer;
    [SerializeField] GameObject LanguageContainer;
    [SerializeField] Slider Music;
    [SerializeField] Slider SoundEffects;
    [SerializeField] GameObject HapticFeedbackOnBtn;
    [SerializeField] GameObject HapticFeedbackOffBtn;
    [SerializeField] GameObject NotificationsOnBtn;
    [SerializeField] GameObject NotificationsOffBtn;
    [SerializeField] GameObject LoginOnBtn;
    [SerializeField] GameObject LoginOffBtn;
    [SerializeField] GameObject ImageLoginOnBtn;
    [SerializeField] GameObject ImageLoginOffBtn;
    [SerializeField] GameObject HapticFeedbackContainer;
    [SerializeField] TextMeshProUGUI LanguageTxt;
    [SerializeField] TextMeshProUGUI osTxt;
    [SerializeField] TextMeshProUGUI accountLinkTxt;
    [SerializeField] Sprite ActiveSprite;
    [SerializeField] Sprite InactiveSprite;
    [SerializeField] AudioSource audioSourceMusic;
    [SerializeField] AudioSource audioSourceGFX;
    [SerializeField] Scrollbar scrollbarVertical;
    [SerializeField] Button btnTutorialBasic;
    [SerializeField] Button btnTutorialAdvance;

    int oldLang;
    bool isNeedSave = false;
    bool isUpdateLang = false;
    bool isFirst = true;
    private float timeBackKeyActive;
    const float TIME_BACK_KEY_ACTIVE = 0.5f;
    string[] ARRAY_LANGUAGE = new string[14]
    {
        "STR_ENGLISH",
        "STR_FRENCH",
        "STR_GERMAN",
        "STR_INDON",
        "STR_ITALIAN",
        "STR_JAPANESE",
        "STR_KOREAN",
        "STR_PORTUGUESE",
        "STR_RUSSIAN",
        "STR_SIMP_CHINESE",
        "STR_SPANISH",
        "STR_THAI",
        "STR_TRAD_CHINESE",
        "STR_TURKISH",
    };


    void Start()
    {
        audioSourceMusic.clip = Resources.Load<AudioClip>("Music/Previews/AKOM");
        if(SystemInfo.deviceModel.Contains("iPad"))
            HapticFeedbackContainer.SetActive(false);
    }

    void OnEnable()
    {
        if(isFirst)
        {
            isFirst = false;
            return;
        }
        scrollbarVertical.value = 1;
        LanguageContainer.SetActive(false);
        oldLang = ProfileMgr.Instance.Language;
        Music.value = ProfileMgr.Instance.MusicVolume*80 - 80;
        SoundEffects.value = ProfileMgr.Instance.SoundEffects*80 - 80;
        HapticFeedbackOnBtn.SetActive(ProfileMgr.Instance.Vibration);
        HapticFeedbackOffBtn.SetActive(!ProfileMgr.Instance.Vibration);
        
        LanguageTxt.text = Localization.Instance.GetString(ARRAY_LANGUAGE[ProfileMgr.Instance.Language >= 0? ProfileMgr.Instance.Language : 0]);
        Game.Instance.SetViewState(Define.VIEW.SETTING);
        isNeedSave = false;
        if(TutorialManager.Instance)
        {
            TutorialManager.Instance.UpdateState();
            btnTutorialBasic.interactable = (TutorialManager.Instance.tutorial == Define.TUTORIAL.DONE);
            btnTutorialAdvance.interactable = (TutorialManager.Instance.tutorial == Define.TUTORIAL.DONE);
        }
    }

    void OnDisable()
    {
        if(isNeedSave && Game.Instance && Game.Instance.m_IsInitData)
        {
            ProfileMgr.Instance.SaveSetting();
        }
        scrollbarVertical.value = 1;
        Resources.UnloadAsset(audioSourceMusic.clip);
    }

    void Update()
    {
        if(isUpdateLang && !LanguageContainer.activeSelf)
        {
            isUpdateLang = false;
            LanguageTxt.text = Localization.Instance.GetString(ARRAY_LANGUAGE[ProfileMgr.Instance.Language >= 0? ProfileMgr.Instance.Language : 0]);
        }
        

        UpdateBackKey();
    }

    public void OnHomeClick()
    {
        GameEventMgr.SendEvent("go_back");
    }

    public void MusicSliderChanged(float newValue)
    {
        SetVolume(masterMixer, "MasterVolume", (newValue + 80)/80);
    }

    public void SoundEffectsSliderChanged(float newValue)
    {
        SetVolume(masterDoozyMixer, "MasterDoozyVolume", (newValue + 80)/80);
    }

    void SetVolume(AudioMixer mixer, string name, float value)
    {
        float db = 20.0f * (value < 0.001f ? -4.0f : Mathf.Log10(value));
        mixer.SetFloat(name, db);
    }

    public void OnHapticFeedbackOnClick()
    {
        ProfileMgr.Instance.Vibration = false;
        HapticFeedbackOnBtn.SetActive(false);
        HapticFeedbackOffBtn.SetActive(true);
    }

    public void OnHapticFeedbackOffClick()
    {
        ProfileMgr.Instance.Vibration = true;
        HapticFeedbackOnBtn.SetActive(true);
        HapticFeedbackOffBtn.SetActive(false);
    }
    
    public void OnLanguageClick()
    {
        isUpdateLang = true;
        LanguageContainer.SetActive(true);
    }
    
    public void OnTutorialBasicClick()
    {
        btnTutorialBasic.interactable = false;
        TutorialManager.Instance.Tutorial = Define.TUTORIAL.BASIC_LOADING;
        Game.Instance.IsDemoTutorial = Define.TUTORIAL_TYPE.BASIC_TUTORIAL_REPLAY;
        Game.Instance.SetStadiumIndex(4);
        Game.Instance.SetSelectedSongDifficultyInfo("SD000", 0, Define.GAME_MODE.EASY);
        LoadingManager.Instance.StartLoadingToEnterAP();
    }
    
    public void OnTutorialAdvancedClick()
    {
        btnTutorialAdvance.interactable = false;
        TutorialManager.Instance.Tutorial = Define.TUTORIAL.DONE;
        Game.Instance.IsDemoTutorial = Define.TUTORIAL_TYPE.ADVANCED_TUTORIAL_REPLAY;
        Game.Instance.SetStadiumIndex(0);
        Game.Instance.SetSelectedSongDifficultyInfo("SD004", 1, Define.GAME_MODE.EASY);
        LoadingManager.Instance.StartLoadingToEnterAP();
    }

    
    public void OnBackFromLanguageClick()
    {
        // Wait finish anim button
        Invoke("HideLanguageScreen", 0.4f);
    }

    public void HideLanguageScreen()
    {
        LanguageContainer.SetActive(false);
    }

    public void OnSetSprite(Image imageBtn)
    {
        oldLang = ProfileMgr.Instance.Language;
        imageBtn.sprite = ActiveSprite;
    }

    public void UpdateBackKey()
    {
        if(timeBackKeyActive < TIME_BACK_KEY_ACTIVE)
        {
            timeBackKeyActive += Time.unscaledDeltaTime;
        }
        if(UIPopup.AnyPopupVisible)
            timeBackKeyActive = 0;
        if (Input.GetKeyDown(KeyCode.Escape) && timeBackKeyActive > TIME_BACK_KEY_ACTIVE)
        {
            SFXManager.Instance.Play(Define.SFX.UI_MENU_BACK);
            if(LanguageContainer.activeSelf)          
            {
                LanguageContainer.SetActive(false);
            }
            else
            {
                GameEventMgr.SendEvent("go_back");
            }
        }
    }

    public void StartSoundMusicSlider()
    {
        audioSourceMusic.Play();
    }

    public void StopSoundMusicSlider()
    {
        isNeedSave = true;
        ProfileMgr.Instance.MusicVolume = (Music.value + 80)/80;
        audioSourceMusic.Stop();
    }

    public void StartSoundGFXSlider()
    {
        audioSourceGFX.Play();
    }

    public void StopSoundGFXSlider()
    {
        isNeedSave = true;
        ProfileMgr.Instance.SoundEffects = (SoundEffects.value + 80)/80;
        audioSourceGFX.Stop();
    }

}
