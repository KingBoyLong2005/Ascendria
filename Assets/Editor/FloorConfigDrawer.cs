using UnityEngine;
using UnityEditor;
using Map;

[CustomPropertyDrawer(typeof(FloorConfig))]
public class FloorConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int index = GetIndexFromPath(property.propertyPath);

        var nameProp = property.FindPropertyRelative("floorName");
        string name = nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue)
            ? nameProp.stringValue
            : $"Floor {index}";

        EditorGUI.PropertyField(position, property, new GUIContent(name), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private int GetIndexFromPath(string path)
    {
        // Tìm tất cả match có dạng [0], [1], ...
        var matches = System.Text.RegularExpressions.Regex.Matches(path, @"\[(\d+)\]");
        if (matches.Count > 0)
        {
            var lastMatch = matches[matches.Count - 1];
            if (int.TryParse(lastMatch.Groups[1].Value, out int index))
                return index;
        }
        return -1; // Không tìm thấy
    }

}
