using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class TimerConvert
{
    public static string IntToTime(int timeFloat, string format = "hh':'mm':'ss")
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timeFloat);
        return timeSpan.ToString(format);
    }

    public static string FloatToTime(float timeFloat, string format = "hh':'mm':'ss'.'fff")
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timeFloat);
        return timeSpan.ToString(format);
    }

    public static float StringToTime(string timeString, string format = "hh:mm:ss.fff")
    {
        System.DateTime date = System.DateTime.ParseExact(timeString, format, CultureInfo.InvariantCulture);
        return (date.Hour * 3600f) + (date.Minute * 60f) + (date.Second * 1f) + (date.Millisecond * 1f / 1000f);
    }
}
