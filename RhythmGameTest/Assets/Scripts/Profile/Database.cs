using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;
using gameoptions;

public static class Database
{
    private static JsonReader jsonReader = new JsonReader();
    private static JSONNode jsonData;

    private static List<Item> items = new List<Item>();
    private static List<ConcertVenues> concertVenues = new List<ConcertVenues>();
    private static List<SongsInVenues> songsInVenues = new List<SongsInVenues>();
    private static List<SongDifficulty> songDifficulty = new List<SongDifficulty>();
    const string NEW_LINE = "\n";
    const string TOKEN = ",";

    public static void LoadData()
    {
        LoadDataBaseJson();
    }

    public static void LoadDataJson()
    {
        if (jsonData == null)
        {
            jsonReader.LoadDataJson(Define.DATABASE_NAME);
            jsonData = jsonReader.jsonContent;
        }
    }

    public static void LoadDataBaseJson()
    {
        LoadDataJson();

        List<string> profileTypes = new List<string>();
        foreach (string key in jsonData.Keys)
        {
            if (key == Define.CONCERTVENUE_SHEET)
            {
                LoadConcertVenueSheet(jsonData[key]);
            }
            else if (key == Define.SONGSINVENUE_SHEET)
            {
                LoadSongsInVenueSheet(jsonData[key]);
            }
            else if (key == Define.SONGDIFFICULTY_SHEET)
            {
                LoadSongDifficultySheet(jsonData[key]);
            }
        }
    }

