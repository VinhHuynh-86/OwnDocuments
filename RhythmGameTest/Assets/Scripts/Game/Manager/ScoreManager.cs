using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using SimpleJSON;
using OPS.AntiCheat;
using OPS.AntiCheat.Field;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance = null;
    public GameObject PrefabPerfectEffect;
    public GameObject PrefabGreatEffect;

    [Header("Score")]
    public int SCORE_SHORT_NOTE;
    public int SCORE_LONG_NOTE;
    public int SCORE_SWIPE_NOTE;
    public int SCORE_DRAG_NOTE;
    public int SCORE_LONG_NOTE_HOLDING;
    public int SCORE_PERFECT_COMBO;
    public float COOL_MULTIPLIER;
    public float GREAT_MULTIPLIER;
    public float PERFECT_MULTIPLIER;

    [Header("Rock Point")]
    public int ROCK_POINT_COOL = 1;
    public int ROCK_POINT_GREAT = 2;
    public int ROCK_POINT_PERFECT = 3;
    public int ROCK_POINT_HOLD = 1;
    public int ROCK_POINT_COMBO = 1;
    public int ROCK_POINT_EXTRA = 2;
    public int ROCK_FULL_EASY = 518;
    public int ROCK_FULL_MEDIUM = 777;
    public int ROCK_FULL_HARD = 1036;
    public float ROCK_METER_MULTIPLIER = 3;
    // public TextMeshProUGUI RockPointText;

    [Header("Coin")]
    public int BASE_SCORE_PER_COIN = 1500;
    public float COIN_MULTI_EASY = 1f;
    public float COIN_MULTI_MEDIUM = 1.5f;
    public float COIN_MULTI_HARD = 2f;

    [Header("Other")]
    public float FAN_MULTIPLIER;
    public float MONEY_MULTIPLIER;
    public float HOLD_TIME;
    public int NUMBER_PERFECT_CHEER = 20;

    /////////////////////////////////////////////////////////
    // private variables
    /////////////////////////////////////////////////////////
    private ProtectedInt32 _score;
    private int streak;
    private int streakMiss;
    private int streakPerfect;
    private int rockPoint;
    private int rockFull;
    private int combo;
    private int scoreSilver;
    private int scoreGold;
    private int scorePlatinum;
    private List<GameObject> listPerfectEffect = new List<GameObject>();
    private List<GameObject> listGreatEffect = new List<GameObject>();    

    private int score
    {
        get { return _score; }
        set { _score = value; }
    }

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        GameObject obj;
        for(int i = 0; i < 3; ++i)
        {
            obj = Instantiate(PrefabGreatEffect, transform);
            obj.SetActive(false);
            listGreatEffect.Add(obj); 
        }

        for(int i = 0; i < 3; ++i)
        {
            obj = Instantiate(PrefabPerfectEffect, transform);
            obj.SetActive(false);
            listPerfectEffect.Add(obj);
        }
    }

    // void Start()
    // {
    //     Init();        
    // }

    public void Init()
    {
        score = 0;
        streak = 0;
        streakMiss = 0;
        streakPerfect = 0;
        rockPoint = 0;
        combo = 0;        
        if(Game.Instance != null)
        {
            switch(Game.Instance.GetGameMode())
            {
                case Define.GAME_MODE.EASY:
                    rockFull = ROCK_FULL_EASY;
                break;
                case Define.GAME_MODE.MEDIUM:
                    rockFull = ROCK_FULL_MEDIUM;
                break;
                case Define.GAME_MODE.HARD:
                    rockFull = ROCK_FULL_HARD;
                break;
            }
        }
        SetScoreDisc();
        // RockPointText.text = rockPoint + " / " + rockFull;
    }

    public void SetScore(int _score)
    {
        score = _score;
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int _score)
    {
        score += _score;
        combo ++;
    }

    public void SetStreak(int _streak)
    {
        streak = _streak;
    }

    public int GetStreak()
    {
        return streak;
    }

    public void AddStreak(int _streak)
    {
        streak += _streak;
    }

    public void CounterStreak()
    {
        streak ++;
    }

    public void ResetStreak()
    {
        streak = 0;
    }

    public void SetStreakMiss(int _streakMiss)
    {
        streakMiss = _streakMiss;
    }    

    public int GetStreakMiss()
    {
        return streakMiss;
    }

    public void AddStreakMiss(int _streakMiss)
    {
        streakMiss += _streakMiss;
    }

    public void CounterStreakMiss()
    {
        streakMiss ++;
    }

    public void ResetStreakMiss()
    {
        streakMiss = 0;
    }

    public void SetStreakPerfect(int _streakPerfect)
    {
        streakPerfect = _streakPerfect;
        GameManager.Instance.SetHighestPerfect(GameManager.Instance.GetHighestPerfect() < streakPerfect ? streakPerfect : GameManager.Instance.GetHighestPerfect());
    }

    public int GetStreakPerfect()
    {
        return streakPerfect;
    }

    public void AddStreakPerfect(int _streakPerfect)
    {
        SetStreakPerfect(streakPerfect + _streakPerfect);
    }

    public void CounterStreakPerfect()
    {
        SetStreakPerfect(++streakPerfect);
        if (streakPerfect > 0 && streakPerfect % NUMBER_PERFECT_CHEER == 0)
        {
            SFXManager.Instance.PlayRandomCrowdCheer();
        }
    }

    public void ResetStreakPerfect()
    {
        SetStreakPerfect(0);
    }

    public int GetScoreNote(Define.NOTE_TYPE _type, Define.TRIGGER _trigger, bool isHoldLongNode = false)
    {
        int _score = 0;

        switch(_type)
        {
            case Define.NOTE_TYPE.SHORT:
                _score = SCORE_SHORT_NOTE;
            break;
            case Define.NOTE_TYPE.LONG:
                _score = SCORE_LONG_NOTE;
            break;
            case Define.NOTE_TYPE.SWIPE:
                _score = SCORE_SWIPE_NOTE;
            break;
            case Define.NOTE_TYPE.DRAG:
                _score = SCORE_DRAG_NOTE;
            break;
        }

        switch(_trigger)
        {
            case Define.TRIGGER.COOL:
                _score = (int)(_score * COOL_MULTIPLIER);
            break;
            case Define.TRIGGER.GREAT:
                _score = (int)(_score * GREAT_MULTIPLIER);;
            break;
            case Define.TRIGGER.PERFECT:
                _score = (int)(_score * PERFECT_MULTIPLIER);
            break;
        }        

        //for hold long node only
        if(isHoldLongNode)
        {
            switch(_trigger)
            {
                case Define.TRIGGER.COOL:
                    _score = (int)(SCORE_LONG_NOTE_HOLDING * COOL_MULTIPLIER);
                break;
                case Define.TRIGGER.GREAT:
                    _score = (int)(SCORE_LONG_NOTE_HOLDING * GREAT_MULTIPLIER);;
                break;
                case Define.TRIGGER.PERFECT:
                    _score = (int)(SCORE_LONG_NOTE_HOLDING * PERFECT_MULTIPLIER);
                break;
            }  
        }

        else if(GetStreakPerfect() > 1)
        {
            _score += SCORE_PERFECT_COMBO;
        }

        return _score;
    }

    public void ResetCombo()
    {
        combo = 0;
    }

    public int GetCombo()
    {
        return combo;
    }

    public void ResetRockPoint()
    {
        rockPoint = 0;
    }

    public int GetRockPoint()
    {
        return rockPoint;
    }

    public float GetRockPointPercent()
    {        
        return (float)rockPoint/(float)rockFull;
    }

    public bool IsRockFull()
    {
        return rockPoint >= rockFull;
    }

    public void AddRockPointByType(Define.NOTE_TYPE _type, Define.TRIGGER _trigger)
    {
        if(_type == Define.NOTE_TYPE.LONG)
        {
            rockPoint += ROCK_POINT_HOLD;
        }
        else
        {
            switch(_trigger)
            {
                case Define.TRIGGER.COOL:
                    rockPoint += ROCK_POINT_COOL;
                break;
                case Define.TRIGGER.GREAT:
                    rockPoint += ROCK_POINT_GREAT;
                break;
                case Define.TRIGGER.PERFECT:
                    rockPoint += ROCK_POINT_PERFECT;
                break;
            }
            if(GetCombo() > 1)
            {
                rockPoint += ROCK_POINT_COMBO;
            }
            if(GetStreakPerfect() > 1)
            {
                rockPoint += ROCK_POINT_EXTRA;
            }
        }        
        // RockPointText.text = rockPoint + " / " + rockFull;
    }

    private GameObject GetInactiveScoreEffect(Define.TRIGGER _trigger)
    {
        switch(_trigger)
        {
            case Define.TRIGGER.GREAT:
                if(listGreatEffect.Count == 0) return null;
                for(int i = 0; i < listGreatEffect.Count; i++)
                {
                    if(!listGreatEffect[i].activeSelf)
                    {
                        return listGreatEffect[i];
                    }
                }
                return null;
            case Define.TRIGGER.PERFECT:
                if(listPerfectEffect.Count == 0) return null;
                for(int i = 0; i < listPerfectEffect.Count; i++)
                {
                    if(!listPerfectEffect[i].activeSelf)
                    {
                        return listPerfectEffect[i];
                    }
                }
                return null;              
        } 
        return null;      
    }

    public void PlayScoreEffect(Define.TRIGGER _trigger, Transform pos, int score)
    {
        if(_trigger == Define.TRIGGER.COOL || _trigger == Define.TRIGGER.NONE)
            return;
        GameObject obj = GetInactiveScoreEffect(_trigger);
        
        if(obj == null)
        {
            switch(_trigger)
            {
                case Define.TRIGGER.GREAT:
                    obj = Instantiate(PrefabGreatEffect, transform);
                    listGreatEffect.Add(obj);                    
                    break;
                case Define.TRIGGER.PERFECT:                    
                    obj = Instantiate(PrefabPerfectEffect, transform);
                    listPerfectEffect.Add(obj);
                    break;
            }             
        }

        obj.SetActive(true);
        obj.transform.position = new Vector3(pos.position.x, obj.transform.position.y, pos.position.z);
        for(int i = 0; i < obj.transform.childCount; i++)
        {
            TextMeshPro tm = obj.transform.GetChild(i).GetComponent<TextMeshPro>();
            tm.text = "+"+score;
            tm.DOFade(0f, 0.5f).SetEase(Ease.InQuart).OnComplete(() => {
                tm.DOFade(1f, 0.001f);
            });;
        }
        Sequence mySequence = DOTween.Sequence();            
        mySequence.Append(obj.transform.DOMoveZ(obj.transform.position.z + 2f, 0.5f));
            mySequence.OnComplete(() => {
                obj.SetActive(false);
            });
    }

    public int GetScoreSilver()
    {
        return scoreSilver;
    }

    public int GetScoreGold()
    {        
        return scoreGold;    
    }

    public int GetScorePlatinum()
    {            
        return scorePlatinum;
    }

    public void SetScoreDisc()
    {
        SongDifficulty sd = Database.GetSongDifficultyByID(Game.Instance.GetSongDifficultyID());
        if(sd != null)
        {
            scoreSilver = sd.scoreToGainSilver;
            scoreGold = sd.scoreToGainGold;
            scorePlatinum = sd.scoreToGainPlatinum;
        }
    }

    public void SetScoreSilver()
    {
        score = scoreSilver;
    }

    public void SetScoreGold()
    {
        score = scoreGold;
    }

    public void SetScorePlatinum()
    {
        score = scorePlatinum;
    }

    public int GetCoin(Define.GAME_MODE gameMode)
    {
        int coin = 0;
        switch(gameMode)
        {
            case Define.GAME_MODE.EASY:
                coin = (int)((score/BASE_SCORE_PER_COIN)*COIN_MULTI_EASY);
            break;
            case Define.GAME_MODE.MEDIUM:
                coin = (int)((score/BASE_SCORE_PER_COIN)*COIN_MULTI_MEDIUM);
            break;
            case Define.GAME_MODE.HARD:
                coin = (int)((score/BASE_SCORE_PER_COIN)*COIN_MULTI_HARD);
            break;
        }

        return coin;
    }

    public void SetRockMeterConfig(string remoteConfig)
    {
        string decryptConfig = VCrypto.Decrypt(remoteConfig, Define.ENCRYPT_KEY_VERSION);

        JSONNode jData;
        jData = JSON.Parse(decryptConfig)[Define.ROCKMETER_SHEET];
        if (jData == null)
        {
            return;
        }

        foreach (JSONNode node in jData.Children)
        {
            if (Database.IsEndJson(node))
            {
                break;
            }

            string id = node[Define.ROCKMETER_ELEMENT_NAME.ID].Value;
            int value = node[Define.ROCKMETER_ELEMENT_NAME.Value].AsInt;
            switch (id)
            {
                case "ROCK_FULL_EASY":
                    ROCK_FULL_EASY = value;
                    break;
                case "ROCK_FULL_MEDIUM":
                    ROCK_FULL_MEDIUM = value;
                    break;
                case "ROCK_FULL_HARD":
                    ROCK_FULL_HARD = value;
                    break;
            }
        }
    }
}
