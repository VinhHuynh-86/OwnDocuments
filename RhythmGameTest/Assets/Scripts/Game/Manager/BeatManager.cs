using UnityEngine;
using System.Collections.Generic;

public class BeatManager : MonoBehaviour
{
    #region Instance
    private static BeatManager instance;
    public static BeatManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BeatManager>();
                if (instance == null)
                {
                    instance = new GameObject("Spawned BeatManager", typeof(BeatManager)).GetComponent<BeatManager>();
                }
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    public int Count
    {
        get
        {
            if (spawers == null)
            {
                return 0;
            }
            return spawers.Length;
        }
    }

    [SerializeField]
    private BeatSpawer[] spawers;

    public List<int> AudiosDisableIndex;

    public List<int> GetAudiosDisableIndex()
    {
        return AudiosDisableIndex;
    } 
    [HideInInspector]
    public bool[] arrAudiosEnable;

    public bool IsEnableNote(int audioIndex)
    {
        return arrAudiosEnable[audioIndex];
    }
	
    public void Initialize()
    {
        if (GameData.GetAudioInfo() != null && GameData.GetAudioInfo().Length > 0)
        {
            arrAudiosEnable = new bool[GameData.GetAudioInfo().Length];
            ResetAudioEnable();
        }
    }

    public Note Generate(int index, float speed)
    {
        if (index >= 0 && index < spawers.Length)
        {
            return spawers[index].GenerateBeat(speed);
        }

        return null;
    }

    private void DisableColor(int audioIndex)
    {
        Define.COLORS colorMissing = Define.COLORS.NONE;
        int count1 = GameManager.Instance.noteList.Length;
        for(int i = 0; i < count1; ++i)
        {
            List<Note> notes = GameManager.Instance.noteList[i];
            int count2 = notes.Count;
            for(int j = 0; j < count2; ++j)
            {
                if (notes[j].indexAudio == audioIndex)
                {
                    notes[j].Color = Define.COLORS.GRAY;
                    if(colorMissing == Define.COLORS.NONE)
                    {
                        colorMissing = notes[j].GetOriginalColor();
                    }
                }
            }
        }
    }

    private void EnableColor(int audioIndex)
    {
        Define.COLORS colorMissing = Define.COLORS.NONE;
        int count1 = GameManager.Instance.noteList.Length;
        for(int i = 0; i < count1; ++i)
        {
            List<Note> notes = GameManager.Instance.noteList[i];
            int count2 = notes.Count;
            for(int j = 0; j < count2; ++j)
            {
                if (notes[j].indexAudio == audioIndex)
                {
                    notes[j].Color = GameData.GetColorInfo()[audioIndex];
                    if(colorMissing == Define.COLORS.NONE)
                    {
                        colorMissing = notes[j].GetOriginalColor();
                    }
                }
            }
        }
    }

    public Note GetTemplate(int index)
    {
        if (index >= 0 && index < spawers.Length)
        {
            BeatFactory factory = spawers[index].GetFactory();
            if (factory != null)
            {
                return factory.GetTemplate();
            }
        }
        return null;
    }

    public void DisableNote(int audioIndex)
    {
        if(TutorialManager.Instance.IsInGamePart1())
        {
            return;
        }
        GameManager.Instance.SetMissingNote(true);
        SFXManager.Instance.PlayTapFail((Define.AUDIOS_INDEX)audioIndex);

        if(arrAudiosEnable[audioIndex])
        {
            // Vibration.Vibrate();
            arrAudiosEnable[audioIndex] = false;
            DisableColor(audioIndex);
            // SFXManager.Instance.PlayOutRandomCrowdBoo();
            GameManager.Instance.DisableSong(audioIndex);
            GameManager.Instance.CameraShake();
        }
    }

    public void EnableNote(int audioIndex)
    {
        if(TutorialManager.Instance.IsInGamePart1())
        {
            return;
        }
        GameManager.Instance.SetMissingNote(false);

        if(audioIndex < arrAudiosEnable.Length && !arrAudiosEnable[audioIndex])
        {
            arrAudiosEnable[audioIndex] = true;
            EnableColor(audioIndex);
            GameManager.Instance.EnableSong(audioIndex);
            // SFXManager.Instance.PlayOutCrowBooWhemEnable();
        }
    }

