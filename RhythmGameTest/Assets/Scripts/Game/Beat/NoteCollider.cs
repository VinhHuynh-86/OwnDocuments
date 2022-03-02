using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Note:

Case 1
    Activator physic: trigger + rigdibody
    Note physic: none trigger + rigidbody
    => note -> NoteCollider trigger -> activator -> ActivatorCollider trigger

Case 2
    Activator physic: trigger + rigdibody
    Note physic: trigger + rigidbody
    => activator -> ActivatorCollider trigger -> note -> NoteCollider trigger

Case 3
    Note physic: no rigidbody
    => note -> not trigger

Case 4
    Activator physic: no rigidbody
    => activator -> not trigger
*/

public interface INoteCollider<T>
{
    void OnNoteEnter(T other);
    void OnNoteExit(T other);
}

public class NoteCollider : MonoBehaviour
{
    public Note root;
    public float scaleMultiple = 1.0f;

    public Collider noteCollider;
    Define.PHYSICS currentPhysic;

    private void Awake()
    {
        ScaleCollider(ref noteCollider);

        if (root == null)
        {
            throw new System.MissingFieldException("root note must not be null");
        }

        currentPhysic = Define.PHYSICS.NONE;
    }

    private void ScaleCollider(ref Collider collider)
    {
        if (collider != null && collider is SphereCollider)
        {
            SphereCollider sphere = collider as SphereCollider;
            sphere.radius *= scaleMultiple;
        }
    }

    void OnEnable()
    {
        currentPhysic = Define.PHYSICS.NONE;
    }

    void OnDisable()
    {
        currentPhysic = Define.PHYSICS.NONE;
    }


    void OnTriggerEnter(Collider other)
    {
        NoteCollider noteCollider = other.GetComponentInParent<NoteCollider>();
        if (noteCollider != null)
        {
            // Debug.Log("NoteCollider " + Convert.FloatToTime(root.startTime) + " enter " + Convert.FloatToTime(noteCollider.root.startTime));

            if (root is INoteCollider<NoteCollider>)
            {
                INoteCollider<NoteCollider> note = root as INoteCollider<NoteCollider>;
                note.OnNoteEnter(noteCollider);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        NoteCollider noteCollider = other.GetComponentInParent<NoteCollider>();
        if (noteCollider != null)
        {
            // Debug.Log("NoteCollider " + Convert.FloatToTime(root.startTime) + " exit " + Convert.FloatToTime(noteCollider.root.startTime));

            if (root is INoteCollider<NoteCollider>)
            {
                INoteCollider<NoteCollider> note = root as INoteCollider<NoteCollider>;
                note.OnNoteExit(noteCollider);
            }
        }
    }


    public void OnActivatorTrigger(Collider myOwnCollider, bool isEnter)
    {
        int val = (int)currentPhysic;
        if (isEnter)
        {
            val = val << 1;
        }
        else
        {
            val = val >> 1;
        }
        currentPhysic = (Define.PHYSICS)val;
    }

    public Bounds bounds
    {
        get
        {
            return noteCollider.bounds;
        }
    }

    public Define.PHYSICS Physic
    {
        get
        {
            return currentPhysic;
        }
    }
}
