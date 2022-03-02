using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    [SerializeField] private GameObject Icon;
    public static Action<GameObject> OnLoaded;

    string mLastMap = "";
    bool mIsStarted = false;

    void OnEnable()
    {
        if (!mIsStarted)
        {
            mIsStarted = true;
            return;
        }

        string name = (ProfileMgr.Instance.Stage < 9 ? "Map00" : "Map0") + (ProfileMgr.Instance.Stage + 1);
        StartCoroutine(LoadScene(name, progress =>
        {
        }));
    }

    void OnDisable()
    {

    }

    void Start()
    {

    }

    void Update()
    {
        Icon.transform.Rotate(new Vector3(0, 0, -6 * 60 * Time.deltaTime));
    }

    IEnumerator LoadScene(string name, Action<float> progress)
    {
        AsyncOperation operation;
        if (mLastMap != "")
        {
            operation = SceneManager.UnloadSceneAsync(mLastMap);
            while (!operation.isDone)
            {
                progress.Invoke(operation.progress);
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.25f);

        operation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            progress.Invoke(operation.progress);
            yield return null;
        }

        GameObject map = GameObject.FindGameObjectWithTag("Map");
        if (map != null)
        {
            if (OnLoaded != null)
            {
                OnLoaded(map);
            }
        }
        mLastMap = name;
    }

    IEnumerable UnloadScene(string name, Action<float> progress)
    {
        AsyncOperation operation = SceneManager.UnloadSceneAsync(name);

        while (!operation.isDone)
        {
            progress.Invoke(operation.progress);
            yield return null;
        }
    }
}
