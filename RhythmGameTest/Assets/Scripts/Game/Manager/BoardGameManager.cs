using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardGameManager : MonoBehaviour
{
    #region Instance
    private static BoardGameManager instance;
    public static BoardGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BoardGameManager>();
                if (instance == null)
                {
                    instance = new GameObject("BoardGameManager", typeof(BoardGameManager)).GetComponent<BoardGameManager>();
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

    //public Camera camera;

    public GameObject LeftPlane;
    public GameObject RightPlane;
    public Note[] notes;
    [HideInInspector]
    public List<Vector3> notesOriginalPosition;

    float deltaAlpha = 0.01f;

    bool isFadeInFadeOut = false;

    void Awake()
    {
    }

    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    public static bool HasInstance()
    {
        return instance != null;
    }

    public void InitializeNotes()
    {
        isFadeInFadeOut = true;

        notesOriginalPosition = new List<Vector3>();

        for(int i = 0; i < notes.Length; ++i)
        {
            notesOriginalPosition.Add(notes[i].transform.position);
        }

        int linesNumber = notesOriginalPosition.Count;
        
        SetBooadTransparent();
    }

    public void SetBooadTransparent()
    {        
        Lane[] children = this.GetComponentsInChildren<Lane>(true);
        Color c;
        foreach(Lane obj in children)
        {
            c = obj.GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
            c.a = 0;
            obj.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
        }

        foreach(Activator obj in ActivatorsManager.Instance.activators3D)
        {
            c = obj.spriteRenderer.color;
            c.a = 0;
            obj.spriteRenderer.color = c;
        }
    }

    public void FadeOut()
    {
        StartCoroutine(DoFadeOut());
    }

    public void FadeIn()
    {
        StartCoroutine(DoFadeIn());
    }

    IEnumerator DoFadeOut()
    {
        isFadeInFadeOut = true;

        float alpha = 1;
        while(alpha > 0)
        {
            //
            alpha -= deltaAlpha;
            if(alpha < 0 )
                alpha = 0f;

            Lane[] childrend = this.GetComponentsInChildren<Lane>();
            Color c;
            foreach(Lane obj in childrend)
            {
                // Color c = obj.GetComponent<MeshRenderer>().material.color;
                // c.a = alpha;
                // obj.GetComponent<MeshRenderer>().material.color = c;
                c = obj.GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
                c.a = alpha;
                obj.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
            }

            foreach(Activator obj in ActivatorsManager.Instance.activators3D)
            {
                c = obj.spriteRenderer.color;
                c.a -= deltaAlpha;
                if(c.a < 0) c.a = 0;
                obj.spriteRenderer.color = c;
            }


            yield return null;
        }
        isFadeInFadeOut = false;

        yield return null;
    }

    IEnumerator DoFadeIn()
    {
        isFadeInFadeOut = true;

        SetBooadTransparent();
        yield return new WaitForSeconds(Define.WAIT_FOR_SECOND);

        float alpha2 = 0;
        while(alpha2 < 1)
        {
            //
            alpha2 += deltaAlpha * 2;
            if(alpha2 > 1 )
                alpha2 = 1f;

            Lane[] childrend = this.GetComponentsInChildren<Lane>();
            Color c;
            foreach(Lane obj in childrend)
            {
                // Color c = obj.GetComponent<MeshRenderer>().material.color;
                // c.a = alpha;
                // obj.GetComponent<MeshRenderer>().material.color = c;
                c = obj.GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
                c.a = alpha2;
                obj.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
            }

            foreach(Activator obj in ActivatorsManager.Instance.activators3D)
            {
                c = obj.spriteRenderer.color;
                c.a += deltaAlpha * 2;
                if(c.a > 1) c.a = 1;

                if(!GameManager.Instance.IsActiveLine(obj.activatorIndex))
                {
                    if(c.a > obj.GetHideTransparent())
                        c.a = obj.GetHideTransparent();
                }
                
                obj.spriteRenderer.color = c;

            }


            yield return null;
        }

        isFadeInFadeOut = false;
        

        yield return null;
    }

    public bool IsFadeInFadeOut()
    {
        return isFadeInFadeOut;
    }
}