    public void ChangeLane(Note note, int newLine)
    {
        note.transform.parent = spawers[newLine].gameObject.transform;
        spawers[newLine].ResetBeatInfo(note);
        Vector3 newPos = new Vector3(BoardGameManager.Instance.notesOriginalPosition[newLine].x, BoardGameManager.Instance.notesOriginalPosition[newLine].y, note.transform.position.z);
        note.transform.position = newPos;
        note.CalculatePosition();

        int oldIndex = note.indexCSV;
        note.indexCSV = newLine;
        GameManager.Instance.noteList[newLine].Add(note);
        GameManager.Instance.noteList[oldIndex].Remove(note);
    }
    public void RealignNotes()
    {
        List<Note> notes;
        int linesNumber = GameManager.Instance.linesNumber;
        int centerOnLeft = linesNumber / 4;
        int centerOnRight = linesNumber * 3 / 4;
    
        int newIndex, newLine;
        for(int i = 0 ; i < linesNumber; ++i)
        {
            notes = GameManager.Instance.GetNoteInLine(i);
            int noteCount = notes.Count;
            if(noteCount < 1)
                continue;


            // Move note from disable line to visible line
            if(!GameManager.Instance.visibleLinesIndex.Contains(i))
            {
                newIndex = (i < linesNumber/2 ? centerOnLeft : centerOnRight);
                for(int v = 0; v < noteCount; ++v)
                {
                    // if(notes[v].type == Define.NOTE_TYPE.SHORT || notes[v].type != Define.NOTE_TYPE.SWIPE)
                    if(notes[v].type != Define.NOTE_TYPE.MIGHTY_SWIPE && notes[v].type != Define.NOTE_TYPE.DRAG && notes[v].type != Define.NOTE_TYPE.LONG)
                    {
                        newLine = newIndex;
                        ChangeLane(notes[v], newLine);
                        
                        noteCount--;
                        if(noteCount > 0)
                            v--;
                    }

                }
            }
            
            {
                // Revert note to original line if this line is visible
                AppearLanesData[] appearLanesData = GameData.GetAppearLanesData();
                noteCount = notes.Count;
                for(int v = 0; v < noteCount; ++v)
                {
                    if(notes[v].type == Define.NOTE_TYPE.MIGHTY_SWIPE)
                    {
                        int laneSize = appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Count;
                        if (notes[v].isOnSide == Define.LANE_ON_SIDE.LEFT)
                        {
                            // Left side fored
                            if((GameManager.Instance.IsActiveLine(0) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-1))
                            || (!GameManager.Instance.IsActiveLine(2) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(3))
                            )
                            {
                                notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_RIGHT;
                                notes[v].forcedDirection = true;
                            }
                            else if((!GameManager.Instance.IsActiveLine(0) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(1))
                            || (GameManager.Instance.IsActiveLine(2) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-3))
                            )
                            {
                                notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_LEFT;
                                notes[v].forcedDirection = true;
                            }
                            // Left side fored : end
                            else if((GameManager.Instance.IsActiveLine(0) && GameManager.Instance.IsActiveLine(1) && GameManager.Instance.IsActiveLine(2))
                            && (appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-1) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-3))
                            )
                            {
                                notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_RIGHT;
                            }
                            else if((!GameManager.Instance.IsActiveLine(0) && GameManager.Instance.IsActiveLine(1) && !GameManager.Instance.IsActiveLine(2))
                            && (appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(1) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(3))
                            )
                            {
                                notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_LEFT;
                            }

                            if(!notes[v].forcedDirection)
                            {
                                // Right side fored
                                if((GameManager.Instance.IsActiveLine(3) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-4))
                                || (!GameManager.Instance.IsActiveLine(5) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(6))
                                )
                                {
                                    // Right side forced Define.INPUT_STATUS.SWIPE_RIGHT
                                    notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_LEFT;
                                }
                                else if((!GameManager.Instance.IsActiveLine(3) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(4))
                                || (GameManager.Instance.IsActiveLine(5) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-6))
                                )
                                {
                                    // Right side forced Define.INPUT_STATUS.SWIPE_LEFT
                                    notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_RIGHT;
                                }
                                // Right side fored : end
                            }
                        }
                        else
                        {
                            // Left side fored
                            if((GameManager.Instance.IsActiveLine(0) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-1))
                            || (!GameManager.Instance.IsActiveLine(2) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(3))
                            )
                            {
                                // Left side forced Define.INPUT_STATUS.SWIPE_RIGHT
                                notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_LEFT;
                            }
                            else if((!GameManager.Instance.IsActiveLine(0) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(1))
                            || (GameManager.Instance.IsActiveLine(2) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-3))
                            )
                            {
                                // Left side forced Define.INPUT_STATUS.SWIPE_LEFT
                                notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_RIGHT;
                            }
                            // Left side fored : end
                            else
                            {
                                // Right side forced
                                if((GameManager.Instance.IsActiveLine(3) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-4))
                                || (!GameManager.Instance.IsActiveLine(5) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(6))
                                )
                                {
                                    notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_RIGHT;
                                }
                                else if((!GameManager.Instance.IsActiveLine(3) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(4))
                                || (GameManager.Instance.IsActiveLine(5) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-6))
                                )
                                {
                                    notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_LEFT;
                                }
                                // Right side forced : end
                                else if((GameManager.Instance.IsActiveLine(3) && GameManager.Instance.IsActiveLine(4) && GameManager.Instance.IsActiveLine(5))
                                && (appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-4) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(-6))
                                )
                                {
                                    notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_LEFT;
                                }
                                else if((!GameManager.Instance.IsActiveLine(3) && GameManager.Instance.IsActiveLine(4) && !GameManager.Instance.IsActiveLine(5))
                                && (appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(4) && appearLanesData[notes[v].linkToAppearLanes].AppearLanesList.Contains(6))
                                )
                                {
                                    notes[v].SwipeRequire = Define.INPUT_STATUS.SWIPE_RIGHT;
                                }
                            }
                        }

                        if (notes[v].isOnSide == Define.LANE_ON_SIDE.LEFT)
                        {
                            if(notes[v].SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                            {
                                // Moving mighty swipe note right on left side
                                if(notes[v].indexCSV != 0)
                                {
                                    ChangeLane(notes[v], 0);
                                    noteCount--;
                                    if(noteCount > 0)
                                        v--;
                                }
                            }
                            else
                            {
                                // Moving mighty swipe note left on left side
                                if(notes[v].indexCSV != (linesNumber / 2 - 1))
                                {
                                    ChangeLane(notes[v], linesNumber / 2 - 1);
                                    noteCount--;
                                    if(noteCount > 0)
                                        v--;
                                }
                            }
                        }
                        else
                        {
                            if(notes[v].SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                            {
                                // Moving mighty swipe note right on right side
                                if(notes[v].indexCSV != linesNumber / 2)
                                {
                                    ChangeLane(notes[v], linesNumber / 2);
                                    noteCount--;
                                    if(noteCount > 0)
                                        v--;
                                }
                            }
                            else
                            {
                                // Moving mighty swipe note left on right side
                                if(notes[v].indexCSV != (linesNumber - 1))
                                {
                                    ChangeLane(notes[v], linesNumber - 1);
                                    noteCount--;
                                    if(noteCount > 0)
                                        v--;
                                }
                            }
                        }
                    }
                    else if(GameManager.Instance.visibleLinesIndex.Contains(notes[v].originalIndexCSV))
                    {
                        if(notes[v].indexCSV != notes[v].originalIndexCSV)
                        {
                            // if((!notes[v].IsDragNote || notes[v].IsLongNote) && !(notes[v].type == Define.NOTE_TYPE.MIGHTY_SWIPE))
                            if(notes[v].type != Define.NOTE_TYPE.MIGHTY_SWIPE && notes[v].type != Define.NOTE_TYPE.DRAG && notes[v].type != Define.NOTE_TYPE.LONG)
                            {
                                ChangeLane(notes[v], notes[v].originalIndexCSV);

                                noteCount--;
                                if(noteCount > 0)
                                    v--;
                            }
                        }
                    }
                }
            }
        }

        // sort notes
        Note temp;
        for(int i = 0 ; i < linesNumber; ++i)
        {
            notes = GameManager.Instance.GetNoteInLine(i);
            int noteCount = notes.Count;
            for(int v = 0; v < noteCount; ++v)
            {
                for(int u = v + 1; u < noteCount; ++u)
                {
                    if(notes[u].transform.position.z < notes[v].transform.position.z)
                    {
                        temp = notes[v];
                        notes[v] = notes[u];
                        notes[u] = temp;
                    }
                }
            }
        }

        GameManager.Instance.MakeSureHeadDragNoteInVisibleLane();
    }

    public void Reset()
    {
        ResetAudioEnable();
    }

    public void ResetAudioEnable()
    {
        if(arrAudiosEnable != null)
        {
            for (int i = 0; i < arrAudiosEnable.Length; i++)
            {
                arrAudiosEnable[i] = true;
            }
        }
    }
}