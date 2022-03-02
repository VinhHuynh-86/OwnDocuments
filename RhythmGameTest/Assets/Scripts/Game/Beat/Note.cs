using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameoptions;


public enum LongNoteSpriteIndex: int
{
    ACTIVE = 0,
    DEACTIVE
}

[System.Serializable]
public class NoteColorsDictionaryEntry
{
    public Define.COLORS key;
    public Sprite[] value;
    public Color glowColor;
}

[System.Serializable]
public class NoteMaterialDictionaryEntry
{
    public Define.COLORS key;
    public Material[] value;
    public Color glowColor;
}


public class Note : MonoBehaviour
{
    // [SerializeField]
    // private List<NoteColorsDictionaryEntry> spriteList;
    [SerializeField]
    private List<NoteMaterialDictionaryEntry> materialList;
    // private Dictionary<Define.COLORS, Sprite[]> dictionary;
    private Dictionary<Define.COLORS, Material[]> dictionary;
    private Dictionary<Define.COLORS, Color> dictionaryGlowColor;
    // public SpriteRenderer noteSprite;    
    public GameObject note3D;
    MeshRenderer note3D_meshRenderer;    
    // public GameObject noteSpriteEnd;    
    public GameObject glowCircle4DualNote;
    public GameObject styleArrow;
    public GameObject styleNormal;
    // [HideInInspector]
    public Define.DRAG_DIRECTION dragRequire = Define.DRAG_DIRECTION.NONE;
    private Define.INPUT_STATUS swipeRequire = Define.INPUT_STATUS.NONE;
    public bool forcedDirection;
    public GameObject longNoteOnBehind;
    public LongNote longNote;
    public GameObject longNoteInfront;
    public GameObject longNoteInCenter;
    public GameObject normalNote;
    public GameObject arrowNote;
    public NoteCollider physicNote;
    private Vector3 centerPos;
    private float radius;
    public GameObject connectingLine;
    // public Corner cornerLineOnBehind;
    public Corner cornerLine;
    // public Corner cornerLineInFront;
    // public Corner cornerLineInCenter;
    public GameObject borderPoint;
    public ParticleSystem normalParticleGlow;
    public ParticleSystem endParticleGlow;
    public Material invisibleMaterial;
    public Material swipeSpecialMaterial;
    public Material swipeNormalMaterial;
    // public Material electricMaterial;
    [HideInInspector]
    public Material visibleMaterial;
    public GameObject[] arrMegaArrowLeft;
    public GameObject[] arrMegaArrowRight;
    public GameObject[] arrNoteArrowLeft;
    public GameObject[] arrNoteArrowRight;
    [HideInInspector]
    public Material[] arrVisibleMaterial = new Material[6];

    // [HideInInspector]
    public float duration;
    // [HideInInspector]
    public bool isInputValid;
    // [HideInInspector]
    public bool forceDestroy;
    // [HideInInspector]
    public Activator target;
    // [HideInInspector]
    public float startTime;
    // [HideInInspector]
    public double startBeatTime;
    // [HideInInspector]
    public int indexAudio = -1;
    // [HideInInspector]
    public int indexCSV = -1;
    // [HideInInspector]
    public int originalIndexCSV = -1;
    // [HideInInspector]
    public int comboNotesKey;
    // [HideInInspector]
    public int linkToAppearLanes;
    // [HideInInspector]
    public bool swipeAnyPlane = false;
    // [HideInInspector]
    public Define.LANE_ON_SIDE isOnSide;
    // [HideInInspector]
    public bool isMissed;
    // [HideInInspector]
    public bool isInCollisionAndHolding;
    // [HideInInspector]
    public bool isHolding;
    // [HideInInspector]
    public int dragNotesGroup;
    // [HideInInspector]
    public int dragNotesKey;
    // [HideInInspector]
    public float dragNoteLenght; // head to tail in seconds
    // [HideInInspector]
    public bool isDragHeadNote;
    // [HideInInspector]
    public bool wasDragHeadNotePressed;
    // [HideInInspector]
    public bool isDragTailNote;
    // [HideInInspector]
    public Define.NOTE_DESTROY destroyType = Define.NOTE_DESTROY.NONE;
    // [HideInInspector]
    public Define.TRIGGER trigger = Define.TRIGGER.NONE;
    // [HideInInspector]
    public bool isCounter = false;
    // [HideInInspector]
    public bool IsLongNote;
    // [HideInInspector]
    public bool IsDragNote;
    // [HideInInspector]
    public bool IsInAutoCollect;
    // [HideInInspector]
    public bool isDualNotes;
    public bool isFirstNote;

