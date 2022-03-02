using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Facebook.Unity;

public class Splash : MonoBehaviour
{
    enum STATE
    {
        NONE,
        INIT_SCENE,
        WAITING,
    }

    Timer mTimer = new Timer();
    AsyncOperation mAsync;
    STATE mState = STATE.NONE;

    void Start()
    {
        FB.Init();
        mTimer.SetDuration(2);
        SetState(STATE.INIT_SCENE);
    }

    void Update()
    {
        mTimer.Update(Time.deltaTime);
        switch (mState)
        {
            case STATE.INIT_SCENE:
                break;

            case STATE.WAITING:
                if (mTimer.IsDone())
                {
                    ActivateScene();
                }
                break;
        }
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.INIT_SCENE:
                StartCoroutine(LoadScene());
                break;

            case STATE.WAITING:
                break;
        }
    }

    public void ActivateScene()
    {
        mAsync.allowSceneActivation = true;
        SetState(STATE.NONE);
    }

    IEnumerator LoadScene()
    {
        mAsync = SceneManager.LoadSceneAsync("Main");
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            yield return null;
        }
        SetState(STATE.WAITING);
    }
}
