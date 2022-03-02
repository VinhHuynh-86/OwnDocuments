using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MyCopyComponents
{
    // public static void CopySerialized(SerializedObject source, SerializedObject dest)
    // {
    //     SerializedProperty serializedPropertyCurrent;

    //     serializedPropertyCurrent = source.GetIterator ();

    //     while (serializedPropertyCurrent.Next(true))
    //     {

    //         dest.CopyFromSerializedProperty (serializedPropertyCurrent);
    //     }

    //     dest.ApplyModifiedProperties ();
    // }

    // public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    //  {
    //      System.Type type = original.GetType();
    //      var dst = destination.GetComponent(type) as T;
    //      if (!dst) dst = destination.AddComponent(type) as T;
    //      var fields = type.GetFields();
    //      foreach (var field in fields)
    //      {
    //          if (field.IsStatic) continue;
    //          field.SetValue(dst, field.GetValue(original));
    //      }
    //      var props = type.GetProperties();
    //      foreach (var prop in props)
    //      {
    //          if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
    //          prop.SetValue(dst, prop.GetValue(original, null), null);
    //      }
    //      return dst as T;
    //  }
}