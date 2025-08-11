using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (MinMaxSliderAttribute)attribute;

        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");

        if (minProp == null || maxProp == null)
        {
            EditorGUI.LabelField(position, label.text, "Missing min/max fields");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Tạo rects cho nhãn, ô input, slider
        var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        var minFieldRect = new Rect(labelRect.xMax, position.y, 40f, position.height);
        var sliderRect = new Rect(minFieldRect.xMax + 2, position.y, position.width - labelRect.width - 90f, position.height);
        var maxFieldRect = new Rect(sliderRect.xMax + 2, position.y, 40f, position.height);

        EditorGUI.LabelField(labelRect, label);

        if (minProp.propertyType == SerializedPropertyType.Float)
        {
            float min = minProp.floatValue;
            float max = maxProp.floatValue;

            // Input fields
            min = EditorGUI.FloatField(minFieldRect, min);
            max = EditorGUI.FloatField(maxFieldRect, max);

            // Slider
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, attr.MinLimit, attr.MaxLimit);

            minProp.floatValue = Mathf.Clamp(min, attr.MinLimit, attr.MaxLimit);
            maxProp.floatValue = Mathf.Clamp(max, attr.MinLimit, attr.MaxLimit);
        }
        else if (minProp.propertyType == SerializedPropertyType.Integer)
        {
            float min = minProp.intValue;
            float max = maxProp.intValue;

            // Input fields
            min = EditorGUI.FloatField(minFieldRect, min);
            max = EditorGUI.FloatField(maxFieldRect, max);

            // Slider
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, attr.MinLimit, attr.MaxLimit);

            minProp.intValue = Mathf.RoundToInt(Mathf.Clamp(min, attr.MinLimit, attr.MaxLimit));
            maxProp.intValue = Mathf.RoundToInt(Mathf.Clamp(max, attr.MinLimit, attr.MaxLimit));
        }
        else
        {
            EditorGUI.LabelField(position, "Unsupported min/max type");
        }

        EditorGUI.EndProperty();
    }
}
