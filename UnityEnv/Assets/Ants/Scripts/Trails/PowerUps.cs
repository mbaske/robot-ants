using UnityEngine;

public class PowerUps : MonoBehaviour
{
    [SerializeField]
    private int num = 10;
    [SerializeField]
    private int minSpawnRadius = 40;
    [SerializeField]
    private int maxSpawnRadius = 80;
    [SerializeField]
    private float contactThreshold = 1f;
    [SerializeField]
    private Material material;

    private Sandbox sandbox;
    private TrailManager trailManager;
    private Collider[] colliders = new Collider[1];
    private float angle = -Mathf.PI;
    private bool active;

    // Called via Ant.Initialize, "Follow Trails" mode only.
    public void Initialize()
    {
        if (!active)
        {
            active = true;
            sandbox = transform.parent.GetComponentInChildren<Sandbox>();
            trailManager = transform.parent.GetComponentInChildren<TrailManager>();
        }
    }

    private void Update()
    {
        if (active && transform.childCount < num)
        {
            Vector3 pos = sandbox.GetPosition(
                angle, Random.Range(minSpawnRadius, maxSpawnRadius), 0.2f);
            angle += Mathf.PI / (float)num * 2f;
            trailManager.AddPowerUpLocation(pos);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.GetComponent<Renderer>().material = material;
            go.transform.parent = transform;
            go.transform.position = new Vector3(pos.x, pos.y - 0.4f, pos.z); 
            go.layer = gameObject.layer;
        }
    }

    public bool HasPowerUpAt(Vector3 pos)
    {
        return HasPowerUpAt(pos, contactThreshold);
    }

    public bool HasPowerUpAt(Vector3 pos, float radius)
    {
        return Physics.OverlapSphereNonAlloc(pos, radius, colliders, Layers.POWERUP) > 0;
    }
}
