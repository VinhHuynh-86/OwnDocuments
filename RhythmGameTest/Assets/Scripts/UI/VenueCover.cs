using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using Doozy.Engine.UI;
// using DG.Tweening;

public enum LOCK_STATUS : int
{
    LOCK = 0,
    ALREADY,
    UNLOCKING,
    UNLOCKED
}

public class VenueCover : MonoBehaviour
{
    public ViewVenue viewVenue;
    public Button button;
    public TextMeshProUGUI textTour;
    public TextMeshProUGUI textCity;
    public TextMeshProUGUI textYear;
    public GameObject statusUI;
    public GameObject lockStatusUI;
    public GameObject unlockStatusUI;
    public GameObject hand;
    public TextMeshProUGUI textBuyGame;
    public Image imgCover;
    
    private Animator animatorStatus;
    private int venueIndex = -1;
    private LOCK_STATUS status;


    void Start()
    {
        animatorStatus = statusUI.GetComponent<Animator>();

        UpdateTextBuyGame();
    }

    void OnEnable()
    {
        CheckStatus();
        UpdateTextBuyGame();
        
    }

    void UpdateTextBuyGame()
    {
        if(textBuyGame != null)
        {
            textBuyGame.gameObject.SetActive(!ProfileMgr.Instance.IsGameUnlocked && venueIndex != 0);
        }
    }

    public void UpdateInfo(ConcertVenues concert)
    {
        // imgCover.sprite = Game.Instance.spritesVenue.GetSprite(concert.ID);
        textTour.text = Localization.Instance.GetString(concert.tour);
        textCity.text = Localization.Instance.GetString(concert.venueCity);
        GameUtils.Instance.UpdateTextUnderlay(textCity);
        if(venueIndex == 0)
        {
            textCity.GetComponent<Canvas>().sortingOrder = 13;
            textCity.GetComponent<MultiUnderlay>().UpdateOrder();
        }
        textYear.text = concert.year;
        if (concert.unlocked)
        {
            status = LOCK_STATUS.UNLOCKED;
        }
        else
        {
            status = LOCK_STATUS.LOCK;
        }
        UpdateStatus();
    }

    public void CheckStatus()
    {
        if (venueIndex == -1)
            return;

        if (Database.VenuesIsUnlocked(venueIndex))
        {
            status = LOCK_STATUS.UNLOCKED;
            SetUnlockUI();
        }
        else if (status != LOCK_STATUS.ALREADY && Database.VenueIsAlready(venueIndex))
        {
            status = LOCK_STATUS.ALREADY;
            Invoke("PlayAlreadyToUnlockAnim", 0.5f);
        }
    }

    public void PlayAlreadyToUnlockAnim()
    {
        if (animatorStatus != null)
        {
            animatorStatus.Play("Tour_ReadyToUnlock");
        }
    }

    public void PerformedUnlocking()
    {
        SFXManager.Instance.Play(Define.SFX.UI_UNLOCK_TOUR);
        animatorStatus.Play("Tour_Unlock");
        status = LOCK_STATUS.UNLOCKING;
        UpdateStatus();
    }

    public bool IsAlready()
    {
        return status == LOCK_STATUS.ALREADY;
    }

    public bool IsLock()
    {
        return status == LOCK_STATUS.LOCK;
    }

    public void UpdateStatus()
    {
        switch (status)
        {
            case LOCK_STATUS.LOCK:
                SetLock();
                break;

            case LOCK_STATUS.ALREADY:
                SetAlready();
                break;
            case LOCK_STATUS.UNLOCKING:
                Invoke("UnlockVenue", 1);
                status = LOCK_STATUS.UNLOCKED;
                break;

            case LOCK_STATUS.UNLOCKED:
                
                break;
        }
    }

    private void UnlockVenue()
    {
        viewVenue.UnlockVenue();
    }


    void SetLock()
    {
        statusUI.SetActive(true);
        lockStatusUI.SetActive(true);
        unlockStatusUI.SetActive(false);
    }

    void SetAlready()
    {
        statusUI.SetActive(true);
        lockStatusUI.SetActive(false);
        unlockStatusUI.SetActive(true);
    }

    public void SetUnlockUI()
    {
        statusUI.SetActive(false);
    }

    public int ID
    {
        get { return venueIndex; }
        set { venueIndex = value; }
    }

    bool AnimatorIsPlaying()
    {
        return animatorStatus.GetCurrentAnimatorStateInfo(0).length >
            animatorStatus.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && animatorStatus.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    // bool IsUnlockSpecialMove(int VenueId)
    // {
    //     ConcertVenues venues = Database.GetConcertVenues()[VenueId];

    //     SpecialMoveStore store = Database.GetSpecialMoveStore();
    //     for(int i = 0; i < store.specialMoveChars.Length; i ++)
    //     {
    //         List<SpecialMoveItem> listSMItem = store.specialMoveChars[i].specialMoveItems;
    //         for(int j = 0; j < listSMItem.Count; j++)
    //         {
    //             if(listSMItem[j].status == Define.ITEM_STATUS.PURCHASE
    //                 && listSMItem[j].venueRequirement == venues.ID
    //             )
    //                 return true;
    //         }
    //     }
    //     return false;
    // }

    // bool IsUnlockCostume(int VenueId)
    // {
    //     ConcertVenues venues = Database.GetConcertVenues()[VenueId];

    //     CostumeStore costumeStore = Database.GetCostumeStore();
    //     for (int i = 0; i < costumeStore.costumeChars.Length; i++)
    //     {
    //         for (int j = 0; j < costumeStore.costumeChars[i].costumes.Count; j++)
    //         {
    //             if(costumeStore.costumeChars[i].costumes[j].status == Define.ITEM_STATUS.PURCHASE
    //                 && costumeStore.costumeChars[i].costumes[j].venueRequirement == venues.ID
    //             )
    //             {
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }

    // void ShowPopupVenue(REWARD_TYPE rewardType, float timeStart, float timeEnd)
    // {
    //         var popup = GameObject.Find(Define.POPUP_REWARD);
    //         if(popup == null)
    //         {
    //             UIPopup popupAchievement = UIPopup.GetPopup(Define.POPUP_REWARD);
    //             popupAchievement.name = Define.POPUP_REWARD;
    //             popupAchievement.Show();
    //             SFXManager.Instance.Play(Define.SFX.UI_MESSAGE_POP_UP);
    //             popupAchievement.GetComponent<Reward>().Init((int)rewardType);
    //         }
    //         else
    //         {
    //             DOTween.To(()=> timeStart, x => timeStart = x, timeEnd, timeEnd).OnComplete(() => {
    //                 popup.GetComponent<UIPopup>().AutoHideAfterShowDelay = timeEnd + TIME_SHOW_ONCE_POPUP;
    //                 Reward rewardScript = popup.GetComponent<Reward>();
    //                 rewardScript.Init((int)rewardType);
    //                 rewardScript.PlayEffect();
    //             });       
    //         }
    // }
}
