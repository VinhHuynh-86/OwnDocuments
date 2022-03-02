using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);

        return color;
    }
}
