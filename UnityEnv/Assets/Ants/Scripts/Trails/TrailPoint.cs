using UnityEngine;

public class TrailPoint : Point
{
    private Vector3 offset = Vector3.up * 0.01f;

    public TrailPoint(ITrailOwner owner)
    {
        Owner = owner;
        Strength = owner.Strength;
        Position = Util.GetGroundPos(owner.Position, offset);
        Hash = GetHash(Position);
    }
}