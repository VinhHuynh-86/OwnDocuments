using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(GameInput))]
public class Activator : MonoBehaviour, IActivatorCollider<NoteCollider>
{
    public GameObject borderPoint;
    private GameInput gameInput;
    private Define.INPUT_STATUS inputStatus = Define.INPUT_STATUS.NONE;
    private Define.INPUT_STATUS lastInputStatus;
    public int activatorIndex = 0;
    GameManager gameManager;
    Vector3 oldTransform;

    Define.TRIGGER trigger = Define.TRIGGER.NONE;
    int state = Define.ACTIVATOR_STATE_NONE;
    Timer timeHoldControl = new Timer();

    private bool isFirstTriggerLongNote = true;
    private bool isCollectTailNote = false;


    private Define.TRIGGER triggerDragHolding = Define.TRIGGER.NONE;
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    [Header("VFX")]
    public ParticleSystem[] VFXTapNormal;
    public ParticleSystem[] VFXTapPerfect;
    public ParticleSystem[] VFXTapLoop;
    public ParticleSystem[] VFXTapPerfectLoop;
    public ParticleSystem[] VFXSwipeLeft;
    public ParticleSystem[] VFXBarSwipe;
    public ParticleSystem VFXShiftLeft;
    public ParticleSystem VFXBadTap;

    float hideTransparent = 0.2f;
    private Vector3 centerPos;
    private float radius;
    private float EASY_DISTANCE = 1f;
    private float EASY_DISTANCE_2 = 1.5f;
    // private float EXACTLY_DISTANCE = 0.7f;
    private ParticleSystem curLoopVFX = null;
    List<Note> notesInLine;// = new List<Note>();
    ParticleSystem _ps = null;
    // float outRange = 0;
    int noteCount = 0;
    private float TIME_DELAY_PRESS_DRAG_NOTE = 0.5f;
    private float timeControlPressDragNote = 0.5f; // for first missing. User can touch at moment.

    bool collectOneNote = false;
    bool touchOnMe = false;

    private void Awake()
    {
        gameInput = GetComponent<GameInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // outRange = transform.position.z - 1.8f;

        gameManager = GameManager.Instance;

        oldTransform = gameObject.transform.localScale;

        centerPos = GetCenterPos();

        //BoderPoint
        radius = (centerPos - GameManager.Instance.gameCamera.WorldToScreenPoint(borderPoint.transform.position)).magnitude;
        Define.NEAR_ACTIVATOR_CENTER = radius/2;
    }

    public Vector3 GetCenterPos()
    {
        // centerPos = GameManager.Instance.gameCamera.WorldToScreenPoint(spriteRenderer.bounds.center);
        return GameManager.Instance.gameCamera.WorldToScreenPoint(transform.position);
    }

    public float GetRadius()
    {
        return radius;
    }

    public void Hide()
    {
        Color c = spriteRenderer.color;
        c.a = hideTransparent;
        spriteRenderer.color = c;
    }
    public void Show()
    {
        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        if((GameManager.Instance.GetState() != Define.GAME_STATE.INGAME && GameManager.Instance.GetState() != Define.GAME_STATE.SPECIAL_MOVE))
            return;

        if (GameManager.Instance.IsGamePaused()) return;

        lastInputStatus = inputStatus;
        inputStatus = Define.INPUT_STATUS.NONE;

        if (gameInput.IsTap())
        {
            inputStatus |= Define.INPUT_STATUS.PRESSED;
        }

        if (gameInput.IsHolding())
        {
            inputStatus |= Define.INPUT_STATUS.HOLDING;
        }

        if (gameInput.IsSwipeLeftOnRightSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_LEFT_ON_RIGHT_SIDE;
        }
        else if (gameInput.IsSwipeLeftOnLeftSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_LEFT_ON_LEFT_SIDE;
        }

        if (gameInput.IsSwipeRightOnRightSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_RIGHT_ON_RIGHT_SIDE;
        }
        else if (gameInput.IsSwipeRightOnLeftSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_RIGHT_ON_LEFT_SIDE;
        }
        if (gameInput.IsSwipeUpOnRightSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_UP_ON_RIGHT_SIDE;
        }
        else if (gameInput.IsSwipeUpOnLeftSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_UP_ON_LEFT_SIDE;
        }

        if (gameInput.IsSwipeDownOnRightSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_DOWN_ON_RIGHT_SIDE;
        }
        else if (gameInput.IsSwipeDownOnLeftSide())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_DOWN_ON_LEFT_SIDE;
        }

