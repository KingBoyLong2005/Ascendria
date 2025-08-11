using UnityEngine;
using UnityEditor;
using Map;

[CustomEditor(typeof(MapView))]
public class MapViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MapView mapView = (MapView)target;

        // Show global variables
        DrawPropertiesExcluding(serializedObject, "CanvasUI", "WorldSpace", "canvasUISettings", "worldSpaceSettings");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Display Mode", EditorStyles.boldLabel);

        // Checkbox CanvasUI
        bool newCanvasUI = EditorGUILayout.Toggle("Canvas UI", mapView.CanvasUI);
        if (newCanvasUI && !mapView.CanvasUI)
        {
            mapView.CanvasUI = true;
            mapView.WorldSpace = false;
        }
        else if (!newCanvasUI && mapView.CanvasUI)
        {
            mapView.CanvasUI = false;
        }

        // Checkbox WorldSpace
        bool newWorldSpace = EditorGUILayout.Toggle("World Space", mapView.WorldSpace);
        if (newWorldSpace && !mapView.WorldSpace)
        {
            mapView.WorldSpace = true;
            mapView.CanvasUI = false;
        }
        else if (!newWorldSpace && mapView.WorldSpace)
        {
            mapView.WorldSpace = false;
        }

        EditorGUILayout.Space();

        // Display the corresponding settings group
        if (mapView.CanvasUI)
        {
            EditorGUILayout.LabelField("Canvas UI Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasUISettings"), true);
        }
        else if (mapView.WorldSpace)
        {
            EditorGUILayout.LabelField("World Space Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("worldSpaceSettings"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
