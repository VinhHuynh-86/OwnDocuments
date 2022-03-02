using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board instance;
    private int m_rowNumber;
    private int m_colNumber;
    public GameObject SquarePrefab;
    public GameObject[,] m_squareArr;

    private Vector3 objectPoolPosition;
    public float m_squareHorizontalLength = 0.46f;
    public float m_squareVerticalLength = 0.46f;

    private Vector2[,] m_posArr;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2[,] Initialize(int row, int col)
    {
        m_rowNumber = row;
        m_colNumber = col;

        objectPoolPosition = new Vector3( -m_squareHorizontalLength * m_colNumber / 2 + m_squareHorizontalLength/2, m_squareVerticalLength * m_rowNumber/2 - m_squareVerticalLength/2);

        m_squareArr = new GameObject[m_rowNumber, m_colNumber];
        m_posArr =  new Vector2[m_rowNumber, m_colNumber];

        for(int i = 0; i < m_rowNumber; ++i)
        {
             for(int j = 0; j < m_colNumber; ++j)
             {
                 m_posArr[i,j] = new Vector2(objectPoolPosition.x + j * m_squareHorizontalLength,objectPoolPosition.y -i * m_squareVerticalLength);
                 m_squareArr[i,j] = Instantiate(SquarePrefab, new Vector3(m_posArr[i,j].x, m_posArr[i,j].y, 0.0f), Quaternion.identity);
             }
        }
        return m_posArr;
    }

    public int GetRowNumber()
    {
        return m_rowNumber;
    }
    public int GetColumnNumber()
    {
        return m_colNumber;
    }

    public Vector2[,] GetBoardPosition()
    {
        return m_posArr;
    }

    public float GetSquareHorizontalLength()
    {
        return m_squareHorizontalLength;
    }

    public float GetSquareVerticalLength()
    {
        return m_squareVerticalLength;
    }
}
