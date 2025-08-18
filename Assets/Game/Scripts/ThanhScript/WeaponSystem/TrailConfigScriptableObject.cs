using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Weapons/Trail Config", order = 3)]
public class TrailConfigScriptableObject : ScriptableObject
{
    public Material Material;
    public AnimationCurve WidthCurve;
    public float Duration = 0.5f;
    public float MinVertexDistance = 1.0f;
    public Gradient Color;

    public float MissDistance = 100f;
    public float SimulationSpeed = 200f;
}