    public TailEnd tailEnd;

    //private variables
    private int ID;
    private Define.NOTE_TYPE m_type;
    private bool isAvailable;
    private float time;
    private double holdingTime;
    private double startHoldSongTime;
    private float startZPosition;
    private float endZPosition;
    private Define.COLORS color = Define.COLORS.GRAY;
    private Define.COLORS originalColor = Define.COLORS.NONE;
    InterpolatedTransform interpolatedTransform;
    private bool isVisible = true;

    private event System.Action<Note> stopVfx = null;
    public void RegisterAction(System.Action<Note> fn)
    {
        stopVfx = fn;
    }

    public void PerformAction()
    {
        if (stopVfx != null)
        {
            stopVfx(this);
        }
    }

    public int GetID()
    {
        return ID;
    }

    private void Awake()
    {                
        this.note3D_meshRenderer = this.note3D.GetComponent<MeshRenderer>();

        interpolatedTransform = GetComponent<InterpolatedTransform>();

        if (longNote == null)
        {
            throw new System.MissingFieldException("long note must not be null");
        }

        if (physicNote == null)
        {
            throw new System.MissingFieldException("physic note must not be null");
        }

        dictionary = new Dictionary<Define.COLORS, Material[]>();
        foreach (NoteMaterialDictionaryEntry entry in materialList)
        {
            dictionary.Add(entry.key, entry.value);
        }

        dictionaryGlowColor = new Dictionary<Define.COLORS, Color>();
        foreach (NoteMaterialDictionaryEntry entry in materialList)
        {
            dictionaryGlowColor.Add(entry.key, entry.glowColor);
        }

        Reset();
    }

    void Start()
    {
        centerPos = GameManager.Instance.gameCamera.WorldToScreenPoint(transform.position);
        //BoderPoint
        radius = (centerPos - GameManager.Instance.gameCamera.WorldToScreenPoint(borderPoint.transform.position)).magnitude;        
        // Debug.Log(noteSprite.transform.localScale);
    }

    public Vector3 GetCenterPos()
    {
        return GameManager.Instance.gameCamera.WorldToScreenPoint(transform.position);
    }

    public float GetRadius()
    {
        radius = (GetCenterPos() - GameManager.Instance.gameCamera.WorldToScreenPoint(borderPoint.transform.position)).magnitude;
        return radius;
    }

    public float GetRadiusInWorld()
    {
        float radiusInWorld = (transform.position - borderPoint.transform.position).magnitude;
        return radiusInWorld;
    }
    public Vector3 GetVectorRadiusInWorld()
    {
        return (transform.position - borderPoint.transform.position);
    }

    public Vector3 GetBorderPointPos()
    {
        return borderPoint.transform.position;
    }

    void OnEnable()
    {
        Reset();
    }

    void OnDisable()
    {        
        Reset();
    }

    public void Reset()
    {
        tailEnd.Reset();    

        isFirstNote = false;
        IsLongNote = false;
        IsDragNote = false;
        isHolding = false;
        IsInAutoCollect = false;
        isDualNotes = false;

        isAvailable = true;
        isInputValid = false;
        forceDestroy = false;

        isMissed = false;
        isInCollisionAndHolding = false;

        time = 0;
        holdingTime = 0;
        swipeRequire = Define.INPUT_STATUS.NONE;
        forcedDirection = false;
        dragRequire = Define.DRAG_DIRECTION.NONE;
        dragNotesGroup = 0;
        dragNotesKey = 0;
        dragNoteLenght = 0;
        isDragHeadNote = false;
        isDragTailNote = false;
        wasDragHeadNotePressed = false;

        comboNotesKey = -1;
        linkToAppearLanes = -1;
        swipeAnyPlane = false;
        isOnSide = Define.LANE_ON_SIDE.NONE;
        glowCircle4DualNote.SetActive(false);
        longNote.gameObject.SetActive(true);

        destroyType = Define.NOTE_DESTROY.NONE;
        trigger = Define.TRIGGER.NONE;
        isCounter = false;

        
    }

