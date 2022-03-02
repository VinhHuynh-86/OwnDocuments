using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Doozy.Engine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System;
using gameoptions;

[System.Serializable]
public class SpeedControll
{
    public float startTime;
    public float endTime;
    public float speed;
}

[System.Serializable]
public class ChacractersStadium
{
    public string stadiumID;
    public GameObject stadium;
    public GameObject characters;
}

public class GameManager : MonoBehaviour
{
    #region Instance
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    instance = new GameObject("GameManager", typeof(GameManager)).GetComponent<GameManager>();
                }
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    public static bool HasInstance()
    {
        return instance != null;
    }

    public BeatManager beatManager;
    public CameraShake cameraShake;
    public Camera gameCamera;

    public Timer timerControl = new Timer();

    [Range(1f, 10f)]
    public float speedNote;
    [Range(10f, 20f)]
    public float maxDuration = 20f;
    public float TIME_DELAY_AUDIO_TRACK = 0f;
    public float TIME_FADE_OUT_AUDIO = 1f;
    public float TIME_INTRO = 7f;
    public float AUDIO_VOLUME_IN_AP = 1f;
    public float HALF_VOLUME = 0.3f;

    float missingTime = -1;
    bool isMissingNote = false;
    public List<AudioSource> listAudioSource;
    AudioSource[] audioSources;
    public UnityEngine.Audio.AudioMixer audioMixer;
    bool wasAudiosPlayed = false;

    float dspSongTime;
    float duration;
    List<LongNote> dragNotes;
    List<LongNote> dragNotes2;
    double[] songPositions;

    public float currentTime;
    public SpeedControll[] speedControlls;

    public List<Note> mightySwipeNotes;
    public List<int> visibleLinesIndex;
    public int linesNumber;
    public ViewInGame viewInGame;
    public ViewPauseMenu viewPauseMenu;

    [HideInInspector]
    public List<float> listTimeMissNote;
    public SceneLoader sceneLoaderIngame;
    //private variables
    [SerializeField]
    Define.GAME_STATE state;
    public Define.GAME_SUB_STATE subState = Define.GAME_SUB_STATE.NONE;
    private GameObject curStadium = null;
    private bool isGamePaused = false;
    private float[] arrTimeCounter;
    private double totalInterruptTime;
    private double interruptTime;
    private double starInterruptTime;
    private bool isReleaseAll = false;
    private float POS_SHOW_NOTE = 27f;
    private float TIME_AUTO_COLLECT = 3f;
    private float TIME_END_AUTO_COLLECT = 0.1f;
    private float timeControlEndAutoCollect = 0;
    private int PLAY_PARTICLE_LOOP_MAX = 3;

    private bool isCheckEndGame = false;
    private float timeControlEndGame = 0f;
    double durationTimelineOfSong = 0;
    double durationOfSong = 0;
    private int[] TrackingNoteInfo =
    {
        0, // normal all
        0, // normal missed
        0, // normal success cool
        0, // normal success great
        0, // normal success perfect
        0, // long all
        0, // long missed
        0, // long success
        0, // long success
        0, // long success
        0, // Drag all
        0, // Drag missed
        0, // Drag success
        0, // Drag success
        0, // Drag success
        0, // swipe all
        0, // swipe missed
        0, // swipe success
        0, // swipe success
        0, // swipe success
        0, // Mighty Swipe all
        0, // Mighty Swipe missed
        0, // Mighty Swipe success
        0, // Mighty Swipe success
        0, // Mighty Swipe success
    };
    int highestPerfect = 0;
    int stageFireParticleCount = 0;

    public List<Note>[] noteList;
    public Dictionary<Define.COLORS, int> colorCounter;
    List<BeatInfo> beatInfos;
    private bool isStartGame = false;
    bool isReset;
    private List<ParticleStageController> stageParticleList = new List<ParticleStageController>();
    public bool finishInit;

    bool canSpawnNote = false;
    bool isFadeOut = false;

    Define.CHARACTERS mainVocal = Define.CHARACTERS.FREDDIE_MERCURY;

    public float GetDurationAudio(int index)
    {
        if(audioSources != null)
        {
            if(audioSources[index] != null && audioSources[index].clip != null)
            {
                return audioSources[index].clip.length;
            }
            else if(audioSources[(int)Define.AUDIOS_INDEX.BACK] != null && audioSources[(int)Define.AUDIOS_INDEX.BACK].clip != null && audioSources[(int)Define.AUDIOS_INDEX.BACK].time > 0) // 5 = audio6: merge other audios together
            {
                return audioSources[(int)Define.AUDIOS_INDEX.BACK].clip.length;
            }
            else
            {
                return audioSources[(int)Define.AUDIOS_INDEX.VOCAL].clip.length;
            }
        }
        else
        {
            return 0;
        }
    }

    public float GetTimeAudio(int index)
    {
        if(audioSources != null)
        {
            if(audioSources[index] != null)
            {
                return audioSources[index].time;
            }
            else if(audioSources[(int)Define.AUDIOS_INDEX.BACK] != null && audioSources[(int)Define.AUDIOS_INDEX.BACK].clip != null && audioSources[(int)Define.AUDIOS_INDEX.BACK].time > 0) // 5 = audio6: merge other audios together
            {
                return audioSources[(int)Define.AUDIOS_INDEX.BACK].time;
            }
            else
            {
                return audioSources[(int)Define.AUDIOS_INDEX.VOCAL].time;
            }
        }
        else
        {
            return 0;
        }
    }
    public double GetSongPosition(int index)
    {
        double time = 0;
        if(audioSources == null)
        {
            time = 0;
        }
        else if(audioSources[index]!= null && audioSources[index].clip != null && audioSources[index].time > 0)
        {           
            time =  AudioSettings.dspTime - songPositions[index] - totalInterruptTime;
        }
        else if(audioSources[(int)Define.AUDIOS_INDEX.BACK] != null && audioSources[(int)Define.AUDIOS_INDEX.BACK].clip != null && audioSources[(int)Define.AUDIOS_INDEX.BACK].time > 0) // 5 = audio6: merge other audios together
        {
            time = AudioSettings.dspTime - songPositions[(int)Define.AUDIOS_INDEX.BACK] - totalInterruptTime;
        }
        else //if(songPositions[(int)Define.AUDIOS_INDEX.VOCAL] > 0)
        {
            // audio3, index 2, lead vocal
            time = AudioSettings.dspTime - songPositions[(int)Define.AUDIOS_INDEX.VOCAL] - totalInterruptTime;
        }
        return time;
    }

    public double GetBeatPosition(int index)
    {
        return GetSongPosition(index) + duration;
    }

    void Awake()
    {
        isStartGame = false;
    }

    // Start is called before the first frame update
    public void InitData(bool isReplay = false)
    {
        ResetData(isReplay);

        Game.Instance.SetActiveGamView(true);

        if(listTimeMissNote == null)
        {
            listTimeMissNote = new List<float>();
        }
        if(visibleLinesIndex == null)
        {
            visibleLinesIndex = new List<int>();
        }

        linesNumber = ActivatorsManager.Instance.activators3D.Length;
        noteList = new List<Note>[linesNumber];
        for (int i = 0; i < linesNumber; ++i)
        {
            noteList[i] = new List<Note>();
        }
        colorCounter = new Dictionary<Define.COLORS, int>();
        for(Define.COLORS key = Define.COLORS.CYAN; key <= Define.COLORS.YELLOW; ++key)
        {
            colorCounter[key] = 0;
        }

        beatInfos = GameData.GetBeatInfo();
        if (beatInfos != null && beatInfos.Count != 0)
        {
            for (int i = 0, len = beatInfos.Count; i < len; i++)
            {
                BeatInfo beatInfo = beatInfos[i];                
                beatInfo.indexBeat = 0;
            }
        }
        if(mightySwipeNotes == null)
        {
            mightySwipeNotes = new List<Note>();
        }

        if (GameData.GetAudioInfo() != null)
        {
            int len = GameData.GetAudioInfo().Length;
            songPositions   = new double[len];
            for(int v = 0 ; v < songPositions.Length; ++v)
            {
                songPositions[v] = 0;
            }
            arrTimeCounter  = new float[len];
            dragNotes       = new List<LongNote>();
            dragNotes2      = new List<LongNote>();
            ResetTimeCounter();
        }
        Array.Clear(TrackingNoteInfo, 0, TrackingNoteInfo.Length);
        highestPerfect = 0;

        SongsInVenues song = Database.GetSongByDifficultyID(Game.Instance.GetSongDifficultyID());
        mainVocal = song.mainVocal;

        ScoreManager.Instance.Init();
        viewInGame.Init();
        InitializeLines();

        SetState(Define.GAME_STATE.INTRO);
    }
    
    public void ResetData(bool isReplay = false)
    {
        StopAllIngameParticles();

        canSpawnNote = false;
        
        StopAudios();

        songPositions = null;
        arrTimeCounter = null;

        if(SFXManager.Instance != null)
        {
            if(TutorialManager.Instance.Tutorial != Define.TUTORIAL.INGAME_TAP)
                SFXManager.Instance.Stop();
        }

        if(BeatManager.Instance != null)
        {
            BeatManager.Instance.Reset();
        }
        if(listTimeMissNote != null)
        {
            listTimeMissNote.Clear();
        }

        if(dragNotes != null && dragNotes.Count > 0)
        {
            dragNotes.Clear();
            dragNotes = null;
        }
        if(dragNotes2 != null && dragNotes2.Count > 0)
        {
            dragNotes2.Clear();
            dragNotes2 = null;
        }
        if(mightySwipeNotes != null && mightySwipeNotes.Count > 0)
        {
            mightySwipeNotes.Clear();
            mightySwipeNotes = null;
        }
        if(visibleLinesIndex != null && visibleLinesIndex.Count > 0)
        {
            visibleLinesIndex.Clear();
            visibleLinesIndex = null;
        }
        if(listTimeMissNote != null && listTimeMissNote.Count > 0)
        {
            listTimeMissNote.Clear();
            listTimeMissNote = null;
        }
        if(!isReplay)
        {
            if(beatInfos != null && beatInfos.Count > 0)
            {
                // beatInfos = GameData.GetBeatInfo();
                GameData.ClearBeatInfo();
                beatInfos.Clear();
                beatInfos = null;
            }
        }
        if(colorCounter != null && colorCounter.Count > 0)
        {
            colorCounter.Clear();
            colorCounter = null;
        }
        SetSpeedNote(); 
        duration = maxDuration / speedNote;

        isGamePaused = false;
        isStartGame = false;
        isCheckEndGame = false;
        isReset = false;
        isReleaseAll = false;
        currentTime = 0;
        interruptTime = 0;
        starInterruptTime = 0;
        totalInterruptTime = 0;

        timeControlEndGame = 0;
        missingTime = -1;
        isMissingNote = false;
        isFadeOut = false;
#if UNITY_EDITOR
        Game.Instance.timeMusicStart = 0;
#endif
        subState = Define.GAME_SUB_STATE.NONE;
    }

    public void ResetAndInitData()
    {
        SFXManager.Instance.StopBG();
        isReset = true;
        if(noteList != null && noteList.Length > 0)
        {
            for(int i = 0; i < noteList.Length; ++i)
            {
                int count = noteList[i].Count;
                for(int j = count - 1; j > -1; --j)
                {
                    noteList[i][j].ResetAndDestroyMe();
                }
                //noteList[i] = null;
            }
            //noteList = null;
        }
        StartCoroutine(DoResetData(0.5f));  
    }

    IEnumerator DoResetData(float seconds)
    {
        // waiting SECOND to make sure all note are killed in Note.LateUpdate()
        yield return new WaitForSeconds(seconds);
        InitData(true);
    }

    public void ResetAndDestroyNotes()
    {
        isReset = true;
        if(noteList != null && noteList.Length > 0)
        {
            for(int i = 0; i < noteList.Length; ++i)
            {
                int count = noteList[i].Count;
                for(int j = count - 1; j > -1; --j)
                {
                    noteList[i][j].ResetAndDestroyMe();
                }
                //noteList[i] = null;
            }
            //noteList = null;
        }
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    public Define.COLORS GetMostColor()
    {
        int count = 0;
        Define.COLORS color = Define.COLORS.GREEN;
        foreach(Define.COLORS key in colorCounter.Keys)
        {
            if(key >= Define.COLORS.CYAN && key <= Define.COLORS.YELLOW && count < colorCounter[key])
            {
                count = colorCounter[key];
                color = key;
            }
        }

        return color;
	}
    
    public Dictionary<Define.COLORS, int> GetColorCounter()
    {
        return colorCounter;
	}


    private void InitializeLines()
    {
        visibleLinesIndex.Clear();

        int length = linesNumber;
        for (int i = 0; i < length; ++i)
        {
            visibleLinesIndex.Add(i);
        }

        AppearLanesData[] appearLanesData;
        appearLanesData = GameData.GetAppearLanesData();

        BoardGameManager.Instance.InitializeNotes();
        if (appearLanesData != null)
        {
            length = appearLanesData.Length;
            if (length > 0 && appearLanesData[length - 1].linkToNote == false)
            {
                int linesSize = appearLanesData[length - 1].AppearLanesList.Count;
                for (int i = 0; i < linesSize; ++i)
                {
                    int lineIndex = appearLanesData[length - 1].AppearLanesList[i];
                    if (lineIndex < 0)
                    {
                        lineIndex *= -1;
                        lineIndex--;						
                        visibleLinesIndex.Remove(lineIndex);
                    }
                }
            }
        }
        
        ActivatorsManager.Instance.InitializeActivators();
        beatManager.Initialize();        
    }
    public void UpdateLinesStatus(int index) //index of AppearLanesData
    {
        AppearLanesData[] appearLanesData;
        appearLanesData = GameData.GetAppearLanesData();

        int linesSize = appearLanesData[index].AppearLanesList.Count;
        for (int i = 0; i < linesSize; ++i)
        {
            int lineIndex = appearLanesData[index].AppearLanesList[i];
            if (lineIndex > 0) // add lines
            {
                lineIndex--;
                if (!visibleLinesIndex.Contains(lineIndex))
                {
                    visibleLinesIndex.Add(lineIndex);
                }
            }
            else // remove lines
            {
                lineIndex *= -1;
                lineIndex--;

                if (visibleLinesIndex.Contains(lineIndex))
                {				
                    visibleLinesIndex.Remove(lineIndex);
                }
            }
        }

        
        {
            visibleLinesIndex.Sort();
            UpdateLines();
            RealignLines();
        }
    }

    public void UpdateLines()
    {
        ActivatorsManager.Instance.UpdateActivatorsStatus();
    }

    public void RealignLines()
    {
        BeatManager.Instance.RealignNotes();
    }

    public void InitAudioSources()
    {
        if (GameData.GetAudioInfo() != null)
        {
            if(audioSources != null && audioSources.Length > 0)
            {
                audioSources = null;
            }

            int len = GameData.GetAudioInfo().Length;
            audioSources = new AudioSource[len];
            int i = 0;
            string musicMixerGroup = "";
            foreach (AudioClip audioClip in GameData.GetAudioInfo())
            {
                AudioSource audioSource = listAudioSource[i];
                audioSource.clip = audioClip;
                audioSource.playOnAwake = false;
                audioSource.loop = false;

                audioSources[i] = (audioSource);

                for(int j = 0; j < Define.SONG_NAME.Length; j++)
                {
                    if(musicMixerGroup == "" && audioClip != null && audioClip.name.Contains(Define.SONG_NAME[j]))
                    {
                        musicMixerGroup = Define.SONG_NAME[j];
                    } 
                }
                audioSources[i].outputAudioMixerGroup = audioMixer.FindMatchingGroups(musicMixerGroup)[0];
                
                i++;
            }
        }
    }
    
    void SetAudioPosition(float durationToStartAudio = 0)
    {
        dspSongTime = (float)AudioSettings.dspTime;
        if (audioSources != null)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                songPositions[i] = dspSongTime + durationToStartAudio;
            }
        }
    }

    IEnumerator PlayAudioCoroutine(float deplay)
    {
        yield return null;
        if (audioSources != null)
        {
            wasAudiosPlayed = true;
            SetAudioPosition(deplay - (float)totalInterruptTime);
            dspSongTime = (float)AudioSettings.dspTime;

            for (int i = 0; i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];

                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.volume = AUDIO_VOLUME_IN_AP;
                    audioSource.PlayScheduled(dspSongTime + deplay);
                }                
            }
            
        }
        yield return null;
    }

    public void PlayAudio(float deplay)
    {
        if(!wasAudiosPlayed)
        {
            wasAudiosPlayed = true;
            StartCoroutine(PlayAudioCoroutine(deplay));
        }
    }

    public void StopAudios()
    {
        if(audioSources != null)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if(audioSources[i] != null && audioSources[i].clip != null)
                {
                    audioSources[i].Stop();
                }
            }
            SFXManager.Instance.Stop();            
            wasAudiosPlayed = false;
        }
    }

    public bool IsActiveLine(int index)
    {
        if (visibleLinesIndex.Contains(index))
        {
            return true;
        }
        return false;
    }

    private void ResetTimeCounter()
    {
        if(arrTimeCounter != null)
        {
            for (int i = 0; i < arrTimeCounter.Length; i++)
            {
                arrTimeCounter[i] = -1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStartGame || isGamePaused) return;

        switch (state)
        {
            case Define.GAME_STATE.SCENARIO:
            break;
            case Define.GAME_STATE.INTRO:
                UpdateIntro();
                break;

            case Define.GAME_STATE.INGAME:
                GameUpdate();
                break;
            case Define.GAME_STATE.CANCEL:
                break;
            case Define.GAME_STATE.ENDGAME:
                UpdateEndGame();
                UpdateStageFireParticle();
                break;
            case Define.GAME_STATE.RESULT:
                Game.Instance.SetActiveGamView(false); 
                break;
        }

        if (Define.IsShowDebugInfo && viewInGame && currentTime >= 0)
        {
            viewInGame.UpdateSongTimerText(currentTime);
        }
    }

    void UpdateIntro()
    {
        if(TutorialManager.Instance.IsNeedPause())
        {
            return;
        }
        timerControl.Update(Time.deltaTime);
        if(timerControl.JustFinished())
        {
            Debug.Log("*** UpdateIntro ***");
            state = Define.GAME_STATE.INGAME;
            SetAudioPosition(duration);
        }
    }

    Note CloneNote(Note other, int lineIndex)
    {
        Note note = beatManager.Generate(lineIndex, other.duration);
        if (note != null)
        {
            note.CopyFrom(other, lineIndex);
            noteList[lineIndex].Add(note);
            colorCounter[note.GetOriginalColor()]++;
            note.Calculate(); 
        }
        return note;
    }

    void SpawnNote(BeatData beatData, int beatInfoIdx, float beatPosition, float startTime, float remainDuration, int lineIndex)
    {
        Note note = beatManager.Generate(lineIndex, remainDuration);
        if (note != null)
        {
            note.Reset();
            note.longNote.Reset();
            // note.cornerLine.Reset();
            note.longNote.Initialize();
            // note.cornerLine.Initialize();
            if(beatData.isDragTailNote)
            {
                note.tailEnd.Initialize();
            }
            note.Spawn(beatData, beatInfos[beatInfoIdx], startTime, beatPosition, lineIndex, linesNumber);

            //LongNote longnote = note.longNote;
            note.longNote.isHead = note.isDragHeadNote;
            note.longNote.isTail = note.isDragTailNote;
            note.DragRequire = beatData.dragDirection;
            noteList[lineIndex].Add(note);
            colorCounter[note.GetOriginalColor()]++;
            
            if (beatData.isDragBeat)
            {
                if (beatData.dragNotesKey < 0)
                {
                    if (dragNotes.Count > 1 && dragNotes[dragNotes.Count - 1].isTail && beatData.startTime > dragNotes[dragNotes.Count - 1].parentNote.startTime)
                    {
                        dragNotes.Clear();
                    }

                    dragNotes.Add(note.longNote);
                    if (beatData.dragDirection == Define.DRAG_DIRECTION.DRAG_UP)
                    {
                        note.longNote.next = beatManager.GetTemplate(note.indexCSV).longNote;

                    }
                    else if (beatData.isDragTailNote)
                    {
                        dragNotes[dragNotes.Count - 1].next = null;
                    }

                    if (dragNotes.Count > 1)
                    {
                        UpdateDragNotes(Define.LANE_ON_SIDE.LEFT); // on left side
                    }
                }

                if (beatData.dragNotesKey > 0)
                {
                    if (dragNotes2.Count > 1 && dragNotes2[dragNotes2.Count - 1].isTail && beatData.startTime > dragNotes2[dragNotes2.Count - 1].parentNote.startTime)
                    {
                        dragNotes2.Clear();
                    }

                    dragNotes2.Add(note.longNote);
                    if (beatData.dragDirection == Define.DRAG_DIRECTION.DRAG_UP)
                    {
                        note.longNote.next = beatManager.GetTemplate(note.indexCSV).longNote;

                    }
                    else if (beatData.isDragTailNote)
                    {
                        dragNotes2[dragNotes2.Count - 1].next = null;
                    }

                    if (dragNotes2.Count > 1)
                    {
                        UpdateDragNotes(Define.LANE_ON_SIDE.RIGHT); // on right side
                    }
                }
            }
            if (!beatManager.IsEnableNote(note.indexAudio))
            {
                note.Color = Define.COLORS.GRAY;
            }
            note.Calculate();
        }
    }
    void GameUpdate()
    {
        if(!isStartGame)
        {
            return;
        }

        if(GameData.GetFirstBeatTime() > duration)
        {
            if(!canSpawnNote)
            {
                if(!WasAudioPlayed())
                {
                    Debug.Log("0. Play song");
                    // MarkAudioWasPlayed(); 
                    SetAudioPosition(0);
                    PlayAudio(0);
                }
                if(GetTimeAudio((int)Define.AUDIOS_INDEX.VOCAL) >= (GameData.GetFirstBeatTime() - duration))
                {
                    canSpawnNote = true;
                }
            }
        }
        else
        {
            canSpawnNote = true;
        }

        currentTime = GetTimeAudio((int)Define.AUDIOS_INDEX.VOCAL); // audio3, index 2, lead vocal


        // if(isMissingNote && TutorialManager.Instance.Tutorial == Define.TUTORIAL.DONE)
        // {
        //     missingTime += Time.deltaTime;
        //     if(!isCheckEndGame && missingTime > Define.MISSING_TIME && currentTime + Define.LAST_DURATION_TO_SKIP_CONCERT_CANCEL < GameData.GetLastBeatTime())
        //     {
        //         StopAudios();
        //         GameEventMgr.SendEvent("concert_cancelled");
        //         state = Define.GAME_STATE.CANCEL;
        //         missingTime = 0;
        //         return;
        //     }
        // }

        if(!canSpawnNote)
            return;

        if(!isCheckEndGame && currentTime >= GameData.GetLastBeatTime())
        {
            timeControlEndGame = 0;
            isCheckEndGame = true;
            durationOfSong = GetDurationAudio((int)Define.AUDIOS_INDEX.VOCAL);
        }

        if(isCheckEndGame)
        {
            timeControlEndGame += Time.deltaTime;
            {
                bool allowFadeOut = true;
                for(int v = 0; v < noteList.Length; ++v)
                {
                    if(noteList[v].Count > 0)
                    {
                        allowFadeOut = false;
                        break;
                    }
                }
                if(!isFadeOut && (allowFadeOut || (durationTimelineOfSong > 0 && currentTime + Define.SECOND_PER_FRAME >= durationTimelineOfSong)))
                {
                    isFadeOut = true;
                    SFXManager.Instance.PlayCrowdEnd();
                    BoardGameManager.Instance.FadeOut();
                }
            }

            float deltaTime = Define.SECOND_PER_FRAME;
            // if(!AnimationTimelineManager.Instance.IsEndSong())
            if(true)
            {
                if(timeControlEndGame >= Define.TIME_DELAY_NO_NOTE || (durationTimelineOfSong > 0 && currentTime + Define.SECOND_PER_FRAME >= durationTimelineOfSong))
                {
                    // AnimationTimelineManager.Instance.EndSong();
                    StopAllIngameParticlesBasic();
                }
            }
            else
            {
                deltaTime = 2*Define.WAIT_FOR_SECOND;
            }

            if(durationOfSong > 0 && currentTime + deltaTime >= durationOfSong)
            {
                isCheckEndGame = false;                

                state = Define.GAME_STATE.ENDGAME;
                
                // if(!AnimationTimelineManager.Instance.IsEndSong())
                if(true)
                {
                    // AnimationTimelineManager.Instance.EndSong();
                    StopAllIngameParticlesBasic();
                }

                // CharactersController.Instance.Goodbye();
                timeControlEndGame = 0;
                PlayStageFireParticle();
                return;
            }
        }

        // beatInfos = GameData.GetBeatInfo();
        if (beatInfos != null)
        {
            int beatCount = beatInfos.Count;

            for (int i = 0; i < beatCount; i++)
            {
                if (beatInfos[i].indexBeat == beatInfos[i].beatDatas.Length)
                {
                    continue;
                }

                if (beatInfos[i].beatDatas.Length == 0)
                {
                    continue;
                }

                BeatData beatData = beatInfos[i].beatDatas[beatInfos[i].indexBeat];

                float startTime = Mathf.Max(beatData.startTime, beatData.endTime);
                // float beatPosition = (float)GetBeatPosition(i % audioSources.Length);
                float beatPosition = (float)GetBeatPosition(beatData.indexAudio);

                if (startTime <= beatPosition)
                {
                    float remainDuration = duration - (beatPosition - startTime);
                    int lineIndex = beatInfos[i].indexCSV;
                    if (remainDuration < 0)
                    {
                        //Debug.LogFormat("warning {0} csv {1} start {2} vs beat {3}", beatInfos[i].beatName, beatInfos[i].indexCSV, Convert.FloatToTime(startTime), Convert.FloatToTime(beatPosition));
                        beatInfos[i].indexBeat++;
                        continue;
                    }

                    
                    if (beatData.type == Define.NOTE_TYPE.MIGHTY_SWIPE)
                    {
                        AppearLanesData[] appearLanesData = GameData.GetAppearLanesData();
                        int laneSize = appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Count;

                        if (lineIndex < linesNumber / 2)
                        {
                            // Left side fored
                            if((IsActiveLine(0) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-1))
                            || (!IsActiveLine(2) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(3))
                            )
                            {
                                beatData.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                                beatData.forcedDirection = true;
                            }
                            else if((!IsActiveLine(0) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(1))
                            || (IsActiveLine(2) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-3))
                            )
                            {
                                beatData.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                                beatData.forcedDirection = true;
                            }
                            // Left side fored : end
                            else if((IsActiveLine(0) && IsActiveLine(1) && IsActiveLine(2))
                            && (appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-1) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-3))
                            )
                            {
                                beatData.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                            }
                            else if((!IsActiveLine(0) && IsActiveLine(1) && !IsActiveLine(2))
                            && (appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(1) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(3))
                            )
                            {
                                beatData.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                            }

                            if(!beatData.forcedDirection)
                            {
                                // Right side fored
                                if((IsActiveLine(3) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-4))
                                || (!IsActiveLine(5) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(6))
                                )
                                {
                                    // Right side forced Define.INPUT_STATUS.SWIPE_RIGHT
                                    beatData.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                                }
                                else if((!IsActiveLine(3) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(4))
                                || (IsActiveLine(5) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-6))
                                )
                                {
                                    // Right side forced Define.INPUT_STATUS.SWIPE_LEFT
                                    beatData.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                                }
                                // Right side fored : end
                            }
                        }
                        else
                        {
                            // Left side fored
                            if((IsActiveLine(0) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-1))
                            || (!IsActiveLine(2) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(3))
                            )
                            {
                                // Left side forced Define.INPUT_STATUS.SWIPE_RIGHT
                                beatData.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                            }
                            else if((!IsActiveLine(0) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(1))
                            || (IsActiveLine(2) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-3))
                            )
                            {
                                // Left side forced Define.INPUT_STATUS.SWIPE_LEFT
                                beatData.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                            }
                            // Left side fored : end
                            else
                            {
                                // Right side forced
                                if((IsActiveLine(3) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-4))
                                || (!IsActiveLine(5) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(6))
                                )
                                {
                                    beatData.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                                }
                                else if((!IsActiveLine(3) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(4))
                                || (IsActiveLine(5) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-6))
                                )
                                {
                                    beatData.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                                }
                                // Right side forced : end
                                else if((IsActiveLine(3) && IsActiveLine(4) && IsActiveLine(5))
                                && (appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-4) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(-6))
                                )
                                {
                                    beatData.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                                }
                                else if((!IsActiveLine(3) && IsActiveLine(4) && !IsActiveLine(5))
                                && (appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(4) && appearLanesData[beatData.linkToAppearLanes].AppearLanesList.Contains(6))
                                )
                                {
                                    beatData.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                                }
                            }
                        }

                        if (lineIndex < linesNumber / 2)
                        {
                            if(beatData.swipe == Define.INPUT_STATUS.SWIPE_RIGHT)
                            {
                                // Moving mighty swipe note right on left side
                                lineIndex = 0;
                            }
                            else
                            {
                                // Moving mighty swipe note left on left side
                                lineIndex = linesNumber / 2 - 1;
                            }
                        }
                        else
                        {
                            if(beatData.swipe == Define.INPUT_STATUS.SWIPE_RIGHT)
                            {
                                // Moving mighty swipe note right on right side
                                lineIndex = linesNumber / 2;
                            }
                            else
                            {
                                // Moving mighty swipe note left on right side
                                lineIndex = linesNumber - 1;
                            }
                        }
                    }

                    if(!IsActiveLine(lineIndex))
                    {
                        if(beatData.type != Define.NOTE_TYPE.MIGHTY_SWIPE && beatData.type != Define.NOTE_TYPE.DRAG && beatData.type != Define.NOTE_TYPE.LONG)
                        {
                            //Change the notes on the disappear lines to appear lines
                            if (lineIndex < linesNumber / 2)
                            {
                                lineIndex = linesNumber / 4;
                            }
                            else
                            {
                                lineIndex = linesNumber * 3 / 4;
                            }
                        }
                    }
                    
                    // spaw note
                    SpawnNote(beatData, i, beatPosition, startTime, remainDuration, lineIndex);

                    // always increase index
                    beatInfos[i].indexBeat++;
                }
                beatData = null;
            } // end for

            if (dragNotes.Count > 0 && dragNotes[dragNotes.Count - 1].isTail)
            {
                dragNotes.Clear();
            }
            if (dragNotes2.Count > 0 && dragNotes2[dragNotes2.Count - 1].isTail)
            {
                dragNotes2.Clear();
            }
        }

        if (arrTimeCounter != null && arrTimeCounter.Length > 0)
        {
            for (int i = 0; i < arrTimeCounter.Length; i++)
            {
                if (arrTimeCounter[i] >= 0)
                {
                    arrTimeCounter[i] += Time.deltaTime;
                    if (arrTimeCounter[i] >= Define.TIME_ENABLE_MUSIC_LAYER)
                    {
                        arrTimeCounter[i] = -1;
                        // SFXManager.Instance.PlayOutRandomCrowdBoo();
                        GameManager.Instance.EnableSongHalfVolume(i);
                    }
                }
            }
        }
    }

    public void UpdateDragNotesList(LongNote longnote)
    {
        if(dragNotes != null && dragNotes.Count > 0)
        {
            dragNotes.Remove(longnote);
        }
        if(dragNotes2 != null && dragNotes2.Count > 0)
        {
            dragNotes2.Remove(longnote);
        }
    }

    void SortDragNotes(List<LongNote> longNotes)
    {
        Note parentNote;
        int count = longNotes.Count;
        if (count == 1)
        {
            longNotes[0].next = null;
            longNotes[0].prev = null;
        }
        else if (count > 1)
        {
            for (int i = 0; i < count - 1; ++i)
            {
                longNotes[i].next = longNotes[i + 1];
                longNotes[i + 1].prev = longNotes[i];
            }
            // if(longNotes[0].isHead)
            {
                longNotes[0].prev = null;
            }
            // if (longNotes[count - 1].isTail)
            {
                longNotes[count - 1].next = null;
            }

            for (int i = 0; i < count - 1; i++)
            {
                parentNote = longNotes[i].parentNote;
                parentNote.UpdateMaterial(parentNote.Color);
            }
        }
    }

    public void CorrectDragNotes(List<LongNote> dragNotesList, Define.LANE_ON_SIDE onSide)
    {
        int count = dragNotesList.Count;
        if(count < 2)
            return;

        for (int i = 0; i < count; ++i)
        {
            if(dragNotesList[i].parentNote.isDragHeadNote) // lineIndex = i == beatInfo.indexCSV
            {
                if(!IsActiveLine(dragNotesList[i].parentNote.indexCSV))
                {
                    int centerLineIdx = linesNumber / 4; // center on Left
                    if (dragNotesList[i].parentNote.indexCSV >= linesNumber / 2)
                    {
                        centerLineIdx = linesNumber * 3 / 4;                                
                    }
                    if(dragNotesList[i].parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_UP)
                    {
                        // Add new head on visible line(the center line of each side is always visible)
                        LongNote newNote = CloneNote(dragNotesList[i].parentNote, centerLineIdx).longNote;
                        if(newNote.parentNote.dragNotesKey < -1 ) // Left side
                        {
                            newNote.parentNote.dragNotesKey++;
                        }
                        else if(newNote.parentNote.dragNotesKey > 1) // Right side
                        {
                            newNote.parentNote.dragNotesKey--;
                        }

                        newNote.parentNote.DragRequire = dragNotesList[i].parentNote.indexCSV < centerLineIdx ? Define.DRAG_DIRECTION.DRAG_LEFT : Define.DRAG_DIRECTION.DRAG_RIGHT;
                        newNote.parentNote.indexCSV = centerLineIdx;

                        newNote.parentNote.isDragHeadNote = true;
                        newNote.isHead = true;
                        newNote.parentNote.isFirstNote = true;
                        newNote.parentNote.InitializeStatus();
                        dragNotesList[i].parentNote.isDragHeadNote = false;
                        dragNotesList[i].isHead = false;
                        dragNotesList[i].parentNote.isFirstNote = false;
                        dragNotesList[i].parentNote.isDualNotes = false;
                        dragNotesList[i].parentNote.InitializeStatus();
                        
                        dragNotesList.Insert(0, newNote);
                    }
                    else // if(beatData.dragDirection == Define.DRAG_DIRECTION.DRAG_LEFT || beatData.dragDirection == Define.DRAG_DIRECTION.DRAG_RIGHT)
                    {
                        // Later: Need to check double note in same position
                        BeatManager.Instance.ChangeLane(dragNotesList[i].parentNote, centerLineIdx);
                    }
                }
            }
        }

        LongNote temp;

        for (int i = 0; i < count; ++i)
        {
            for (int j = i + 1; j < count; ++j)
            {
                if ((onSide == Define.LANE_ON_SIDE.LEFT && dragNotesList[j].parentNote.dragNotesKey > dragNotesList[i].parentNote.dragNotesKey)
                    || (onSide == Define.LANE_ON_SIDE.RIGHT && dragNotesList[j].parentNote.dragNotesKey < dragNotesList[i].parentNote.dragNotesKey)
                    )
                {
                    temp = dragNotesList[i];
                    dragNotesList[i] = dragNotesList[j];
                    dragNotesList[j] = temp;
                }
            }
        }

        SortDragNotes(dragNotesList);
        // Sure dragNotesList[0] is head
        // Sure dragNotesList[dragNotesList[0].Count-1] is tail
        // Later: Need to check double head notes in same position
        // (dragNotesList[0].parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_LEFT || dragNotesList[0].parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
        if(dragNotesList[0].parentNote.isDragHeadNote && dragNotesList[0].parentNote.indexCSV == dragNotesList[1].parentNote.indexCSV
        && dragNotesList[0].parentNote.startTime == dragNotesList[1].parentNote.startTime)
        {
            dragNotesList[1].parentNote.isDragHeadNote = true;
            dragNotesList[1].isHead = true;
            dragNotesList[1].parentNote.isFirstNote = true;
            dragNotesList[1].prev = null;
            dragNotesList[1].parentNote.InitializeStatus();
            dragNotesList[0].parentNote.ResetAndDestroyMe();
        }

        // Update direction left/right
        for (int i = 0; i < dragNotesList.Count - 1; ++i)
        {
            if (dragNotesList[i].parentNote.startTime == dragNotesList[i + 1].parentNote.startTime)
            {
                if (dragNotesList[i].parentNote.indexCSV < dragNotesList[i + 1].parentNote.indexCSV)
                {
                    dragNotesList[i].parentNote.DragRequire = Define.DRAG_DIRECTION.DRAG_RIGHT;
                }
                else if (dragNotesList[i].parentNote.indexCSV > dragNotesList[i + 1].parentNote.indexCSV)
                {
                    dragNotesList[i].parentNote.DragRequire = Define.DRAG_DIRECTION.DRAG_LEFT;
                }
            }

            dragNotesList[i].parentNote.ChangeNoteColor(dragNotesList[i].parentNote.Color);
        }
    }

    void UpdateDragNotes(Define.LANE_ON_SIDE onSide)
    {
        if (onSide == Define.LANE_ON_SIDE.LEFT) 
        {
            CorrectDragNotes(dragNotes, onSide);
        }
        else // on right side
        {
            CorrectDragNotes(dragNotes2, onSide);
        }
    }

    public void MakeSureHeadDragNoteInVisibleLane()
    {
        List<List<LongNote>> lists1 = new List<List<LongNote>>();
        List<List<LongNote>> lists2 = new List<List<LongNote>>();
        int k1, k2;

        // Find each drag-notes group then correct it
        for(int v = 0; v < linesNumber; ++v)
        {
            int count = noteList[v].Count;
            for(int i = 0; i < count; ++i)
            {
                if(noteList[v][i].IsDragNote)
                {
                    k1 = -1;
                    k2 = -1;
                    for(int k = 0; k < lists1.Count; ++k)
                    {
                        // if(lists1[k].Find(x => x.parentNote.dragNotesGroup == noteList[v][i].dragNotesGroup))
                        if(lists1[k].Count > 0 && lists1[k][0].parentNote.dragNotesGroup == noteList[v][i].dragNotesGroup)
                        {
                            k1 = k;
                            break;
                        }
                    }
                    if(k1 == -1)
                    {
                        for(int k = 0; k < lists2.Count; ++k)
                        {
                            if(lists2[k].Count > 0 && lists2[k][0].parentNote.dragNotesGroup == noteList[v][i].dragNotesGroup)
                            {
                                k2 = k;
                                break;
                            }
                        }
                    }

                    if(k1 > -1)
                    {
                        lists1[k1].Add(noteList[v][i].longNote);
                    }
                    else if(v < linesNumber/2)
                    {
                        List<LongNote> list1 = new List<LongNote>();
                        list1.Add(noteList[v][i].longNote);
                        lists1.Add(list1);
                    }

                    if(k2 > -1)
                    {
                        lists2[k2].Add(noteList[v][i].longNote);
                    }
                    else if(v >= linesNumber/2)
                    {
                        List<LongNote> list2 = new List<LongNote>();
                        list2.Add(noteList[v][i].longNote);
                        lists2.Add(list2);
                    }
                    
                }
            }
        }

        for(int k = 0; k < lists1.Count; ++k)
        {
            CorrectDragNotes(lists1[k], Define.LANE_ON_SIDE.LEFT);
            lists1[k].Clear();
        }
        for(int k = 0; k < lists2.Count; ++k)
        {
            CorrectDragNotes(lists2[k], Define.LANE_ON_SIDE.RIGHT);
            lists2[k].Clear();
        }
        lists1.Clear();
        lists2.Clear();
    }

    // public void OnRestartFromConcertCancelled()
    // {
    //     OnReplay();
    //     GameEventMgr.SendEvent("Restart_Game");
    // }
    // public void SongSelectionFromConcertCancelled()
    // {
    //     state = Define.GAME_STATE.NONE;
    //     LoadingManager.Instance.StartLoadingToExitAP(Define.VIEW.SONG);
    //     GameEventMgr.SendEvent("Waiting_Song_Selection");
    // }

    // public void GoHomeFromConcertCancelled()
    // {
    //     GameEventMgr.SendEvent("Waiting_Go_Home");
    //     OnBackToMainMenu();
    // }
    
    void UpdateEndGame()
    {
        // finish play crowcheer
        timeControlEndGame += Time.deltaTime;
        if(timeControlEndGame >= Define.DELAY_END_GAME)
        {
            SFXManager.Instance.FadeOutBG();
            {
                GameEventMgr.SendEvent("end_game");
                state = Define.GAME_STATE.RESULT;
            }
        }
    }

    public void FadeOutSongs()
    {
        if(audioSources != null)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if(audioSources[i] != null && audioSources[i].clip != null)
                {
                    StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[i], Define.DELAY_END_GAME, AUDIO_VOLUME_IN_AP, 0f));
                }
            }
        }

    }

    public void SetMissingNote(bool value)
    {
        isMissingNote = value;
        if(!isMissingNote)
        {
            missingTime = 0;
        }
    }

    
    public void DisableSong(int audioIndex)
    {
        if(audioIndex < 0 || audioIndex > audioSources.Length)
        {
            return;
        }
        
        arrTimeCounter[audioIndex] = 0;
        if(mainVocal == Define.CHARACTERS.FREDDIE_MERCURY)
        {
            if(audioSources[audioIndex] != null && audioSources[audioIndex].clip != null)
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, AUDIO_VOLUME_IN_AP, 0f));
                if(audioIndex == (int)Define.AUDIOS_INDEX.VOCAL
                && GameData.GetInfo().audios[(int)Define.AUDIOS_INDEX.MERGE_OTHERS].Contains("PIANO"))
                {
                    StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[(int)Define.AUDIOS_INDEX.MERGE_OTHERS], TIME_DELAY_AUDIO_TRACK, AUDIO_VOLUME_IN_AP, 0f));
                }
            }
        }
        else
        {
            if(mainVocal == Define.ToGamePlayCharacterName((Define.AUDIOS_INDEX)audioIndex))
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, AUDIO_VOLUME_IN_AP, 0f));
                StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[(int)Define.AUDIOS_INDEX.VOCAL], TIME_DELAY_AUDIO_TRACK, AUDIO_VOLUME_IN_AP, 0f));
            }
            else if((Define.AUDIOS_INDEX)audioIndex != Define.AUDIOS_INDEX.VOCAL)
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, AUDIO_VOLUME_IN_AP, 0f));
            }

            if(Define.ToGamePlayCharacterName((Define.AUDIOS_INDEX)audioIndex) == Define.CHARACTERS.FREDDIE_MERCURY
            && GameData.GetInfo().audios[(int)Define.AUDIOS_INDEX.MERGE_OTHERS].Contains("PIANO"))
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeOut(audioSources[(int)Define.AUDIOS_INDEX.MERGE_OTHERS], TIME_DELAY_AUDIO_TRACK, AUDIO_VOLUME_IN_AP, 0f));
            }
        }
    }

    public void EnableSong(int audioIndex)
    {
        if(audioIndex < 0 || audioIndex > audioSources.Length)
        {
            return;
        }

        arrTimeCounter[audioIndex] = -1;
        if(mainVocal == Define.CHARACTERS.FREDDIE_MERCURY)
        {
            if(audioSources[audioIndex] != null && audioSources[audioIndex].clip != null)
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, 0f, AUDIO_VOLUME_IN_AP));
                if(audioIndex == (int)Define.AUDIOS_INDEX.VOCAL)
                {
                    if(GameData.GetInfo().audios[(int)Define.AUDIOS_INDEX.MERGE_OTHERS].Contains("PIANO"))
                    {
                        StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[(int)Define.AUDIOS_INDEX.MERGE_OTHERS], TIME_DELAY_AUDIO_TRACK, 0f, AUDIO_VOLUME_IN_AP));
                    }
                }
            }
        }
        else
        {
            if(mainVocal == Define.ToGamePlayCharacterName((Define.AUDIOS_INDEX)audioIndex))
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, 0f, AUDIO_VOLUME_IN_AP));
                StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[(int)Define.AUDIOS_INDEX.VOCAL], TIME_DELAY_AUDIO_TRACK, 0f, AUDIO_VOLUME_IN_AP));
            }
            else if((Define.AUDIOS_INDEX)audioIndex != Define.AUDIOS_INDEX.VOCAL)
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, 0f, AUDIO_VOLUME_IN_AP));
            }

            if(Define.ToGamePlayCharacterName((Define.AUDIOS_INDEX)audioIndex) == Define.CHARACTERS.FREDDIE_MERCURY
            && GameData.GetInfo().audios[(int)Define.AUDIOS_INDEX.MERGE_OTHERS].Contains("PIANO"))
            {
                StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[(int)Define.AUDIOS_INDEX.MERGE_OTHERS], TIME_DELAY_AUDIO_TRACK, 0f, AUDIO_VOLUME_IN_AP));
            }
        }
    } 

    public void EnableSongHalfVolume(int audioIndex)
    {
        if(audioSources[audioIndex] != null && audioSources[audioIndex].clip != null)
        {
            StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[audioIndex], TIME_DELAY_AUDIO_TRACK, 0f, HALF_VOLUME));
            if(audioIndex == (int)Define.AUDIOS_INDEX.VOCAL)
            {
                if(GameData.GetInfo().audios[(int)Define.AUDIOS_INDEX.MERGE_OTHERS].Contains("PIANO"))
                {
                    StartCoroutine(AudioFadeEffect.FadeVolumeIn(audioSources[(int)Define.AUDIOS_INDEX.MERGE_OTHERS], TIME_DELAY_AUDIO_TRACK, 0f, HALF_VOLUME));
                }
            }
        }
    }   

    public AudioSource[] GetArrayAudioSrouce()
    {
        return audioSources;
    }

    public void CameraShake()
    {
        cameraShake.Shake();
    }

    public bool UpdateComboNotes()
    {
        bool result = false;
        // No need
        //if(mightySwipeNotes != null && mightySwipeNotes.Count == 2)
        {
            // No need
            //if(mightySwipeNotes[0].LinkToAppearLanes == mightySwipeNotes[1].LinkToAppearLanes)
            {
                //execute combo notes success
                result = true;

                UpdateLinesStatus(mightySwipeNotes[0].linkToAppearLanes);
                for (int v = 0; v < mightySwipeNotes.Count; v++)
                {
                    // colorCounter[mightySwipeNotes[v].GetOriginalColor()]--;
                    noteList[mightySwipeNotes[v].indexCSV].Remove(mightySwipeNotes[v]);
                    mightySwipeNotes[v].MyKill();
                }

            }
        }
        mightySwipeNotes.Clear();
        return result;
    }    

    public void OnReplay()
    {
        StopAudios();

        if(isGamePaused)
        {
            OnResumeGame();
        }
        SetState(Define.GAME_STATE.RESET);
    }

    public void OnNext()
    {
        if(isGamePaused)
        {
            OnResumeGame();
            state = Define.GAME_STATE.NONE;
        }

        int currentIndex = Game.Instance.GetSongDifficultyIndex();
        int nextIndex = currentIndex + 1;
        Define.VIEW viewNeedBack = Define.VIEW.SONG;        
        if (nextIndex < Database.GetSongDifficulty().Count)
        {
            SongsInVenues currentSongInVenue = Database.GetSongByID(Database.GetSongDifficulty()[currentIndex].songID);
            SongsInVenues nextSongInVenue = Database.GetSongByID(Database.GetSongDifficulty()[nextIndex].songID);
            ConcertVenues nextVenues = Database.GetConcertVenuesByID(nextSongInVenue.concertVenuesID);
            if ((Database.GetCurrentDisc() >= nextVenues.discRequirement)) // check unlock mode or Venue
            {
                if (currentSongInVenue.concertVenuesID == nextSongInVenue.concertVenuesID) // this is same Stadium
                {
                    if (Database.GetSongDifficultyBy(currentIndex).songID != Database.GetSongDifficultyBy(nextIndex).songID)
                    {
                        Game.Instance.SongIndex ++;
                    }
                }
                else
                {
                    if(nextVenues.unlocked)
                    {
                        Game.Instance.SongIndex++;
                    }
                    else
                    {
                        viewNeedBack = Define.VIEW.VENUE;
                    }
                }                
            }
        }
        
        LoadingManager.Instance.StartLoadingToExitAP(viewNeedBack);
        GameEventMgr.SendEvent("Back to Waiting");
    }

    public void OnSelectSong()
    {
        if(isGamePaused)
        {
            OnResumeGame();
            state = Define.GAME_STATE.NONE;
        }
        Define.VIEW viewNeedBack = Define.VIEW.SONG;
        LoadingManager.Instance.StartLoadingToExitAP(viewNeedBack);
    }

    public Define.GAME_STATE GetState()
    {
        return state;
    }

    public void SetState(Define.GAME_STATE _state)
    {
        state = _state;
        switch(state)
        {
            case Define.GAME_STATE.INIT:                
                InitData();
                InitAudioSources();
                // CrowdManager.Instance.ShowAudience();
            break;

            case Define.GAME_STATE.RESET:
                ResetAndInitData();
            break;
            case Define.GAME_STATE.SCENARIO:
                isStartGame = true;                               
            break;
            case Define.GAME_STATE.INTRO:                
                BoardGameManager.Instance.FadeIn();
                timerControl.SetDuration(TIME_INTRO - duration);
                isStartGame = true;                
                PlaySoundIntro();
                TutorialManager.Instance.UpdateState();
            break;
        }
    }

    public void PlaySoundIntro()
    {if(Game.Instance.IsDemoTutorial == Define.TUTORIAL_TYPE.BASIC_TUTORIAL_REPLAY)
        {
            SFXManager.Instance.FadeInBG(Define.SFX.CROWD_BED, 1f, true);
            SFXManager.Instance.FadeInCrowdCheer(1f);
        }
        else
        {
            if(TutorialManager.Instance.Tutorial > Define.TUTORIAL.INGAME_SCENARIO_END)
            {
                SFXManager.Instance.PlayBackGround(Define.SFX.CROWD_BED, true);
                SFXManager.Instance.PlayRandomCrowdCheer();
            }
        }
    }

    public void SetStateNone()
    {
        state = Define.GAME_STATE.NONE;
    }

    public bool IsAutoCollect()
    {
        return subState == Define.GAME_SUB_STATE.AUTO_COLLECT;
    }
    
    public void CleanNoteOnScreen()
    {
        for (int idx = 0; idx < noteList.Length; idx++)
        {
            for (int i = 0; i < noteList[idx].Count; i++)
            {
                if (noteList[idx][i].transform.localPosition.z < POS_SHOW_NOTE)
                {
                    if (noteList[idx][i].IsDragNote)
                    {
                        if (noteList[idx][i].isDragTailNote)
                        {
                            noteList[idx][i].ResetAndDestroyAllFamily(noteList[idx][i]);
                        }
                        else
                        {
                            if(noteList[idx][i].longNote.next != null)
                            {
                                if(noteList[idx][i].longNote.next.parentNote.isDragTailNote)
                                {
                                    noteList[idx][i].longNote.next.parentNote.ResetAndDestroyAllFamily(noteList[idx][i].longNote.next.parentNote);
                                }
                                else 
                                {
                                    // set head note for onather note
                                    noteList[idx][i].longNote.next.isHead = true;
                                    noteList[idx][i].longNote.next.parentNote.isDragHeadNote = true;
                                    noteList[idx][i].longNote.next.parentNote.note3D.SetActive(true);

                                    noteList[idx][i].ResetAndDestroyPrevious(noteList[idx][i]);
                                    noteList[idx][i].ResetAndDestroyMe();
                                }
                            }
                        }

                        continue;
                    }

                    bool isComboNotes = false;
                    if (noteList[idx][i].type == Define.NOTE_TYPE.MIGHTY_SWIPE)
                    {
                        if (noteList[idx][i].comboNotesKey != -1)
                        {
                            if ((mightySwipeNotes.Count == 0 || !mightySwipeNotes.Contains(noteList[idx][i])))
                            {
                                int count = mightySwipeNotes.Count;
                                if (count == 1 && mightySwipeNotes[0].comboNotesKey != noteList[idx][i].comboNotesKey)
                                {
                                    mightySwipeNotes.Clear();
                                }

                                mightySwipeNotes.Add(noteList[idx][i]);

                                if (mightySwipeNotes.Count == 2)
                                {
                                    isComboNotes = true;
                                    UpdateComboNotes();
                                }
                            }
                        }
                        else
                        {
                            mightySwipeNotes.Add(noteList[idx][i]);
                            {
                                isComboNotes = true;
                                UpdateComboNotes();
                            }
                        }
                    }

                    if(!isComboNotes)
                    {
                        int score = ScoreManager.Instance.GetScoreNote(noteList[idx][i].type, Define.TRIGGER.PERFECT);
                        ScoreManager.Instance.AddScore(score);
                        // GameManager.Instance.colorCounter[noteList[idx][i].GetOriginalColor()]--;
                        noteList[idx][i].MyKill();
                        noteList[idx].RemoveAt(i);
                        i--;
                    }
                }
            }
        }        
    }

    public void CleanAllNote()
    {
        for (int idx = 0; idx < noteList.Length; idx++)
        {
            for (int i = 0; i < noteList[idx].Count; i++)
            {
                // colorCounter[noteList[idx][i].GetOriginalColor()]--;       
                noteList[idx][i].MyKill();
                noteList[idx].RemoveAt(i);
                i--;
            }
        }
    }

    public bool ReleaseAll()
    {
        return isReleaseAll;
    }

    public List<Note> GetNoteInLine(int idx)
    {
        if (noteList != null && idx >= 0 && idx < noteList.Length)
        {
            return noteList[idx];
        }
        else
        {
            return null;
        }
    }

    public void RemoveLine(int idx)
    {
        if (noteList != null)
        {
            int count = noteList[idx].Count;
            for (int i = 0; i < count; ++i)
            {
                // GameManager.Instance.colorCounter[noteList[idx][i].GetOriginalColor()]--;
                noteList[idx][i].MyKill();
            }
            noteList[idx].Clear();
        }
    }


    public void SetBackToGameCountdown()
    {
        Time.timeScale = 1;
        viewPauseMenu.OnClickResume();
    }

    public void OnBackToMainMenu()
    {
        Time.timeScale = 1;
        LoadingManager.Instance.StartLoadingToExitAP();
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }
    public bool IsResetting()
    {
        return isReset;
    }

    public void OnPauseGame()
    {
        if(TutorialManager.Instance.IsNeedPause())
        {
            return;
        }
        OnPauseGameOnly();
        GameEventMgr.SendEvent("ingame_pause");

    }

    public bool CanInterruptIngame()
    {
        if(    state == Define.GAME_STATE.NONE
            || state == Define.GAME_STATE.INIT
            || state == Define.GAME_STATE.RESET
            || state == Define.GAME_STATE.INTRO
            || state == Define.GAME_STATE.SCENARIO
            || TutorialManager.Instance.Tutorial == Define.TUTORIAL.INGAME_SCENARIO_END
            || audioSources == null
        )
        {
            return false;
        }
        return true;
    }
    public void OnPauseGameOnly()
    {
        if (isGamePaused) return;
        isGamePaused = true;
        Time.timeScale = 0;

        if(state == Define.GAME_STATE.INIT
            || state == Define.GAME_STATE.RESET
            || state == Define.GAME_STATE.INTRO
            || state == Define.GAME_STATE.SCENARIO
            || TutorialManager.Instance.Tutorial == Define.TUTORIAL.INGAME_SCENARIO_END
            || audioSources == null
        )
        {
            return;
        }

        // Debug.Log("GameManager::OnPauseGameOnly()");

        if(GameManager.Instance.WasAudioPlayed())
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if(audioSources[i] != null && audioSources[i].clip != null)
                {
                    {
                        audioSources[i].Pause();
                    }
                }
            }
        }
        starInterruptTime = AudioSettings.dspTime;
        // SFXManager.Instance.PauseAllSFX();
    }

    public void OnResumeGame()
    {
        if (!isGamePaused) return;
        isGamePaused = false;
        Time.timeScale = 1;
        
        if(state == Define.GAME_STATE.INIT
            || state == Define.GAME_STATE.RESET
            || state == Define.GAME_STATE.INTRO
            || state == Define.GAME_STATE.SCENARIO
            || TutorialManager.Instance.Tutorial == Define.TUTORIAL.INGAME_SCENARIO_END
            || audioSources == null
        )
        {
            return;
        }
        
       
        if(GameManager.Instance.WasAudioPlayed())
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if(audioSources[i] != null && audioSources[i].clip != null && GetTimeAudio(i) > 0)
                {
                    audioSources[i].UnPause();
                }
            }
        }
        interruptTime = AudioSettings.dspTime - starInterruptTime;
        totalInterruptTime += interruptTime;
        // SFXManager.Instance.ResumeAllSFX();
    }

    public double GetTotalInterruptTime()
    {
        return totalInterruptTime;
    }

    private void SetSpeedNote()
    {
        switch (Game.Instance.GetGameMode())
        {
            case Define.GAME_MODE.EASY:
			    speedNote = 3f;
                break;
            case Define.GAME_MODE.MEDIUM:
                speedNote = 3f;
                break;
            case Define.GAME_MODE.HARD:
                speedNote = 3.5f;
                break;
        }
    }

    public void CheatFinishGame()
    {
        OnResumeGame();
        Time.timeScale = 1;
        CleanAllNote();
        SFXManager.Instance.FadeVolumeOutBackGround(0, 0f);
        var allAudioSources = gameObject.GetComponentsInChildren<AudioSource>();
        GameEventMgr.SendEvent("end_game");
        state = Define.GAME_STATE.RESULT;
        
        StopAudios();
    }

    public bool IsEndgame()
    {
        return state == Define.GAME_STATE.ENDGAME;
    }

    public void PlayStageFireParticle()
    {
        if(GameOptions.IsLowProfile())
        {
            return;
        }

        stageFireParticleCount = 0;
        PlayNextStageFireParticle();
    }

    void PlayFireworksSFX()
    {
        SFXManager.Instance.Play(Define.SFX.FIREWORKS);
    }

    void UpdateStageFireParticle()
    {
        if(GameOptions.IsLowProfile())
        {
            return;
        }
        if(stageFireParticleCount < PLAY_PARTICLE_LOOP_MAX - 1)
        {
            if(!stageParticleList[stageParticleList.Count - 1].specialmoveStartParticle.isPlaying)
            {
                stageFireParticleCount++;
                PlayNextStageFireParticle();
            }
        }
    }

    void PlayNextStageFireParticle()
    {
        Invoke("PlayFireworksSFX", 0.05f);
        for(int i = 0; i < stageParticleList.Count; i++)
        {
            stageParticleList[i].StartFireParticle();
        }
    }


    public int GetHighestPerfect()
    {
        return highestPerfect;
    }

    public void SetHighestPerfect(int value)
    {
        highestPerfect = value;
    }


    enum TutorialSongList : int
    {
        [StringValue("S001")]
        KEEP_YOURSELF_ALIVE = 0,
        [StringValue("S002")]
        SEVEN_SEAS_OF_RHYE = 1
    }

    public float GetDuration()
    {
        return duration;
    }

    public bool WasAudioPlayed()
    {
        return wasAudiosPlayed;
    }
    public void MarkAudioWasPlayed()
    {
        wasAudiosPlayed = true;
    }

    public bool IsCheckEndGame()
    {
        return isCheckEndGame;
    }

    public bool IsBoardGameFadeOut()
    {
        return isFadeOut;
    }

    public void PreloadIngameParticles()
    {
        Game.Instance.gameView.SetActive(true);
        viewInGame.PreloadUIVFX();
    }

    public void StopAllIngameParticles()
    {
        StopAllIngameParticlesBasic();
        Game.Instance.gameView.SetActive(false);
    }
    public void StopAllIngameParticlesBasic()
    {
        ActivatorsManager.Instance.StopAllParticles();
        viewInGame.StopAllUIVFX();
    }
}
