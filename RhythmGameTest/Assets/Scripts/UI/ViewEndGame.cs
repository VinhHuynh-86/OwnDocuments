using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Doozy.Engine.UI;
using System.Collections.Generic;
using System;

using OPS.AntiCheat;
using OPS.AntiCheat.Field;

public class ViewEndGame : MonoBehaviour
{
    public TextMeshProUGUI textScore;
    // public TextMeshProUGUI textCoin;
    public Button btnBackMenu;
    public Button btnNext;
    public Transform transTitle;
    public TextMeshProUGUI textSubTitle;
    public TopBarUI topbarUI;
    [Header("ProgressBar")]
    public Image barFill;
    public GameObject[] discs;
    public GameObject[] markDiscs;
    public ParticleSystem[] discGainParticles;
    public GameObject[] discAnimPrefab;
    public GameObject coinAnimPrefab;
    public RectTransform[] discsPos;
    // public RectTransform coinPos;
    public RectTransform discTartgetPos;
    public RectTransform coinTartgetPos;
    public Animator discAnim;
    public Animator coinAnim;
    public GameObject gradientBar;
    public GameObject[] dicsContainer;
    public GameObject objTextReward;

    [SerializeField] Transform RewardContainer;
    [SerializeField] GameObject RewardItemPrefab;
    [SerializeField] List<GameObject> mRewardItemList = new List<GameObject>();

    private ProtectedInt32 mCoin;
    private int mScore;
    private Timer coinTimer = new Timer();
    private Define.DISC_TYPE discGained;
    private int mDiscCollect;
    private SongDifficulty songDiff;
    private float barScaleSilver = 0;
    private float barScaleGold = 0;
    private float barScalePlatinum = 0;
    private float currentDiscCollected;
    private bool isUnlockArchive = false;
    private bool isUnlockSong = false;

    enum REWARD_TYPE : int
    {
        ARCHIVE = 0,
        SONG,
        COINS,
        VENUE,
        COSTUME,
        SPECIAL_MOVE,
    }

    public enum END_STATE
    {
        INIT,
        ANIM_SCORE_DISC,
        INIT_REWARD,
        UNLOCK_ARCHIVE,
        HIDE_POPUP_ARCHIVE,
        UNLOCK_SONG,
        HIDE_POPUP_SONG,
        REWARD_COIN,
        FINISHED
    }
    [SerializeField]
    END_STATE mState = END_STATE.INIT;
    Timer timerControl = new Timer();
    Reward rewardCoinsScript;

    //const
    const int SILVER = 0;
    const int GOLD = 1;
    const int PLATINUM = 2;
    const float TIME_ANIM_SCORE = 1f;
    const float TIME_ANIM_DISC = 0.5f;
    const float TIME_ANIM_COIN = 1f;
    const float ANIM_DURATION = 1f;
    const float TIME_SHOW_POPUP = 1.5f;
    const float TIME_REWARD_COIN = 2.5f;
    const string TRIGGER_GLOW = "TriggerGlow";
    const string TRIGGER_GLOW_END = "TriggerGlowEnd";
    const string REWARD_ARCHIVE = "rewardArchive";
    const string REWARD_SONG = "rewardSong";
    const string REWARD_COIN = "rewardCoin";
    const float SCALE_TITLE = 0.73f;

    void OnEnable()
    {
        if(!Game.Instance.m_IsInitData) return;
        GameUtils.Instance.ScaleTitle(transTitle, SCALE_TITLE);
        SetState(END_STATE.INIT);
    }

    void OnDisable()
    {
        if(mRewardItemList.Count > 0)
        {
            for(int i = 0; i < mRewardItemList.Count; i++)
                Destroy(mRewardItemList[i]);
            mRewardItemList.Clear();
        }
    }

    
    void Update()
    {
        switch(mState)
        {
            case END_STATE.ANIM_SCORE_DISC:
                timerControl.Update(Time.deltaTime);
                if (timerControl.JustFinished())
                {
                    SetState(END_STATE.INIT_REWARD);
                }
            break;
            case END_STATE.UNLOCK_ARCHIVE:
                if(isUnlockArchive)
                {
                    timerControl.Update(Time.deltaTime);
                    if (timerControl.JustFinished())
                    {
                        for(int i = 0; i < mRewardItemList.Count; i++)
                        {
                            if(mRewardItemList[i].name == REWARD_ARCHIVE)
                            {
                                mRewardItemList[i].SetActive(true);
                            }
                        }
                        SetState(END_STATE.HIDE_POPUP_ARCHIVE);
                    }
                }
                else
                {
                    SetState(END_STATE.UNLOCK_SONG);
                }
            break;

            case END_STATE.HIDE_POPUP_ARCHIVE:
                    timerControl.Update(Time.deltaTime);
                    if (timerControl.JustFinished())
                    {
                        SetState(END_STATE.UNLOCK_SONG);
                    }
            break;

            case END_STATE.UNLOCK_SONG:
                if(isUnlockSong)
                {
                    timerControl.Update(Time.deltaTime);
                    if (timerControl.JustFinished())
                    {
                        for(int i = 0; i < mRewardItemList.Count; i++)
                        {
                            if(mRewardItemList[i].name == REWARD_SONG)
                            {
                                mRewardItemList[i].SetActive(true);
                            }
                        }
                        SetState(END_STATE.HIDE_POPUP_SONG);
                    }
                }
                else
                {
                    SetState(END_STATE.REWARD_COIN);
                }
            break;

            case END_STATE.HIDE_POPUP_SONG:
                timerControl.Update(Time.deltaTime);
                if (timerControl.JustFinished())
                {
                    SetState(END_STATE.REWARD_COIN);
                }
            break;

            case END_STATE.REWARD_COIN:
                timerControl.Update(Time.deltaTime);
                if (timerControl.JustFinished())
                {
                    SetState(END_STATE.FINISHED);
                }
            break;
        }
        UpdateBackKey();
    }

