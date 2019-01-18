using UnityEngine;

public class Point
{
    public static int GetHash(int x, int z)
    {
        return x + (z + 128) * 256; // Sandbox is 256 x 256
    }

    public static int GetHash(Vector3 p)
    {
        return GetHash(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.z)); 
    }

    public int Hash;
    public float Strength;
    public Vector3 Position;
    public ITrailOwner Owner;
}