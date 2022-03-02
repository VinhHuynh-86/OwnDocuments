using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour
{
    #if UNITY_EDITOR
    [HideInInspector]
    public string[] levelDesign;
    [HideInInspector]
    public int startShow = 0;
    [HideInInspector]
    public int endShow = 0;
    [HideInInspector]
    public float timer = 0;
    public string levelName;    
    public int numLane = 6;
    public float timeShow = 5f;
    public float timeDistance = 5f;    

    void Start()
    {
        levelDesign = Game.Instance.levelDesign;
        System.Array.Reverse(levelDesign);
        levelName = Game.Instance.namefile;        
        GetEndShow();
        if (levelDesign != null && levelDesign.Length > 0)
        {
            numLane = levelDesign[0].Split(',').Length - 1;            
        }
    }

    void Update()
    {
        timer = GameManager.Instance.GetTimeAudio(0);
        if (timer > timeShow)
        {
            timeShow += timeDistance;
            startShow = endShow - 1;
            GetEndShow();
        }
    }

    public string GetTime(int id)
    {
        if (levelDesign != null && levelDesign.Length > 0)
        {
            string[] columns = levelDesign[id].Split(',');
            return columns[0];
        }
        return "";
    }

    public string GetNote(int id, int idnote)
    {
        if (levelDesign != null && levelDesign.Length > 0)
        {
            string[] columns = levelDesign[id].Split(',');
            // if(idnote >= columns.Length)
            // {
            //     Debug.Log("Wrong format in line:" + levelDesign[id]);
            // }
            return columns[idnote];
        }
        return "";
    }

    public void ChangeNote(int id, int idnote, string value)
    {
        if (levelDesign != null && levelDesign.Length > 0)
        {
            string[] columns = levelDesign[id].Split(',');
            columns[idnote] = value;
            levelDesign[id] = "";
            for (int i = 0; i < columns.Length - 1; i++)
            {
                levelDesign[id] += columns[i] + ",";
            }
            levelDesign[id] += columns[columns.Length - 1];
        }
    }

    public void GetEndShow()
    {
        if (levelDesign != null && levelDesign.Length > 0)
        {
            for (int i = startShow; i < levelDesign.Length; i++)
            {
                float time = TimerConvert.StringToTime(GetTime(i));
                if (time > timeShow)
                {
                    endShow = i;
                    break;
                }
            }
        }
    }
    #endif
}
