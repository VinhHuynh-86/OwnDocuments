using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.Soundy;
using DG.Tweening;
using System;

[System.Serializable]
public class SfxClip
{
    public Define.SFX key;
    public AudioClip clip;
}

public class SFXManager : MonoBehaviour
{
    //public
    public static SFXManager Instance;
    public SfxClip[] listSfxClip;    

    public AudioSource asSFX;
    public AudioSource asSFXBackGround;
    public AudioSource asMusic;
    [HideInInspector]
    public AudioClip musicClip = null;

    //Private
    private Define.SFX[] ARRAY_CROWD_BOO = {Define.SFX.CROWD_BOO_01, Define.SFX.CROWD_BOO_02, Define.SFX.CROWD_BOO_03};
    private int indexCrowdBoo = 0;
    private Define.SFX[] ARRAY_CROWD_CHEER = {Define.SFX.CROWD_CHEER_01, Define.SFX.CROWD_CHEER_02, Define.SFX.CROWD_CHEER_03};
    private int indexCrowdCheer = 0;  
    private Dictionary<Define.SFX, AudioClip> dicSfx = new Dictionary<Define.SFX, AudioClip>();
    private bool isPausingCrowdSFX = false;
    private bool isPausingBGSFX = false;
    private List<Timer> failedTimer = new List<Timer>();
    Timer cheerTimer = new Timer();
    private AudioClip bgMusicClip = null;
    private bool isSkip = false;
    
    //Constant
    private const string BG_MUSIC_LOOP_PATH = "SFX/ActionPhrase/";
    private const string SFX_DATABASE = "General";//sfx database name was set in Doozy database (Tools/Doozy/Control Panels/Soundy)
    private const string SFX_LOOP_DATABASE = "Loop";//sfx database name was set in Doozy database (Tools/Doozy/Control Panels/Soundy)
    private const string SFX_TAP_FAIL_DATABASE = "TapFail";
    private const float TIME_WASTE_CHEER = 5f;
    private const float TIME_FADE_BOO = 2f;
    private const float TIME_FAILED_COOLDOWN = 5f;

    int MAX_MISTAKE = 6;

    int mistakesNumber;

    void Awake()
    {        
        if(Instance == null)
        {
            Instance = this;
        }

        foreach(SfxClip sfxClip in listSfxClip)
        {
            dicSfx.Add(sfxClip.key, sfxClip.clip);
        }
        mistakesNumber = 0;
    }

    void Start()
    {
        for(int i = 0; i < (int)Define.AUDIOS_INDEX.BACK; i++)
        {
            Timer mTimer = new Timer();
            mTimer.SetDuration(TIME_FAILED_COOLDOWN);
            failedTimer.Add(mTimer);
        }
        cheerTimer.SetDuration(TIME_FAILED_COOLDOWN);
    }

    void Update()
    {
        for(int i = 0; i < failedTimer.Count; i++)
        {
            failedTimer[i].Update(Time.deltaTime);
        }
        cheerTimer.Update(Time.deltaTime);        
    }

    private void FadeVolumeIn(float duration, float from, float to)
    {
        StartCoroutine(AudioFadeEffect.FadeVolumeIn(asSFX, duration, from, to));
    }

    public void FadeVolumeOut(float duration, float to)
    {
        float from = asSFX.volume;
        FadeVolumeOut(duration, from, to);
    }

    private void FadeVolumeOut(float duration, float from, float to)
    {
        StartCoroutine(AudioFadeEffect.FadeVolumeOut(asSFX, duration, from, to));
    }

    public void FadeVolumeOutBackGround(float duration, float to)
    {
        float from = asSFXBackGround.volume;
        FadeVolumeOut(asSFXBackGround, duration, from, to);
    }