    public void InitializeStatus()
    {
        note3D.SetActive(true);
        if(IsDragNote)
        {
            if(isDragTailNote)
            {
                cornerLine.gameObject.SetActive(false);
                if(longNote.prev != null)
                {
                    longNote.prev.parentNote.cornerLine.gameObject.SetActive(false);
                }
            }
            else if(longNote.next != null && longNote.next.parentNote.isDragTailNote)
            {
                cornerLine.gameObject.SetActive(false);
            }
            else
            {
                cornerLine.gameObject.SetActive(true);
            }

            if(isDragTailNote)
            {
                note3D.SetActive(false);
                longNote.gameObject.SetActive(false);
            }
            else if(!isDragHeadNote)                         
            {
                note3D.SetActive(false);
            }
        }
        else
        {
            longNote.gameObject.SetActive(false);
            cornerLine.gameObject.SetActive(false);
        }

        SetActiveDualAnim(isDualNotes & !(m_type == Define.NOTE_TYPE.MIGHTY_SWIPE));
    }
    public void CopyFrom(Note other, int newLineIndex)
    {
        ID                  = other.ID + 1;
        name                = other.ID.ToString();
        startTime           = other.startTime;
        originalIndexCSV    = newLineIndex; // other.originalIndexCSV;
        indexCSV            = newLineIndex; // other.indexCSV;
        startBeatTime       = other.startBeatTime;
        isOnSide            = other.isOnSide;
        m_type              = other.type;
        indexAudio          = other.indexAudio;
        IsLongNote          = other.IsLongNote;
        swipeAnyPlane       = other.swipeAnyPlane;
        comboNotesKey       = other.comboNotesKey;
        isDualNotes         = other.isDualNotes;
        linkToAppearLanes   = other.linkToAppearLanes;
        SwipeRequire        = other.SwipeRequire;
        forcedDirection     = other.forcedDirection;
        IsDragNote          = other.IsDragNote;
        dragNotesGroup      = other.dragNotesGroup;
        dragNotesKey        = other.dragNotesKey;
        isDragHeadNote      = other.isDragHeadNote;
        dragNoteLenght      = other.dragNoteLenght;
        isDragTailNote      = other.isDragTailNote;
        originalColor       = other.originalColor;
        Color               = other.color;
        isFirstNote         = other.isFirstNote;
    }

    public void Spawn(BeatData beatData, BeatInfo beatInfo, float noteStartTime, float beatPosition, int lineIndex, int linesNumber)
    {
        ID                  = beatData.ID;
        name                = "" + ID;
        startTime           = noteStartTime;
        originalIndexCSV    = beatInfo.indexCSV;
        indexCSV            = lineIndex;
        startBeatTime       = beatPosition;
        isOnSide            = (lineIndex < linesNumber / 2) ? Define.LANE_ON_SIDE.LEFT : Define.LANE_ON_SIDE.RIGHT;
        m_type              = beatData.type;
        indexAudio          = beatData.indexAudio;
        IsLongNote          = beatData.isLongBeat;
        swipeAnyPlane       = beatData.swipeAnyPlane;
        comboNotesKey       = beatData.comboNotesKey;
        isDualNotes         = beatData.isDualNotes;
        linkToAppearLanes   = beatData.linkToAppearLanes;
        SwipeRequire        = beatData.swipe;
        forcedDirection     = beatData.forcedDirection;
        IsDragNote          = beatData.isDragBeat;
        dragNotesGroup      = beatData.dragNotesGroup;
        dragNotesKey        = beatData.dragNotesKey;
        isDragHeadNote      = beatData.isDragHeadNote;
        dragNoteLenght      = beatData.dragNoteLenght;
        isDragTailNote      = beatData.isDragTailNote;
        originalColor       = beatData.color;
        Color               = beatData.color;
        isFirstNote         = beatData.isFirstNote;

        InitializeStatus();
    }

