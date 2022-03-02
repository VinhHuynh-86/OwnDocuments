using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollision : MonoBehaviour
{
    public MeshCollider charCollider;
    
    public void SetActiveCollider(bool isActive)
    {
        charCollider.enabled = isActive;
    }

    public void SetTag(string tag)
    {
        charCollider.gameObject.tag = tag;
    }
}
