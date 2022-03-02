using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public enum LOADING_STATE: int
{
    NONE = 0,
    START_LOADING,
    UPDATE_PROGRESS,
    LOADING_FINISHED, // ready to start game
    DONE
}

public enum LOADING_TO: int
{
    NONE = 0,
    TO_AP,
    EXIT_AP
}

public class LoadingManager : Singleton<LoadingManager>
{
    public Camera menuCamera;
    public ViewLoading loadingObj;
    private AsyncOperation mAsyncCommonAdditive;
    private AsyncOperation mAsyncAdditive;
    private AsyncOperation mAsyncStudioAdditive;
    private bool stadiumIsLoaded = false;
    private bool isStudioBandLoaded = false;
    private float audioLoadProgress;
    private float sceneLoadProgress;
    float percent = 0f;
    bool scenesAreReady = false;
    bool audiosAreReady = false;

    LOADING_STATE loadingState;
    LOADING_TO loadingTo;
    Define.VIEW backToView;

    bool canStartGame;

    void OnEnable()
    {
        canStartGame = false;
        loadingState = LOADING_STATE.NONE;
        loadingTo = LOADING_TO.NONE;
        audioLoadProgress = 0f;
        sceneLoadProgress = 0f;
        loadingObj.UpdateProgress(0);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    void OnDisable()
    {
        canStartGame = false;
        audioLoadProgress = 0f;
        sceneLoadProgress = 0f;
        loadingObj.UpdateProgress(0);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public float GetLoadingProgress()
    {
        return (audioLoadProgress + sceneLoadProgress) / 2;
    }

    public void SetAudiosProgress(float value)
    {
        audioLoadProgress = value;
        if(audioLoadProgress < 0f)
            audioLoadProgress = 0f;
        if(audioLoadProgress > 1f)
            audioLoadProgress = 1f;
    }
    public void SetSceneProgress(float value)
    {
        sceneLoadProgress = value;
        if(sceneLoadProgress < 0f)
            sceneLoadProgress = 0f;
        if(sceneLoadProgress > 1f)
            sceneLoadProgress = 1f;
    }

    public void HideLoading()
    {
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(mode == LoadSceneMode.Additive)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene.name));
        }
    }
    private void OnSceneUnloaded(Scene current)
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
    }

    public string PreloadSong(string[] audiosOfSong)
    {
        string curSong = "";

        for(int i = 0; i < audiosOfSong.Length; ++i)
        {
            GameData.audioClipList.Add(null);
            if(audiosOfSong[i] == "NULL")
            {
                GameData.audioClipList[i] = null;
            }
            else
            {
                if(curSong == "")
                {
                    for(int v = 0; v < Define.SONG_NAME.Length; ++v)
                    {
                        if(audiosOfSong[i].Contains(Define.SONG_NAME[v]))
                        {
                            curSong = Define.SONG_NAME[v].ToLower();
                            break;
                        } 
                    }
                }
            }
        }

        return curSong;
    }

    public void ActiveViewIngame()
    {
        GameEventMgr.SendEvent(Define.GotoAP);
    }

    public void StartLoadingToEnterAP()
    {
        loadingObj.ShowProgessBar();
        loadingTo = LOADING_TO.TO_AP;
        loadingState = LOADING_STATE.START_LOADING;
        Game.Instance.SetGameInfo();
    }
    public void StartLoadingToExitAP(Define.VIEW toView = Define.VIEW.MAIN_MENU)
    {
        loadingObj.HideProgessBar();
        backToView = toView;
        loadingTo = LOADING_TO.EXIT_AP;
        loadingState = LOADING_STATE.START_LOADING;
    }

    public void LoadStadiumAndSong()
    {
        // SpriteManager.Instance.UnloadStudioSprite();
        GameEventMgr.SendEvent("LoadingEnterAP");
        percent = 0f;
        audioLoadProgress = 0f;
        sceneLoadProgress = 0f;
        scenesAreReady = false;
        audiosAreReady = false;
        string venueId = Database.GetConcertVenues()[Game.Instance.GetCurrStadiumIndex()].ID;
        List<SongsInVenues> songs = Database.GetSongInVenues(venueId);
        string[] audiosOfSong = songs[Game.Instance.GetSongIndexInStadium()].audios.ToArray();
        StartCoroutine(LoadAssetBundles.Instance.LoadAudioAsync(audiosOfSong, PreloadSong(audiosOfSong), GameData.audioClipList));     
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        // mAsyncAdditive = SceneManager.LoadSceneAsync(Define.VENUE_SCENE[Game.Instance.GetCurrStadiumIndex()], LoadSceneMode.Additive);
        mAsyncAdditive = LoadAssetBundles.Instance.LoadVenueBundle(Define.STADIUM);
        mAsyncAdditive.allowSceneActivation = false;
        
        // Wait for to make sure audios are ready.
        while(mAsyncAdditive.progress < 0.9f)
        {
            SetSceneProgress(mAsyncAdditive.progress);
            yield return null;
        }
        SetSceneProgress(1);
        
        while(audioLoadProgress < 1)
        {
            yield return null;
        }

        mAsyncAdditive.allowSceneActivation = true;
        menuCamera.gameObject.SetActive(false);
        stadiumIsLoaded = true;

        yield return new WaitForSeconds(Define.WAIT_FOR_SECOND);                                
        {
            scenesAreReady = true;
        }
    }

    public void UpdateLoadingToAP()
    {
        switch(loadingState)
        {
            case LOADING_STATE.NONE:
            break;

            case LOADING_STATE.START_LOADING:
                Game.Instance.gamePlay.SetActive(true);
                LoadStadiumAndSong();
                loadingState = LOADING_STATE.UPDATE_PROGRESS;
            break;

            case LOADING_STATE.UPDATE_PROGRESS:
                if(loadingObj.gameObject.activeSelf)
                {
                    loadingObj.UpdateProgress(GetLoadingProgress());
                }
                if(IsStadiumLoaded() && BaseCamera.Instance)
                {
                    if(!canStartGame)
                    {
                        canStartGame = true;
                        Resources.UnloadUnusedAssets();
                        System.GC.Collect();
                        // Shader.WarmupAllShaders();
                        GameManager.Instance.PreloadIngameParticles();
                        
                        // loadingState = LOADING_STATE.LOADING_FINISHED;

                        DOTween.To(x => {}, 0, 1f, Define.WAIT_FOR_SECOND)
                        .OnComplete(() => {
                            GameManager.Instance.StopAllIngameParticles();
                            loadingState = LOADING_STATE.LOADING_FINISHED;
                            canStartGame = false;
                        });
                    }
                }
            break;

            case LOADING_STATE.LOADING_FINISHED:
                Game.Instance.StartGamePlay();
                ActiveViewIngame();
                loadingState = LOADING_STATE.DONE;
            break;

            case LOADING_STATE.DONE:
                loadingState = LOADING_STATE.NONE;
                loadingTo = LOADING_TO.NONE;
            break;
        }
    }

    public void UpdateLoadingExitAP()
    {
        switch(loadingState)
        {
            case LOADING_STATE.NONE:
            break;

            case LOADING_STATE.START_LOADING:
                SFXManager.Instance.StopBG();
                GameManager.Instance.ResetAndDestroyNotes();
                StartCoroutine(DoResetDataAndUnloadStadium());
                loadingState = LOADING_STATE.UPDATE_PROGRESS;
            break;

            case LOADING_STATE.UPDATE_PROGRESS:
                if(!stadiumIsLoaded)
                {
                    loadingState = LOADING_STATE.LOADING_FINISHED;
                }
            break;

            case LOADING_STATE.LOADING_FINISHED:
                switch(backToView)
                {
                    case Define.VIEW.MAIN_MENU:
                        GameEventMgr.SendEvent("waiting_back_main_menu");
                    break;
                    case Define.VIEW.VENUE:
                        GameEventMgr.SendEvent("waiting_back_venue");
                        break;
                    case Define.VIEW.SONG:
                        GameEventMgr.SendEvent("waiting_back_SelectSong");
                        break;
                }

                percent = 0f;
                audioLoadProgress = 0f;
                sceneLoadProgress = 0f;
                scenesAreReady = false;
                audiosAreReady = false;
                Game.Instance.ResetIngameMode();
                loadingState = LOADING_STATE.DONE;
            break;

            case LOADING_STATE.DONE:
                loadingState = LOADING_STATE.NONE;
                loadingTo = LOADING_TO.NONE;
            break;
        }
    }

    void Update()
    {
        if(loadingTo == LOADING_TO.TO_AP)
        {
            UpdateLoadingToAP();
        }
        else if(loadingTo == LOADING_TO.EXIT_AP)
        {
            UpdateLoadingExitAP();
        }
    }

    public void StartUnloadCurrentStadium(Define.VIEW toView = Define.VIEW.MAIN_MENU)
    {
        backToView = toView;
    }
    IEnumerator DoResetDataAndUnloadStadium()
    {
        // waiting SECOND to make sure all note are killed in Note.LateUpdate()
        yield return new WaitForSeconds(Define.WAIT_FOR_SECOND);
        GameManager.Instance.ResetData();
        GameData.UnloadSong();
        if(Game.Instance.GetCurrStadiumIndex() > -1)
        {
            StartCoroutine(UnloadScene(backToView));
        }
        Game.Instance.DeactiveGamePlay();
        // SpriteManager.Instance.LoadStudioSprite();
    }

    IEnumerator UnloadScene(Define.VIEW backToView)
    {
        // mAsyncAdditive = SceneManager.UnloadSceneAsync(Define.VENUE_SCENE[Game.Instance.GetCurrStadiumIndex()]);
        mAsyncAdditive = SceneManager.UnloadSceneAsync(Define.STADIUM);
        while(!mAsyncAdditive.isDone)
        {     
            yield return null;
        }

        LoadAssetBundles.Instance.UnloadAllBundle();
        menuCamera.gameObject.SetActive(true);
        stadiumIsLoaded = false;
        yield return null;
    }
    
    public bool IsStadiumLoaded()
    {
        return (GetLoadingProgress() >= 1 && scenesAreReady && audiosAreReady);
    }
    IEnumerator WaitForStudioBand()
    {
        yield return new WaitForSeconds(Define.WAIT_FOR_SECOND);
        isStudioBandLoaded = true;
    }
    public bool IsStudioBandLoaded()
    {
        return isStudioBandLoaded;
    }

    public void UpdateLoadingAudiosStatus(bool ready)
    {
        audiosAreReady = ready;
    }
    public bool GetLoadingAudiosStatus(bool ready)
    {
        return audiosAreReady;
    }
}
