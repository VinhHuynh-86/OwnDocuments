using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConnectingBarDictionaryEntry
{
    public Define.COLORS key;
    public Sprite[] value;
}

public class LongNote : MonoBehaviour
{
    public List<ConnectingBarDictionaryEntry> spriteList;
    private Dictionary<Define.COLORS, Sprite[]> dictionary;
    public SpriteRenderer noteSprite;
    // [HideInInspector]
    public LongNote next;
    // [HideInInspector]
    public LongNote prev;
    // [HideInInspector]
    public bool isTail;
    // [HideInInspector]
    public bool isHead;

    private Vector3 direction;
    //public Vector3 directionFollowLine;
    private float distance;
    private Vector3 originalLocalScale;
    private bool isCollided;
    public Note parentNote;
    public Transform facingTarget;
    //public bool isMissed;
    //public bool canTouch;

    private GameInput gameInput;
    [HideInInspector]
    public Vector3 currentTouch;

    private void Awake()
    {
        if (parentNote == null)
        {
            throw new System.MissingFieldException("parent note must not be null");
        }

        originalLocalScale = transform.localScale;
        dictionary = new Dictionary<Define.COLORS, Sprite[]>();
        foreach (ConnectingBarDictionaryEntry entry in spriteList)
        {
            dictionary.Add(entry.key, entry.value);
        }

        Reset();
    }

    public void Reset()
    {
        next = null;
        prev = null;
        isTail = false;
        isHead = false;

        direction = Vector3.zero;
        distance = 0;

        isCollided = false;
        //isMissed = false;
        //canTouch = false;

        currentTouch = Vector3.zero; //Vector3.positiveInfinity;

        transform.localPosition = parentNote.longNoteOnBehind.transform.localPosition;
        transform.rotation = Quaternion.identity;
        facingTarget.gameObject.SetActive(true);
        
        gameInput = GetComponent<GameInput>();
    }

    public void ChangeNoteColor(Define.COLORS newcolor, int index)
    {
        Sprite newSprite = dictionary[newcolor][index];

        if (newSprite != null)
        {
            noteSprite.sprite = newSprite;
        }
    }

    // public bool IsMissed(LongNote myNote)
    // {
    //     if (myNote.isMissed)
    //     {
    //         return true;
    //     }

    //     if (myNote.parentNote.isDragHeadNote && !myNote.parentNote.wasDragHeadNotePressed)
    //     {
    //         return true;
    //     }

    //     if (myNote.prev != null)
    //     {
    //         return IsMissed(myNote.prev);
    //     }
    //     //check corner missed
        
    //     return false;
    // }
    public void Initialize()
    {
        transform.localPosition = parentNote.longNoteOnBehind.transform.localPosition;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (next != null)
        {
            DrawLine(next);
        }
    }

    void LateUpdate()
    {
        if (next != null)
        {
            if (next.parentNote.forceDestroy)
            {
                // Debug.Log("unlink to note " + Convert.FloatToTime(next.parentNote.startTime) + " but keep the cube line");
                next = null;
                prev = null;
            }
        }
    }

    private void DrawLine(Vector3 srcPosition, LongNote other)
    {
        if(next != null && next.parentNote.isDragTailNote)
        {
            if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_UP)
            {
                next.transform.position = next.parentNote.longNoteOnBehind.transform.position;
            }
            else
            {
                next.transform.position = next.parentNote.longNoteOnBehind.transform.position;
            }
        }
        transform.position = srcPosition;

        direction = other.transform.position - transform.position;
        distance = direction.magnitude;
        
        {
            if (direction != Vector3.zero)
            {
                if (facingTarget != null)
                {
                    transform.rotation = Quaternion.LookRotation(direction, facingTarget.forward);
                }
                else
                {
                    transform.rotation = Quaternion.LookRotation(direction, other.transform.rotation * Vector3.up);
                }
            }

            distance /= parentNote.transform.localScale.z;

            if(next != null && next.parentNote.isDragTailNote)
            {
                if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_UP)
                {
                    next.transform.position = next.parentNote.longNoteOnBehind.transform.position;                    
                }
                else
                {
                    transform.position = parentNote.longNoteOnBehind.transform.position;
                }

                
            }

            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, distance);

        }

    }


    private void DrawLine(LongNote other)
    {
        if(next != null && next.parentNote.isDragTailNote)
        {
            if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_UP)
            {
                next.transform.position = next.parentNote.longNoteOnBehind.transform.position;
            }
            else
            {
                next.transform.position = next.parentNote.longNoteOnBehind.transform.position;
            }
        }

        direction = other.transform.position - transform.position;
        distance = direction.magnitude;
        
        if (direction != Vector3.zero)
        {
            if (facingTarget != null)
            {
                transform.rotation = Quaternion.LookRotation(direction, facingTarget.forward);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(direction, other.transform.rotation * Vector3.up);
            }
        }

        distance /= parentNote.transform.localScale.z;

        if(next != null && next.parentNote.isDragTailNote)
        {
            if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_UP)
            {
                next.transform.position = next.parentNote.longNoteOnBehind.transform.position;
                
            }
            else //if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_LEFT || parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
            {
                transform.position = parentNote.longNoteOnBehind.transform.position;
            }

            
        }

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, distance);
    }


    public bool IsCollided()
    {
        return isCollided;
    }

    public void OnNoteEnter(LongNote other)
    {

        isCollided = true;
        // Debug.Log("LongNote::OnNoteEnter " + Convert.FloatToTime(parentNote.startTime) + " enter " + Convert.FloatToTime(other.parentNote.startTime));
    }
    public void SetActiveCollider(bool value)
    {

        isCollided = value;
        // Debug.Log("LongNote::OnNoteEnter " + Convert.FloatToTime(parentNote.startTime) + " enter " + Convert.FloatToTime(other.parentNote.startTime));
    }

    public void OnNoteExit(LongNote other)
    {
        // Debug.Log("LongNote::OnNoteExit " + Convert.FloatToTime(parentNote.startTime) + " exit " + Convert.FloatToTime(other.parentNote.startTime));
    }
}
