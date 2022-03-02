using UnityEngine;
using Doozy.Engine.Nody;

public class Init : MonoBehaviour
{
    enum STATE
    {
        NONE,
        INIT_DOOZY,
        FINISHED,
    }

    [SerializeField] private GraphController MainGraph;

    STATE mState;

    void Awake()
    {
        ProfileMgr.Instance.Init();
        ProfileMgr.Instance.InitSettings();
    }

    void Start()
    {
        SetState(STATE.INIT_DOOZY);
    }

    void Update()
    {
        switch (mState)
        {
            case STATE.INIT_DOOZY:
                if (MainGraph.Initialized)
                {
                    SetState(STATE.FINISHED);
                }
                break;

            case STATE.FINISHED:
                break;
        }
    }

    void SetState(STATE state)
    {
        mState = state;
        switch (mState)
        {
            case STATE.INIT_DOOZY:
                break;

            case STATE.FINISHED:
                GameEventMgr.SendEvent("init_completed");
                break;
        }
    }
}

