using UnityEngine;

public class KeyInput : MonoBehaviour
{
    [Header("Tweaks")]
    public KeyCode key;
    public float doubleClickedDelta = 0.1f;

    [Header("Logic")]
    private bool clicked, doubleClicked;
    private bool isPressing;
    private float lastClicked;

    #region Properties

    public bool Clicked
    {
        get
        {
            return clicked;
        }
    }

    public bool DoubleClicked
    {
        get
        {
            return doubleClicked;
        }
    }

    public bool Pressing
    {
        get
        {
            return isPressing;
        }
    }

    #endregion

    private void Start()
    {
        isPressing = false;
        lastClicked = Time.time;
    }

    private void Update()
    {
        // reset our bools
        clicked = doubleClicked = false;

        if (Input.GetKeyDown(key))
        {
            Init();
        }
        else if (Input.GetKey(key))
        {
            if (Time.time - lastClicked > doubleClickedDelta)
            {
                isPressing = true;
            }
        }
        else if (Input.GetKeyUp(key))
        {
            Reset();
        }
    }

    private void Reset()
    {
        isPressing = false;
        clicked = false;
    }

    private void Init()
    {
        clicked = true;
        isPressing = false;
        doubleClicked = Time.time - lastClicked < doubleClickedDelta;
        lastClicked = Time.time;
    }
}