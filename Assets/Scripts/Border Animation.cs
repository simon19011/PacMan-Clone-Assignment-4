using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class BorderAnimation : MaskableGraphic
{
    [Header("Wave Settings")]
    [Min(1)]public int thickness = 2;
    [Min(1)]public int pixelStep = 4;
    [Min(0)]public int amplitude = 10;
    [Min(1)]public int cycles = 2;
    [Min(1)] public int gridSize = 4;
    public float speed = 10f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();

        float phase = Application.isPlaying ? Mathf.Floor(Time.time * speed / gridSize) * gridSize : 0f;

        int width = Mathf.FloorToInt(rect.width);

        for (int x = 0; x < width; x += pixelStep)
        {
            float nomralisedHorizontal = (float)x / width;
            float angle = nomralisedHorizontal * cycles * Mathf.PI * 2f + phase;

            int y = Mathf.RoundToInt(Mathf.Sin(angle) * amplitude / gridSize) * gridSize;

            AddPixel(vh, rect.xMin + x, y - thickness / 2f, pixelStep, thickness);
        }
    }

    void AddPixel(VertexHelper vh, float x, float y, float width, float height)
    {
        int index = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = new Vector3(x, y);
        vh.AddVert(v);
        v.position = new Vector3(x, y + height);
        vh.AddVert(v);
        v.position = new Vector3(x + width, y + height);
        vh.AddVert(v);
        v.position = new Vector3(x + width, y);
        vh.AddVert(v);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }

    void Update()
    {
        if (Application.isPlaying && speed != 0f)
        {
            SetVerticesDirty();
        }
    }
}
