using UnityEngine;

// only support single swipe
public class SwipeInput : MonoBehaviour
{
    [Header("Tweaks")]
    public float deadzone = 5f;

    [Header("Logic")]
    private bool swipeLeft, swipeRight, swipeUp, swipeDown;
    private Vector2 startTouch, swipeDelta, currentTouch;
    private bool isDragging;
    private float sqrDeadZone;
    private bool isRightTouch;

    #region Properties
    public Vector2 SwipeDelta
    {
        get
        {
            return swipeDelta;
        }
    }

    public bool SwipeLeft
    {
        get
        {
            return swipeLeft;
        }
    }

    public bool SwipeRight
    {
        get
        {
            return swipeRight;
        }
    }

    public bool IsRightTouch
    {
        get
        {
            return isRightTouch;
        }
    }

    public bool SwipeUp
    {
        get
        {
            return swipeUp;
        }
    }

    public bool SwipeDown
    {
        get
        {
            return swipeDown;
        }
    }

    public bool Dragging
    {
        get
        {
            return isDragging;
        }
    }

    #endregion

    private void Start()
    {
        isDragging = false;
        sqrDeadZone = deadzone * deadzone;
    }

    private void Update()
    {
        // reset our bools
        swipeLeft = swipeRight = swipeUp = swipeDown = false;

#if UNITY_EDITOR
        StandaloneUpdate();
#else
        MobileUpdate();
#endif

        swipeDelta = Distance(currentTouch);

        if (swipeDelta.sqrMagnitude > sqrDeadZone)
        {
            // which direction?
            Direction(swipeDelta);

            Reset();
        }
    }

    private void StandaloneUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Init((Vector2)Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }
        else if (Input.GetMouseButton(0))
        {
            currentTouch = (Vector2)Input.mousePosition;
        }
    }

    private void MobileUpdate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                Init(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                Reset();
            }
            else
            {
                currentTouch = touch.position;
            }
        }
    }

    private Vector2 Distance(Vector2 currentPosition)
    {
        if (isDragging)
        {
            return currentPosition - startTouch;
        }

        return Vector2.zero;
    }

    private void Direction(Vector2 v)
    {
        float x = v.x;
        float y = v.y;

        if((currentTouch.x + startTouch.x) / 2 > (Screen.width / 2))
        {
            isRightTouch = true;
        }
        else
        {
            isRightTouch = false;
        }

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            // left or right
            if (x > 0)
            {
                // Debug.Log("swipeRight");
                swipeRight = true;
            }
            else
            {
                // Debug.Log("swipeLeft");
                swipeLeft = true;
            }
        }
        else
        {
            // up or down
            if (y > 0)
            {
                // Debug.Log("swipeUp");
                swipeUp = true;
            }
            else
            {
                // Debug.Log("swipeDown");
                swipeDown = true;
            }
        }
    }

    private void Reset()
    {
        startTouch = currentTouch = swipeDelta = Vector2.zero;
        isDragging = false;
    }

    private void Init(Vector2 position)
    {
        isDragging = true;
        startTouch = currentTouch = position;
    }
}