    // For drag notes
    public void ResetAndDestroyAllFamily(bool isSuccess = true)
    {
        ResetAndDestroyAllFamily(this, isSuccess);
    }

    public void ResetAndDestroyAllFamily(Note myNote, bool isSuccess)
    {
        if (myNote.longNote.prev != null)
        {
            ResetAndDestroyAllFamily(myNote.longNote.prev.parentNote, isSuccess);
        }
        ResetAndDestroyNote(myNote, isSuccess);
    }

    public void ResetAndDestroyMe(bool isSuccess = true, bool isOtherResetConnect = true)
    {
        ResetAndDestroyNote(this, isSuccess, isOtherResetConnect);
    }
    public void ResetAndDestroyNote(Note myNote, bool isSuccess, bool isOtherResetConnect = true)
    {
        if (myNote.longNote.prev != null)
        {
            if(isOtherResetConnect)
            {
                myNote.longNote.prev.next = null;
            }
        }
        if (myNote.longNote.next != null)
        {
            if(isOtherResetConnect)
            {
                myNote.longNote.next.prev = null;
            }
        }
        
        myNote.longNote.next = null;
        myNote.longNote.prev = null;

        ActivatorsManager.Instance.activators3D[myNote.indexCSV].StopVfx(null);
        // GameManager.Instance.colorCounter[myNote.GetOriginalColor()]--; 
        GameManager.Instance.UpdateDragNotesList(myNote.longNote);           
        GameManager.Instance.noteList[myNote.indexCSV].Remove(myNote);
        myNote.MyKill();
    }

    public void ResetAndDestroyPrevious(bool isSuccess = true)
    {
        ResetAndDestroyPrevious(this, isSuccess);
    }
    public void ResetAndDestroyPrevious(Note myNote, bool isSuccess)
    {
        if (myNote.longNote.prev != null)
        {
            ResetAndDestroyPrevious(myNote.longNote.prev.parentNote, isSuccess);
            ResetAndDestroyNote(myNote.longNote.prev.parentNote, isSuccess);
        }
    }

    public bool IsRecover()
    {
        return isVisible;
    }

    public void UpdateStatus()
    {
        if(IsDragNote)
        {
            if(!isDragTailNote)
            {
                if(!longNote.gameObject.activeSelf)
                {
                    longNote.gameObject.SetActive(true);
                }
            }
            else
            {
                if(longNote.gameObject.activeSelf)
                {
                    longNote.gameObject.SetActive(false);
                }
            }

            if(isDragTailNote)
            {
                if(cornerLine.gameObject.activeSelf)
                {
                    cornerLine.gameObject.SetActive(false);
                }

                if(longNote.prev != null)
                {
                    if(longNote.prev.parentNote.cornerLine.gameObject)
                    {
                        longNote.prev.parentNote.cornerLine.gameObject.SetActive(false);
                    }
                }
            }
            else if(longNote.next != null && longNote.next.parentNote.isDragTailNote)
            {
                if(cornerLine.gameObject.activeSelf)
                {
                    cornerLine.gameObject.SetActive(false);
                }
            }
            else
            {
                if(!cornerLine.gameObject.activeSelf)
                {
                    cornerLine.gameObject.SetActive(true);
                }
            }

            gameObject.SetActive(true);
        }
    }

    public bool IsMissed()
    {
        return isMissed;
    }

    public bool IsMissed(Note myNote)
    {
        if (myNote.isMissed)
        {
            return true;
        }

        if (myNote.isDragHeadNote && !myNote.wasDragHeadNotePressed)
        {
            return true;
        }

        if (myNote.longNote.prev != null)
        {
            return IsMissed(myNote.longNote.prev.parentNote);
        }
        
        return false;
    }

    public void Calculate()
    {
        // avoid divide by zero
        duration = Mathf.Max(Mathf.Epsilon, duration);

        startZPosition = gameObject.transform.position.z;
        endZPosition = target ? target.transform.position.z : 0;

        interpolatedTransform.ForgetPreviousTransforms();
    }

