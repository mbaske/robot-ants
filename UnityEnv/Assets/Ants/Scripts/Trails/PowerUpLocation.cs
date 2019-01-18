using UnityEngine;

public class PowerUpLocation : Point
{
    public PowerUpLocation(Vector3 pos)
    {
        Strength = 1f;
        Position = pos;
        Hash = GetHash(Position);
    }
}