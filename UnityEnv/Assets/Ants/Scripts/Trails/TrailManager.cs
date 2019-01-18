using System.Collections.Generic;
using UnityEngine;

public class TrailManager : MonoBehaviour
{
    [Range(0.25f, 2.5f)]
    public float Spacing = 1f;
    [SerializeField]
    [Range(1f, 120f)]
    private float lifeTime = 60f;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float thickness = 0.1f;
    [SerializeField]
    private Color color0 = Color.cyan;
    [SerializeField]
    private Color color1 = Color.blue;
    // Trails that are currently being created (all non-orphaned trails).
    private Dictionary<ITrailOwner, Trail> activeTrails;
    private Dictionary<int, List<Point>> pointLists;
    private List<Trail> allTrails;
    private bool active;

    // Called via Ant.Initialize, "Follow Trails" mode only.
    public void Initialize()
    {
        if (!active)
        {
            active = true;
            allTrails = new List<Trail>();
            pointLists = new Dictionary<int, List<Point>>();
            activeTrails = new Dictionary<ITrailOwner, Trail>();
        }
    }

    private void Update()
    {
        if (active)
        {
            float decr = Time.deltaTime / lifeTime;

            List<Trail> expired = new List<Trail>();
            foreach (Trail trail in allTrails)
            {
                trail.Update(decr);

                if (trail.IsEmpty)
                {
                    expired.Add(trail);
                }
                else
                {
                    trail.Render();
                }
            }

            foreach (Trail trail in expired)
            {
                OrphanTrail(trail.Owner);
                allTrails.Remove(trail);
                trail.Destroy();
            }
        }
    }

    public void OrphanTrail(ITrailOwner owner)
    {
        if (owner != null && activeTrails.ContainsKey(owner))
        {
            activeTrails[owner].Owner = null;
            activeTrails.Remove(owner);
        }
    }

    public void AddTrailPoint(ITrailOwner owner)
    {
        Trail trail;
        if (activeTrails.ContainsKey(owner))
        {
            trail = activeTrails[owner];
        }
        else
        {
            trail = new Trail(owner, RemovePoint, color0, color1, thickness);
            trail.SetParent(transform);
            activeTrails.Add(owner, trail);
            allTrails.Add(trail);
        }

        AddPoint(trail.AddPoint(owner));
    }

    public void AddPowerUpLocation(Vector3 pos)
    {
        AddPoint(new PowerUpLocation(pos));
    }

    public Vector4 SearchVicinity(int radius, 
                                  ITrailOwner owner, 
                                  bool ignoreOwnerTrail = true, 
                                  bool skipOnPowerUp = true)
    {
        Vector3 pos = owner.Position;
        int xMin = Mathf.RoundToInt(pos.x) - radius;
        int zMin = Mathf.RoundToInt(pos.z) - radius;
        int xMax = Mathf.RoundToInt(pos.x) + radius;
        int zMax = Mathf.RoundToInt(pos.z) + radius;
        Vector3 centroid = Vector3.zero;
        float strength = 0f;
        int i = 0;
        for (int x = xMin; x <= xMax; x++)
        {
            for (int z = zMin; z <= zMax; z++)
            {
                int hash = Point.GetHash(x, z);
                if (pointLists.ContainsKey(hash))
                {
                    List<Point> list = pointLists[hash];
                    foreach (Point point in list)
                    {
                        if (ignoreOwnerTrail && point.Owner == owner)
                        {
                            continue;
                        }

                        if (Vector3.Distance(point.Position, pos) <= radius)
                        {
                            if (skipOnPowerUp && point is PowerUpLocation)
                            {
                                Vector4 resultPowerUp = point.Position;
                                resultPowerUp.w = 1f;
                                // Debug.DrawLine(pos, resultPowerUp, Color.white);
                                return resultPowerUp;
                            }
                            strength += point.Strength;
                            centroid += (point.Position - pos) * point.Strength;
                            i++;
                            // Debug.DrawLine(pos, point.Position, Color.magenta * point.Strength);
                        }
                    }
                }
            }
        }

        i = Mathf.Max(1, i);
        Vector4 result = pos + centroid / i;
        result.w = strength / i;
        // Debug.DrawLine(pos, result, Color.white);
        return result;
    }

    private void AddPoint(Point point)
    {
        List<Point> list;
        if (pointLists.ContainsKey(point.Hash))
        {
            list = pointLists[point.Hash];
        }
        else
        {
            list = new List<Point>();
            pointLists.Add(point.Hash, list);
        }
        list.Add(point);
    }

    private void RemovePoint(Point point)
    {
        List<Point> list = pointLists[point.Hash];
        list.Remove(point);

        if (list.Count == 0)
        {
            pointLists.Remove(point.Hash);
        }
    }
}