    void InitReward()
    {
        objTextReward.SetActive(true);
        
        GameObject rewardCoins = Instantiate(RewardItemPrefab, RewardContainer);
        rewardCoins.name = REWARD_COIN;

        rewardCoinsScript = rewardCoins.GetComponent<Reward>();
        rewardCoins.GetComponent<Reward>().Init((int)REWARD_TYPE.COINS);
        rewardCoins.SetActive(false);
        mRewardItemList.Add(rewardCoins);
        SetState(END_STATE.UNLOCK_ARCHIVE);
    }

    public void UpdateCoin()
    {
        if (mScore > songDiff.bestScore)
        {
            Database.SetBestScoreSongDifficulty(songDiff, mScore);
        }

        
        {
            coinTimer.SetDuration(0.1f);
            int tmpCoin = 0;
            DOTween.To(() => tmpCoin, x => tmpCoin = x, mCoin, TIME_ANIM_COIN).OnStart(() =>
            {
                coinAnim.SetTrigger(TRIGGER_GLOW);
                SFXManager.Instance.Play(Define.SFX.UI_COLLECT_COINS);
            }).OnUpdate(() =>
            {
                UpdateCoinCounter(tmpCoin, Time.deltaTime);
            }).OnComplete(() =>
            {
                topbarUI.UpdateCoin(mCoin);
                DOTween.To(x => { }, 0, 1, TIME_ANIM_COIN).OnComplete(() =>
                {
                    coinAnim.SetTrigger(TRIGGER_GLOW_END);                        
                });
            });
        }
    }

    public void ShowButton()
    {
        btnBackMenu.gameObject.SetActive(true);
        btnNext.gameObject.SetActive(true);
    }

    public void HideButton()
    {
        btnBackMenu.gameObject.SetActive(false);
        btnNext.gameObject.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        textScore.text = "" + score;
        GameUtils.Instance.UpdateTextUnderlay(textScore);
        int scoreSilver = ScoreManager.Instance.GetScoreSilver();
        int scoreGold = ScoreManager.Instance.GetScoreGold();
        int scorePlatinum = ScoreManager.Instance.GetScorePlatinum();
        barScalePlatinum = gradientBar.GetComponent<RectTransform>().sizeDelta.x;
        barScaleSilver = dicsContainer[(int)Define.DISC_TYPE.SILVER - 1].GetComponent<RectTransform>().anchoredPosition.x + dicsContainer[(int)Define.DISC_TYPE.SILVER - 1].GetComponent<RectTransform>().sizeDelta.x/2;
        barScaleGold = dicsContainer[(int)Define.DISC_TYPE.GOLD - 1].GetComponent<RectTransform>().anchoredPosition.x + dicsContainer[(int)Define.DISC_TYPE.GOLD - 1].GetComponent<RectTransform>().sizeDelta.x/2;
        float progress = 0;

        if (score < scoreSilver)
        {
            progress = barScaleSilver * (float)score / (float)scoreSilver;
        }
        else if (score >= scoreSilver && score < scoreGold)
        {
            progress = barScaleSilver + (barScaleGold - barScaleSilver) * (float)(score - scoreSilver) / (float)(scoreGold - scoreSilver);
            if (!discs[SILVER].activeSelf)
            {
                discs[SILVER].SetActive(true);
                markDiscs[SILVER].SetActive(false);
                SFXManager.Instance.Play(Define.SFX.UI_EARN_DISC_SILVER);
                if((int)Define.DISC_TYPE.SILVER > currentDiscCollected)
                    PlayAnimDisc(SILVER);
            }
        }
        else if (score >= scoreGold && score < scorePlatinum)
        {
            progress = barScaleGold + (barScalePlatinum - barScaleGold) * (float)(score - scoreGold) / (float)(scorePlatinum - scoreGold);
            
            if (!discs[GOLD].activeSelf)
            {
                if (!discs[SILVER].activeSelf)
                {
                    discs[SILVER].SetActive(true);
                    markDiscs[SILVER].SetActive(false);
                }

                discs[GOLD].SetActive(true);
                markDiscs[GOLD].SetActive(false);
                SFXManager.Instance.Play(Define.SFX.UI_EARN_DISC_GOLD);
                if((int)Define.DISC_TYPE.GOLD > currentDiscCollected)
                    PlayAnimDisc(GOLD);
            }
        }
        else if (score >= scorePlatinum)
        {
            progress = barScalePlatinum;

            if (!discs[PLATINUM].activeSelf)
            {
                if (!discs[GOLD].activeSelf)
                {
                    if (!discs[SILVER].activeSelf)
                    {
                        discs[SILVER].SetActive(true);
                        markDiscs[SILVER].SetActive(false);
                    }
                    discs[GOLD].SetActive(true);
                    markDiscs[GOLD].SetActive(false);
                }
                discs[PLATINUM].SetActive(true);
                markDiscs[PLATINUM].SetActive(false);
                SFXManager.Instance.Play(Define.SFX.UI_EARN_DISC_PLATINUM);
                if((int)Define.DISC_TYPE.PLATINUM > currentDiscCollected)
                    PlayAnimDisc(PLATINUM);
            }
        }
        UpdateBarScore(progress);
    }

