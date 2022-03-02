using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GameData
{
    private static SongInfo info = null;
    public static int dragNotesGroups = 0; 
    public static int dragNotesKeyOnLeftSide = -2; // will begin -2,...
    public static int dragNotesKeyOnRightSide = 2; // will begin 2,...
    public static List<AudioClip> audioClipList = new List<AudioClip>();
    public static void SetInfo(SongInfo information)
    {
        info = information;
        info.lastBeatTime = 0;
    }

    public static SongInfo GetInfo()
    {
        return info;
    }
    public static void SelectSong()
    {
        if(info != null)
        {
            info.audiosClip = audioClipList.ToArray();
        }
    }
    public static bool IsSong(string songName)
    {
        return info.songName.Contains(songName);
    }

    public static void LoadLevel(bool isLoadClip = true)
    {
        SelectSong();
        ParseCSV(info.difficultFile);
    }

    public static void UnloadSong()
    {        
        if(info != null && info.audiosClip != null)
        { 
            GameManager.Instance.StopAudios();
            info.Reset();
        }
        if(audioClipList != null && audioClipList.Count > 0)
        {
            int count = audioClipList.Count;
            for(int i = 0 ; i < count; ++i)
            {
                if(audioClipList[i] != null)
                {
                    audioClipList[i].UnloadAudioData();
                    Resources.UnloadAsset(audioClipList[i]);
                }
            }
            audioClipList.Clear();
        }
    }
    public static void ClearInfo()
    {
        if (info != null)
        {
            UnloadAudio(info.audiosClip);
            info = null;
        }
    }
    private static void UnloadAudio(AudioClip[] audiosClip)
    {
        foreach (AudioClip audio in audiosClip)
        {
            if (!audio.UnloadAudioData())
            {
                // Debug.LogFormat("Unload audio {0} fail", audio.name);
            }
        }
    }

    private static void ParseCSV(string diffFile)
    {
        TextAsset textAsset = Resources.Load("Difficulty\\" + diffFile) as TextAsset;

        const string NEW_LINE = "\n";
        const string TOKEN = ",";

        List<BeatData>[] lists = null;

        string[] lines = textAsset.text.Split(NEW_LINE[0]);
        #if UNITY_EDITOR
        if(Game.Instance != null)
        {
            Game.Instance.namefile = diffFile;
            Game.Instance.levelDesign = lines;
        }
        #endif

        List<AppearLanesData> AppearLanesDataList = null;
        if(AppearLanesDataList == null)
        {
            AppearLanesDataList = new List<AppearLanesData>();
        }
        else
        {
            AppearLanesDataList.Clear();
        }

        List<Vector2Int> trackNote = new List<Vector2Int>();
        int comboKey = 0;
        List<BeatData> tempBeatDataOnLeftSide = new List<BeatData>();
        List<BeatData> tempBeatDataOnRightSide = new List<BeatData>();

        bool needToSortLeft = false;
        bool needToSortRight = false;
        int beatColumnsCount = 7;
        int lineNum = -1;
        int id = 2;

        string[] columns = new string[beatColumnsCount + (int)Define.CHARACTERS_STATUS_COLUMNS.COUNT];
        string[] beatColumns = new string[beatColumnsCount];
        string[] statusColumns = new string[(int)Define.CHARACTERS_STATUS_COLUMNS.COUNT];
        
        int lineDelay = lines.Length;
        for(int a = 0; a < lines.Length; ++ a)
        {
            if(lines[a].Length == 0 || lines[a].Equals("") || lines[a].Equals("\r"))
            {
                continue;   
            }

            lineNum++;
            int idx = lines[a].LastIndexOf("\r");
            if(idx > 0)
            {
               lines[a] = lines[a].Substring(0, idx);
            }

            columns = lines[a].Split(TOKEN[0]);
            //if(columns.Length > beatColumnsCount) 
            {
                System.Array.Copy(columns, 0, beatColumns, 0, beatColumnsCount);
                System.Array.Copy(columns, beatColumnsCount, statusColumns, 0, (int)Define.CHARACTERS_STATUS_COLUMNS.COUNT);
            }

            if (lists == null)
            {
                lists = new List<BeatData>[beatColumnsCount - 1]; // except time column
                for (int i = 0; i < lists.Length; i++)
                {
                    lists[i] = new List<BeatData>();
                }
            }

            AppearLanesData appearLanes = new AppearLanesData();
            float time = TimerConvert.StringToTime(beatColumns[0]);
            
            // Check state Tutorial
            if(columns[columns.Length - 1].Contains("End"))
            {
                TutorialManager.Instance.listTimeState.Add(time);
            }

            trackNote.Clear();
            
            List<BeatData> dataInALine = new List<BeatData>();
            for (int i = 1; i < beatColumnsCount; i++)
            {
                if (beatColumns[i].Trim().Length == 0)
                {
                    continue;
                }
                #if UNITY_EDITOR
                if(Game.Instance != null && time >= Game.Instance.timeMusicStart)
                #endif
                {
					BeatData data = ParseLine(i, beatColumns[i], time, appearLanes);

					if(data != null)
					{
                        if(info.lastBeatTime == 0)
                        {
                            info.lastBeatTime = data.startTime > -1f ? data.startTime : data.endTime;
                        }

						data.indexAudio = GetAudiosIndexByColor(data.color);
						
						if (data.isDragBeat)
						{
							data.dragIndex = i - 1;
							if(i - 1 < beatColumnsCount / 2)
                            {
                                tempBeatDataOnLeftSide.Add(data);
                                if(data.isDragHeadNote)
                                {
                                    needToSortLeft = true;
                                }
                            }
                            else
                            {
                                tempBeatDataOnRightSide.Add(data);
                                if(data.isDragHeadNote)
                                {
                                    needToSortRight = true;
                                }
                            }
                            
						}
                        data.ID = id;
                        id += 2;
                        dataInALine.Add(data);
						lists[i - 1].Add(data);
						trackNote.Add(new Vector2Int(i - 1, lists[i - 1].Count - 1));
					}
                }                
            }

            // Check for dual notes

            {
                for(int u = 0; u < dataInALine.Count; ++u)
                {
                    for(int v = u + 1; v < dataInALine.Count; ++v)
                    {
                        if(dataInALine[u].startTime != -1f && dataInALine[u].startTime == dataInALine[v].startTime
                         && ((!dataInALine[u].isDragBeat && !dataInALine[v].isDragBeat)
                            ||(dataInALine[u].isDragHeadNote && dataInALine[v].isDragHeadNote)
                            )
                        )
                        {
                            dataInALine[u].isDualNotes = true;
                            dataInALine[v].isDualNotes = true;
                        }
                    }
                }
                dataInALine.Clear();
                dataInALine = null;
            }

            // For drag note
            
            {
                if(needToSortLeft)
                {
                    SortDragNote(tempBeatDataOnLeftSide, false);
                    tempBeatDataOnLeftSide.Clear();
                    needToSortLeft = false;
                }
                if(needToSortRight)
                {
                    SortDragNote(tempBeatDataOnRightSide, true);
                    tempBeatDataOnRightSide.Clear();
                    needToSortRight = false;
                }
            }

			if(appearLanes.AppearLanesList.Count > 0)
            {
                int size = trackNote.Count;
                AppearLanesDataList.Add(appearLanes);
                if(size == 2)
                {
                    comboKey++;
                    for (int v = 0; v < size; ++v)
                    {
                        if(lists[trackNote[v].x][trackNote[v].y].swipe != Define.INPUT_STATUS.NONE)
                        {
                            lists[trackNote[v].x][trackNote[v].y].type = Define.NOTE_TYPE.MIGHTY_SWIPE;
                            lists[trackNote[v].x][trackNote[v].y].comboNotesKey = comboKey;
                            lists[trackNote[v].x][trackNote[v].y].linkToAppearLanes = AppearLanesDataList.Count - 1;
                        }
                    }
                }
                else if(size == 1)
                {
                    int v = 0;
                    if(lists[trackNote[v].x][trackNote[v].y].swipe != Define.INPUT_STATUS.NONE)
                    {
                        lists[trackNote[v].x][trackNote[v].y].type = Define.NOTE_TYPE.MIGHTY_SWIPE;
                        lists[trackNote[v].x][trackNote[v].y].comboNotesKey = -1;
                        lists[trackNote[v].x][trackNote[v].y].linkToAppearLanes = AppearLanesDataList.Count - 1;
                    }
                }
            }
            else
            {
                appearLanes = null;
            }

        }
        SaveBeatData(lists, AppearLanesDataList);

        if (lists != null)
        {
            for (int i = 0; i < lists.Length; i++)
            {
                lists[i].Clear();
            }
            lists = null;
        }
        AppearLanesDataList.Clear();
        AppearLanesDataList = null;
        trackNote.Clear();
        trackNote = null;

    }

    private static void SortDragNote(List<BeatData> beatdata, bool onSide)
    {
        dragNotesGroups++;

        int count = beatdata.Count;
        
        for(int i = 0; i < count - 1; ++i)
        {
            if(beatdata[i].isDragHeadNote && i > 0)
            {
                BeatData temp = beatdata[0];
                beatdata[0] = beatdata[i];
                beatdata[i] = temp;
            }
            if(beatdata[i].isDragTailNote && i < count - 1)
            {
                BeatData temp = beatdata[count - 1];
                beatdata[count - 1] = beatdata[i];
                beatdata[i] = temp;
            }

            for(int j = i + 1; j < count; ++j)
            {
                if(beatdata[j].startTime > 0 && 
                    (beatdata[j].startTime < beatdata[i].startTime
                    || (beatdata[j].startTime == beatdata[i].startTime
                        && (beatdata[i].dragDirection != Define.DRAG_DIRECTION.DRAG_LEFT && beatdata[i].dragDirection != Define.DRAG_DIRECTION.DRAG_RIGHT)
                        && (beatdata[j].dragDirection == Define.DRAG_DIRECTION.DRAG_LEFT || beatdata[j].dragDirection == Define.DRAG_DIRECTION.DRAG_RIGHT)
                       )
                    )
                  )
                {
                    BeatData temp = beatdata[j];
                    beatdata[j] = beatdata[i];
                    beatdata[i] = temp;
                }
            }
        }

        dragNotesKeyOnLeftSide--;
        dragNotesKeyOnRightSide++;
        for(int i = 0; i < count; ++i)
        {
            beatdata[i].dragNotesGroup = dragNotesGroups;
            if(onSide == false) // on left side
            {
                beatdata[i].dragNotesKey = dragNotesKeyOnLeftSide;
                dragNotesKeyOnLeftSide--;
            }
            else // on right side
            {
                beatdata[i].dragNotesKey = dragNotesKeyOnRightSide;
                dragNotesKeyOnRightSide++;
            }

            if(i == count -1)
            {
                beatdata[i].dragNoteLenght = 0;
            }
            else
            {
                beatdata[i].dragNoteLenght = beatdata[beatdata.Count - 1].endTime - beatdata[i].startTime;
            }
        }
        dragNotesKeyOnLeftSide--;
        dragNotesKeyOnRightSide++;

        if(count == 2 && beatdata[0].isDragHeadNote && beatdata[0].dragDirection == Define.DRAG_DIRECTION.DRAG_UP)
        {
            beatdata[0].isLongBeat = true; // long note up
            beatdata[1].isLongBeat = true; // long note up
        }

    }

    private static BeatData ParseLine(int column, string line, float time, AppearLanesData appearlanes)
    {
        BeatData data = new BeatData();

        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions
        // https://o7planning.org/vi/10795/huong-dan-su-dung-bieu-thuc-chinh-quy-trong-csharp
        //Regex regex = new Regex(@"([*#]?)(\$?)([a-z]+)([0-9]+)");
        //Regex regex = new Regex(@"([\*#]?)(&?)([a-z]?)([0-9]?)");
		//Regex regex = new Regex(@"([a-z<^>@]+)([0-9]+)");
		Regex regex = new Regex(@"([\*#]?)([&\$]?)([a-z<^>@]?)([0-9]?)");

        // http://regexstorm.net/tester
        Match match = regex.Match(line.Trim().ToLower());

        string type, colorCode;

        if(match.Groups[1].Value != "")
        {
            type = match.Groups[1].Value;
            switch (type)
            {
                case "#":
                    appearlanes.AppearLanesList.Add(column);
                    break;
                    
                case "*":
                    appearlanes.AppearLanesList.Add(-column);
                    break;
            }
        }
        
        data.swipeAnyPlane = false;
        if(match.Groups[2].Value != "")
        {
            type = match.Groups[2].Value;
            switch (type)
            {
                case "&":
                    data.swipeAnyPlane = true;
                    break;
                case "$":
                    data.isDragHeadNote = true;
                    data.isDragBeat = true;
                    break;
            }
        }

        bool Is_A_Note = false;
        if(match.Groups[3].Value != "")
        {
            Is_A_Note = true;
            type = match.Groups[3].Value;
            switch (type)
            {
                case "x":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.type = Define.NOTE_TYPE.SHORT;
                    break;

                case "u":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.swipe = Define.INPUT_STATUS.SWIPE_UP;
                    data.type = Define.NOTE_TYPE.SWIPE;
                    break;

                case "d":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.swipe = Define.INPUT_STATUS.SWIPE_DOWN;
                    data.type = Define.NOTE_TYPE.SWIPE;
                    break;

                case "l":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.swipe = Define.INPUT_STATUS.SWIPE_LEFT;
                    data.type = Define.NOTE_TYPE.SWIPE;
                    break;

                case "r":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.swipe = Define.INPUT_STATUS.SWIPE_RIGHT;
                    data.type = Define.NOTE_TYPE.SWIPE;
                    break;

				case "^":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.isDragBeat = true;
                    data.dragDirection = Define.DRAG_DIRECTION.DRAG_UP;
                    data.type = Define.NOTE_TYPE.DRAG;
                break;

                case ">":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.isDragBeat = true;
                    data.dragDirection = Define.DRAG_DIRECTION.DRAG_RIGHT;
                    data.type = Define.NOTE_TYPE.DRAG;
                    break;

                case "<":
                    data.startTime = time;
                    data.endTime = -1f;
                    data.isDragBeat = true;
                    data.dragDirection = Define.DRAG_DIRECTION.DRAG_LEFT;
                    data.type = Define.NOTE_TYPE.DRAG;
                    break;

                case "@":
                    data.isDragTailNote = true;
                    data.startTime = -1f;
                    data.endTime = time;
                    data.isDragBeat = true;
                    data.dragDirection = Define.DRAG_DIRECTION.DRAG_END;
                    data.type = Define.NOTE_TYPE.DRAG;
                    break;

                default:
                    Is_A_Note = false;
                    // Debug.Log("unknown beat at time " + Convert.FloatToTime(time) + " line " + line);

                    // for (int i = 0; i < match.Groups.Count; i++)
                    // {
                    //     Debug.Log("i " + i + " = " + match.Groups[i].Value);
                    // }
                    break;
            }
        }

        if(Is_A_Note)
        {
            if(data.swipe != Define.INPUT_STATUS.NONE)
            {
                appearlanes.linkToNote = true;
            }
        }
        else
        {
            data = null;
            return data;
        }
            

        colorCode = match.Groups[4].Value;

        data.color = ParseColor(colorCode);

        return data;
    }

    private static Define.COLORS ParseColor(string colorCode)
    {
        switch (colorCode)
        {
            case "1":
                return Define.COLORS.CYAN;

            case "2":
                return Define.COLORS.RED;

            case "3":
                return Define.COLORS.GREEN;

            case "4":
                return Define.COLORS.YELLOW;
        }


        return Define.COLORS.GRAY;
    }

    private static void SaveBeatData(List<BeatData>[] lists, List<AppearLanesData> appearLanesDataList)
    {
        info.beats.Clear();
        for (int i = 0, len = lists.Length; i < len; i++)
        {
            List<BeatData> list = lists[i];

            // Get Long note from Drag note
            for(int j = 0; j < list.Count; j++)
            {
                if(list[j].isDragHeadNote && list[j].dragDirection == Define.DRAG_DIRECTION.DRAG_UP)
                {
                    if(j - 1 >= 0 && list[j - 1].isDragTailNote)
                    {
                        list[j].type = Define.NOTE_TYPE.LONG;
                        list[j - 1].type = Define.NOTE_TYPE.LONG;
                    }
                }
            }
            BeatInfo beatInfo = new BeatInfo();

            beatInfo.beatName = "CSV column " + i;
            beatInfo.indexCSV = i;

            beatInfo.beatDatas = list.ToArray();
            System.Array.Reverse(beatInfo.beatDatas);
            info.beats.Add(beatInfo);
        }

        int firstNoteIdx = 0;
        float smallestStartTime = 100;
        for(int v = 0; v < info.beats.Count; ++v)
        {
            if(info.beats[v].beatDatas.Length > 0 && info.beats[v].beatDatas[0].startTime != -1 && info.beats[v].beatDatas[0].startTime < smallestStartTime
            )
            {
                smallestStartTime = info.beats[v].beatDatas[0].startTime;
                firstNoteIdx = v;
            }
        }
        info.beats[firstNoteIdx].beatDatas[0].isFirstNote = true;
        info.firstBeatTime = info.beats[firstNoteIdx].beatDatas[0].startTime;
        info.AppearLanesDataList = appearLanesDataList.ToArray();
    }

    public static string GetSongInfo()
    {
        if (info == null)
        {
            return "";
        }
        return info.songName;
    }

    public static AudioClip[] GetAudioInfo()
    {
        if (info == null)
        {
            return null;
        }
        return info.audiosClip;
    }

    public static Define.COLORS[] GetColorInfo()
    {
        if (info == null)
        {
            return null;
        }
        return info.colors;
    }

    public static int GetAudiosIndexByColor(Define.COLORS color)
    {
        int length = info.colors.Length;
        for(int i = 0 ; i < length; ++i)
        {
            if(info.colors[i] == color)
            {
                return i;
            }
        }
        return length;
    }

    public static void ClearBeatInfo()
    {
        info.beats.Clear();
    }

    public static List<BeatInfo> GetBeatInfo()
    {
        if (info == null)
        {
            return null;
        }
        return info.beats;
    }

    public static AppearLanesData[] GetAppearLanesData()
    {
        if (info == null)
        {
            return null;
        }
        return info.AppearLanesDataList;
    }

    public static float GetFirstBeatTime()
    {
        return info.firstBeatTime;
    }
    public static float GetLastBeatTime()
    {
        return info.lastBeatTime;
    }
}
