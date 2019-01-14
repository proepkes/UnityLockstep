using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using UnityEditor;
using UnityEngine;



[CustomPropertyDrawer(typeof(Fix64Attribute))]
public class EditorFix64Drawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {


        var f = (Fix64)property.longValue;

        f = EditorGUI.LongField(position, label, (long)f);

        property.longValue = (long)f;
    }
}

