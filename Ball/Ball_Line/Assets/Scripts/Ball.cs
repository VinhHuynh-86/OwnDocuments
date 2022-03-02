using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorsBall
    {
        blue = 1,
        boxdeaux_red = 2,
        cyan = 3,
        ghost = 4,
        green = 5,
        pink = 6,
        red = 7,
        yellow = 8,
    }

    public enum StatusBall
    {
        NO_QUEUE = 1,
        IN_QUEUE = 2,
        ON_BOARD = 3
    }

    public enum TypesBall
    {
        NORMAL = 1,
        GHOST = 2
    }

    public enum AnimBallType
    {
        GROW_UP1 = 1,
        GROW_UP2 = 2,
        SELECTED = 3,
        IDLE = 4
    }

public class Ball : MonoBehaviour
{
    private Animator anim;

    private Vector2 m_pos;
    private int m_row;
    private int m_col;
    private ColorsBall m_color;
    private StatusBall m_status;
    private TypesBall m_type;

    void Awake()
    {
        //Get reference to the Animator component attached to this GameObject.
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetProperties(int row, int col, Vector2 pos, ColorsBall color, StatusBall status)
    {
        m_row = row;
        m_col = col;
        m_pos = pos;
        m_color = color;
        m_status = status;
        if(m_color == ColorsBall.ghost)
        {
            m_type = TypesBall.GHOST;
        }
        else
        {
            m_type = TypesBall.NORMAL;
        }
    }
    public void Move(Vector2 pos)
    {
        m_pos = pos;
    }
    public void SetStatus(StatusBall status)
    {
        m_status = status;
    }

    public bool IsType(TypesBall types)
    {
        return types == m_type;
    }

    public int GetRowIndex()
    {
        return m_row;
    }

    public int GetColumnIndex()
    {
        return m_col;
    }

    public ColorsBall GetColor()
    {
        return m_color;
    }
    public void RelocateTo(Vector2Int pos)
    {
        m_row = pos.x;
        m_col = pos.y;
    }
    public void ActiveTrigger(AnimBallType animtype)
    {
        switch(animtype)
        {
            case AnimBallType.GROW_UP2:
                //...tell the animator about it and then...
                anim.SetTrigger("GrowUp1ToGrowUp2");
            break;

            case AnimBallType.SELECTED:
                //...tell the animator about it and then...
                anim.SetTrigger("GrowUp2ToSelected");
            break;

            case AnimBallType.IDLE:
                //...tell the animator about it and then...
                anim.SetTrigger("SelectedToIdle");
            break;

        }
    }
}
