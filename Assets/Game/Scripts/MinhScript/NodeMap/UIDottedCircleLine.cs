using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIDottedCircleLine : MaskableGraphic
{
    [Header("Line Settings")]
    public Vector2 startPoint;
    public Vector2 endPoint;
    public float dotSpacing = 20f;              // Khoảng cách giữa các dot
    public float dotRadius = 5f;                // Bán kính hình tròn
    public int circleSegments = 20;             // Độ mịn của hình tròn (càng cao càng mịn)

    public void SetLine(Vector2 start, Vector2 end)
    {
        startPoint = start;
        endPoint = end;
        SetVerticesDirty();
    }    

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 dir = (endPoint - startPoint).normalized;
        float totalDistance = Vector2.Distance(startPoint, endPoint);

        int dotCount = Mathf.FloorToInt(totalDistance / dotSpacing) + 1;
        Vector2 currentPos = startPoint;

        for (int i = 0; i < dotCount; i++)
        {
            AddCircle(vh, currentPos, dotRadius, color, circleSegments);
            currentPos += dir * dotSpacing;
        }
    }

    void AddCircle(VertexHelper vh, Vector2 center, float radius, Color col, int segments)
    {
        int startIndex = vh.currentVertCount;
        float angleStep = 360f / segments;

        // Thêm đỉnh tâm
        vh.AddVert(center, col, Vector2.zero);

        // Thêm các đỉnh xung quanh
        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep);
            Vector2 pos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            vh.AddVert(pos, col, Vector2.zero);
        }

        // Tạo các tam giác
        for (int i = 0; i < segments; i++)
        {
            vh.AddTriangle(startIndex, startIndex + i + 1, startIndex + i + 2);
        }
    }
}
