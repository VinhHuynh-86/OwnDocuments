using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using Doozy.Engine.UI;

public class ViewSelectSong : MonoBehaviour
{
    public TextMeshProUGUI title;
    public SelectSong_SongInfo songInfoPrefab;
    public ScrollRect scrollRect;
    public Transform scrollContent;    
    public Scrollbar scrollbar;
    public GameObject prevButton;
    public GameObject nextButton;
    public PaginationDot dotPrefab;
    public Transform dotParentTrans;

    float TIME_TO_START_SONG = 1.0f;

    // [SerializeField]
    // private Image imageBG;
    // [SerializeField]
    // private Image imageBG_Transparent;

    //private variables
    private float[] pos;
    private float distance;
    private float scroll_pos = 0;
    private int currentScrollIndex;// == songIndex
    private List<SelectSong_SongInfo> songList = new List<SelectSong_SongInfo>();    
    private List<PaginationDot> dotList = new List<PaginationDot>();
    private float deltaScroll = 0f;
    private float DELTA_DISTANCE = 0.001f;    
    private float TIME_SCROLL = 0.4f;
    const int BASE_WIDTH = 1920;
    const int BASE_HEIGHT = 1080;
    Animator animator;
    // Material material;
    float cutOffValue;
    AudioSource audioSource;
    AudioClip songPreviewClip;
    Timer previewPlayTimer = new Timer();

    float delayTimeToReplayPreviewMusic = 0;
    bool isPreviewMusicFinished = false;
    public bool isClickLeftRight = false;

    void Awake()
	{
        animator = gameObject.GetComponent<Animator>();
        // material = imageBG.material;
        previewPlayTimer.SetDuration(TIME_TO_START_SONG);
	}

    void OnEnable()
    {
        Game.Instance.SetViewState(Define.VIEW.SONG);
        if(TutorialManager.Instance != null)
        {
            TutorialManager.Instance.UpdateState();
        }

        // material.SetFloat("Vector1_9D7C1239", 0f); // working

        if(Game.Instance.GetCurrStadiumIndex() > -1)
        {
            // imageBG_Transparent.gameObject.SetActive(true);
            Init();
            pos = new float[songList.Count];
            distance = 1f / (pos.Length - 1f);
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = distance * i;
            }
            int startIndex = Game.Instance.GetCurrStadiumIndex() * 2;
            if(currentScrollIndex < startIndex || currentScrollIndex > startIndex + 1)
            {
                currentScrollIndex = startIndex;
            }
            currentScrollIndex = Game.Instance.SongIndex;
            SetButtonState();
            UpdatePageDot(); 
            SetSelectSongIndex(currentScrollIndex, 0);
            // material.SetTexture("Texture2D_183A1C25", listSpriteBG[Game.Instance.GetCurrStadiumIndex()].texture); // working
        }
        
