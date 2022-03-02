using UnityEngine;
public class GameInput : MonoBehaviour
{    
    private TouchInput touchInput;
    private KeyInput keyInput;


    [Header("Config")]
    public Camera raycastCamera;
    public GameObject[] target;
    public KeyCode key;
    public LayerMask layerMask;
    float rayCastMaxDistance = 20;

    private void Awake()
    {
        if (target == null)
        {
            throw new System.MissingFieldException("can not find input target");
        }

        if (raycastCamera == null)
        {
            raycastCamera = GameManager.Instance.gameCamera;
        }

        touchInput = TouchInput.Instance;
        // each activator has own key
        keyInput = new GameObject("Spawned KeyInput", typeof(KeyInput)).GetComponent<KeyInput>();
        keyInput.transform.SetParent(transform);
        keyInput.key = key;
    }

    private void Start()
    {
        // touchInput = TouchInput.Instance;

        // // each activator has own key
        // keyInput = new GameObject("Spawned KeyInput", typeof(KeyInput)).GetComponent<KeyInput>();
        // keyInput.transform.SetParent(transform);
        // keyInput.key = key;
        // Debug.Log("BBBBBBBBBBBBBBBBBBBBB:" + keyInput);
    }

    #region Tap

    public bool IsStartOnMe()
    {
        if (touchInput.HasAnyTap())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.tap)
                {
                    // StartTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.startTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("hit " + hit.transform.gameObject);
                        if (hit.transform.gameObject == target[0]) // on specific lane
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool IsTap()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        if(touchInput == null)
        {
            touchInput = TouchInput.Instance;
        }
        if (touchInput.HasAnyTap())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.tap)
                {
                    // StartTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.startTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("hit " + hit.transform.gameObject.name);
                        for(int v = 0; v < target.Length; ++v)
                        {
                            if (hit.transform.gameObject == target[v])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool IsTapOnLane()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        if(touchInput == null)
        {
            touchInput = TouchInput.Instance;
        }
        if (touchInput.HasAnyTap())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.tap)
                {
                    // StartTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.startTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("hit " + hit.transform.gameObject.name);
                        // for(int v = 0; v < target.Length; ++v)
                        {
                            if (hit.transform.gameObject == target[0])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool IsTapOnLeftBorderLane()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        if(touchInput == null)
        {
            touchInput = TouchInput.Instance;
        }
        if (touchInput.HasAnyTap())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.tap)
                {
                    // StartTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.startTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("hit " + hit.transform.gameObject.name);
                        // for(int v = 1; v < target.Length; ++v)
                        {
                            if (hit.transform.gameObject == target[1])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    public bool IsTapOnRightBorderLane()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        if(touchInput == null)
        {
            touchInput = TouchInput.Instance;
        }
        if (touchInput.HasAnyTap())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.tap)
                {
                    // StartTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.startTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("hit " + hit.transform.gameObject.name);
                        // for(int v = 1; v < target.Length; ++v)
                        {
                            if (hit.transform.gameObject == target[2])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region Swipe
    public bool IsSwipeLeft()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeLeft)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsSwipeRight()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeRight)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSwipeRightOnRightSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeRight && info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsSwipeRightOnLeftSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeRight && !info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSwipeLeftOnRightSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeLeft && info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsSwipeLeftOnLeftSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeLeft && !info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSwipeUpOnRightSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeUp && info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsSwipeUpOnLeftSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeUp && !info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSwipeDownOnRightSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeDown && info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsSwipeDownOnLeftSide()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeDown && !info.isRightTouch)
            {
                return true;
            }
        }

        return false;
    }

    // public bool IsRightTouch()
    // {
    //     foreach (TouchInfo info in touchInput.Touches)
    //     {
    //         if (info.isRightTouch)
    //         {
    //             return true;
    //         }
    //     }

    //     return false;
    // }

    public bool IsSwipeUp()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeUp)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsSwipeDown()
    {
        if (keyInput.Clicked)
        {
            return true;
        }

        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.swipeDown)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsStartSwipeOnMe()
    {
        if (keyInput.Clicked)
        {
            return true;
        }
        if (touchInput.HasAnyDragging())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.isDragging)
                {
                    // StartTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.startTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("hit " + hit.transform.gameObject);
                        if (hit.transform.gameObject == target[0]) // on specific lane
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Holding
    public bool IsHolding()
    {
        if (keyInput.Pressing)
        {
            return true;
        }
        if (touchInput.HasAnyHolding())
        {
            foreach (TouchInfo info in touchInput.Touches)
            {
                if (info.isHolding)
                {
                    // CurrentTouch
                    Ray ray = raycastCamera.ScreenPointToRay(info.currentTouch);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                    {
                        // Debug.Log("holding " + hit.transform.gameObject);
                        for(int v = 0; v < target.Length; ++v)
                        {
                            if (hit.transform.gameObject == target[v])
                            {
                                // Debug.Log("IsHolding : " + info.currentTouch);

                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public Vector3 GetCurrentTouch()
    {
        foreach (TouchInfo info in touchInput.Touches)
        {
            if (info.isHolding)
            {
                // CurrentTouch
                Ray ray = raycastCamera.ScreenPointToRay(info.currentTouch);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, rayCastMaxDistance, layerMask))
                {
                    // Debug.Log("holding " + hit.transform.gameObject);
                    for(int v = 0; v < target.Length; ++v)
                    {
                        if (hit.transform.gameObject == target[v])
                        {
                            return hit.point;
                        }
                    }
                }
            }
        }

        return Vector3.zero; //Vector3.positiveInfinity;
    }

    #endregion
}