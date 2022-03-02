using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConnerDictionaryEntry
{
    public Define.COLORS key;
    public Sprite[] value;
}

public class Corner : MonoBehaviour
{
    public List<ConnerDictionaryEntry> spriteList;
    private Dictionary<Define.COLORS, Sprite[]> dictionary;
    SpriteRenderer cornerSprite;
    public GameObject cornerObj;
    public Note parentNote;
    private bool isCollided;
    public bool isMissed;

    private void Awake()
    {
        dictionary = new Dictionary<Define.COLORS, Sprite[]>();
        foreach (ConnerDictionaryEntry entry in spriteList)
        {
            dictionary.Add(entry.key, entry.value);
        }
        cornerSprite = cornerObj.GetComponent<SpriteRenderer>();

        Reset();
    }

    public void Reset()
    {
        isCollided = false;
        isMissed = false;

        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if(parentNote.IsDragNote)
        {
            if(parentNote.isDragTailNote)
            {
                parentNote.cornerLine.gameObject.SetActive(false);
                if(parentNote.longNote.prev != null)
                {
                    parentNote.longNote.prev.parentNote.cornerLine.gameObject.SetActive(false);
                }
            }
            else if(parentNote.longNote.next != null && parentNote.longNote.next.parentNote.isDragTailNote)
            {
                parentNote.cornerLine.gameObject.SetActive(false);
            }

            Quaternion rot = parentNote.longNote.transform.localRotation;
            
            if(parentNote.longNote.next != null)
            {
                if(parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_LEFT && parentNote.longNote.next.parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_UP)
                {
                    rot.eulerAngles = new Vector3(90, rot.eulerAngles.y, rot.eulerAngles.z);
                }
                else if(parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_UP && parentNote.longNote.next.parentNote.DragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
                {
                    rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, 180);
                }
                transform.localRotation = rot;
                transform.position = parentNote.longNote.next.transform.position;
            }
        }
    }

    public void ChangeNoteColor(Define.COLORS newcolor, int index)
    {
        if(parentNote.isDragTailNote)
        {
            cornerObj.SetActive(false);
        }
        else
        {
            cornerObj.SetActive(true);
        }

        Sprite newSprite = dictionary[newcolor][index];

        if (newSprite != null)
        {
            cornerSprite.sprite = newSprite;
        }
    }

    public bool IsCollided()
    {
        return isCollided;
    }

    public void OnNoteEnter(Corner other)
    {

        isCollided = true;
        // Debug.Log("Corner::OnNoteEnter " + Convert.FloatToTime(parentNote.startTime) + " enter " + Convert.FloatToTime(other.parentNote.startTime));
    }
    public void SetActiveCollider(bool value)
    {

        isCollided = value;
        // Debug.Log("Corner::OnNoteEnter " + Convert.FloatToTime(parentNote.startTime) + " enter " + Convert.FloatToTime(other.parentNote.startTime));
    }

    public void OnNoteExit(Corner other)
    {
        // Debug.Log("Corner::OnNoteExit " + Convert.FloatToTime(parentNote.startTime) + " exit " + Convert.FloatToTime(other.parentNote.startTime));
    }
}
