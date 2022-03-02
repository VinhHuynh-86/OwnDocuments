using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongDifficulty
{
    public string ID;
    public string songID;
    public Define.GAME_MODE difficult;
    public string levelDesign; // .csv file
    public Define.DISC_TYPE discRequirement;
    public string requirementFrom; // ID of another SongDifficulty
    public int scoreToGainPlatinum;
    public int scoreToGainGold;
    public int scoreToGainSilver;
    public int coinsRewardOnPlatinum;
    public int coinsRewardOnGold;
    public int coinsRewardOnSilver;
    public bool unlocked;
    public int dicsCollect;
    public int bestScore;
    public int perfectCombo;
}
