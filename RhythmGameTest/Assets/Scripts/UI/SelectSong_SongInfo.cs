using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SelectSong_SongInfo : MonoBehaviour
{
    public VenueCover venueCover;
    public TextMeshProUGUI songTitle;
    public TextMeshProUGUI songDuration;
    public GameObject goEasyUnlock;
    public GameObject goMediumUnlock;
    public GameObject goHardUnlock;
    public GameObject goHardLock;
    public GameObject[] silverDisc;
    public GameObject[] goldDisc;
    public GameObject[] platinumDisc;
    public TextMeshProUGUI[] textNumberBestScore;
    public Transform handTutorialEasy;

    //private variables
    private Define.GAME_MODE currentMode;
    private SongsInVenues songData;
    private int songIdxInStadium;
    private bool isPlayTriggered = false;
    private List<SongDifficulty> listSongDiff;

    public delegate void LoadingStadiumAndGameplay(); // declare delegate type
 
    protected LoadingStadiumAndGameplay callbackFct; // to store the function

    void OnEnable()
    {
        isPlayTriggered = false;
    }

    // void Update()
    // {
    // }

    public void RegistryCallback(LoadingStadiumAndGameplay callback)
    {
        callbackFct = callback;
    }

    public void SetCurrentMode(int mode)
    {
        if(isPlayTriggered) return;

        currentMode = (Define.GAME_MODE)mode;
        isPlayTriggered = true;
        Invoke("PlayGame", 0.05f);       
    }

    public void SetPlayTrigger()
    {
        isPlayTriggered = true;
    }

    public Define.GAME_MODE GetCurrentMode()
    {
        return currentMode;
    }

    public void SetSongData(SongsInVenues song, int songIdx)
    {
        songIdxInStadium = songIdx;
        songData = song;
        songTitle.text = Localization.Instance.GetString(songData.songName);
        songDuration.text = song.length;
        listSongDiff = Database.GetSongDifficulty(song.ID);
        for(int i = 0; i < listSongDiff.Count; i ++)
        {
            switch((Define.DISC_TYPE)listSongDiff[i].dicsCollect)
            {
                case Define.DISC_TYPE.PLATINUM:
                    silverDisc[i].SetActive(true);
                    goldDisc[i].SetActive(true);
                    platinumDisc[i].SetActive(true);
                break;
                case Define.DISC_TYPE.GOLD:
                    silverDisc[i].SetActive(true);
                    goldDisc[i].SetActive(true);
                break;
                case Define.DISC_TYPE.SILVER:
                    silverDisc[i].SetActive(true);
                break;
                default:
                    silverDisc[i].SetActive(false);
                    goldDisc[i].SetActive(false);
                    platinumDisc[i].SetActive(false);
                break;
            }
            int score = listSongDiff[i].bestScore;
            textNumberBestScore[i].text = score.ToString();            
        }

        goHardUnlock.SetActive(listSongDiff[(int)Define.GAME_MODE.HARD].unlocked);
        goHardLock.SetActive(!listSongDiff[(int)Define.GAME_MODE.HARD].unlocked);
        venueCover.UpdateInfo(Database.GetConcertVenuesByID(song.concertVenuesID));
        venueCover.button.gameObject.SetActive(false);              
        venueCover.statusUI.SetActive(false);              
    }

    public SongsInVenues GetSongsInVenues()
    {
        return songData;
    }

    public int GetSongIdxInStadium()
    {
        return songIdxInStadium;
    }

    public void PlayGame()
    {
        int songDiffIdx = -1;
        for(int i = 0; i < listSongDiff.Count; i++)
        {
            if(listSongDiff[i].difficult == currentMode)
            {
                songDiffIdx = i;
                break;
            }
        }

        if(!listSongDiff[songDiffIdx].unlocked)
        {
            return;
        }
        TutorialManager.Instance.UpdateState();
        Game.Instance.SetSelectedSongDifficultyInfo(listSongDiff[songDiffIdx].ID, songIdxInStadium, currentMode);
        GameEventMgr.SendEvent("select_song");
        SFXManager.Instance.Play(Define.SFX.UI_CONFIRM_LEVEL);
        callbackFct();
    }

    
}
