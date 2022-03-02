using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Doozy.Engine.UI;
using System;

using OPS.AntiCheat;
using OPS.AntiCheat.Field;

[System.Serializable]
public class ProfileItem
{
    public string ID;
    public Define.ITEM_STATUS status;
    public Define.NEW_STATUS newStatus;

    public void UpdateInfo(Item item)
    {
        ID = item.ID;
        status = item.status;
        if(status == Define.ITEM_STATUS.DEFAULT)
        {
            newStatus = Define.NEW_STATUS.DONE;
            item.newStatus = Define.NEW_STATUS.DONE;
        }
        else
        {
            newStatus = item.newStatus;
        }
    }
}


[System.Serializable]
public class ProfileConcertVenues
{
    public string ID;
    public bool isUnlocked;
}

[System.Serializable]
public class ProfileSongDifficulty
{
    public string ID;
    public bool isUnlocked;
    public int dicsCollect = 0; //Same 1,2,3Star - 1 Silver, 2: Silver & Gold, 3: Silver, Gold & Platinum
    public int bestScore = 0;
}

public class Profiles
{
    public int structureVersion = 1;
    public int saveVersion = 0;
    public string userID = Define.NULL_VALUE;
    public int Tutorial = (int)Define.TUTORIAL.DONE;
    // public int Age = -1;
    public ProtectedInt32 Coin = new ProtectedInt32(200);
    public ProtectedInt32 Disc = new ProtectedInt32(0);
    public bool mIsGameUnlocked = false;
    public bool firstEnterStudioInfirstLaunch = true; // first launch game
    public int counterPopupRating;
    public string datePopupRating;

    public List<ProfileConcertVenues> concertVenues = new List<ProfileConcertVenues>();
    public List<ProfileSongDifficulty> songDifficulty = new List<ProfileSongDifficulty>();

    public int ToursUnlocked = 0;
    public int CostumesUnlocked = 4;
    public int playedSongDemoIAP = 0;
}

public class ProfileSetting
{
    public bool mIsAudioEnable = true;
    public int mLanguage = -1;
    public bool mVibration = true;
    public bool mIsPushNotificaltion = true;
    public float mMusicVolume = 1f;
    public float mSoundEffects = 1f;
}


public class ProfileMgr : Singleton<ProfileMgr>
{
    private Profiles mProfiles = new Profiles();
    private ProfileSetting mSettings = new ProfileSetting();
    private bool mIsLoaded = false;

    /// for conflict screen
    public bool isSolveConflict = true;
    bool isNeedToDisplayConlictPopup = false;
    Profiles mLocal = null;
    Profiles mCloud = null;
    ///////////////////////////////////////


    // Profile
    public int Tutorial
    {
        get { return mProfiles.Tutorial; }
        set { mProfiles.Tutorial = value; }
    }

    public bool FirstEnterStudioInfirstLaunch
    {
        get { return mProfiles.firstEnterStudioInfirstLaunch; }
        set { mProfiles.firstEnterStudioInfirstLaunch = value; }
    }
    public ProtectedInt32 Coin
    {
        get { return mProfiles.Coin; }
        set { mProfiles.Coin = value; }
    }

    public bool IsGameUnlocked
    {
        get { return mProfiles.mIsGameUnlocked; }
        set { mProfiles.mIsGameUnlocked = value; }
    }

    public int ToursUnlocked
    {
        get { return mProfiles.ToursUnlocked; }
        set { mProfiles.ToursUnlocked = value; }
    }

    public int CounterPopupRating
    {
        get { return mProfiles.counterPopupRating; }
        set { mProfiles.counterPopupRating = value; }
    }


    public int Version
    {
        get { return mProfiles.saveVersion; }
        set { mProfiles.saveVersion = value; }
    }

    public string UserID
    {
        get { return mProfiles.userID; }
        set { mProfiles.userID = value; }
    }

    // Setting Profile
    public bool IsAudioEnable
    {
        get { return mSettings.mIsAudioEnable; }
        set { mSettings.mIsAudioEnable = value; }
    }

    public int Language
    {
        get { return mSettings.mLanguage; }
        set { mSettings.mLanguage = value; }
    }

    public bool Vibration
    {
        get { return mSettings.mVibration; }
        set { mSettings.mVibration = value; }
    }

    public float MusicVolume
    {
        get { return mSettings.mMusicVolume; }
        set { mSettings.mMusicVolume = value; }
    }

    public float SoundEffects
    {
        get { return mSettings.mSoundEffects; }
        set { mSettings.mSoundEffects = value; }
    }

    public void Init()
    {

        if (PlayerPrefs.HasKey(Define.KEY_PROFILES))
        {
            Load();
        }
        else
        {
            mIsLoaded = true;
            Initialize();
        }

        // if (IAPManager.Instance.CheckUnlockGame())
        {
            IsGameUnlocked = true;
        }
        TutorialManager.Instance.Tutorial = (Define.TUTORIAL)Tutorial;
    }


    public void InitSettings()
    {
        if (PlayerPrefs.HasKey(Define.KEY_SETTINGS))
        {
            string json = PlayerPrefs.GetString(Define.KEY_SETTINGS);
            mSettings = JsonUtility.FromJson<ProfileSetting>(json);
        }
    }

    public void Save()
    {
        if (mIsLoaded)
        {
            SaveProfile();
            SaveSetting();
        }
    }

    public string SaveProfile()
    {
        UpdateProfile();
        return OnlySaveProfile();
    }

