using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using gameoptions;
using Doozy.Engine.UI;
using System;

public class ViewInGame : MonoBehaviour
{
    public TextMeshProUGUI textScore;
    public GameObject debugInfo;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI cameraText;
    public GameObject textGreatPrefab;
    public GameObject textPerfectPrefab;
    public Transform transTextPerfect;
    public GameObject btnSkipTutorial;

    [Header("ProgressBar")]
    public GameObject objectScoreBarParent;
    public GameObject objectScoreBar;
    public Image barFill;
    public RectTransform barFillEffect;
    public GameObject[] discs;
    public GameObject[] markDiscs;
    public ParticleSystem[] discGainParticles;

    [Header("RockBar")]
    public GameObject[] rockFills;
    public GameObject rockBlink;
    public ParticleSystem specialMoveRockBar;
    public ParticleSystem specialMoveLoopParticle;
    public Color rockEmptyColor;
    public Color rockFillColor = Color.white;
    public Animator rockBarAnim;

    private float TIME_BLINK = 0.2f;

    //const
    const int SILVER = 0;
    const int GOLD = 1;
    const int PLATINUM = 2;
    const string TRIGGER_IDLE = "TriggerIdle";
    const string TRIGGER_95 = "Trigger95";
    const string TRIGGER_FULL = "TriggerFull";
    const float TIME_DISC_FADE_IN = 2f;