    public static void OverrideDataSongDifficulty(string remoteConfig, string Config_Sheet)
    {
        string decryptConfig = VCrypto.Decrypt(remoteConfig, Define.ENCRYPT_KEY_VERSION);

        JSONNode jData = JSON.Parse(decryptConfig)[Config_Sheet];
        if (jData == null)
        {
            return;
        }

        foreach (JSONNode node in jData.Children)
        {
            string id = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ID].Value;
            SongDifficulty sd = GetSongDifficultyByID(id);
            sd.scoreToGainSilver = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ScoreToGainSilverDisc].AsInt;
            sd.scoreToGainGold = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ScoreToGainGoldDisc].AsInt;
            sd.scoreToGainPlatinum = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ScoreToGainPlatinumDisc].AsInt;
            sd.coinsRewardOnSilver = node[Define.SONGDIFFICULTY_ELEMENT_NAME.CoinsRewardOnSilverDisc].AsInt;
            sd.coinsRewardOnGold = node[Define.SONGDIFFICULTY_ELEMENT_NAME.CoinsRewardOnGoldDisc].AsInt;
            sd.coinsRewardOnPlatinum = node[Define.SONGDIFFICULTY_ELEMENT_NAME.CoinsRewardOnPlatinumDisc].AsInt;
        }
    }

    public static void OverrideDataConcertVenue(string remoteConfig)
    {
        string decryptConfig = VCrypto.Decrypt(remoteConfig, Define.ENCRYPT_KEY_VERSION);

        JSONNode jData = JSON.Parse(decryptConfig)[Define.CONCERTVENUE_SHEET];
        if (jData == null)
        {
            return;
        }
        foreach (JSONNode node in jData.Children)
        {
            for(int i = 0; i < concertVenues.Count; i++)
            {
                if(concertVenues[i].ID == node[Define.CONCERTVENUE_ELEMENT_NAME.ID].Value)
                {
                    concertVenues[i].year = node[Define.CONCERTVENUE_ELEMENT_NAME.Year].Value;
                    concertVenues[i].discRequirement = node[Define.CONCERTVENUE_ELEMENT_NAME.DiscRequirement].AsInt;
                }
            }
        }
    }


    public static void LoadConcertVenueSheet(JSONNode jData)
    {
        if (jData == null)
        {
            return;
        }

        foreach (JSONNode node in jData.Children)
        {
            if (IsEndJson(node))
            {
                break;
            }

            ConcertVenues item = new ConcertVenues();
            item.ID = node[Define.CONCERTVENUE_ELEMENT_NAME.ID].Value;
            item.stadium = node[Define.CONCERTVENUE_ELEMENT_NAME.Stadium].Value;
            item.venueCity = node[Define.CONCERTVENUE_ELEMENT_NAME.VenueCity].Value;
            item.tour = node[Define.CONCERTVENUE_ELEMENT_NAME.Tour].Value;
            item.year = node[Define.CONCERTVENUE_ELEMENT_NAME.Year].Value;
            item.unlockSpecialMoveID = node[Define.CONCERTVENUE_ELEMENT_NAME.UnlockSpecialMove].Value;
            item.discRequirement = node[Define.CONCERTVENUE_ELEMENT_NAME.DiscRequirement].AsInt;
            item.unlocked = node[Define.CONCERTVENUE_ELEMENT_NAME.Unlocked].AsBool;

            concertVenues.Add(item);
        }
    }

    public static void LoadSongsInVenueSheet(JSONNode jData)
    {
        if (jData == null)
        {
            return;
        }

        foreach (JSONNode node in jData.Children)
        {
            if (IsEndJson(node))
            {
                break;
            }

            SongsInVenues item = new SongsInVenues();
            item.ID = node[Define.SONGSINVENUE_ELEMENT_NAME.ID].Value;
            item.songName = node[Define.SONGSINVENUE_ELEMENT_NAME.SongName].Value;
            item.audios.Add(node[Define.SONGSINVENUE_ELEMENT_NAME.GuitarAudio].Value);
            item.colors.Add(Define.ParseColor(node[Define.SONGSINVENUE_ELEMENT_NAME.GuitarNoteColor].Value));
            item.audios.Add(node[Define.SONGSINVENUE_ELEMENT_NAME.DrumsAudio].Value);
            item.colors.Add(Define.ParseColor(node[Define.SONGSINVENUE_ELEMENT_NAME.DrumsNoteColor].Value));
            item.audios.Add(node[Define.SONGSINVENUE_ELEMENT_NAME.VocalAudio].Value);
            item.colors.Add(Define.ParseColor(node[Define.SONGSINVENUE_ELEMENT_NAME.VocalNoteColor].Value));
            item.audios.Add(node[Define.SONGSINVENUE_ELEMENT_NAME.BassAudio].Value);
            item.colors.Add(Define.ParseColor(node[Define.SONGSINVENUE_ELEMENT_NAME.BassNoteColor].Value));
            item.audios.Add(node[Define.SONGSINVENUE_ELEMENT_NAME.MergeOthersAudio].Value);
            item.audios.Add(node[Define.SONGSINVENUE_ELEMENT_NAME.BackAudio].Value);
            item.concertVenuesID = node[Define.SONGSINVENUE_ELEMENT_NAME.ConcertVenueID].Value;
            item.album = node[Define.SONGSINVENUE_ELEMENT_NAME.Album].Value;
            item.length = node[Define.SONGSINVENUE_ELEMENT_NAME.Length].Value;
            item.year = node[Define.SONGSINVENUE_ELEMENT_NAME.SongYear].Value;
            item.mainVocal = Define.GetGamePlayCharacterName(node[Define.SONGSINVENUE_ELEMENT_NAME.MainVocal].Value);

            songsInVenues.Add(item);
        }
    }

    public static void LoadSongDifficultySheet(JSONNode jData)
    {
        if (jData == null)
        {
            return;
        }

        foreach (JSONNode node in jData.Children)
        {
            if (IsEndJson(node))
            {
                break;
            }

            SongDifficulty item = new SongDifficulty();
            item.ID = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ID].Value;
            item.songID = node[Define.SONGDIFFICULTY_ELEMENT_NAME.SongID].Value;
            item.difficult = Define.GetDifficultBy(node[Define.SONGDIFFICULTY_ELEMENT_NAME.Difficult].Value);
            item.levelDesign = node[Define.SONGDIFFICULTY_ELEMENT_NAME.LevelDesign].Value;
            item.discRequirement = Define.GetDiscTypeBy(node[Define.SONGDIFFICULTY_ELEMENT_NAME.DiscRequirement].Value);
            item.requirementFrom = node[Define.SONGDIFFICULTY_ELEMENT_NAME.RequirementFrom].Value;
            item.scoreToGainSilver = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ScoreToGainSilverDisc].AsInt;
            item.scoreToGainGold = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ScoreToGainGoldDisc].AsInt;
            item.scoreToGainPlatinum = node[Define.SONGDIFFICULTY_ELEMENT_NAME.ScoreToGainPlatinumDisc].AsInt;
            item.coinsRewardOnSilver = node[Define.SONGDIFFICULTY_ELEMENT_NAME.CoinsRewardOnSilverDisc].AsInt;
            item.coinsRewardOnGold = node[Define.SONGDIFFICULTY_ELEMENT_NAME.CoinsRewardOnGoldDisc].AsInt;
            item.coinsRewardOnPlatinum = node[Define.SONGDIFFICULTY_ELEMENT_NAME.CoinsRewardOnPlatinumDisc].AsInt;
            item.unlocked = node[Define.SONGDIFFICULTY_ELEMENT_NAME.Unlocked].AsBool;
            songDifficulty.Add(item);
        }
    }

    public static bool IsEndJson(JSONNode node)
    {
        if (node == null || node.Value == null || node["ID"].Value == null || node["ID"].Value.Length == 0)
        {
            return true;
        }
        return false;
    }

    public static List<Item> GetListItem()
    {
        return items;
    }

    public static List<ConcertVenues> GetConcertVenues()
    {
        return concertVenues;
    }

    public static ConcertVenues GetConcertVenuesByID(string concertVenueID)
    {
        return concertVenues.Find(x => x.ID == concertVenueID);
    }

    public static void SetUnlockAllConcertVenues(bool value = false)
    {
        for (int i = 0; i < concertVenues.Count; i++)
            SetUnlockConcertVenuesByIndex(i, value);
    }

    public static void SetUnlockConcertVenuesByIndex(int index, bool value = false)
    {
        concertVenues[index].unlocked = value;
    }

    public static List<SongsInVenues> GetSongInVenues()
    {
        return songsInVenues;
    }

    public static List<SongsInVenues> GetSongInVenues(string venuesID)
    {
        List<SongsInVenues> songs = new List<SongsInVenues>();
        for (int i = 0; i < songsInVenues.Count; ++i)
        {
            if (songsInVenues[i].concertVenuesID == venuesID)
            {
                songs.Add(songsInVenues[i]);
            }
        }
        return songs;
    }

    public static string GetLastYearVenueUnlocked()
    {
        for (int i = concertVenues.Count - 1; i >= 0; i--)
        {
            if (concertVenues[i].unlocked)
            {
                return concertVenues[i].year;
            }
        }
        return "1974";
    }

    public static SongsInVenues GetSongByID(string songID)
    {
        return songsInVenues.Find(x => x.ID == songID);
    }

    public static SongsInVenues GetSongByDifficultyID(string diffID)
    {
        SongDifficulty sd = songDifficulty.Find(x => x.ID == diffID);
        return songsInVenues.Find(x => x.ID == sd.songID);
    }

    public static List<SongDifficulty> GetSongDifficulty()
    {
        return songDifficulty;
    }

    public static SongDifficulty GetSongDifficultyByID(string songDiffID)
    {
        return songDifficulty.Find(x => x.ID == songDiffID);
    }

    public static SongDifficulty GetSongDifficultyBy(int index)
    {
        if (index < songDifficulty.Count)
        {
            return songDifficulty[index];
        }
        return null;
    }

    public static List<SongDifficulty> GetSongDifficulty(string songID)
    {
        List<SongDifficulty> listSongDiff = new List<SongDifficulty>();
        int count = songDifficulty.Count;
        for (int i = 0; i < count; ++i)
        {
            if (songDifficulty[i].songID == songID && songDifficulty[i].ID != "SD000")
            {
                listSongDiff.Add(songDifficulty[i]);
            }
        }

        return listSongDiff;
    }

    public static void SetUnlockAllSongDifficulty(bool value = false)
    {
        for (int i = 0; i < songDifficulty.Count; i++)
            SetUnlockSongDifficultyById(i, value);
    }

    public static void SetUnlockSongDifficultyById(int id, bool value = false)
    {
        songDifficulty[id].unlocked = value;
    }

    public static void SetDicsColletSongDifficulty(SongDifficulty songDif, int value = (int)Define.DISC_TYPE.NONE)
    {
        songDif.dicsCollect = value;
    }

    public static void SetBestScoreSongDifficulty(SongDifficulty songDif, int value = 0)
    {
        songDif.bestScore = value;
    }

    public static void SetPerfectComboSongDifficulty(SongDifficulty songDif, int value = 0)
    {
        songDif.perfectCombo = value;
    }

    public static string UnlockNextSongDifficuty(SongDifficulty songDiff)
    {
        string name = "";
        if (songDiff.dicsCollect < (int)Define.DISC_TYPE.SILVER)
            return name;

        int count = songDifficulty.Count;
        for (int i = 0; i < count; ++i)
        {
            if (songDifficulty[i].requirementFrom == songDiff.ID
            && (int)songDifficulty[i].discRequirement <= (int)songDiff.dicsCollect)
            {
                if (songDifficulty[i].unlocked)
                {
                    continue;
                }

                //songDifficulty[i].unlocked = true;
                SetUnlockSongDifficultyById(i, true);
                SongsInVenues songInVenue = GetSongByID(songDifficulty[i].songID);
                switch (songDifficulty[i].difficult)
                {
                    case Define.GAME_MODE.EASY:
                        name += "\n" + "Unlocked:" + Localization.Instance.GetString(songInVenue.songName) + " " + Localization.Instance.GetString("STR_EASY");
                        break;

                    case Define.GAME_MODE.MEDIUM:
                        name = "Unlocked:" + Localization.Instance.GetString(songInVenue.songName) + " " + Localization.Instance.GetString("STR_MEDIUM");
                        break;

                    case Define.GAME_MODE.HARD:
                        name = "Unlocked:" + Localization.Instance.GetString(songInVenue.songName) + " " + Localization.Instance.GetString("STR_HARD");
                        break;
                }
            }
        }

        return name;
    }

    public static bool IsUnlockNextVenue()
    {
        int disc = GetCurrentDisc();
        for (int i = 1; i < concertVenues.Count; i++)
        {
            if (disc >= concertVenues[i].discRequirement && !concertVenues[i].unlocked)
            {
                return true;
            }
        }
        return false;
    }

    public static bool VenuesIsUnlocked(string id)
    {
        ConcertVenues venues = concertVenues.Find(x => x.ID == id);

        if (venues != null)
        {
            return venues.unlocked;
        }

        return false;
    }

    public static bool VenuesIsUnlocked(int index)
    {
        if(index >= 0 && index < concertVenues.Count)
        {
            return concertVenues[index].unlocked;
        }
        return false;
    }

    public static bool VenueIsAlready(int index)
    {
        if(index >= 0 && index < concertVenues.Count)
        {
            return (!concertVenues[index].unlocked && (GetCurrentDisc() >= concertVenues[index].discRequirement) && (index == 0 || VenuesIsUnlocked(index - 1)));
        }
        return false;
    }

    public static int GetCurrentDisc()
    {
        int disc = 0;
        for (int i = 0; i < songDifficulty.Count; i++)
        {
            disc += songDifficulty[i].dicsCollect;
        }
        return disc;
    }

    public static int GetAllDiscType(int type)
    {
        int disc = 0;
        for (int i = 0; i < songDifficulty.Count; i++)
        {
            if (songDifficulty[i].dicsCollect >= type)
                disc += 1;
        }
        return disc;
    }

    public static int GetHighestScoreMode(Define.GAME_MODE mode = Define.GAME_MODE.NONE)
    {
        int highestScore = 0;
        for (int i = 0; i < songDifficulty.Count; i++)
        {
            if (songDifficulty[i].difficult == mode && highestScore < songDifficulty[i].bestScore)
                highestScore = songDifficulty[i].bestScore;
        }
        return highestScore;
    }

    public static int GetVenueIndexBy(string venueID)
    {
        return concertVenues.FindIndex(x => x.ID == venueID);
    }
    public static int GetSongIndexOfVenueBy(string venueID, string songID)
    {
        int idx = -1;
        for (int v = 0; v < songsInVenues.Count; ++v)
        {
            if (songsInVenues[v].concertVenuesID == venueID)
            {
                idx++;
                if (songsInVenues[v].ID == songID)
                {
                    break;
                }
            }
        }
        return idx;
    }

    public static bool IsSongUnlockedByName(string songName)
    {
        if(songName.Contains(Define.SONG_NAME[(int)Define.SONG_INDEX.WWRY]) || songName.Contains(Define.SONG_NAME[(int)Define.SONG_INDEX.RGGA]))
        {
            return true;
        }
        for (int v = 0; v < songsInVenues.Count; ++v)
        {
            if (songsInVenues[v].songName.Contains(songName) || songName.Contains(songsInVenues[v].songName))
            {
                return VenuesIsUnlocked(songsInVenues[v].concertVenuesID);
            }
        }
        return false;
    }

    public static bool IsSongUnlockedByID(string songID)
    {
        return false;
    }

}
