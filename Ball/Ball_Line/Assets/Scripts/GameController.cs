using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public int ROW_BOARD = 9;
    public int COL_BOARD = 9;
    private bool m_gameOver = false;

    public Text scoreText;
    public GameObject m_gameOverAndRestartText;
    public GameObject m_newGameButton;
    private float m_newGameBtnHeight = 1.12f;
    private float m_newGameBtnWidth = 0.52f;
    private int m_score = 0;

    // Start is called before the first frame update
    void Start()
    {
        Vector2[,] boardPos = Board.instance.Initialize(ROW_BOARD, COL_BOARD);
        BallManager.instance.Initialize(boardPos);
    }

    // Update is called once per frame
    void Update()
    {
        //If the game is over and the player has pressed some input...
        if(!m_gameOver)
        {
            if(Input.GetMouseButtonDown(0))
            {
                //Check touch on ball
                BallManager.instance.CheckTouchOnBall(Input.mousePosition);
            }

            UpdateScored();
        }

        if (m_gameOver) 
        {
            //Restart game after game over
            if(Input.GetMouseButtonDown(0))
            {
                m_gameOver = false;
                //...reload the current scene.
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        //New Game button
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 worldPoint2d = new Vector2(worldPoint.x, worldPoint.y);

            if(worldPoint2d.x > m_newGameButton.transform.position.x - m_newGameBtnHeight/2 && worldPoint2d.x < m_newGameButton.transform.position.x + m_newGameBtnHeight/2
            && worldPoint2d.y > m_newGameButton.transform.position.y - m_newGameBtnWidth/2 && worldPoint2d.y < m_newGameButton.transform.position.y + m_newGameBtnWidth/2)
            {
                //...reload the current scene.
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (!m_gameOver) 
        {
            m_gameOver = BallManager.instance.IsGameOver();
            if(m_gameOver)
            {
                m_gameOverAndRestartText.SetActive(true);
            }
        }
    }

    public void NewGame()
    {
        BallManager.instance.ReNewAll();
        m_score = 0;
    }

    public void UpdateScored()
    {
        m_score += BallManager.instance.GetScore();
        //...and adjust the score text.
        scoreText.text = "Score: " + m_score.ToString();
    }
}