    public string OnlySaveProfile()
    {
        string json = JsonUtility.ToJson(mProfiles);
        string encrtyptProfile = VCrypto.Encrypt(json, Define.ENCRYPT_KEY_VERSION);
        PlayerPrefs.SetString(Define.KEY_PROFILES, encrtyptProfile);
        
        return encrtyptProfile;
    }

    public void SaveSetting()
    {
        string settingString = JsonUtility.ToJson(mSettings);
        PlayerPrefs.SetString(Define.KEY_SETTINGS, settingString);
    }

    public void Load()
    {
        if (!mIsLoaded)
        {
            string json = PlayerPrefs.GetString(Define.KEY_PROFILES);
            string decryptProfile = VCrypto.Decrypt(json, Define.ENCRYPT_KEY_VERSION);
            mProfiles = JsonUtility.FromJson<Profiles>(decryptProfile);
            mIsLoaded = true;
            UpdateDatabase();            
        }
    }

    public void ResetProfile()
    {
        PlayerPrefs.DeleteKey(Define.KEY_PROFILES);
    }

    public void ResetSetting()
    {
        PlayerPrefs.DeleteKey(Define.KEY_SETTINGS);
    }

    public void ResetAll()
    {
        ResetProfile();
        ResetSetting();
    }

    private void Initialize()
    {
        List<ConcertVenues> venuesList = Database.GetConcertVenues();
        for (int i = 0; i < venuesList.Count; i++)
        {
            ProfileConcertVenues venues = new ProfileConcertVenues();
            venues.ID = venuesList[i].ID;
            venues.isUnlocked = venuesList[i].unlocked;
            mProfiles.concertVenues.Add(venues);
        }

        List<SongDifficulty> difficultyList = Database.GetSongDifficulty();
        for (int i = 0; i < difficultyList.Count; i++)
        {
            ProfileSongDifficulty difficulty = new ProfileSongDifficulty();
            difficulty.ID = difficultyList[i].ID;
            difficulty.isUnlocked = difficultyList[i].unlocked;
            mProfiles.songDifficulty.Add(difficulty);
        }
    }


    private void UpdateDatabase()
    {
        List<ConcertVenues> venuesList = Database.GetConcertVenues();
        for (int i = 0; i < venuesList.Count; i++)
        {
            ProfileConcertVenues venues = mProfiles.concertVenues.Find(x => x.ID == venuesList[i].ID);

            if (venues != null)
            {
                Database.SetUnlockConcertVenuesByIndex(i, venues.isUnlocked);
            }
            else // Add new one to profile
            {
                ProfileConcertVenues newVenues = new ProfileConcertVenues();
                newVenues.ID = venuesList[i].ID;
                newVenues.isUnlocked = venuesList[i].unlocked;
                mProfiles.concertVenues.Add(newVenues);
            }
        }

        List<SongDifficulty> difficultyList = Database.GetSongDifficulty();
        for (int i = 0; i < difficultyList.Count; i++)
        {
            SongDifficulty curSongDifficulty = difficultyList[i];
            ProfileSongDifficulty difficulty = mProfiles.songDifficulty.Find(x => x.ID == curSongDifficulty.ID);

            if (difficulty != null)
            {
                Database.SetUnlockSongDifficultyById(i, difficulty.isUnlocked);
                Database.SetBestScoreSongDifficulty(curSongDifficulty, difficulty.bestScore);
            }
            else // Add new one to profile
            {
                ProfileSongDifficulty newDifficulty = new ProfileSongDifficulty();
                newDifficulty.ID = curSongDifficulty.ID;
                newDifficulty.isUnlocked = curSongDifficulty.unlocked;
                mProfiles.songDifficulty.Add(newDifficulty);
            }
        }
    }

    private void UpdateProfile()
    {  
        List<ConcertVenues> venuesList = Database.GetConcertVenues();
        for (int i = 0; i < mProfiles.concertVenues.Count; i++)
        {
            ConcertVenues venues = venuesList.Find(x => x.ID == mProfiles.concertVenues[i].ID);

            if (venues != null)
            {
                mProfiles.concertVenues[i].isUnlocked = venues.unlocked;
            }
        }

        List<SongDifficulty> difficultyList = Database.GetSongDifficulty();
        for (int i = 0; i < mProfiles.songDifficulty.Count; i++)
        {
            SongDifficulty difficulty = difficultyList.Find(x => x.ID == mProfiles.songDifficulty[i].ID);

            if (difficulty != null)
            {
                mProfiles.songDifficulty[i].isUnlocked = difficulty.unlocked;
                mProfiles.songDifficulty[i].bestScore = difficulty.bestScore;
            }
        }
    }

    public void UnlockAllSong()
    {
        Database.SetUnlockAllConcertVenues(true);
        int venuesListCount = Database.GetConcertVenues().Count;
        for (int i = 0; i < venuesListCount; i++)
        {
            mProfiles.concertVenues[i].isUnlocked = true;
        }

        Database.SetUnlockAllSongDifficulty(true);
        int SongDifficultyCount = Database.GetSongDifficulty().Count;
        for (int i = 0; i < SongDifficultyCount; i++)
        {
            mProfiles.songDifficulty[i].isUnlocked = true;
        }
    }

    public void EnableTutorial()
    {
        ResetProfile();
    }

    public bool HasProfile()
    {
        return PlayerPrefs.HasKey(Define.KEY_PROFILES);
    }
}
