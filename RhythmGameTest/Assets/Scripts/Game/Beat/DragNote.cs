using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DragRotate
{
    public Define.DRAG_DIRECTION drag;
    public float angle;
}

public class DragNote : MonoBehaviour
{
    public Note parentNote;
    [SerializeField]
    private Vector3 rotateAxis = Vector3.forward;
    [SerializeField]
    private List<DragRotate> dragConfigs;
    private Dictionary<Define.DRAG_DIRECTION, float> dictionary;

    // void Awake()
    // {
    //     if (parentNote == null)
    //     {
    //         throw new System.MissingFieldException("parent note must not be null");
    //     }

    //     dictionary = new Dictionary<Define.DRAG_DIRECTION, float>();
    //     foreach (DragRotate config in dragConfigs)
    //     {
    //         dictionary.Add(config.drag, config.angle);
    //     }
    // }

    // void Update()
    // {
    //     Rotate();
    // }

    // void Rotate()
    // {
    //     Define.DRAG_DIRECTION drag = parentNote.DragRequire;
    //     if (drag != Define.DRAG_DIRECTION.NONE)
    //     {
    //         if (drag == Define.DRAG_DIRECTION.DRAG_END)
    //         {
    //             transform.Rotate(rotateAxis, dictionary[drag] * Time.deltaTime);
    //         }
    //         else
    //         {
    //             transform.localRotation = Quaternion.AngleAxis(dictionary[drag], rotateAxis);
    //         }
    //     }
    // }
}