    //private variables
    private float barScaleSilver = 0;
    private float barScaleGold = 0;
    private float barScalePlatinum = 0;
    public GameObject gradientBar;
    public GameObject[] dicsContainer;
    private int rockFillIndex = 0;
    private List<GameObject> listPerfectObject = new List<GameObject>();
    private List<GameObject> listGreatObject = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Init();
        // Sequence blink_sequence = DOTween.Sequence();
        // blink_sequence.Append(rockBlink.GetComponent<Image>().DOFade(0f, TIME_BLINK));
        // blink_sequence.Append(rockBlink.GetComponent<Image>().DOFade(1f, TIME_BLINK));
        // blink_sequence.SetLoops(-1);
    }

    void OnEnable()
    {
        if(!Game.Instance.m_IsInitData)
        {
            return;
        }
        
        Game.Instance.SetViewState(Define.VIEW.INGAME);
        if(TutorialManager.Instance != null)
        {
            btnSkipTutorial.SetActive(TutorialManager.Instance.Tutorial != Define.TUTORIAL.DONE && Define.ENABLE_CHEAT);
        }
        Game.Instance.DestroyObjInit();        
    }

    public void Init()
    {   
        timerText.text = "Time : 00:00:00.000";

        barScalePlatinum = gradientBar.GetComponent<RectTransform>().sizeDelta.x; // barScalePlatinum = dicsContainer[(int)Define.DISC_TYPE.PLATINUM - 1].GetComponent<RectTransform>().anchoredPosition.x + dicsContainer[(int)Define.DISC_TYPE.PLATINUM - 1].GetComponent<RectTransform>().sizeDelta.x/2;
        barScaleSilver = dicsContainer[(int)Define.DISC_TYPE.SILVER - 1].GetComponent<RectTransform>().anchoredPosition.x + dicsContainer[(int)Define.DISC_TYPE.SILVER - 1].GetComponent<RectTransform>().sizeDelta.x/2; //barScaleSilver = (dicsContainer[(int)Define.DISC_TYPE.SILVER - 1].GetComponent<RectTransform>().offsetMin.x + dicsContainer[(int)Define.DISC_TYPE.SILVER - 1].GetComponent<RectTransform>().offsetMax.x)/2;
        barScaleGold = dicsContainer[(int)Define.DISC_TYPE.GOLD - 1].GetComponent<RectTransform>().anchoredPosition.x + dicsContainer[(int)Define.DISC_TYPE.GOLD - 1].GetComponent<RectTransform>().sizeDelta.x/2;
        
        UpdateSongTimerText(0);
        debugInfo.SetActive(Define.IsShowDebugInfo && Define.ENABLE_CHEAT);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBackKey();
        
        if(GameManager.Instance.GetState() != Define.GAME_STATE.INGAME && GameManager.Instance.GetState() != Define.GAME_STATE.SPECIAL_MOVE)
        {
            return;
        }

        if(Define.IsShowDebugInfo != debugInfo.activeSelf)
        {
            debugInfo.SetActive(Define.IsShowDebugInfo && Define.ENABLE_CHEAT);
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if(ScoreManager.Instance)
        {
            UpdateScore(ScoreManager.Instance.GetScore());
            
        }
    }

    public void UpdateScore(int score)
    {
        textScore.text = "" + score;
        GameUtils.Instance.UpdateTextUnderlay(textScore);
    }

    public void InitRockFill()
    {
        rockFillIndex = 0;
        Color c;
        for(int i = 0; i < rockFills.Length; i++)
        {
            c = rockEmptyColor;
            c.a = rockFills[i].GetComponent<Image>().color.a;
            rockFills[i].GetComponent<Image>().color = c;
        }
        SetBlinkIndex(0);
        rockBlink.SetActive(false);
        ScoreManager.Instance.ResetRockPoint();
    }

    public void SetRockFull()
    {
        for(int i = 0; i < rockFills.Length; i++)
        {            
            rockFills[i].GetComponent<Image>().color = rockFillColor;
        }
    }

    void UpdateRockFill(float percent)
    {
        if(percent > 0 && !rockBlink.activeSelf && GameManager.Instance.GetState() != Define.GAME_STATE.ENDGAME)
        {
            rockBlink.SetActive(true);
        }
        
        if(percent >= (float)(rockFillIndex + 1) / (float)rockFills.Length)
        {
            if(rockFillIndex < rockFills.Length - 1)
            {
                int nextIndex = rockFillIndex + 1;
                SetBlinkIndex(nextIndex);
            }
            else
            {
                RemoveBlink();
            }
        }
    }

    void SetBlinkIndex(int nextIndex)
    {
        if(nextIndex != 0) rockFills[rockFillIndex].GetComponent<Image>().color = rockFillColor;
        rockFillIndex = nextIndex;
        rockBlink.transform.rotation = rockFills[rockFillIndex].transform.rotation;
    }
    void RemoveBlink()
    {
        rockBlink.SetActive(false);
    }

    public void UpdateFillPercent(float percent)
    {
        if(percent <= (float)(rockFillIndex) / (float)rockFills.Length)
        {
            if(rockFillIndex > 0)
            {
                rockFills[rockFillIndex].GetComponent<Image>().color = rockEmptyColor;
                rockFillIndex -= 1;
            }
            
            if(percent <= 0)
            {
                rockFillIndex = 0;
                rockFills[rockFillIndex].GetComponent<Image>().color = rockEmptyColor;
            }
        }
        else
        {
            if(rockFills[rockFillIndex].GetComponent<Image>().color == rockEmptyColor)
            {
                rockFills[rockFillIndex].GetComponent<Image>().color = rockFillColor;
            }
        }
    }

    public void UpdateSongTimerText(float timer)
    {
        if(Define.IsShowDebugInfo)
        {
            if((GameManager.HasInstance() && GameManager.Instance.GetState() == Define.GAME_STATE.INGAME || GameManager.Instance.GetState() == Define.GAME_STATE.SPECIAL_MOVE || GameManager.Instance.GetState() == Define.GAME_STATE.ENDGAME))
            {
                timerText.text = "Time :" + TimerConvert.FloatToTime(timer);
            }
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && Game.Instance.m_IsInitData)
        {
            GameManager.Instance.OnPauseGame();
        }
    }

    public void ShowTextPerfect()
    {
        GameObject go = GetFreeObject(true);
        go.GetComponent<PerfectText>().SetComboText(ScoreManager.Instance.GetStreakPerfect());
        PlayAnimAndDeactive(go, "Text_Perfect");
    }

    public void ShowTextGreat()
    {
        GameObject go = GetFreeObject(false);
        PlayAnimAndDeactive(go, "Text_Great");
    }

    void PlayAnimAndDeactive(GameObject target, string clipName)
    {
        target.SetActive(true);
        Animator anim = target.GetComponent<Animator>();
        anim.Play(clipName);
        DOTween.To(x => {}, 0, 1, 0.35f).OnComplete(() => {
            target.SetActive(false);
        });
    }

    GameObject GetFreeObject(bool isPerfect)
    {
        if(isPerfect)
        {
            for(int i = 0; i < listPerfectObject.Count; i++)
            {
                if(!listPerfectObject[i].activeSelf)
                    return listPerfectObject[i];
            }
            //can not find
            GameObject obj;
            obj = Instantiate(textPerfectPrefab, transTextPerfect);
            obj.name = "STR_PERFECT";
            obj.SetActive(false);
            listPerfectObject.Add(obj);
            return listPerfectObject[listPerfectObject.Count - 1];
        }
        else
        {
            for(int i = 0; i < listGreatObject.Count; i++)
            {
                if(!listGreatObject[i].activeSelf)
                    return listGreatObject[i];
            }
            //can not find
            GameObject obj;
            obj = Instantiate(textGreatPrefab, transTextPerfect);
            obj.name = "STR_GREAT";
            obj.SetActive(false);
            listGreatObject.Add(obj);
            return listGreatObject[listGreatObject.Count - 1];
        }
    }

    public void UpdateBackKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(TutorialManager.Instance.IsNeedPause())
            {
                AndroidToast.ShowCannotBack();
            }
            else
            {
                GameManager.Instance.OnPauseGame();
                SFXManager.Instance.Play(Define.SFX.UI_MESSAGE_POP_UP);
            }     
        }
    }

    public void SkipTutorial()
    {
        GameManager.Instance.SetState(Define.GAME_STATE.NONE);
        Time.timeScale = 1;
        LoadingManager.Instance.StartLoadingToExitAP();
        GameEventMgr.SendEvent("ingame_to_waiting");
        TutorialManager.Instance.SkipTutorial();               
    }

    public void PreloadUIVFX()
    {
        
    }
    public void StopAllUIVFX()
    {
    }
}
