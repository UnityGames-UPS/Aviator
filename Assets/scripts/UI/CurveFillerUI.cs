using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class CurveFillerUI : MaskableGraphic
{
    [Header("Curve Settings")]
    [SerializeField, Min(2)] private int points = 50;
    [SerializeField] private float heightMultiplier = 0.7f;   // Relative to rect height
    [SerializeField] private float widthMultiplier  = 0.5f;   // Relative to rect width

    [Header("Border")]
    [SerializeField] private float borderThickness = 3f;      // UI units (pixels with CanvasScaler)
    [SerializeField] private Color borderColor = Color.red;

    [Header("Plane Follow")]
    [SerializeField] private RectTransform PlaneParent;

    // Expose if you want to read it elsewhere
    public Vector2 LastTopPoint { get; private set; }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // Safety
        if (points < 2)
            return;

        Rect rect = rectTransform.rect;
        float curveWidth = rect.width  * widthMultiplier;
        float maxHeight  = rect.height * heightMultiplier;

        // 1) Build top & bottom arrays
        Vector2[] top = new Vector2[points];
        Vector2[] bottom = new Vector2[points];

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)(points - 1);

            // Centered local UI space (pivot 0.5,0.5)
            float x = t * curveWidth - rect.width * 0.5f;
            float y = Mathf.Pow(t, 2) * maxHeight - rect.height * 0.5f;

            top[i] = new Vector2(x, y);
            bottom[i] = new Vector2(x, -rect.height * 0.5f);
        }

        LastTopPoint = top[points - 1];

        // 2) Fill geometry (same as before)
        for (int i = 0; i < points - 1; i++)
        {
            int vi = vh.currentVertCount;

            vh.AddVert(top[i],     color, Vector2.zero);
            vh.AddVert(top[i + 1], color, Vector2.zero);
            vh.AddVert(bottom[i],  color, Vector2.zero);
            vh.AddVert(bottom[i+1],color, Vector2.zero);

            vh.AddTriangle(vi + 0, vi + 1, vi + 2);
            vh.AddTriangle(vi + 1, vi + 3, vi + 2);
        }

        // 3) Compute per-vertex normals along the curve for CONSTANT thickness
        //    - segmentNormal[k] = normal of segment (k -> k+1)
        //    - vertex normal = average of adjacent segment normals (smoothed)
        Vector2[] segNormals = new Vector2[points - 1];
        for (int i = 0; i < points - 1; i++)
        {
            Vector2 d = top[i + 1] - top[i];
            if (d.sqrMagnitude < 1e-6f) d = new Vector2(1f, 0f); // fallback
            d.Normalize();
            segNormals[i] = new Vector2(-d.y, d.x); // 90° CCW
        }

        Vector2[] normals = new Vector2[points];
        normals[0] = segNormals[0];
        normals[points - 1] = segNormals[points - 2];
        for (int i = 1; i < points - 1; i++)
        {
            Vector2 n = segNormals[i - 1] + segNormals[i];
            if (n.sqrMagnitude < 1e-6f) n = segNormals[i]; // fallback when perfectly straight in opposite dirs
            normals[i] = n.normalized;
        }

        // 4) Border strip (centered on the curve) with uniform thickness
        float half = Mathf.Max(0f, borderThickness) * 0.5f;

        for (int i = 0; i < points - 1; i++)
        {
            // Two-edge quad following the curve
            Vector2 aOuter = top[i]     + normals[i]     * half;
            Vector2 aInner = top[i]     - normals[i]     * half;
            Vector2 bOuter = top[i + 1] + normals[i + 1] * half;
            Vector2 bInner = top[i + 1] - normals[i + 1] * half;

            int vi = vh.currentVertCount;

            // NOTE: Using borderColor for all four border vertices
            vh.AddVert(aOuter, borderColor, Vector2.zero);
            vh.AddVert(bOuter, borderColor, Vector2.zero);
            vh.AddVert(aInner, borderColor, Vector2.zero);
            vh.AddVert(bInner, borderColor, Vector2.zero);

            vh.AddTriangle(vi + 0, vi + 1, vi + 2);
            vh.AddTriangle(vi + 1, vi + 3, vi + 2);
        }

        // 5) Plane follow (works in edit mode too)
        if (PlaneParent != null)
            PlaneParent.anchoredPosition = LastTopPoint;
    }

#if UNITY_EDITOR
    // Live refresh when tweaking in inspector
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