        if(!scrollRect.horizontal)
        {
            scrollRect.horizontal = true;
        }
    }

    void Update()
    {        
        if(!isPreviewMusicFinished && audioSource != null && songPreviewClip != null && audioSource.time >= songPreviewClip.length)
        {
            isPreviewMusicFinished = true;
        }

        if(isPreviewMusicFinished)
        {
            delayTimeToReplayPreviewMusic += Time.deltaTime;
            if(delayTimeToReplayPreviewMusic > 3)
            {
                delayTimeToReplayPreviewMusic = 0;
                isPreviewMusicFinished = false;
                audioSource.Play();
            }
        }

        UpdateScroll();
        UpdateBackKey();
    }

    void UpdateScroll()
    {
        if (Input.GetMouseButtonDown(0))
        {
            scroll_pos = scrollbar.value;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if(isClickLeftRight)
            {
                return;
            }
            deltaScroll = scrollbar.value - scroll_pos;  
            if(deltaScroll >= DELTA_DISTANCE && currentScrollIndex < pos.Length - 1)
            {
                SetSelectSongIndex(currentScrollIndex + 1);
            }
            else if(deltaScroll <= -DELTA_DISTANCE && currentScrollIndex > 0)
            {
                SetSelectSongIndex(currentScrollIndex - 1);
            }
        }
    }

    public void StopAndUnloadSongPreview()
    {
        if(audioSource != null)
        {
            audioSource.Stop();
        }

        if(songPreviewClip != null)
        {
            songPreviewClip.UnloadAudioData();
            Resources.UnloadAsset(songPreviewClip);
        }
    }
    IEnumerator LoadSongPreviewsAsync(string path)
    {
        // unload audio
        StopAndUnloadSongPreview();

        //Request data to be loaded
        ResourceRequest loadAsync = Resources.LoadAsync(path);

        //Wait till we are done loading
        while (!loadAsync.isDone)
        {
            Debug.Log("Load Progress: " + loadAsync.progress);
            yield return null;
        }
        //Get the loaded data
        songPreviewClip = loadAsync.asset as AudioClip;
        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.clip = songPreviewClip;

        yield return new WaitForSeconds(TIME_TO_START_SONG);
        audioSource.Play();
    }

    public void SetSelectSongIndex(int index, float transferDuration = -1)
    {
        if(index < 0 || index >= pos.Length)
            return;
            
        currentScrollIndex = index;
        DOTween.To(x => scrollbar.value = x, scrollbar.value, pos[currentScrollIndex], transferDuration > -1 ? transferDuration : TIME_SCROLL).OnComplete(()=> Invoke("FinishScroll", 0.25f));
        SetButtonState();
        UpdatePageDot();
        
        StopAndUnloadSongPreview();
        previewPlayTimer.Reset();
        // for(int v = 0; v < Define.SONG_NAME.Length; ++v)
        // {
        //     if(songList[currentScrollIndex].GetSongsInVenues().songName.Contains(Define.SONG_NAME[v]))
        //     {
        //         StartCoroutine(LoadSongPreviewsAsync(Define.PREVIEW_PATH + Define.SONG_NAME[v]));
        //     }
        // }
        int stadiumIndex = currentScrollIndex/2;
        title.text = Localization.Instance.GetString(Database.GetConcertVenues()[stadiumIndex].stadium);
        title.GetComponent<MultiUnderlay>().SetActive(false);
        Invoke("UpdateTitleUnderlay", 0.1f);
        Game.Instance.SetStadiumIndex(stadiumIndex);
        Game.Instance.SongIndex = currentScrollIndex;
    }

    public void FinishScroll()
    {
        isClickLeftRight = false;
    }

    public void UpdateTitleUnderlay()
    {
        title.GetComponent<MultiUnderlay>().SetActive(true);
        GameUtils.Instance.UpdateTextUnderlay(title);
    }

    public int GetSelectSongIndex()
    {
        return currentScrollIndex;
    }

    public void Init()   
    {
        List<ConcertVenues> concertVenues = Database.GetConcertVenues();
        int venueCount = concertVenues.Count;
        
        float width = BASE_WIDTH;         
        float height = BASE_HEIGHT;
        width = height * (float)Screen.safeArea.width / (float)Screen.safeArea.height;
        float scaleRatio = width / BASE_WIDTH;
        
        int index = 0;

        for(int i = 0; i < venueCount; i++)
        {
            if(concertVenues[i].unlocked)
            {                
                List<SongsInVenues> songsInVenues = Database.GetSongInVenues(concertVenues[i].ID);
                int songsCount = songsInVenues.Count;
                for(int j = 0; j < songsCount; j++)
                {
                    index ++;
                    if(index > songList.Count)
                    {
                        SelectSong_SongInfo go = Instantiate(songInfoPrefab, scrollContent);
                        RectTransform rectTransform = go.GetComponent<RectTransform>();
                        rectTransform.sizeDelta = new Vector2(width, height);
                        if(scaleRatio < 1)
                        {
                            go.transform.localScale = Vector3.one * scaleRatio*0.98f;
                        }
                        songList.Add(go);
                        songList[songList.Count - 1].RegistryCallback(LoadingStadiumAndGameplay);
                        go.SetSongData(songsInVenues[j], j);

                        PaginationDot pd = Instantiate(dotPrefab, dotParentTrans);
                        dotList.Add(pd);
                    }
                    else
                    {
                        songList[index - 1].SetSongData(songsInVenues[j], j);
                    }
                }
                
            }
        }
    }

    public void OnClickButtonPlay()
    {
        TutorialManager.Instance.UpdateState();
        Define.GAME_MODE gameMode = songList[currentScrollIndex].GetCurrentMode();
        List<SongDifficulty> list = Database.GetSongDifficulty();
        int songDiffIdx = -1;
        int count = list.Count;
        for(int i = 0; i < count; ++i)
        {
            if(list[i].songID == songList[currentScrollIndex].GetSongsInVenues().ID && list[i].ID != "SD000")
            {
                if(list[i].difficult == gameMode)
                {
                    songDiffIdx = i;
                    break;
                }
            }
        }

        if(!list[songDiffIdx].unlocked)        
        {
            return;
        }
        songList[currentScrollIndex].SetPlayTrigger();
        Game.Instance.SetSelectedSongDifficultyInfo(list[songDiffIdx].ID, currentScrollIndex, gameMode);
        LoadingStadiumAndGameplay();
    }

    public void LoadingStadiumAndGameplay()
    {
        StopAndUnloadSongPreview();
        LoadingManager.Instance.StartLoadingToEnterAP();
    }

    public void OnClickBack()
    {
        StopAndUnloadSongPreview();
        scrollbar.value = pos[currentScrollIndex];
    }
    public void OnClickGoHome()
    {
        StopAndUnloadSongPreview();
    }

    public void OnClickNextSong()
    {
        if(isClickLeftRight)
        {
            return;
        }
        TutorialManager.Instance.UpdateState();
        if(currentScrollIndex < pos.Length - 1)
        {
            isClickLeftRight = true;
            SetSelectSongIndex(currentScrollIndex + 1);
        }        
    }

    public void OnClickPrevSong()
    {
        if(isClickLeftRight)
        {
            return;
        }
        if(currentScrollIndex > 0)
        {
            isClickLeftRight = true;
            SetSelectSongIndex(currentScrollIndex - 1);
        }
    }

    private void SetButtonState()
    {
        if(currentScrollIndex == 0)
        {
            if(prevButton.activeSelf) prevButton.SetActive(false);
        }
        else
        {
            if(!prevButton.activeSelf) prevButton.SetActive(true);
        }

        if(currentScrollIndex == pos.Length - 1)
        {
            if(nextButton.activeSelf) nextButton.SetActive(false);
        }
        else
        {
            if(!nextButton.activeSelf) nextButton.SetActive(true);
        }
    }

    public void UpdatePageDot()
    {
        for(int i = 0; i < dotList.Count; i++)
        {
            dotList[i].SetFill(i == currentScrollIndex);
        }
    }

    public void UpdateBackKey()
    {
        if(Game.Instance.GetViewState() == Define.VIEW.LOADING)
        {
            return;
        }
        
        if (GameUtils.Instance.IsActiveBackKey())
        {
            {
                GameEventMgr.SendEvent("go_back");
                SFXManager.Instance.Play(Define.SFX.UI_MENU_BACK);
            }
        }
    }  
}
