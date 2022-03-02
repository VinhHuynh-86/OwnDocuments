using System;
using UnityEngine;
using Newtonsoft.Json;

public class Profiles
{
    public int Coin = 0;
    public int Squad = 0;
    public int Damage = 0;
    public int Income = 0;
    public int Map = 0;
    public int Stage = 0;
    public int Reset = 0;
    public int LastLogin = 0;
}

public class ProfileSetting
{
    public bool Vibrate = true;
}

public class ProfileMgr : Singleton<ProfileMgr>
{
    private string KEY_PROFILES = "profiles";
    private string KEY_SETTINGS = "settings";
    private Profiles mProfiles = new Profiles();
    private ProfileSetting mSettings = new ProfileSetting();
    private bool mIsLoaded = false;

    public int Coin
    {
        get { return mProfiles.Coin; }
        set { mProfiles.Coin = value; }
    }
    public int Squad
    {
        get { return mProfiles.Squad; }
        set { mProfiles.Squad = value; }
    }
    public int Damage
    {
        get { return mProfiles.Damage; }
        set { mProfiles.Damage = value; }
    }
    public int Income
    {
        get { return mProfiles.Income; }
        set { mProfiles.Income = value; }
    }
    public int Map
    {
        get { return mProfiles.Map; }
        set { mProfiles.Map = value; }
    }
    public int Stage
    {
        get { return mProfiles.Stage; }
        set { mProfiles.Stage = value; }
    }

    public int Reset
    {
        get { return mProfiles.Reset; }
        set { mProfiles.Reset = value; }
    }

    public int LastLogin
    {
        get { return mProfiles.LastLogin; }
        set { mProfiles.LastLogin = value; }
    }

    public bool IsVibrate
    {
        get { return mSettings.Vibrate; }
        set { mSettings.Vibrate = value; }
    }

    void Save()
    {
        if (mIsLoaded)
        {
            string json = JsonConvert.SerializeObject(mProfiles);
            PlayerPrefs.SetString(KEY_PROFILES, json);
        }
    }

    void Load()
    {
        string json = PlayerPrefs.GetString(KEY_PROFILES);
        mProfiles = JsonConvert.DeserializeObject<Profiles>(json);

        mIsLoaded = true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            LastLogin = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Save();
        }
    }

    void OnApplicationQuit()
    {
        LastLogin = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        Save();
    }

    public void Init()
    {
        if (PlayerPrefs.HasKey(KEY_PROFILES))
        {
            Load();
        }
        else
        {
            mIsLoaded = true;
        }
    }

    public void InitSettings()
    {
        if (PlayerPrefs.HasKey(KEY_SETTINGS))
        {
            string json = PlayerPrefs.GetString(KEY_SETTINGS);
            mSettings = JsonConvert.DeserializeObject<ProfileSetting>(json);
        }
    }
}
