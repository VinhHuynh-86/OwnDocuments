using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager instance;

    public GameObject BlueBallPrefab;
    public GameObject BoxdeauxRedBallPrefab;
    public GameObject CyanBallPrefab;
    public GameObject GhostBallPrefab;
    public GameObject GreenBallPrefab;
    public GameObject PinkBallPrefab;
    public GameObject RedBallPrefab;
    public GameObject YellowBallPrefab;
    public GameObject ExplosionBall;

    public GameObject DisplayBallDummy;
    private Vector2 m_ballDisplayDummyPos;
    private float m_displayBallVerticalLength;

    private GameObject[,] m_ballsArray;
    private List<GameObject> m_ballInQueueList;
    private List<GameObject> m_nextDisplayBallList;
    private List<GameObject> m_earnedBallsList;
    private List<GameObject> m_explosionBallList;
    private Vector2[,] m_boardPos;
    private Vector2Int m_activeBallPoint;
    private Vector2Int m_targetBallPoint;

    private int INIT_BALL = 5;
    private int NEXT_BALL = 3;
    private int MIN_BALL_NUM_TO_EARN = 5;

    private int[,] m_binaryMatrix;
    private List<Vector2Int> m_pathList;
    private int m_movingDone = -2;
    private bool m_gameOver = false;
    private float m_timeToUpdateBallList = 0;
    private float m_timeToUpdateMovingBall = 0;
    private float m_timeToPlayExplosion = 0;

    public int m_score = 0;

    void Awake()
    {
        //If we don't currently have a game control...
        if (instance == null)
            //...set this one to be it...
            instance = this;
        //...otherwise...
        else if(instance != this)
            //...destroy this one because it is a duplicate.
            Destroy (gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
         m_ballDisplayDummyPos = DisplayBallDummy.GetComponent<BoxCollider2D>().transform.position;
         m_displayBallVerticalLength = DisplayBallDummy.GetComponent<BoxCollider2D>().size.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsFullBoard())
        {
            m_gameOver = false;
            if(m_movingDone == 1) //moving done
            {
                m_movingDone = -1;//No moving
                m_score += EarnBalls();
                UpdateBallList();
                UpdateBinaryMatrix();
                GenerateNextBalls();

                m_timeToUpdateBallList = 0;
                m_timeToPlayExplosion = 0;
            }

            if(m_movingDone == - 1)
            {
                m_timeToUpdateBallList += Time.deltaTime;
                if(m_timeToUpdateBallList > 1)
                {
                    m_timeToUpdateBallList = 0;
                    m_movingDone = -2;

                    m_score += EarnBalls();
                    m_timeToPlayExplosion = 0;
                }
            }

            UpdateMovingBall();

            //Destroy explosion ball
            if(m_timeToPlayExplosion > 1.0f)
            {
                for(int i = 0; i < m_explosionBallList.Count; ++i)
                {
                    Destroy(m_explosionBallList[i]);
                }
                m_explosionBallList.Clear();
                m_timeToPlayExplosion = 0;
            }
            m_timeToPlayExplosion += Time.deltaTime;
            
        }
        else if(!m_gameOver) //update last time when ball full on board
        {
            m_gameOver = true;
            m_score += EarnBalls();
            m_timeToPlayExplosion = 0;
            UpdateBallList();
            UpdateBinaryMatrix();
            GenerateNextBalls();
        }        
    }

    public void Initialize(Vector2[,] boardPos)
    {
        m_boardPos = boardPos;
        m_activeBallPoint = new Vector2Int(-1, -1);
        m_targetBallPoint = new Vector2Int(-1, -1);

        m_binaryMatrix = new int[Board.instance.GetRowNumber(), Board.instance.GetColumnNumber()];
        m_ballsArray = new GameObject[Board.instance.GetRowNumber(), Board.instance.GetColumnNumber()];
        m_ballInQueueList = new List<GameObject>();
        m_earnedBallsList = new List<GameObject>();
        m_explosionBallList = new List<GameObject>();
        m_nextDisplayBallList = new List<GameObject>();
        m_pathList = new List<Vector2Int>();

        
        InitBalls();
        GenerateNextBalls();
        UpdateBinaryMatrix();

    }

    public void ReNewAll()
    {
        Reset();
        InitBalls();
        GenerateNextBalls();
        UpdateBinaryMatrix();
    }

    private void Reset()
    {
        m_activeBallPoint.Set(-1, -1);
        m_targetBallPoint.Set(-1, -1);
        m_score = 0;

        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                if(m_ballsArray[i,j] != null)
                {
                    Destroy(m_ballsArray[i,j]);
                    m_ballsArray[i,j] = null;
                }
            }
        }
        for(int i = 0; i < m_ballInQueueList.Count; ++i)
        {
            if(m_ballInQueueList[i] != null)
            {
                Destroy(m_ballInQueueList[i]);
                m_ballInQueueList.RemoveAt(i);

                Destroy(m_nextDisplayBallList[i]);
                m_nextDisplayBallList.RemoveAt(i);
            }
        }
        //m_ballInQueueList.Clear();


        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                m_binaryMatrix[i,j] = 1;
            }
        }
        m_earnedBallsList.Clear();
    }
    
    private void InitBalls()
    {
        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                m_ballsArray[i,j] = null;
            }
        }

        for(int i = 0; i < INIT_BALL;)
        {
            int row = Random.Range(0, Board.instance.GetRowNumber());
            int col = Random.Range(0, Board.instance.GetColumnNumber());

            if(m_ballsArray[row, col] == null)
            {
                ColorsBall color = (ColorsBall)Random.Range((int)ColorsBall.blue, (int)ColorsBall.yellow);;
                m_ballsArray[row, col] = NewBall(row, col, m_boardPos[row, col], color, StatusBall.ON_BOARD);
                m_ballsArray[row, col].GetComponent<Ball>().SetProperties(row, col, m_boardPos[row, col], color, StatusBall.ON_BOARD);
                m_ballsArray[row, col].GetComponent<Ball>().ActiveTrigger(AnimBallType.GROW_UP2);
                
                ++i;
            }
        }
    }

    private void GenerateNextBalls()
    {
        int emptySquare = GetEmptySquare();
        for(int i = 0; i < NEXT_BALL && i < emptySquare;)
        {
            int row = Random.Range(0, Board.instance.GetRowNumber());
            int col = Random.Range(0, Board.instance.GetColumnNumber());

            bool alreadyExisted = false;
            if(m_ballsArray[row, col] != null)
            {
                alreadyExisted = true;
            }

            for(int j = 0; j< m_ballInQueueList.Count; ++j)
            {
                if(row == m_ballInQueueList[j].GetComponent<Ball>().GetRowIndex() && col == m_ballInQueueList[j].GetComponent<Ball>().GetColumnIndex())
                {
                    alreadyExisted = true;
                }
            }

            if(!alreadyExisted)
            {
                ColorsBall color = (ColorsBall)Random.Range((int)ColorsBall.blue, (int)ColorsBall.yellow);;
                m_ballInQueueList.Add(NewBall(row, col, m_boardPos[row, col], color, StatusBall.IN_QUEUE));
                m_ballInQueueList[m_ballInQueueList.Count - 1].GetComponent<Ball>().SetProperties(row, col, m_boardPos[row, col], color, StatusBall.IN_QUEUE);
                
                m_nextDisplayBallList.Add(NewBall(row, col, m_ballDisplayDummyPos - new Vector2(0, i * m_displayBallVerticalLength), color, StatusBall.NO_QUEUE));
                m_nextDisplayBallList[m_nextDisplayBallList.Count - 1].GetComponent<Ball>().SetProperties(row, col, m_ballDisplayDummyPos - new Vector2(0, i * m_displayBallVerticalLength), color, StatusBall.NO_QUEUE);
                m_nextDisplayBallList[m_nextDisplayBallList.Count - 1].GetComponent<Ball>().ActiveTrigger(AnimBallType.GROW_UP2);

                ++i;
            }
        }
    }

    public GameObject NewBall(int row, int col, Vector2 pos, ColorsBall color, StatusBall status)
    {
        switch(color)
        {
            case ColorsBall.blue:
            return (Instantiate(BlueBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;
            
            case ColorsBall.boxdeaux_red:
            return (Instantiate(BoxdeauxRedBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;
            
            case ColorsBall.cyan:
            return (Instantiate(CyanBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;

            case ColorsBall.ghost:
            return (Instantiate(GhostBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;

            case ColorsBall.green:
            return (Instantiate(GreenBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;

            case ColorsBall.pink:
            return (Instantiate(PinkBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;

            case ColorsBall.red:
            return (Instantiate(RedBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;

            case ColorsBall.yellow:
            return (Instantiate(YellowBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            break;
        }
        return null;
    }
    // public void NewBall(int row, int col, Vector2 pos, ColorsBall color, StatusBall status)
    // {
    //     switch(color)
    //     {
    //         case ColorsBall.blue:
    //         m_ballsArray[row, col] = (Instantiate(BlueBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;
            
    //         case ColorsBall.boxdeaux_red:
    //         m_ballsArray[row, col] = (Instantiate(BoxdeauxRedBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;
            
    //         case ColorsBall.cyan:
    //         m_ballsArray[row, col] = (Instantiate(CyanBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.ghost:
    //         m_ballsArray[row, col] = (Instantiate(GhostBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.green:
    //         m_ballsArray[row, col] = (Instantiate(GreenBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.pink:
    //         m_ballsArray[row, col] = (Instantiate(PinkBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.red:
    //         m_ballsArray[row, col] = (Instantiate(RedBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.yellow:
    //         m_ballsArray[row, col] = (Instantiate(YellowBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;
    //     }
    //     m_ballsArray[row, col].GetComponent<Ball>().SetProperties(row, col, pos, color, StatusBall.ON_BOARD);
    //     m_ballsArray[row, col].GetComponent<Ball>().ActiveTrigger(AnimBallType.GROW_UP2);
    // }

    // public void NewBallInQueue(int row, int col, Vector2 pos, ColorsBall color, StatusBall status)
    // {
    //     switch(color)
    //     {
    //         case ColorsBall.blue:
    //         m_ballInQueueList.Add(Instantiate(BlueBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;
            
    //         case ColorsBall.boxdeaux_red:
    //         m_ballInQueueList.Add(Instantiate(BoxdeauxRedBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;
            
    //         case ColorsBall.cyan:
    //         m_ballInQueueList.Add(Instantiate(CyanBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.ghost:
    //         m_ballInQueueList.Add(Instantiate(GhostBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.green:
    //         m_ballInQueueList.Add(Instantiate(GreenBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.pink:
    //         m_ballInQueueList.Add(Instantiate(PinkBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.red:
    //         m_ballInQueueList.Add(Instantiate(RedBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;

    //         case ColorsBall.yellow:
    //         m_ballInQueueList.Add(Instantiate(YellowBallPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
    //         break;
    //     }
    //     m_ballInQueueList[m_ballInQueueList.Count - 1].GetComponent<Ball>().SetProperties(row, col, pos, color, StatusBall.IN_QUEUE);
    // }

    public bool CheckTouchOnBall(Vector3 mousePressedPos)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPoint2d = new Vector2(worldPoint.x, worldPoint.y);

        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                if(m_boardPos[i,j].x - Board.instance.GetSquareHorizontalLength()/2 < worldPoint2d.x && m_boardPos[i,j].x + Board.instance.GetSquareHorizontalLength()/2 > worldPoint2d.x
                && m_boardPos[i,j].y - Board.instance.GetSquareVerticalLength()/2 < worldPoint2d.y && m_boardPos[i,j].y + Board.instance.GetSquareVerticalLength()/2 > worldPoint2d.y
                )
                {
                    if(m_activeBallPoint.Equals(new Vector2Int(-1, -1)))
                    {
                        //int index = GetBallIndexBy(i, j);
                        //if(index != -1) // touch on ball
                        if(m_ballsArray[i,j] != null)
                        {
                            m_ballsArray[i,j].GetComponent<Ball>().ActiveTrigger(AnimBallType.SELECTED);
                            m_activeBallPoint.Set(i,j);
                        }
                    }
                    else
                    {
                        //int index = GetBallIndexBy(i, j);
                        //if(index != -1) // touch on ball
                        if(m_ballsArray[i,j] != null)
                        {
                            if(m_activeBallPoint.Equals(new Vector2Int(i, j))) // touch on active ball
                            {
                                m_ballsArray[i,j].GetComponent<Ball>().ActiveTrigger(AnimBallType.IDLE);
                                m_activeBallPoint.Set(-1,-1);
                            }
                            else // touch on another ball
                            {
                                m_ballsArray[m_activeBallPoint.x,m_activeBallPoint.y].GetComponent<Ball>().ActiveTrigger(AnimBallType.IDLE);
                                m_ballsArray[i,j].GetComponent<Ball>().ActiveTrigger(AnimBallType.SELECTED);
                                m_activeBallPoint.Set(i, j);
                            }
                        }
                        else // touch on empty square
                        {
                            m_targetBallPoint.Set(i, j);

                            if(ShortestPathBinary(m_binaryMatrix, m_activeBallPoint, m_ballsArray[m_activeBallPoint.x,m_activeBallPoint.y].GetComponent<Ball>().IsType(TypesBall.GHOST), m_targetBallPoint) == -1)
                            {
                                // No Path -> deselect ball
                                m_ballsArray[m_activeBallPoint.x,m_activeBallPoint.y].GetComponent<Ball>().ActiveTrigger(AnimBallType.IDLE);
                                m_activeBallPoint.Set(-1,-1);
                            }
                        }
                    }
                    return true;
                }
            }
        }

        return false;
    }

    private void CopyList(List<GameObject> list, List<GameObject> toList)
    {
        for(int i = 0; i < list.Count; ++i)
        {
            if(toList.Find(x => x == list[i]) == null)
            {
                toList.Add(list[i]);
            }
        }
    }
    private int EarnBalls()
    {
        //m_earnedBallsList
        List<GameObject> earnedBalllistInCase = new List<GameObject>();
        int earnBallsTotal = 0;
        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                if(m_ballsArray[i,j] != null)
                {
                    ColorsBall color = m_ballsArray[i,j].GetComponent<Ball>().GetColor();

                    //check horizontal
                    earnedBalllistInCase.Clear();
                    earnedBalllistInCase.Add(m_ballsArray[i,j]);
                    int earnBall = 1; //one in focus
                    for(int u = j+1; u < Board.instance.GetColumnNumber(); ++u)
                    {
                        if(m_ballsArray[i,u] != null && color == m_ballsArray[i,u].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[i,u]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    for(int u = j - 1; u > -1; --u)
                    {
                        if(m_ballsArray[i,u] != null && color == m_ballsArray[i,u].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[i,u]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(earnBall >= MIN_BALL_NUM_TO_EARN)
                    {
                        int listSize = m_earnedBallsList.Count;
                        CopyList(earnedBalllistInCase, m_earnedBallsList);
                        if(listSize != m_earnedBallsList.Count)
                        {
                            earnBallsTotal += earnBall;
                        }
                    }

                    //check vertical
                    earnedBalllistInCase.Clear();
                    earnedBalllistInCase.Add(m_ballsArray[i,j]);
                    earnBall = 1; //one in focus
                    for(int u = i+1; u < Board.instance.GetRowNumber(); ++u)
                    {
                        if(m_ballsArray[u,j] != null && color == m_ballsArray[u,j].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[u,j]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    for(int u = i - 1; u > -1; --u)
                    {
                        if(m_ballsArray[u,j] != null && color == m_ballsArray[u,j].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[u,j]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(earnBall >= MIN_BALL_NUM_TO_EARN)
                    {
                        int listSize = m_earnedBallsList.Count;
                        CopyList(earnedBalllistInCase, m_earnedBallsList);
                        if(listSize != m_earnedBallsList.Count)
                        {
                            earnBallsTotal += earnBall;
                        }
                    }

                    //check diagonal 1
                    earnedBalllistInCase.Clear();
                    earnedBalllistInCase.Add(m_ballsArray[i,j]);
                    earnBall = 1; //one in focus
                    int r = i, c = j;
                    while( r > 0 && c > 0 )
                    {
                        r--;
                        c--;
                        if(m_ballsArray[r,c] != null && color == m_ballsArray[r,c].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[r,c]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    r = i; c = j;
                    while( r < Board.instance.GetRowNumber() - 1 && c < Board.instance.GetColumnNumber() - 1)
                    {
                        r++;
                        c++;
                        if(m_ballsArray[r,c] != null && color == m_ballsArray[r,c].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[r,c]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(earnBall >= MIN_BALL_NUM_TO_EARN)
                    {
                        int listSize = m_earnedBallsList.Count;
                        CopyList(earnedBalllistInCase, m_earnedBallsList);
                        if(listSize != m_earnedBallsList.Count)
                        {
                            earnBallsTotal += earnBall;
                        }
                    }
                    
                    //check diagonal 2
                    earnedBalllistInCase.Clear();
                    earnedBalllistInCase.Add(m_ballsArray[i,j]);
                    earnBall = 1; //one in focus
                    r = i; c = j;
                    while( r > 0 && c < Board.instance.GetColumnNumber() - 1 )
                    {
                        r--;
                        c++;
                        if(m_ballsArray[r,c] != null && color == m_ballsArray[r,c].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[r,c]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    r = i; c = j;
                    while( r < Board.instance.GetRowNumber() - 1 && c > 0)
                    {
                        r++;
                        c--;
                        if(m_ballsArray[r,c] != null && color == m_ballsArray[r,c].GetComponent<Ball>().GetColor())
                        {
                            earnBall++;
                            earnedBalllistInCase.Add(m_ballsArray[r,c]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(earnBall >= MIN_BALL_NUM_TO_EARN)
                    {
                        int listSize = m_earnedBallsList.Count;
                        CopyList(earnedBalllistInCase, m_earnedBallsList);
                        if(listSize != m_earnedBallsList.Count)
                        {
                            earnBallsTotal += earnBall;
                        }
                    }
                    earnedBalllistInCase.Clear();
                }
            }
        }

        if(earnBallsTotal > 0)
        {
            for(int i = 0; i < m_earnedBallsList.Count; ++i)
            {
                int r = m_earnedBallsList[i].GetComponent<Ball>().GetRowIndex();
                int c = m_earnedBallsList[i].GetComponent<Ball>().GetColumnIndex();
                Destroy(m_ballsArray[r,c]);
                m_ballsArray[r,c] = null;
                
                m_explosionBallList.Add(Instantiate(ExplosionBall, new Vector3(m_boardPos[r,c].x, m_boardPos[r,c].y, 0.0f), Quaternion.identity));
                
            }

            m_earnedBallsList.Clear();
        }

        return earnBallsTotal;
    }

    private int GetEmptySquare()
    {
        int count = Board.instance.GetRowNumber() * Board.instance.GetColumnNumber();
        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                if(m_ballsArray[i,j] != null)
                {
                    count--;
                }
            }
        }
        count -= m_ballInQueueList.Count;//NEXT_BALL

        return count;
    }

    private Vector2Int GenerateNewPosition()
    {
        int emptySquare = GetEmptySquare();
        while(emptySquare > 0)
        {
            int row = Random.Range(0, Board.instance.GetRowNumber());
            int col = Random.Range(0, Board.instance.GetColumnNumber());

            bool alreadyExisted = false;
            if(m_ballsArray[row, col] != null)
            {
                alreadyExisted = true;
            }

            for(int j = 0; j< m_ballInQueueList.Count; ++j)
            {
                if(row == m_ballInQueueList[j].GetComponent<Ball>().GetRowIndex() && col == m_ballInQueueList[j].GetComponent<Ball>().GetColumnIndex())
                {
                    alreadyExisted = true;
                }
            }

            if(!alreadyExisted)
            {
                return new Vector2Int(row, col);
            }
        }
        return new Vector2Int(-1, -1);
        
    }
    private void UpdateBinaryMatrix()
    {
        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                m_binaryMatrix[i,j] = m_ballsArray[i,j] != null ? 0 : 1;
            }
        }
    }

    private void UpdateBallList()
    {
        for(int i = 0; i < m_ballInQueueList.Count;)
        {
            if(m_ballInQueueList[i].GetComponent<Ball>().GetRowIndex() == m_targetBallPoint.x
            && m_ballInQueueList[i].GetComponent<Ball>().GetColumnIndex() == m_targetBallPoint.y)
            {
                // new position overlap on one ball in Queue
                // Get new position for ball in queue
                Vector2Int newPos = GenerateNewPosition();
                if(!newPos.Equals(new Vector2Int(-1, -1)))
                {
                    m_ballInQueueList[i].GetComponent<Ball>().RelocateTo(newPos);
                    m_ballInQueueList[i].transform.position = m_boardPos[newPos.x,newPos.y];
                }
                else if(m_ballsArray[m_activeBallPoint.x, m_activeBallPoint.y] == null)
                {
                    m_ballInQueueList[i].GetComponent<Ball>().RelocateTo(m_activeBallPoint);
                    m_ballInQueueList[i].transform.position = m_boardPos[m_activeBallPoint.x,m_activeBallPoint.y];
                }
            }
                   
            {
                m_ballInQueueList[i].GetComponent<Ball>().ActiveTrigger(AnimBallType.GROW_UP2);
                m_ballInQueueList[i].GetComponent<Ball>().SetStatus(StatusBall.ON_BOARD);
                m_ballsArray[m_ballInQueueList[i].GetComponent<Ball>().GetRowIndex(),m_ballInQueueList[i].GetComponent<Ball>().GetColumnIndex()] = m_ballInQueueList[i];
                m_ballInQueueList.RemoveAt(i);
            }

            Destroy(m_nextDisplayBallList[i]);
            m_nextDisplayBallList.RemoveAt(i);
        }

        m_activeBallPoint.Set(-1, -1);
    }
    private void UpdateMovingBall()
    {
        if(m_pathList.Count > 0)
        {
            if(m_timeToUpdateMovingBall > 0.001f)
            {
                m_timeToUpdateMovingBall = 0;
                m_movingDone = 0; //moving
                Vector2Int point = m_pathList[0];
                m_ballsArray[m_activeBallPoint.x, m_activeBallPoint.y].transform.position = m_boardPos[point.x,point.y];
                m_pathList.RemoveAt(0);

                if(m_pathList.Count == 0)
                {
                    //Update ball position                
                    m_ballsArray[point.x,point.y] = m_ballsArray[m_activeBallPoint.x, m_activeBallPoint.y];
                    m_ballsArray[point.x,point.y].GetComponent<Ball>().ActiveTrigger(AnimBallType.IDLE);
                    m_ballsArray[point.x,point.y].GetComponent<Ball>().RelocateTo(point);
                    m_ballsArray[m_activeBallPoint.x, m_activeBallPoint.y] = null;
                    //m_activeBallPoint.Set(-1, -1);

                    m_movingDone = 1; //moving done
                }
            }
            else
            {
                m_timeToUpdateMovingBall += Time.deltaTime;
            }
        }
    }

    public int GetScore()
    {
        int score = m_score;
        m_score = 0;
        return score;
    }

    public bool IsFullBoard()
    {
        int count = Board.instance.GetRowNumber() * Board.instance.GetColumnNumber();
        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                if(m_ballsArray[i,j] != null)
                {
                    count--;
                }
            }
        }

        return count < 2 ? true : false;
    }

    public bool IsGameOver()
    {
        return m_gameOver;
    }

    // A Data Structure for queue used in BFS 
    private struct queueNode 
    { 
        public Vector2Int pt;  // The cordinates of a cell 
        public int dist;  // cell's distance of from the source
        public int parent; 
    }; 
    
    // check whether given cell (row, col) is a valid 
    // cell or not. 
    private bool isValid(int row, int col) 
    { 
        // return true if row number and column number 
        // is in range 
        return (row >= 0) && (row < Board.instance.GetRowNumber()) && 
            (col >= 0) && (col < Board.instance.GetColumnNumber()); 
    } 

    private List<Vector2Int> GetPathFrom(List<queueNode> pathList)
    {
        if (pathList.Count == 0) 
            return null;

        List<Vector2Int> lPath = new List<Vector2Int>();
        // Lấy đường đi đảo ngược
        queueNode note = pathList[pathList.Count - 1];
        do
        {
            lPath.Add(note.pt);
            note = pathList[note.parent];
        } while (note.parent != 0);
        lPath.Add(note.pt);
        lPath.Add(pathList[0].pt);

        // lấy đường đi thuận
        for (int i = 0; i < lPath.Count / 2; i++)
        {
            note.pt = lPath[i];
            lPath[i] = lPath[lPath.Count - i - 1];
            lPath[lPath.Count - i - 1] = note.pt;
        }
        return lPath;
    }

    private int ShortestPathBinary(int[,] mat, Vector2Int src, bool isGhost, Vector2Int dest) 
    { 
        // check source and destination cell 
        // of the matrix have value 1 
        if (mat[dest.x,dest.y] == 0) 
            return -1; 
    
        // These arrays are used to get row and column 
        // numbers of 4 neighbours of a given cell 
        int[] rowNum = {-1, 0, 0, 1}; 
        int[] colNum = {0, -1, 1, 0}; 

        List<queueNode> pathStoredList = new List<queueNode>();

        bool[,] visited = new bool[Board.instance.GetRowNumber(),Board.instance.GetColumnNumber()]; 
        
        for(int i = 0; i < Board.instance.GetRowNumber(); ++i)
        {
            for(int j = 0; j < Board.instance.GetColumnNumber(); ++j)
            {
                // Mark the source cell as visited 
                visited[src.x,src.y] = true;
            }
        } 
        
        // Create a queue for BFS 
        Queue<queueNode> queue = new Queue<queueNode>(); 
        
        // Distance of source cell is 0 
        queueNode s;
        s.pt = src;
        s.dist = 0;
        s.parent = 0;
        queue.Enqueue(s);  // Enqueue source cell 
    
        int iParent = 0;

        // Do a BFS starting from source cell 
        while (queue.Count > 0) 
        { 
            queueNode curr = queue.Dequeue(); 
            Vector2Int pt = curr.pt; 
    
            // If we have reached the destination cell, 
            // we are done 
            if (pt.x == dest.x && pt.y == dest.y) 
            {
                pathStoredList.Add(curr);

                m_pathList = GetPathFrom(pathStoredList);
                return curr.dist; 
            }
    
            iParent = pathStoredList.Count;//luu vi tri myPoint trong list de gan cho con myPoint
            pathStoredList.Add(curr);

            // Otherwise dequeue the front cell in the queue 
            // and enqueue its adjacent cells 
    
            for (int i = 0; i < 4; i++) 
            { 
                int row = pt.x + rowNum[i]; 
                int col = pt.y + colNum[i]; 
                
                // if adjacent cell is valid, has path and 
                // not visited yet, enqueue it. 
                if (isValid(row, col) && (isGhost || mat[row,col] == 1) && !visited[row,col]) 
                { 
                    // mark cell as visited and enqueue it 
                    visited[row,col] = true; 
                    queueNode Adjcell;
                    Adjcell.pt = new Vector2Int(row, col);
                    Adjcell.dist =  curr.dist + 1;
                    Adjcell.parent = iParent;
                    queue.Enqueue(Adjcell); 
                } 
            } 
        } 
    
        // Return -1 if destination cannot be reached 
        return -1; 
    }

    
}