    public void CalculatePosition()
    {
        interpolatedTransform.ForgetPreviousTransforms();
    }

    float Lerp(float a, float b, float percent)
    {
        return a + ((b - a) * percent);
    }

    void LateUpdate()
    {
        
        if (!forceDestroy)
        {
            if(!this.IsDragNote || this.isDragHeadNote)
            {
                
            }
        }
        else
        {
            longNote.Reset();
            cornerLine.Reset();
            Reset();
            gameObject.Kill();
        }
    }
    
    public void MyKill()
    {
        if(!forceDestroy)
        {
            GameManager.Instance.colorCounter[GetOriginalColor()]--;   
        }
        forceDestroy = true;

        //If audios didn't play yet, Play them when user collects the first note.
        if(!GameManager.Instance.WasAudioPlayed() && isFirstNote)
        {
            float delayTime = duration - (float)GetPositionInTime();
            {
                Debug.Log("2. Play song");
                GameManager.Instance.PlayAudio(delayTime);
            }
        }
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.IsGamePaused())
        {
            return;
        }

        if (isHolding || indexAudio == -1)
        {
            return;
        }

        if(GameManager.Instance.GetState() != Define.GAME_STATE.INGAME && GameManager.Instance.GetState() != Define.GAME_STATE.SPECIAL_MOVE)
        {
            return;
        }

        double notePosition = GameManager.Instance.GetBeatPosition(indexAudio) - startBeatTime - holdingTime;
        float percent = (float)notePosition / duration;