    void UpdateBarScore(float progress)
    {
        RectTransform rt = barFill.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(progress, rt.sizeDelta.y);
        barFill.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
    }

    public void ResetBarScore()
    {
        UpdateBarScore(0);
        textScore.text = "0";
        for (int i = 0; i < discs.Length; i++)
        {
            discs[i].SetActive(false);
            markDiscs[i].SetActive(true);
        }
    }

    private void PlayAnimDisc(int discIndex)
    {
        discGainParticles[discIndex].Play();
        GameObject curDisc = Instantiate(discAnimPrefab[discIndex], transform);
        curDisc.transform.localPosition = discsPos[discIndex].localPosition;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(curDisc.transform.DOLocalMove(discTartgetPos.localPosition, TIME_ANIM_DISC));
        mySequence.Join(curDisc.transform.DOScale(0.7f, TIME_ANIM_DISC));
        mySequence.OnComplete(() =>
        {
            GameObject.Destroy(curDisc);
            discAnim.SetTrigger(TRIGGER_GLOW);            
        });
    }
    private void PlayAnimCoin(int coins)
    {
        GameObject curCoin = Instantiate(coinAnimPrefab, transform);

        Vector3 rewardCoinsPos = new Vector3(0, 0, 0);
        for(int i = 0; i < mRewardItemList.Count; i++)
        {
            if(mRewardItemList[i].name == REWARD_COIN)
            {
                rewardCoinsPos = mRewardItemList[i].transform.localPosition + RewardContainer.localPosition;
            }
        }
        
        curCoin.transform.localPosition = rewardCoinsPos;
        Vector3 nextPos = new Vector3(rewardCoinsPos.x + UnityEngine.Random.Range(-1f, 1f) * 100, rewardCoinsPos.y + UnityEngine.Random.Range(-1f, 1f) * 100, 0f);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(curCoin.transform.DOLocalMove(coinTartgetPos.localPosition, ANIM_DURATION));
        mySequence.Prepend(curCoin.transform.DOLocalMove(nextPos, ANIM_DURATION / 2).SetEase(Ease.OutBack));
        mySequence.OnComplete(() =>
        {
            GameObject.Destroy(curCoin);            
        });
    }

    private void UpdateCoinCounter(int coins, float deltaTime)
    {
        rewardCoinsScript.SetCoins(coins);
        coinTimer.Update(deltaTime);

        if (coinTimer.JustFinished())
        {
            PlayAnimCoin(coins);
            coinTimer.Reset();
        }
    }

