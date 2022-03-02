using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearLanesData
{   
    public List<int> AppearLanesList;
    public bool linkToNote;

    public AppearLanesData()
    {
        AppearLanesList = new List<int>();
        linkToNote = false;
    }

    public AppearLanesData(bool linktonote, List<int> lanesList)
    {
        AppearLanesList = lanesList;
        linkToNote = linktonote;
    }

    public void Reset()
    {
        AppearLanesList.Clear();
        linkToNote = false;
    }
}

public class BeatData
{
    public int ID;
    public Define.NOTE_TYPE type;
    public float startTime;
    public float endTime;
    public bool isLongBeat;
    public bool isDragBeat;
    public int indexAudio;

    public Define.INPUT_STATUS swipe;
    public bool forcedDirection;
    public bool swipeAnyPlane;
    public int linkToAppearLanes;
    public int comboNotesKey;
    public Define.COLORS color;
    public int dragIndex;
    public Define.DRAG_DIRECTION dragDirection;
    public int dragNotesGroup;
    public int dragNotesKey;
    public float dragNoteLenght; // head to tail in seconds
    public bool isDragHeadNote;
    public bool isDragTailNote;
    public bool isDualNotes;
    public bool isFirstNote;

    public BeatData()
    {
        ID = -1;
        type = Define.NOTE_TYPE.NONE;
        startTime = endTime = 0;
        isLongBeat = false;
        swipe = Define.INPUT_STATUS.NONE;
        forcedDirection = false;
        color = Define.COLORS.GRAY;
        isDragBeat = false;
        dragIndex = -1;
        dragDirection = Define.DRAG_DIRECTION.NONE;
        dragNotesGroup = 0;
        dragNotesKey = 0;
        dragNoteLenght = 0f;
        isDragHeadNote = false;
        isDragTailNote = false;

        swipeAnyPlane = false;
        comboNotesKey = -1;
        linkToAppearLanes = -1;

        isDualNotes = false;
    }

    public override string ToString()
    {
        string str = "";
        if (isLongBeat)
        {
            if (startTime != -1f)
            {
                str = string.Format("{0} I {1}", TimerConvert.FloatToTime(startTime), color);
            }
            else
            {
                str = string.Format("{0} O {1}", TimerConvert.FloatToTime(endTime), color);
            }
        }
        else if (isDragBeat)
        {
            if (startTime != -1f)
            {
                str = string.Format("drag {0} index {1} direction {2} color {3}", TimerConvert.FloatToTime(startTime), dragIndex, dragDirection, color);
            }
            else
            {
                str = string.Format("drag {0} index {1} END color {2}", TimerConvert.FloatToTime(endTime), dragIndex, color);
            }
        }
        else
        {
            str = string.Format("{0} X {1}", TimerConvert.FloatToTime(startTime), color);
        }

        return str;
    }
}

public class BeatInfo
{
    public string beatName;
    public int indexCSV;
    public BeatData[] beatDatas;

    public int indexBeat;

    public BeatInfo()
    {
        indexBeat = 0;
    }

    public override string ToString()
    {
        string str = string.Format("Beat name: {0} from CSV {1}\n", beatName, indexCSV);
        foreach (BeatData data in beatDatas)
        {
            str += data.ToString() + "\n";
        }

        return str;
    }
}

[System.Serializable]
public class SongInfo
{
    public string songName;
    public string difficultFile;
    public string[] audios;
    public AudioClip[] audiosClip;
    public Define.COLORS[] colors;
    public List<BeatInfo> beats;
    public AppearLanesData[] AppearLanesDataList;

    public float firstBeatTime = 0;
    public float lastBeatTime = 0;

    public SongInfo()
    {
        Initialize();
    }

    public void Initialize()
    {
        if(beats == null)
        {
            beats = new List<BeatInfo>();
        }
        // if(charactersStatus == null)
        // {
        //     charactersStatus = new CharacterStatus[(int)Define.CHARACTERS_STATUS_COLUMNS.COUNT];
        // }
        // if(specificAnimInSong == null)
        // {
        //     specificAnimInSong = new SpecificAnimation[(int)Define.CHARACTERS_STATUS_COLUMNS.FREDDIE_INSTRUMENT + 1]; // 4 instrument colums
        // }
    }
    public void Reset()
    {
        audios = null;
        audiosClip = null;
        colors = null;
        if(beats != null)
        {
            beats.Clear();
            beats = null;
        }
        
        if(AppearLanesDataList != null)
        {
            for(int i = 0 ; i < AppearLanesDataList.Length ; ++i)
            {
                if(AppearLanesDataList[i] != null)
                {
                    AppearLanesDataList[i].AppearLanesList.Clear();
                }
            }
            AppearLanesDataList = null;
        }
        
        Initialize();
    }
}
