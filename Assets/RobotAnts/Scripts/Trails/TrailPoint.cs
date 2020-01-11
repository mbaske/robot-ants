using UnityEngine;

public struct TrailPoint
{
    public Vector2 uv { get; private set; }
    public float strength { get; private set; }
    public bool isObsolete { get; private set; }

    public TrailPoint(Searcher agent)
    {
        uv = agent.UVCoord;
        strength = agent.Energy;
        isObsolete = false;
    }

    public void Attenuate(float amount)
    {
        strength = Mathf.Max(0f, strength - amount);
        isObsolete = strength < Mathf.Epsilon;
    }
}