    public void UpdateBackKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            {
                GameManager.Instance.OnBackToMainMenu();
                GameEventMgr.SendEvent("Back to Waiting");
                SFXManager.Instance.Play(Define.SFX.UI_MENU_BACK);
            }
        }
    }


    bool IsUnlockSong()
    {
        if ((int)discGained > songDiff.dicsCollect)
        {
            int oldSilverDisc = Database.GetAllDiscType((int)Define.DISC_TYPE.SILVER);
            int oldGoldDisc = Database.GetAllDiscType((int)Define.DISC_TYPE.GOLD);
            int oldPlatiumDisc = Database.GetAllDiscType((int)Define.DISC_TYPE.PLATINUM);
            Database.SetDicsColletSongDifficulty(songDiff, (int)discGained);
            string songUnlockName = Database.UnlockNextSongDifficuty(songDiff);
            if (songUnlockName != "")
            {
                return true;
            }
        }
        return false;
    }

    void SetState(END_STATE state)
    {
        mState = state;
        switch(state)
        {
            case END_STATE.INIT:
                Game.Instance.SetViewState(Define.VIEW.RESULT);
                topbarUI.UpdateResources();
                mDiscCollect = 0;
                isUnlockArchive = false;
                isUnlockSong = false;
                objTextReward.SetActive(false);
                HideButton();
                ResetBarScore();
                textSubTitle.text = Game.Instance.GetTextSongName() + " - " + Game.Instance.GetTextGameMode();
                mScore = ScoreManager.Instance.GetScore();

                songDiff = Database.GetSongDifficultyByID(Game.Instance.GetSongDifficultyID());
                currentDiscCollected = songDiff.dicsCollect;
                
                {
                    SetState(END_STATE.FINISHED);
                }
            break;

            case END_STATE.ANIM_SCORE_DISC:
                SFXManager.Instance.PlayBackGround(Define.SFX.UI_SCORE_TALLY, true);
                int score = 0;
                DOTween.To(() => score, x => score = x, mScore, TIME_ANIM_SCORE).OnUpdate(() =>
                {
                    textScore.text = "" + score;
                    GameUtils.Instance.UpdateTextUnderlay(textScore);
                    UpdateScore(score);
                }).OnComplete(() => {
                    SFXManager.Instance.FadeOutBG();
                    if(mDiscCollect > 0)
                    {
                        timerControl.SetDuration(TIME_ANIM_SCORE);
                    }
                    else
                    {
                        timerControl.SetDuration(0.1f);
                    }
                }); 
            break;

            case END_STATE.INIT_REWARD:
                InitReward();
            break;

            case END_STATE.UNLOCK_ARCHIVE:
                timerControl.SetDuration(0.1f);
                for(int i = 0; i < mRewardItemList.Count; i++)
                {
                    if(mRewardItemList[i].name == REWARD_ARCHIVE)
                    {
                        UIPopup popup = UIPopup.GetPopup(Define.POPUP_REWARD);
                        popup.Show();
                        SFXManager.Instance.Play(Define.SFX.UI_NEW_REWARD_POPUP);
                        Reward popupOK = popup.GetComponent<Reward>();
                        popupOK.Init((int)REWARD_TYPE.ARCHIVE);
                        timerControl.SetDuration(TIME_SHOW_POPUP);
                        isUnlockArchive = true;
                        break;
                    }
                }
            break;

            case END_STATE.HIDE_POPUP_ARCHIVE:
                try
                {
                    UIPopup.HidePopup(Define.POPUP_REWARD);
                }
                catch (Exception) { }  
                timerControl.SetDuration(ANIM_DURATION/2);
            break;

            case END_STATE.UNLOCK_SONG:
                timerControl.SetDuration(0.1f);
                for(int i = 0; i < mRewardItemList.Count; i++)
                {
                    if(mRewardItemList[i].name == REWARD_SONG)
                    {
                        UIPopup popup = UIPopup.GetPopup(Define.POPUP_REWARD);
                        popup.Show();
                        SFXManager.Instance.Play(Define.SFX.UI_NEW_REWARD_POPUP);
                        Reward popupOK = popup.GetComponent<Reward>();
                        popupOK.Init((int)REWARD_TYPE.SONG);
                        timerControl.SetDuration(TIME_SHOW_POPUP);
                        isUnlockSong = true;
                        break;
                    }
                }
            break;

            case END_STATE.HIDE_POPUP_SONG:
                try
                {
                    UIPopup.HidePopup(Define.POPUP_REWARD);
                }
                catch (Exception) { }
                timerControl.SetDuration(ANIM_DURATION/2);
            break;

            case END_STATE.REWARD_COIN:
                timerControl.SetDuration(0.1f);
                for(int i = 0; i < mRewardItemList.Count; i++)
                {
                    if(mRewardItemList[i].name == REWARD_COIN)
                    {
                        mRewardItemList[i].SetActive(true);
                        UpdateCoin();
                        timerControl.SetDuration(TIME_REWARD_COIN);
                        break;
                    }
                }
            break;

            case END_STATE.FINISHED:
                ShowButton();        
                
                {
                    ProfileMgr.Instance.Coin += mCoin;
                    TutorialManager.Instance.UpdateState();
                    if(TutorialManager.Instance.Tutorial == Define.TUTORIAL.DONE)
                    {
                        ProfileMgr.Instance.Save();
                    }
                }
            break;
        }
    }
}
