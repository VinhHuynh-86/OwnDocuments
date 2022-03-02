using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Video;

public class ViewVenue : MonoBehaviour
{
    // Public
    public VenueCover venueCoverPrefab;
    public ScrollRect scrollRect;
    public Transform venueContainer;
    public Transform markerContainer;
    public Transform transTitle;
    int newUnlockIdx;
    
    //Private
    private List<VenueCover> listVenue = new List<VenueCover>();
    private List<int> listDiscRequire = new List<int>();
    
    void Start()
    {
        int concertVenuesCount = Database.GetConcertVenues().Count;
        for (int i = 0; i < concertVenuesCount; ++i)
        {
            VenueCover vc = Instantiate(venueCoverPrefab, venueContainer);
            vc.viewVenue = this;
            listVenue.Add(vc);            
            int index = i;
            vc.ID = i;
            vc.UpdateInfo(Database.GetConcertVenues()[i]);
            vc.button.onClick.AddListener(()=>OnClickVenue(index));
            Marker mk = markerContainer.GetChild(i).GetComponent<Marker>();
            int discRequirement = Database.GetConcertVenues()[i].discRequirement;
            listDiscRequire.Add(discRequirement);
            mk.numberDisc.text = discRequirement.ToString();
            vc.CheckStatus();
            
                scrollRect.horizontal = true;
        }
    }

    void OnEnable()
    {
        GameUtils.Instance.ScaleTitle(transTitle);
        Game.Instance.SetViewState(Define.VIEW.VENUE);
        if(TutorialManager.Instance != null)
        {
            TutorialManager.Instance.UpdateState();
        } 
        
        if(listVenue.Count > 0)
        {
            int concertVenuesCount = Database.GetConcertVenues().Count;
            for (int i = 0; i < listVenue.Count; ++i)
            {
                listVenue[i].UpdateInfo(Database.GetConcertVenues()[i]);
            }
           
            {
                scrollRect.horizontal = true;
            }
        }        
    }

    void Update()
    {
        UpdateBackKey();
    }

    public void OnClickVenue(int index)
    {
        if(!ProfileMgr.Instance.IsGameUnlocked && index != 0)
        {
            GameEventMgr.SendEvent("venue_go_iap");
            SFXManager.Instance.Play(Define.SFX.UI_LOCKED);
            return;
        }

        if(listVenue[index].IsLock())
        {
            SFXManager.Instance.Play(Define.SFX.UI_LOCKED);
            return;
        }
        if(listVenue[index].IsAlready())
        {
            newUnlockIdx = index;
            listVenue[index].PerformedUnlocking();
            return;
        }

        if(Database.VenuesIsUnlocked(index))
        {
            if(Game.Instance.SongIndex < index *2 || Game.Instance.SongIndex > index * 2 + 1)
            {
                Game.Instance.SongIndex = index * 2;
            }
            Game.Instance.SetStadiumIndex(index);
            GameEventMgr.SendEvent("play_game");
            SFXManager.Instance.Play(Define.SFX.UI_MENU_SELECT);
        }
    }

    public SongsInVenues GetFirstSong()
    {
        List<SongsInVenues> songs = Database.GetSongInVenues(Database.GetConcertVenues()[newUnlockIdx].ID);
        return songs[0];
    }

    public void UnlockVenue()
    {
        if (newUnlockIdx == -1)
            return;


        Database.SetUnlockConcertVenuesByIndex(newUnlockIdx, true);
        ProfileMgr.Instance.ToursUnlocked ++;
        ProfileMgr.Instance.Save();

         listVenue[newUnlockIdx].SetUnlockUI();
        // Check next venue if it's readly to unlock
        if(newUnlockIdx + 1 < listVenue.Count)
        {
            listVenue[newUnlockIdx + 1].CheckStatus();
        }
    }

    public void UpdateBackKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            {
                GameEventMgr.SendEvent("go_back");
                SFXManager.Instance.Play(Define.SFX.UI_MENU_BACK);
            }
        }
    }
}
