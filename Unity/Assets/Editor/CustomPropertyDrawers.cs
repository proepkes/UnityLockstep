

using FixMath.NET;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FixedNumberAttribute))]
public class EditorFixedNumber : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {         
        FixedNumberAttribute a = ((FixedNumberAttribute)this.attribute);
        var target = property.serializedObject.targetObject;
        var prop = target.GetType().GetField(property.name);

        var orgValue = (Fix64)prop.GetValue(target) ;
        var value = (Fix64)EditorGUI.FloatField(
            position,
            label,
            (float)orgValue
        );

        if (orgValue != value)
            prop.SetValue(target, value);
    }
}