        if (gameInput.IsSwipeUp())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_UP;
        }

        if (gameInput.IsSwipeDown())
        {
            inputStatus |= Define.INPUT_STATUS.SWIPE_DOWN;
        }

        // // Fixed: after collect long note, vfx doesn't stop
        // if (inputStatus == Define.INPUT_STATUS.NONE || (lastInputStatus & inputStatus) != Define.INPUT_STATUS.HOLDING)
        // {
        //     // StopVfx(null);
        // }

        if(!GameManager.Instance.IsCheckEndGame() && !GameManager.Instance.IsBoardGameFadeOut())
        {
            if ((inputStatus & Define.INPUT_STATUS.PRESSED) > 0)
            {
                PlayTapAnimation();
            }
        }

        notesInLine = GameManager.Instance.GetNoteInLine(activatorIndex);
        if(notesInLine == null)
            return;
            
        noteCount = notesInLine.Count;
        if (noteCount < 1)
            return;

        //If audios didn't play yet, Play them when first note reaches the activator.
        if(!GameManager.Instance.WasAudioPlayed() && notesInLine[0].isFirstNote)
        {
            if((float)notesInLine[0].GetPositionInTime() + notesInLine[0].startTime + Time.deltaTime > GameManager.Instance.GetDuration())
            {
                Debug.Log("1. Play song ");
                // GameManager.Instance.MarkAudioWasPlayed(); 
                GameManager.Instance.PlayAudio(0);
            }
        }

        if(inputStatus != Define.INPUT_STATUS.NONE)
        {
            // if(!touchOnMe)
            // {
            //     touchOnMe = gameInput.IsStartOnMe();
            // }
            // if(!touchOnMe)
            // {
            //     inputStatus = Define.INPUT_STATUS.NONE;
            //     collectOneNote = false;
            //     touchOnMe = false;
            //     return;

            // }
            if(collectOneNote)
                return;
        }
        else
        {
            collectOneNote = false;
            touchOnMe = false;
        }

        centerPos = GetCenterPos(); // need to get here to make sure we have exact calue

        CheckIntoCollision();

        // check to destroy and stop effect for drag note
        bool isReset = false;
        noteCount = notesInLine.Count;
        for (int i = 0; i < noteCount && i < 2; ++i)
        {
            if (isReset == true)
            {
                isReset = false;
                i = 0;
            }

            if (i < notesInLine.Count && notesInLine[i].IsDragNote)
            {
                Vector3 noteCenterPos = notesInLine[i].GetCenterPos();
                // centerPos = GetCenterPos();
                float dist = (noteCenterPos - centerPos).magnitude;

                // if(notesInLine[i].name == "797")
                // {
                //     Debug.Log("797 , dist = " + dist);

                //     Debug.Log("notesInLine[i].transform.position = " + notesInLine[i].transform.position + "transform.position = " + transform.position);
                //     Debug.Log("noteCenterPos = " + noteCenterPos + " ; centerPos = " + centerPos);
                // }

                if (dist < Define.NEAR_ACTIVATOR_CENTER || (dist > Define.NEAR_ACTIVATOR_CENTER && noteCenterPos.y < centerPos.y)) // Stop render note(remove) when note near/aground center of activator/perfect-line.
                {
                    // float dist2 = radius + notesInLine[i].GetRadius();
                    // Debug.Log("dist2 = " + dist2);

                    if (notesInLine[i].longNote.prev != null && notesInLine[i].longNote.prev.parentNote.isInCollisionAndHolding || notesInLine[i].isInCollisionAndHolding)
                    {
                        if (notesInLine[i].isDragTailNote)
                        {
                            PlayNormalTapVFX(notesInLine[i], trigger);
                            notesInLine[i].ResetAndDestroyAllFamily();
                            if(notesInLine.Count != noteCount)
                            {
                                noteCount = notesInLine.Count;
                                isReset = true;
                            }
                        }
                        else if (!notesInLine[i].isDragHeadNote)
                        {
                            
                            if (notesInLine[i].longNote.prev != null)
                            {
                                // Debug.Log("ResetAndDestroyPrevious notesInLine[i].name = " + notesInLine[i].name);

                                notesInLine[i].ResetAndDestroyPrevious();
                                if(notesInLine.Count != noteCount)
                                {
                                    noteCount = notesInLine.Count;
                                    isReset = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        CheckEndCollision();
    }

    public void ReleaseNote(Note note)
    {
        // Debug.Log("ReleaseNote " + note.name);

        ProcessMissingNote(note);

        StopVfx(null);

        note.longNote.facingTarget.gameObject.SetActive(true);
        note.Release();
        timeHoldControl.Reset();
        isFirstTriggerLongNote = true;
    }
    bool CheckIntoCollision()
    {
        noteCount = notesInLine.Count;
        int i = 0;

        if (IsAutoCollect())
        {
            for(i = 0; i < noteCount; ++i)
            {
                if(GameManager.Instance.ReleaseAll() && !TutorialManager.Instance.IsKeepGoingState())
                {
                    if (notesInLine[i].isInCollisionAndHolding)
                    {
                        ReleaseNote(notesInLine[i]);
                        return true;
                    }
                }
                Vector3 noteCenter = notesInLine[i].GetCenterPos();
                // centerPos = GetCenterPos();
                float dist1 = (noteCenter - centerPos).magnitude;
                float dist2 = radius + notesInLine[i].GetRadius();

                if (dist1 < dist2 * Define.DISTANCE_HAND_TUTORIAL)
                {
                    if(!notesInLine[i].isCounter)
                    { 
                        HapticNote(notesInLine[i]);
                        notesInLine[i].isCounter = true; 
                        if(!notesInLine[i].isDragHeadNote && notesInLine[i].longNote.prev != null && notesInLine[i].longNote.prev.parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_LEFT)
                        {
                            TutorialManager.Instance.UpdateHand(notesInLine[i].longNote.prev.parentNote);
                        }
                        else
                        {
                            TutorialManager.Instance.UpdateHand(notesInLine[i]);
                        }
                    }
                }

                if (dist1 < dist2 * Define.DISTANCE_PERFECT)
                {        
                    trigger = Define.TRIGGER.PERFECT;
                    notesInLine[i].trigger = trigger;
                    System.Action<Note> a = StopVfx;
                    notesInLine[i].RegisterAction(a);
                    notesInLine[i].isInputValid = true;

                    if (notesInLine[i].isDragHeadNote)
                    {
                        notesInLine[i].IsInAutoCollect = true;
                        notesInLine[i].wasDragHeadNotePressed = true;
                    }          

                    // != drag note
                    if (UpdateNote(notesInLine[i]) == true)
                    {
                        if(notesInLine[i].type == Define.NOTE_TYPE.MIGHTY_SWIPE)
                        {
                            GameManager.Instance.UpdateComboNotes();
                        }
                        else
                        {
                            // GameManager.Instance.colorCounter[notesInLine[i].GetOriginalColor()]--;
                            notesInLine[i].MyKill();
                            notesInLine.RemoveAt(i);
                        }                        
                        return true;
                    }                   
                }
            }
            return false;
        }

        if(TutorialManager.Instance.IsKeepGoingState())
        {
            for(i = 0; i < noteCount; ++i)
            {
                Vector3 noteCenter = notesInLine[i].GetCenterPos();
                float dist1 = (noteCenter - centerPos).magnitude;
                float dist2 = radius + notesInLine[i].GetRadius();
                
                if (dist1 < dist2 * Define.DISTANCE_HAND_TUTORIAL)
                {
                    if(!notesInLine[i].isCounter)
                    { 
                        notesInLine[i].isCounter = true; 
                        if(!notesInLine[i].isDragHeadNote && notesInLine[i].longNote.prev != null && notesInLine[i].longNote.prev.parentNote.dragRequire == Define.DRAG_DIRECTION.DRAG_LEFT)
                        {
                            TutorialManager.Instance.UpdateHand(notesInLine[i].longNote.prev.parentNote);
                        }
                        else
                        {
                            TutorialManager.Instance.UpdateHand(notesInLine[i]);
                        }
                    }
                }
            }
        }

        // Check holding and release        
        if (inputStatus == Define.INPUT_STATUS.NONE && lastInputStatus != Define.INPUT_STATUS.NONE)
        {
            for (i = 0; i < noteCount; ++i)
            {
                if (notesInLine[i].IsDragNote && !notesInLine[i].isDragTailNote)
                {
                    if (notesInLine[i].isInCollisionAndHolding || notesInLine[i].isHolding)
                    {
                        ReleaseNote(notesInLine[i]);

                        // return true;
                    }
                }
            }
        }

        if (timeControlPressDragNote < TIME_DELAY_PRESS_DRAG_NOTE)
        {
            timeControlPressDragNote += Time.deltaTime;
        }

        if (inputStatus != Define.INPUT_STATUS.NONE)
        {
            if(!collectOneNote)
            {                
                // check collision 2D
                for (i = 0; i < noteCount; ++i)
                {
                    Vector3 noteCenter = notesInLine[i].GetCenterPos();
                    // centerPos = GetCenterPos();
                    float dist1 = (noteCenter - centerPos).magnitude;
                    float noteRadius = notesInLine[i].GetRadius();
                    float dist2 = radius + noteRadius;

                    //noteIndex++;
                    //Debug.LogFormat("Activator->CheckCollision : note.isInCollisionAndHolding {0} ; Note index({1})", note.isInCollisionAndHolding, noteIndex);


                    {
                        if(noteCenter.y < centerPos.y || (notesInLine[i].IsDragNote && !notesInLine[i].isDragHeadNote)) // extend touch-zon for narmal note below perfect circles/activators
                        {
                            // Debug.Log("2) dist1 = " + dist1 + "; dist2 = " + dist2);
                            // Debug.LogFormat("Activator::EASY_DISTANCE_2");
                            dist2 *= EASY_DISTANCE_2;
                        }
                        else
                        {
                            // Debug.Log("dist1 = " + dist1 + "; dist2 = " + dist2);
                            // Debug.LogFormat("Activator::EASY_DISTANCE");
                            dist2 *= EASY_DISTANCE;
                        }
                    }

                    if (dist1 < dist2)
                    {
                        // compute trigger
                        trigger = Define.TRIGGER.COOL;

                        if (dist1 < dist2 * Define.DISTANCE_PERFECT)
                        {
                            trigger = Define.TRIGGER.PERFECT;
                        }
                        else if (dist1 < dist2 * Define.DISTANCE_GREAT)
                        {
                            trigger = Define.TRIGGER.GREAT;
                        }

                        notesInLine[i].trigger = trigger;

                        // Trigger tail note same with trigger Head note
                        if (notesInLine[i].isDragTailNote)
                        {
                            if (notesInLine[i].longNote.prev != null)
                            {
                                Note prev = notesInLine[i].longNote.prev.parentNote;
                                if (prev.DragRequire == Define.DRAG_DIRECTION.DRAG_UP)
                                {
                                    trigger = prev.trigger;
                                }
                            }
                        }

                        System.Action<Note> a = StopVfx;
                        notesInLine[i].RegisterAction(a);

                        notesInLine[i].isInputValid = true;

                        if (notesInLine[i].isDragHeadNote)
                        {
                            
                            if(!notesInLine[i].wasDragHeadNotePressed)
                            {
                                if(ProfileMgr.Instance.Vibration)
                                {
                                    #if UNITY_ANDROID
                                        Vibration.Vibrate(Define.VIBRATION_STRENGTH);
                                    #elif UNITY_IOS
                                        Vibration.VibratePop();
                                    #endif
                                }
                            }
                            notesInLine[i].wasDragHeadNotePressed = true;
                        }

                        if (notesInLine[i].longNote.prev != null && notesInLine[i].IsDragNote && notesInLine[i].longNote.prev.parentNote.IsMissed())
                        {
                            notesInLine[i].isInputValid = false;
                            notesInLine[i].isMissed = true;
                            notesInLine[i].isInCollisionAndHolding = false;
                        }

                        SFXManager.Instance.ResetMistakes();

                        // != drag note
                        if (UpdateNote(notesInLine[i]) == true)
                        {
                            if(ProfileMgr.Instance.Vibration)
                            {
                                #if UNITY_ANDROID
                                    Vibration.Vibrate(Define.VIBRATION_STRENGTH);
                                #elif UNITY_IOS
                                    Vibration.VibratePop();
                                #endif
                            }

                            if(notesInLine[i].type == Define.NOTE_TYPE.MIGHTY_SWIPE)
                            {
                                GameManager.Instance.UpdateComboNotes();
                            }
                            else
                            {
                                // GameManager.Instance.colorCounter[notesInLine[i].GetOriginalColor()]--;
                                notesInLine[i].MyKill();
                                notesInLine.RemoveAt(i);
                            }
                            collectOneNote = true;
                            return true;
                        }
                    }
                    else
                    {
                        if (notesInLine[i].transform.position.z <= transform.position.z)
                        {
                            // Check head or mid note is missed: detect tap on bar long note
                            if (notesInLine[i].isMissed && timeControlPressDragNote >= TIME_DELAY_PRESS_DRAG_NOTE && notesInLine[i].DragRequire == Define.DRAG_DIRECTION.DRAG_UP && !notesInLine[i].isDragTailNote)
                            {
                                Vector3 nextPos = notesInLine[i].longNote.next.parentNote.GetCenterPos();
                                if (nextPos.z > centerPos.z)
                                {
                                    notesInLine[i].isInputValid = true;
                                    notesInLine[i].isMissed = false;
                                    notesInLine[i].isInCollisionAndHolding = true;
                                    notesInLine[i].SetAvailable(true);
                                    timeControlPressDragNote = 0;

                                    trigger = Define.TRIGGER.COOL;
                                    notesInLine[i].trigger = trigger;
                                    UpdateNote(notesInLine[i]);
                                }
                            }
                        }
                        else
                        {
                            if ((inputStatus & Define.INPUT_STATUS.PRESSED) > 0 && dist1 < 2 * dist2 && ((!notesInLine[i].IsDragNote) || (notesInLine[i].IsDragNote && notesInLine[i].isDragHeadNote))) // penalty: dist1 > dist2 && dist1 < 2 * dist2
                            {
                                // Debug.LogFormat("PlayBadAnimation activatorIndex = {0}", activatorIndex);
                                PlayBadAnimation();
                                SFXManager.Instance.AddMistakes();
                                ScoreManager.Instance.ResetStreakPerfect();
                                ScoreManager.Instance.ResetCombo();
                            }

                            return false;
                        }
                    }
                }
            }
        }

        return false;

    }

    void ProcessMissingNote(Note note)
    {
        trigger = Define.TRIGGER.NONE;
        note.isInputValid = false;
        StopVfx(null);
        if (note.longNote.prev != null)
        {
            ActivatorsManager.Instance.activators3D[note.longNote.prev.parentNote.indexCSV].StopVfx(null);
        }

        //if (note.IsAvailable())
        {
            note.SetAvailable(false);

            if(note.type == Define.NOTE_TYPE.MIGHTY_SWIPE)
            {
                gameManager.mightySwipeNotes.Clear();
            }

            if (note.IsDragNote)
            {
                timeControlPressDragNote = 0;
                note.isMissed = true;
                note.isInCollisionAndHolding = false;
                triggerDragHolding = Define.TRIGGER.NONE;
            }

            ScoreManager.Instance.ResetStreakPerfect();
            ScoreManager.Instance.CounterStreakMiss();
            ScoreManager.Instance.ResetCombo();
            gameManager.listTimeMissNote.Add(note.startTime);
            if (note.type != Define.NOTE_TYPE.SWIPE)
            {
                BeatManager.Instance.DisableNote(note.indexAudio);
            }
        }
    }

    void CheckEndCollision()
    {
        bool isReset = false;
        for (int i = 0; i < notesInLine.Count; ++i)
        {
            if (isReset == true)
            {
                isReset = false;
                i = 0;
            }

            {
                //dist = outRange;
                Vector3 noteCenter = notesInLine[i].GetCenterPos();
                // centerPos = GetCenterPos();
                float dist1 = (noteCenter - centerPos).magnitude;
                float dist2 = radius + notesInLine[i].GetRadius();

                if(noteCenter.z > centerPos.z)
                {
                    return;
                }

                // if (notesInLine[i].IsDragNote || notesInLine[i].SwipeRequire != Define.INPUT_STATUS.NONE)
                {
                    dist2 *= EASY_DISTANCE_2;
                }

                {
                    if (notesInLine[i].IsAvailable() == true)
                    {
                        if (dist1 > dist2 && noteCenter.z < centerPos.z)
                        {
                            SFXManager.Instance.AddMistakes();
                            ProcessMissingNote(notesInLine[i]);
                        }
                    }
                }

                // check to destroy

                {

                    if (noteCenter.y < 0 && dist1 > dist2)
                    {
                        if (notesInLine[i].IsDragNote)
                        {
                            if (notesInLine[i].isDragTailNote)
                            {
                                notesInLine[i].ResetAndDestroyAllFamily(false);
                                return;
                            }
                            else
                            {
                                if (notesInLine[i].longNote.prev != null)
                                {
                                    notesInLine[i].ResetAndDestroyPrevious(false);
                                    // isReset = true;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // GameManager.Instance.colorCounter[notesInLine[i].GetOriginalColor()]--;
                            notesInLine[i].MyKill();
                            GameManager.Instance.noteList[notesInLine[i].indexCSV].Remove(notesInLine[i]);

                            // isReset = true;
                            return;
                        }

                    }
                }

            }
        }
    }

    private bool UpdateNote(Note noteComponent)
    {
        bool needToRemove = false;
        if (noteComponent == null)
        {
            return needToRemove;
        }

        if ((!noteComponent.isInputValid || !noteComponent.IsAvailable() || noteComponent.forceDestroy))
        {
            return needToRemove;
        }

        bool hasScore = false;
        int score = 0;

        if (noteComponent.IsLongNote || noteComponent.IsDragNote)
        {
            if (triggerDragHolding != Define.TRIGGER.NONE)
            {
                trigger = triggerDragHolding;
            }

            if ((inputStatus & Define.INPUT_STATUS.HOLD) > 0
                || IsAutoCollect()
            )
            {
                if (isFirstTriggerLongNote)
                {
                    isFirstTriggerLongNote = false;
                    PlayScoreTextVfx(trigger);
                    BeatManager.Instance.EnableNote(noteComponent.indexAudio);
                    hasScore = true;
                    score = ScoreManager.Instance.GetScoreNote(noteComponent.type, trigger);
                    timeHoldControl.SetDuration(ScoreManager.Instance.HOLD_TIME);
                    PlayTapVfx(noteComponent, trigger, true);

                    if (noteComponent.longNote.prev)
                    {
                        Define.DRAG_DIRECTION prev_dir = noteComponent.longNote.prev.parentNote.DragRequire;
                        ActivatorsManager.Instance.activators3D[noteComponent.longNote.prev.parentNote.indexCSV].PlayBarSwipeVfx(noteComponent, prev_dir);
                    }
                }
                else
                {
                    timeHoldControl.Update(Time.deltaTime);
                    if (timeHoldControl.JustFinished())
                    {
                        PlayScoreTextVfx(trigger);
                        hasScore = true;
                        score = ScoreManager.Instance.GetScoreNote(noteComponent.type, trigger, true);
                        timeHoldControl.Reset();
                    }
                }

                noteComponent.isInCollisionAndHolding = true;

                if (noteComponent.isDragTailNote)
                {
                    triggerDragHolding = Define.TRIGGER.NONE;
                    // Reset all
                    noteComponent.destroyType = Define.NOTE_DESTROY.ALL;

                }
                else
                {
                    if (curLoopVFX == null)
                    {
                        PlayTapVfx(noteComponent, trigger, true);
                        if (noteComponent.longNote.prev)
                        {
                            Define.DRAG_DIRECTION prev_dir = noteComponent.longNote.prev.parentNote.DragRequire;
                            ActivatorsManager.Instance.activators3D[noteComponent.longNote.prev.parentNote.indexCSV].PlayBarSwipeVfx(noteComponent, prev_dir);
                        }
                    }
                    triggerDragHolding = trigger;

                    noteComponent.destroyType = Define.NOTE_DESTROY.PREVIOUS;

                    if (noteComponent.DragRequire == Define.DRAG_DIRECTION.DRAG_UP)
                    {
                        noteComponent.transform.position = new Vector3(noteComponent.transform.position.x, noteComponent.transform.position.y, transform.position.z);
                        noteComponent.CalculatePosition();

                        // Reset me
                        noteComponent.longNote.facingTarget.gameObject.SetActive(false);
                        noteComponent.Hold();

                        // {
                        //     Debug.Log("Hold noteComponent.name = " + noteComponent.name);
                        //     Debug.Log("Hold noteComponent.transform.position = " + noteComponent.transform.position);
                        //     Debug.Log("Hold transform.position = " + transform.position);
                        // }
                    }
                }                
            }
        }
        else
        {
            if (noteComponent.SwipeRequire != Define.INPUT_STATUS.NONE)
            {
                if ((noteComponent.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT && noteComponent.isOnSide == Define.LANE_ON_SIDE.RIGHT && (inputStatus & Define.INPUT_STATUS.SWIPE_RIGHT_ON_RIGHT_SIDE) > 0)
                || (noteComponent.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT && noteComponent.isOnSide == Define.LANE_ON_SIDE.LEFT && (inputStatus & Define.INPUT_STATUS.SWIPE_RIGHT_ON_LEFT_SIDE) > 0)
                || (noteComponent.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT && noteComponent.isOnSide == Define.LANE_ON_SIDE.RIGHT && (inputStatus & Define.INPUT_STATUS.SWIPE_LEFT_ON_RIGHT_SIDE) > 0)
                || (noteComponent.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT && noteComponent.isOnSide == Define.LANE_ON_SIDE.LEFT && (inputStatus & Define.INPUT_STATUS.SWIPE_LEFT_ON_LEFT_SIDE) > 0)
                || IsAutoCollect()
                )
                {
                    //bool touchOnMe = gameInput.IsStartSwipeOnMe();
                    //if(touchOnMe || noteComponent.swipeAnyPlane)
                    {
                        if(noteComponent.type == Define.NOTE_TYPE.MIGHTY_SWIPE)
                        {
                            if (noteComponent.comboNotesKey != -1)
                            {
                                if ((gameManager.mightySwipeNotes.Count == 0 || !gameManager.mightySwipeNotes.Contains(noteComponent)))
                                {
                                    int count = gameManager.mightySwipeNotes.Count;
                                    if (count == 1 && gameManager.mightySwipeNotes[0].comboNotesKey != noteComponent.comboNotesKey)
                                    {
                                        gameManager.mightySwipeNotes.Clear();
                                    }
                                    hasScore = false;

                                    gameManager.mightySwipeNotes.Add(noteComponent);

                                    if (gameManager.mightySwipeNotes.Count == 2)
                                    {
                                        hasScore = true;
                                        PlayScoreTextVfx(trigger);

                                        for (int v = 0; v < gameManager.mightySwipeNotes.Count; v++)
                                        {
                                            ActivatorsManager.Instance.activators3D[gameManager.mightySwipeNotes[v].indexCSV].PlayTapVfx(gameManager.mightySwipeNotes[v], trigger, false);
                                        }
                                        needToRemove = true;
                                    }
                                }
                            }
                            else
                            {
                                gameManager.mightySwipeNotes.Add(noteComponent);
                                hasScore = true;
                                PlayScoreTextVfx(trigger);
                                ActivatorsManager.Instance.activators3D[noteComponent.indexCSV].PlayTapVfx(noteComponent, trigger, false);
                                needToRemove = true;
                            }
                        }
                        else
                        {
                            // if(gameInput.IsStartSwipeOnMe() || IsAutoCollect())
                            {
                                hasScore = true;
                                PlayScoreTextVfx(trigger);
                                PlayTapVfx(noteComponent, trigger, false);
                                needToRemove = true;
                            }
                        }
                    }

                    if (hasScore)
                    {
                        score = ScoreManager.Instance.GetScoreNote(noteComponent.type, trigger);
                    }
                }
            }
            else
            {
                if ((inputStatus & Define.INPUT_STATUS.PRESSED) > 0
                    || IsAutoCollect()
                )
                {
                    hasScore = true;
                    PlayTapVfx(noteComponent, trigger, false);
                    PlayScoreTextVfx(trigger);
                    score = ScoreManager.Instance.GetScoreNote(noteComponent.type, trigger);

                    // Debug.Log("Activator => destroy tap note " + Convert.FloatToTime(noteComponent.startTime));
                    needToRemove = true;
                }
            }

        }

        if (hasScore)
        {
            ScoreManager.Instance.PlayScoreEffect(trigger, transform, score);
            ScoreManager.Instance.AddScore(score);
            // ScoreManager.Instance.CounterStreak();
            ScoreManager.Instance.ResetStreakMiss();
            ScoreManager.Instance.AddRockPointByType(noteComponent.type, trigger);
            if (noteComponent.type != Define.NOTE_TYPE.SWIPE)
            {
                BeatManager.Instance.EnableNote(noteComponent.indexAudio);
            }
           
        }
        return needToRemove;
    }

    private void PlayScoreTextVfx(Define.TRIGGER _trigger)
    {
        switch (_trigger)
        {
            case Define.TRIGGER.GREAT:
                GameManager.Instance.viewInGame.ShowTextGreat();
                ScoreManager.Instance.ResetStreakPerfect();
                break;
            case Define.TRIGGER.PERFECT:
                GameManager.Instance.viewInGame.ShowTextPerfect();
                ScoreManager.Instance.CounterStreakPerfect();
                break;
            default:
                ScoreManager.Instance.ResetStreakPerfect();
                break;
        }
    }

    public void PlayBarSwipeVfx(Note note, Define.DRAG_DIRECTION prev_dir)
    {
        //play vfx when changing direction (left/right) on drag note
        //_ps.Clear();
        _ps = null;
        int colorIndex = (int)note.GetOriginalColor() - 1;
        _ps = VFXBarSwipe[colorIndex];

        if (_ps != null)
        {
            float direction = (note.indexCSV < GameManager.Instance.linesNumber / 2) ? 1f : -1f;
            _ps.transform.localScale = new Vector3(direction, 1f, 1f);
            float rotate_angle = (prev_dir == Define.DRAG_DIRECTION.DRAG_LEFT) ? 0f : 180f;
            _ps.transform.localRotation = Quaternion.Euler(rotate_angle, rotate_angle, 0f);

            // if(prev_dir == Define.DRAG_DIRECTION.DRAG_RIGHT)
            // {
            //     _ps.transform.localRotation = Quaternion.Euler(90, 0, 180);
            // }
            // else
            // {
            //     _ps.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            // }

            // Vector3 pos = _ps.transform.localPosition;
            // _ps.transform.localPosition = new Vector3(pos.x, pos.y, note.transform.localPosition.z);

            _ps.Play();
        }
    }

    public void PlayNormalTapVFX(Note note, Define.TRIGGER _trigger)
    {
        //_ps.Clear();
        _ps = null;
        int colorIndex = (int)note.GetOriginalColor() - 1;
        _ps = (_trigger == Define.TRIGGER.PERFECT) ? VFXTapPerfect[colorIndex] : VFXTapNormal[colorIndex];

        if (_ps != null)
        {
            _ps.transform.localScale = Vector3.one;

            // Vector3 pos = _ps.transform.localPosition;
            // _ps.transform.localPosition = new Vector3(pos.x, pos.y, note.transform.localPosition.z);
            
            _ps.Play();
        }
    }

    public void PlayTapVfx(Note note, Define.TRIGGER _trigger, bool loop)
    {
        //_ps.Clear();
        _ps = null;
        int colorIndex = (int)note.GetOriginalColor() - 1;
        if (loop)
        {
            _ps = (_trigger == Define.TRIGGER.PERFECT) ? VFXTapPerfectLoop[colorIndex] : VFXTapLoop[colorIndex];
            curLoopVFX = _ps;
            _ps.transform.localScale = Vector3.one;
        }
        else
        {
            if (note.type == Define.NOTE_TYPE.SWIPE)
            {
                if (note.comboNotesKey == -1)
                {
                    _ps = (_trigger == Define.TRIGGER.PERFECT) ? VFXSwipeLeft[1] : VFXSwipeLeft[0];
                    float direction = (note.indexCSV < GameManager.Instance.linesNumber / 2) ? 1f : -1f;
                    _ps.transform.localScale = new Vector3(direction, 1f, 1f);
                    float rotate_angle = (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT) ? 0f : 180f;
                    _ps.transform.localRotation = Quaternion.Euler(rotate_angle, rotate_angle, 0f);

                }
                
            }
            else if (note.type == Define.NOTE_TYPE.MIGHTY_SWIPE)
            {
                _ps = VFXShiftLeft;//(_trigger == Define.TRIGGER.PERFECT) ? VFXShiftLeft[1] : VFXShiftLeft[0];
                float direction = (note.indexCSV < GameManager.Instance.linesNumber / 2) ? 1f : -1f;
                _ps.transform.localScale = new Vector3(direction, 1f, 1f);
                float rotate_angle = (note.SwipeRequire == Define.INPUT_STATUS.SWIPE_LEFT) ? 0f : 180f;
                _ps.transform.localRotation = Quaternion.Euler(rotate_angle, rotate_angle, 0f);

                // _ps = VFXShiftLeft;//(_trigger == Define.TRIGGER.PERFECT) ? VFXShiftLeft[1] : VFXShiftLeft[0];
                // if(note.SwipeRequire == Define.INPUT_STATUS.SWIPE_RIGHT)
                // {
                //     _ps.transform.localRotation = Quaternion.Euler(90, 0, 180);
                // }
                // else
                // {
                //     _ps.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                // }
                SFXManager.Instance.Play(Define.SFX.SWIPE_ROW_CHANGE);
            }
            else
            {
                _ps = (_trigger == Define.TRIGGER.PERFECT) ? VFXTapPerfect[colorIndex] : VFXTapNormal[colorIndex];
                _ps.transform.localScale = Vector3.one;
            }
        }
        if (_ps != null)
        {
            // Vector3 pos = _ps.transform.localPosition;
            // _ps.transform.localPosition = new Vector3(pos.x, pos.y, note.transform.localPosition.z);
            _ps.Play();
        }
    }

    public void StopVfx(Note note)
    {
        if (curLoopVFX != null)
        {

            curLoopVFX.Stop();
            curLoopVFX = null;
        }
    }

    public void PlayTapAnimation()
    {
        if (anim != null)
        {
            anim.Play("Tap", 0, 0f);
        }
    }

    public void PlayBadAnimation()
    {
        if (VFXBadTap != null)
        {
            VFXBadTap.Play();
        }
    }

    public void OnActivatorEnter(NoteCollider other)
    {
        // if (other.Physic == Define.PHYSICS.COOL)
        // {
        //     //Debug.LogFormat("Activator::OnActivatorEnter()");
        //     OnEnter(other);
        // }
    }

    public void OnActivatorExit(NoteCollider other)
    {
        // if (other.Physic == Define.PHYSICS.NONE)
        // {
        //     OnExit(other);
        // }
    }

    void SetState(int next)
    {
        state = next;
    }

    void OnDisable()
    {
        StopVfx(null);
    }

    private bool IsAutoCollect()
    {
        // return GameManager.Instance.IsAutoCollect() || TutorialManager.Instance.IsAutoCollect() || Define.ENABLE_AUTOPLAY;
        return TutorialManager.Instance.IsAutoCollect() || Define.ENABLE_AUTOPLAY;
    }

    public float GetHideTransparent()
    {
        return hideTransparent;
    }

    public void PreloadParticles()
    {
        foreach(ParticleSystem ps in VFXTapNormal)
        {
            ps.Play();
        }
        foreach(ParticleSystem ps in VFXTapPerfect)
        {
            ps.Play();
        }
        foreach(ParticleSystem ps in VFXTapLoop)
        {
            ps.Play();
        }
        foreach(ParticleSystem ps in VFXTapPerfectLoop)
        {
            ps.Play();
        }
        foreach(ParticleSystem ps in VFXSwipeLeft)
        {
            ps.Play();
        }
        foreach(ParticleSystem ps in VFXBarSwipe)
        {
            ps.Play();
        }
        VFXShiftLeft.Play();
        VFXBadTap.Play();
    }

    public void StopAllParticles()
    {
        foreach(ParticleSystem ps in VFXTapNormal)
        {
            ps.Stop();
        }
        foreach(ParticleSystem ps in VFXTapPerfect)
        {
            ps.Stop();
        }
        foreach(ParticleSystem ps in VFXTapLoop)
        {
            ps.Stop();
        }
        foreach(ParticleSystem ps in VFXTapPerfectLoop)
        {
            ps.Stop();
        }
        foreach(ParticleSystem ps in VFXSwipeLeft)
        {
            ps.Stop();
        }
        foreach(ParticleSystem ps in VFXBarSwipe)
        {
            ps.Stop();
        }
        VFXShiftLeft.Stop();
        VFXBadTap.Stop();
    }

    public void HapticNote(Note note)
    {
        if(ProfileMgr.Instance.Vibration
            && (TutorialManager.Instance.Tutorial < Define.TUTORIAL.DONE)
            &&(
                note.type == Define.NOTE_TYPE.SHORT
                || note.type == Define.NOTE_TYPE.SWIPE
                || note.type == Define.NOTE_TYPE.MIGHTY_SWIPE
                || (note.type == Define.NOTE_TYPE.LONG && note.isDragHeadNote)
                || (note.type == Define.NOTE_TYPE.DRAG && note.isDragHeadNote)
            )
        )
        {
            #if UNITY_ANDROID
                Vibration.Vibrate(Define.VIBRATION_STRENGTH);
            #elif UNITY_IOS
                Vibration.VibratePop();
            #endif
        }
    }
}
