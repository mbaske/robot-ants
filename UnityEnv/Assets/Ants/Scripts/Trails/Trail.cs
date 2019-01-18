using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class Trail
{
    public bool IsEmpty => queue.Count == 0;
    public ITrailOwner Owner;

    private LineRenderer line;
    private Queue<TrailPoint> queue;
    private Action<TrailPoint> onRemovePoint;
    private GradientColorKey[] colorKeys;
    private GradientAlphaKey[] alphaKeys;

    public Trail(ITrailOwner owner, 
                 Action<TrailPoint> onRemovePoint, 
                 Color color0, 
                 Color color1, 
                 float thickness)
    {
        Owner = owner;
        queue = new Queue<TrailPoint>();
        this.onRemovePoint = onRemovePoint;

        CreateLineRenderer(thickness);

        colorKeys = new GradientColorKey[] {
            new GradientColorKey(color0, 0f),
            new GradientColorKey(color1, 1f) };
        alphaKeys = new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f) };
    }

    public void SetParent(Transform parent)
    {
        line.transform.parent = parent;
    }

    public void Destroy()
    {
        queue.Clear();
        onRemovePoint = null;
        UnityEngine.Object.Destroy(line.gameObject);
    }

    public TrailPoint AddPoint(ITrailOwner owner)
    {
        TrailPoint point = new TrailPoint(owner);
        queue.Enqueue(point);
        return point;
    }

    public void Update(float decr)
    {
        foreach (TrailPoint tp in queue)
        {
            tp.Strength -= decr;
        }
        RemoveExpired();
    }

    public void Render()
    {
        TrailPoint[] points = queue.ToArray();
        Array.Reverse(points);

        int n = points.Length;
        alphaKeys[0].alpha = points[0].Strength;
        alphaKeys[1].alpha = points[n - 1].Strength;

        Vector3[] pos = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            pos[i] = points[i].Position;
        }

        line.positionCount = n;
        line.SetPositions(pos);
        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        line.colorGradient = gradient;
    }

    private void RemoveExpired()
    {
        if (!IsEmpty && queue.Peek().Strength <= 0f)
        {
            onRemovePoint(queue.Dequeue());
            RemoveExpired();
        }
    }

    private void CreateLineRenderer(float thickness)
    {
        line = new GameObject().AddComponent<LineRenderer>();
        line.gameObject.name = "Trail";
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = thickness;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.receiveShadows = false;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
    }
}