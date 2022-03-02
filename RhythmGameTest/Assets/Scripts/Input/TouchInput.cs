using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchInfo
{
    public int id;
    public bool tap;
    public bool doubleTap;
    public Vector2 startTouch;
    public Vector2 currentTouch;
    public bool isDragging;
    public float lastTap;
    public bool isHolding;
    public bool swipeLeft, swipeRight, swipeUp, swipeDown;
    public Vector2 swipeDelta;
    public bool isRightTouch;
}

public class TouchInput : MonoBehaviour
{
    public static TouchInput Instance = null;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    [Header("Tweaks")]
    public float doubleTapDelta = 0.5f;
    public float holdDelta = 0.05f;
    public float deadzone = 10f;

    [Header("Logic")]
    private List<TouchInfo> touches = new List<TouchInfo>();
    private float sqrDeadZone;

    #region Properties

    public List<TouchInfo> Touches
    {
        get
        {
            return touches;
        }
    }

    public bool HasAnyTap()
    {
        int index = touches.FindIndex(delegate (TouchInfo info)
        {
            return info.tap;
        });
        return index != -1;
    }

    public bool HasAnyDoubleTap()
    {
        int index = touches.FindIndex(delegate (TouchInfo info)
        {
            return info.doubleTap;
        });
        return index != -1;
    }

    public bool HasAnyDragging()
    {
        int index = touches.FindIndex(delegate (TouchInfo info)
        {
            return info.isDragging;
        });
        return index != -1;
    }

    public bool HasAnyHolding()
    {
        int index = touches.FindIndex(delegate (TouchInfo info)
        {
            return info.isHolding;
        });
        return index != -1;
    }

    public bool HasAnySwipe()
    {
        int index = touches.FindIndex(delegate (TouchInfo info)
        {
            return info.swipeLeft || info.swipeRight || info.swipeUp || info.swipeDown;
        });
        return index != -1;
    }

    // public bool IsReleased()
    // {
    //     if (thisTouch != null)
    //     {
    //         thisTouch.startTouch = thisTouch.currentTouch = Vector2.zero;
    //         thisTouch.isDragging = false;
    //         thisTouch.isHolding = false;
    //         // don't reset tap / double tap / swipe detection here
    //     }
    //     else
    //     {
    //         return true;
    //     }
    // }

    #endregion

    private void Start()
    {
        sqrDeadZone = deadzone * deadzone;
    }

    private void Update()
    {
        // reset our bools
        foreach (TouchInfo info in touches)
        {
            info.tap = false;
            info.doubleTap = false;
            info.swipeLeft = info.swipeRight = info.swipeUp = info.swipeDown = false;
        };

#if UNITY_EDITOR || UNITY_STANDALONE
        StandaloneUpdate();
#else
        MobileUpdate();
#endif

        // support multi swipes
        SwipeDetection();
    }

    private void StandaloneUpdate()
    {
        const int MouseFingerID = -999;

        if (Input.GetMouseButtonDown(0))
        {
            Init(MouseFingerID, (Vector2)Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset(MouseFingerID);
        }
        else if (Input.GetMouseButton(0))
        {
            Move(MouseFingerID, (Vector2)Input.mousePosition);
        }
    }

    private void MobileUpdate()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    Init(touch.fingerId, touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    Reset(touch.fingerId);
                }
                else
                {
                    Move(touch.fingerId, touch.position);
                }
            }
        }
    }

    private void Move(int id, Vector2 position)
    {
        TouchInfo thisInfo = touches.Find(delegate (TouchInfo info)
                            {
                                return info.id == id;
                            });

        if (thisInfo != null)
        {
            // Debug.Log("Touch move : " + position);
            thisInfo.isDragging = Vector3.Distance(position, thisInfo.currentTouch) > 0;
            thisInfo.currentTouch = position;
            thisInfo.isHolding = true; //Time.time - thisInfo.lastTap > holdDelta;
        }
    }

    private void Reset(int id)
    {
        TouchInfo thisTouch = touches.Find(delegate (TouchInfo info)
                    {
                        return info.id == id;
                    });

        if (thisTouch != null)
        {
            thisTouch.startTouch = thisTouch.currentTouch = Vector2.zero;
            thisTouch.isDragging = false;
            thisTouch.isHolding = false;
            // don't reset tap / double tap / swipe detection here
        }
    }

    private void Init(int id, Vector2 position)
    {
        TouchInfo thisTouch = touches.Find(delegate (TouchInfo info)
                    {
                        return info.id == id;
                    });

        bool isNew = false;

        if (thisTouch == null)
        {
            thisTouch = new TouchInfo();

            thisTouch.id = id;
            thisTouch.doubleTap = false;
            thisTouch.lastTap = Time.time;

            isNew = true;
        }
        else
        {
            thisTouch.doubleTap = Time.time - thisTouch.lastTap < doubleTapDelta;
            thisTouch.lastTap = Time.time;
        }

        thisTouch.tap = true;
        thisTouch.isDragging = false;
        thisTouch.isHolding = false;
        thisTouch.startTouch = thisTouch.currentTouch = position;
        thisTouch.swipeLeft = thisTouch.swipeRight = thisTouch.swipeUp = thisTouch.swipeDown = false;

        if (isNew)
        {
            touches.Add(thisTouch);
        }
    }

    private void SwipeDetection()
    {
        foreach (TouchInfo info in touches)
        {
            info.swipeDelta = Distance(info);
            if (info.swipeDelta.sqrMagnitude > sqrDeadZone)
            {
                Direction(info);
                // reset swiping instead of dragging ?!?
                // Reset(info.id);
            }
        }
    }

    private Vector2 Distance(TouchInfo info)
    {
        if (info.isDragging)
        {
            return info.currentTouch - info.startTouch;
        }

        return Vector2.zero;
    }

    private void Direction(TouchInfo info)
    {
        float x = info.swipeDelta.x;
        float y = info.swipeDelta.y;

        if((info.currentTouch.x + info.startTouch.x) / 2 > (Screen.width / 2))
        {
            info.isRightTouch = true;
        }
        else
        {
            info.isRightTouch = false;
        }

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            // left or right
            if (x > 0)
            {
                // Debug.Log("swipeRight");
                info.swipeRight = true;
            }
            else
            {
                // Debug.Log("swipeLeft");
                info.swipeLeft = true;
            }
        }
        else
        {
            // up or down
            if (y > 0)
            {
                // Debug.Log("swipeUp");
                info.swipeUp = true;
            }
            else
            {
                // Debug.Log("swipeDown");
                info.swipeDown = true;
            }
        }
    }


}