    public void FadeVolumeOut(AudioSource audioSource, float duration, float from, float to)
    {
        StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSource, duration, from, to));
    }

    public void SetNoneSkip()
    {
        isSkip = false;
    }

    public void Play(Define.SFX key, bool loop = false)
    {
        if(isSkip)
        {
            return;
        }

        if(key == Define.SFX.SWIPE_ROW_CHANGE && !isSkip)
        {
            isSkip = true;            
            Invoke("SetNoneSkip", 0.5f);
        }
        string sfxDatabase = loop? SFX_LOOP_DATABASE : SFX_DATABASE;
        SoundyManager.Play(sfxDatabase, key.GetStringValue());
    }
    
    public void Play(string audioName)
    {
        //play audio directly by name from database
        SoundyManager.Play(SFX_DATABASE, audioName);
    }

    public void PlayBackGround(Define.SFX key, bool isLoop = false)
    {
        if(bgMusicClip != null)
        {
            Resources.UnloadAsset(bgMusicClip);
            bgMusicClip = null;
        }

        bgMusicClip = Resources.Load<AudioClip>(BG_MUSIC_LOOP_PATH + key.GetStringValue());
        asSFXBackGround.clip = bgMusicClip;//dicSfx[key];
        SetVolume(asSFXBackGround, ProfileMgr.Instance.SoundEffects);
        asSFXBackGround.loop = isLoop;
        asSFXBackGround.Play();
    }

    public void FadeInBG(Define.SFX key, float duration, bool isLoop)
    {
        if(bgMusicClip != null)
        {
            asSFXBackGround.Stop();
            Resources.UnloadAsset(bgMusicClip);
            bgMusicClip = null;
        }

        bgMusicClip = Resources.Load<AudioClip>(BG_MUSIC_LOOP_PATH + key.GetStringValue());
        asSFXBackGround.clip = bgMusicClip;//dicSfx[key];
        asSFXBackGround.volume = 0;
        asSFXBackGround.loop = isLoop;
        asSFXBackGround.Play();
        asSFXBackGround.DOFade(ProfileMgr.Instance.SoundEffects, duration);
    }

    public void Stop()
    {
        asSFX.Stop();
        mistakesNumber = 0;
    }

    public void StopBG()
    {        
        asSFXBackGround.Stop();
        Resources.UnloadAsset(bgMusicClip);
        bgMusicClip = null;
    }

    public void FadeOutBG()
    {
        asSFXBackGround.DOFade(0f, 1f).OnComplete(() => {
            asSFXBackGround.Stop();
            Resources.UnloadAsset(bgMusicClip);
            bgMusicClip = null;
        });
    }

    public void StopAll()
    {
        Stop();
        StopBG();
        StopMusic();
    }

    public bool IsSFXCrowdPlaying()
    {
        return asSFX.isPlaying;
    }

    public void PlayCrowdEnd()
    {                                
        Play(Define.SFX.CROWD_CHEER_END);
    }

    public void AddMistakes()
    {
        if(TutorialManager.Instance.Tutorial < Define.TUTORIAL.INGAME_SCENARIO_END
            || Game.Instance.GetGameMode() != Define.GAME_MODE.HARD
        )
        {
            return;
        }
        
        mistakesNumber++;
        if(mistakesNumber == MAX_MISTAKE)
        {
            mistakesNumber = 0;
            PlayOutRandomCrowdBoo();
        }
    }
    public void ResetMistakes()
    {
        mistakesNumber = 0;
    }

    public void PlayOutRandomCrowdBoo()
    {
        //block during first tutorial
        if(TutorialManager.Instance.Tutorial < Define.TUTORIAL.INGAME_SCENARIO_END)
        {
            return;
        }

        if(cheerTimer.IsDone() 
        && !(IsSFXCrowdPlaying() && IsClipCrowdBoo())
        )
        {
            asSFX.clip = dicSfx[ARRAY_CROWD_BOO[indexCrowdBoo]];        
            indexCrowdBoo++;
            if(indexCrowdBoo >= ARRAY_CROWD_BOO.Length)
            {
                indexCrowdBoo = 0;
            }
            // FadeVolumeOut(TIME_FADE_BOO, 1f, 0f);
            asSFX.Play();
        }
    }

    bool IsClipCrowdBoo()
    {
        for(int i = 0; i < ARRAY_CROWD_BOO.Length; i++)
            if(asSFX.clip == dicSfx[ARRAY_CROWD_BOO[i]])
                return true;
        return false;
    }
    
    public void PlayRandomCrowdCheer()
    {
        if(IsSFXCrowdPlaying())
        {
            return;
        }
        asSFX.clip = dicSfx[ARRAY_CROWD_CHEER[indexCrowdCheer]];
        indexCrowdCheer++;
        if(indexCrowdCheer >= ARRAY_CROWD_CHEER.Length)
        {
            indexCrowdCheer = 0;
        };
        asSFX.Play();
        cheerTimer.Reset();
    }

    public void FadeInCrowdCheer(float duration)
    {
        asSFX.Stop();
        asSFX.clip = dicSfx[ARRAY_CROWD_CHEER[indexCrowdCheer]];
        indexCrowdCheer++;
        if(indexCrowdCheer >= ARRAY_CROWD_CHEER.Length)
        {
            indexCrowdCheer = 0;
        };
        asSFX.volume = 0;
        asSFX.Play();
        asSFX.DOFade(ProfileMgr.Instance.SoundEffects, duration);
    }

    public bool IsFinished(Define.SFX id, float timeWaste = 0)
    {
        if(asSFX.clip == null)
        {
            return false;
        }
        return asSFX.time + timeWaste >= asSFX.clip.length;
    }

    public void PauseAllSFX()
    {
        // AudioListener.pause = true;
        if(asSFX.isPlaying)
        {
            isPausingCrowdSFX = true;
            asSFX.Pause();
        }
        if(asSFXBackGround.isPlaying)
        {
            isPausingBGSFX = true;
            asSFXBackGround.Pause();
        }
        SoundyManager.StopAllControllers();
    }

    public void ResumeAllSFX()
    {
        // AudioListener.pause = false;
        if(isPausingCrowdSFX)
        {
            isPausingCrowdSFX = false;
            asSFX.UnPause();
        }
        if(isPausingBGSFX)
        {
            isPausingBGSFX = false;
            asSFXBackGround.UnPause();
        }
    }

    static void SetVolume(AudioSource audioSource, float value)
    {
        audioSource.volume = value;
    }

    public void PlayTapFail(Define.AUDIOS_INDEX audioIdx)
    {
        int randIndex = UnityEngine.Random.Range(1, 5);
        string audioName = "";
        switch(audioIdx)
        {
            case Define.AUDIOS_INDEX.DRUM:
                audioName = "sfx_tap_fail_drums_0"+randIndex;
                
            break;
            case Define.AUDIOS_INDEX.BASS:
                audioName = "sfx_tap_fail_bass_0"+randIndex;
                SoundyManager.Play(SFX_TAP_FAIL_DATABASE, audioName);
            break;
            case Define.AUDIOS_INDEX.GUITAR:
                audioName = "sfx_tap_fail_guitar_0"+randIndex;
            break;
            case Define.AUDIOS_INDEX.VOCAL:
                audioName = "sfx_tap_fail_vocal_0"+randIndex;
            break;
        }

        int soundIndex = (int)audioIdx;
        if(audioName != "" && failedTimer[soundIndex].IsDone())
        {
            failedTimer[soundIndex].Reset();
            SoundyManager.Play(SFX_TAP_FAIL_DATABASE, audioName);
        }
    }

    public void LoadClipMusic(string path)
    {
        if(musicClip == null)
        {
            musicClip = Resources.Load<AudioClip>(path);
        }
    }

    public void PlayMusic()
    {
        asMusic.volume = ProfileMgr.Instance.MusicVolume;
        asMusic.clip = musicClip;
        asMusic.Play();
    }

    public void StopMusic()
    {
        if(asMusic != null)
        {
            asMusic.Stop();
        }
    }

    public void UnloadMusic()
    {
        if(musicClip != null && TutorialManager.Instance.Tutorial >= Define.TUTORIAL.INGAME_SCENARIO_END)
        {
            Resources.UnloadAsset(musicClip);
            musicClip = null;
        }
    }

    public void PlayLondon1974()
    {
        asSFX.clip = Resources.Load<AudioClip>("SFX/lic_mm_studio/m_lic_sfx_London_1974");
        asSFX.Play();
    }

    public void StopSoundyManager()
    {
        SoundyManager.StopAllControllers();
    }
}
