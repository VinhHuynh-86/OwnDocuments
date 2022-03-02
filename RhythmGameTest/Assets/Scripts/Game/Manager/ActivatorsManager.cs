using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivatorsManager : MonoBehaviour
{
    #region Instance
    private static ActivatorsManager instance;
    public static ActivatorsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ActivatorsManager>();
                if (instance == null)
                {
                    instance = new GameObject("Dynamic lines manager", typeof(ActivatorsManager)).GetComponent<ActivatorsManager>();
                }
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion
    public Activator[] activators3D;

    public void InitializeActivators()
    {
        for(int i = 0 ; i < GameManager.Instance.linesNumber; ++i)
        {
            bool isVisible = GameManager.Instance.visibleLinesIndex.Contains(i);
            if(isVisible)
            {
                activators3D[i].Show();
            }
            else
            {
                activators3D[i].Hide();
            }
        }
    }

    public void UpdateActivatorsStatus()
    {
        for(int i = 0 ; i < GameManager.Instance.linesNumber; ++i)
        {
            bool isVisible = GameManager.Instance.visibleLinesIndex.Contains(i);
            if(isVisible)
            {
                activators3D[i].Show();
            }
            else
            {
                activators3D[i].Hide();
            }
        }
    }

    public void PreloadParticles()
    {
        foreach(Activator atv in activators3D)
        {
            if(atv) atv.PreloadParticles();
        }
    }

    public void StopAllParticles()
    {
        foreach(Activator atv in activators3D)
        {
            if(atv) atv.StopAllParticles();
        }
    }
}