        transform.position = new Vector3(transform.position.x, transform.position.y, Lerp(startZPosition, endZPosition, percent));
    }

    public double GetPositionInTime()
    {
        return GameManager.Instance.GetBeatPosition(indexAudio) - startBeatTime - holdingTime;
    }

    public bool IsAvailable()
    {
        return isAvailable;
    }
    public void SetAvailable(bool b)
    {
        isAvailable = b;
    }

    public Define.DRAG_DIRECTION DragRequire
    {
        get
        {
            return dragRequire;
        }
        set
        {
            if ((value & Define.DRAG_DIRECTION.DRAGGED) > 0)
            {
                dragRequire = value;
            }
            else
            {
                dragRequire = Define.DRAG_DIRECTION.NONE;
            }

            Change3dNote();
        }
    }

    public Define.INPUT_STATUS SwipeRequire
    {
        get
        {
            return swipeRequire;
        }
        set
        {
            if ((value & Define.INPUT_STATUS.SWIPED) > 0)
            {
                swipeRequire = value;
            }
            else
            {
                swipeRequire = Define.INPUT_STATUS.NONE;
            }

            Change3dNote();
        }
    }

    private void Change3dNote()
    {
        if (styleArrow != null)
        {
            styleArrow.SetActive(
                swipeRequire != Define.INPUT_STATUS.NONE
#if __DEBUG_DRAG
            || dragRequire != Define.DRAG_DIRECTION.NONE
#endif
            );
        }

        if (styleNormal != null)
        {
            styleNormal.SetActive(
                swipeRequire == Define.INPUT_STATUS.NONE
#if __DEBUG_DRAG
            && dragRequire == Define.DRAG_DIRECTION.NONE
#endif
            );
        }

        SwipeNote[] swipeNote = GetComponentsInChildren<SwipeNote>();
        if (swipeNote != null && swipeNote.Length > 0)
        {
            swipeNote[0].Rotate();
        }
    }

    public Define.COLORS Color
    {
        get
        {
            return color;
        }
        set
        {
            ChangeNoteColor(value);
        }
    }

    public Define.NOTE_TYPE type
    {
        get
        {
            return m_type;
        }
        set
        {
            m_type = type;
        }
    }

    public Define.COLORS GetOriginalColor()
    {
        return originalColor;
    } 

    public static int count = 0;
    public void ChangeNoteColor(Define.COLORS newcolor)
    {
        int index = (int)Define.NOTE_SPRITE_ID.NORMAL;
        if(newcolor == Define.COLORS.GRAY)        
        {
            index = (int)Define.NOTE_SPRITE_ID.GRAY;            
        }
        else
        {
            originalColor = newcolor;
        }
        color = newcolor;

        ParticleSystem.MainModule glowMain = normalParticleGlow.main;
        glowMain.startColor = dictionaryGlowColor[originalColor];
        glowMain = endParticleGlow.main;
        glowMain.startColor = dictionaryGlowColor[originalColor];
         
        if (IsDragNote)
        {
            UpdateMaterial(newcolor);
        }
    }

    public void UpdateMaterial(Define.COLORS newcolor)
    {        
        // if(newcolor == Define.COLORS.GRAY)
        // {
        //     return; // Cheat because we donot gray color for connecting-line and conner
        // }
        if(newcolor != Define.COLORS.GRAY)
        {
            longNote.ChangeNoteColor(newcolor, (int)LongNoteSpriteIndex.ACTIVE);
            cornerLine.ChangeNoteColor(newcolor, (int)LongNoteSpriteIndex.ACTIVE);

            if(isDragTailNote)
            {
                tailEnd.gameObject.SetActive(true);
                tailEnd.prev = longNote.prev;
                tailEnd.ChangeNoteColor(newcolor, (int)LongNoteSpriteIndex.ACTIVE);
            }
            else if(longNote.next != null && longNote.next.isTail)
            {
                longNote.next.parentNote.tailEnd.gameObject.SetActive(true);
                longNote.next.parentNote.tailEnd.prev = longNote;
                longNote.next.parentNote.tailEnd.ChangeNoteColor(newcolor, (int)LongNoteSpriteIndex.ACTIVE);
            }
            else
            { 
                tailEnd.gameObject.SetActive(false);
            }
        }
        else
        {
            longNote.ChangeNoteColor(originalColor, (int)LongNoteSpriteIndex.DEACTIVE);
            cornerLine.ChangeNoteColor(originalColor, (int)LongNoteSpriteIndex.DEACTIVE);

            if(isDragTailNote)
            {
                tailEnd.gameObject.SetActive(true);
                tailEnd.prev = longNote.prev;
                tailEnd.ChangeNoteColor(originalColor, (int)LongNoteSpriteIndex.DEACTIVE);
            }
            else if(longNote.next != null && longNote.next.isTail)
            {
                longNote.next.parentNote.tailEnd.gameObject.SetActive(true);
                longNote.next.parentNote.tailEnd.prev = longNote;
                longNote.next.parentNote.tailEnd.ChangeNoteColor(originalColor, (int)LongNoteSpriteIndex.DEACTIVE);
            }
            else
            {
                tailEnd.gameObject.SetActive(false);
            }
        }

    }

    public void Hold()
    {
        if (forceDestroy)
        {
            return;
        }

        if (!isHolding)
        {
            // Debug.LogWarningFormat("Hold {0} drag {1}", startTime, dragRequire);
            isHolding = true;
            startHoldSongTime = GameManager.Instance.GetSongPosition(indexAudio);

            interpolatedTransform.enabled = false;
        }
    }

    public void Release()
    {
        if (isHolding)
        {
            isHolding = false;
            holdingTime = GameManager.Instance.GetSongPosition(indexAudio) - startHoldSongTime;
            // Debug.LogWarningFormat("Release {0} drag {1} holding time {2}", startTime, dragRequire, holdingTime);

            PerformAction();

            interpolatedTransform.enabled = true;
            interpolatedTransform.ForgetPreviousTransforms();
        }
    }

    public void OnNoteEnter(NoteCollider other)
    {
        Note otherNote = other.root;
        this.longNote.OnNoteEnter(otherNote.longNote);
    }

    public void OnNoteExit(NoteCollider other)
    {
        Note otherNote = other.root;
        this.longNote.OnNoteExit(otherNote.longNote);
    }
    public void SetActiveDualAnim(bool value)
    {
        if (value)
        {
            glowCircle4DualNote.SetActive(true);
        }
        else
        {
            glowCircle4DualNote.SetActive(false);
        }
    }

}
