using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailEnd : MonoBehaviour
{
    public List<ConnectingBarDictionaryEntry> spriteList;
    private Dictionary<Define.COLORS, Sprite[]> dictionary;
    SpriteRenderer noteSprite;
    // [HideInInspector]
    public LongNote prev;
    private Vector3 direction;
    public Note parentNote;
    public Transform facingTarget;

    public GameObject end;
    public GameObject tailParticle;

    private void Awake()
    {
        if (parentNote == null)
        {
            throw new System.MissingFieldException("parent note must not be null");
        }

        dictionary = new Dictionary<Define.COLORS, Sprite[]>();
        foreach (ConnectingBarDictionaryEntry entry in spriteList)
        {
            dictionary.Add(entry.key, entry.value);
        }
        noteSprite = end.GetComponent<SpriteRenderer>();

        Reset();
    }

    public void Reset()
    {
        tailParticle.SetActive(false);
        end.SetActive(false);

        prev = null;
        direction = Vector3.zero;

        transform.localPosition = parentNote.longNoteOnBehind.transform.localPosition;
        transform.rotation = Quaternion.identity;
        facingTarget.gameObject.SetActive(true);
        
    }

    public void ChangeNoteColor(Define.COLORS newcolor, int index)
    {
        if(parentNote.isDragTailNote)
        {
            tailParticle.SetActive(true);
            end.SetActive(true);
        }
        else
        {
            tailParticle.SetActive(false);
            end.SetActive(false);
        }

        Sprite newSprite = dictionary[newcolor][index];

        if (newSprite != null)
        {
            noteSprite.sprite = newSprite;
        }
    }

    public void Initialize()
    {
        transform.localPosition = parentNote.longNoteOnBehind.transform.localPosition;
    }
    
    // Update is called once per frame
    void Update()
    {
        prev = parentNote.longNote.prev;
        if (prev != null)
        {
            UpdateLookAt(prev);
        }
    }

    private void UpdateLookAt(LongNote other)
    {
        direction = other.transform.position - transform.position;
        
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

        if(other != null && other.parentNote.isDragTailNote)
        {
            if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_UP)
            {
                other.transform.position = other.parentNote.longNoteOnBehind.transform.position;
                
            }
            else //if(parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_LEFT || parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_RIGHT)
            {
                transform.position = parentNote.longNoteOnBehind.transform.position;
            }
        }
    }
}
