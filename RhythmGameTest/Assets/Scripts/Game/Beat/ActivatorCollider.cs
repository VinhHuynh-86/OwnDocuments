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

public interface IActivatorCollider<T>
{
    void OnActivatorEnter(T other);
    void OnActivatorExit(T other);
}

public class ActivatorCollider : MonoBehaviour
{
    public Activator root;
    public Collider colliderCool;

    public float scaleMultiple = 1.0f;
    [SerializeField] bool showCollider;


    private void Awake()
    {
        if (root == null)
        {
            throw new System.MissingFieldException("root activator must not be null");
        }
        ScaleCollider(ref colliderCool);
    }

    private void ScaleCollider(ref Collider collider)
    {
        if (collider != null && collider is SphereCollider)
        {
            SphereCollider sphere = collider as SphereCollider;
            sphere.radius *= 1f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        NoteCollider noteCollider = other.GetComponentInParent<NoteCollider>();
        if (noteCollider == null)
        {
            return;
        }

        if (noteCollider.root.target != this.root)
        {
            // Debug.Log("overlapped ActivatorCollider enter " + Convert.FloatToTime(noteCollider.root.startTime) + " vs " + root.name);
            return;
        }

        noteCollider.OnActivatorTrigger(other, true);

        // Debug.Log("ActivatorCollider " + Convert.FloatToTime(noteCollider.root.startTime) + " enter " + noteCollider.Physic);
        if (root is IActivatorCollider<NoteCollider>)
        {
            IActivatorCollider<NoteCollider> activator = root as IActivatorCollider<NoteCollider>;
            activator.OnActivatorEnter(noteCollider);
        }
    }

    void OnTriggerExit(Collider other)
    {
        NoteCollider noteCollider = other.GetComponentInParent<NoteCollider>();
        if (noteCollider == null)
        {
            return;
        }

        if (noteCollider.root.target != this.root)
        {
            // Debug.Log("overlapped ActivatorCollider exit " + Convert.FloatToTime(noteCollider.root.startTime) + " vs " + root.name);
            return;
        }

        noteCollider.OnActivatorTrigger(other, false);

        // Debug.Log("ActivatorCollider " + Convert.FloatToTime(noteCollider.root.startTime) + " exit " + noteCollider.Physic);
        if (root is IActivatorCollider<NoteCollider>)
        {
            IActivatorCollider<NoteCollider> activator = root as IActivatorCollider<NoteCollider>;
            activator.OnActivatorExit(noteCollider);
        }
    }

    void OnDrawGizmos() {
        if (showCollider)
        {    
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, colliderCool.bounds.extents.magnitude/2);

            // Gizmos.color = Color.red;
            // Gizmos.DrawWireSphere(transform.position, colliderStart.bounds.extents.magnitude);
            //  Gizmos.matrix = this.transform.localToWorldMatrix;
            //  Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
 }
}
