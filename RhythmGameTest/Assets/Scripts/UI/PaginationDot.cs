using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaginationDot : MonoBehaviour
{
    public GameObject fill;

    public void SetFill(bool isFill)
    {
        fill.SetActive(isFill);
    }
}
