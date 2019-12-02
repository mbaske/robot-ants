using UnityEngine;

public class PowerUps : MonoBehaviour
{
    public float ContactRadius { get; private set; }

    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private int amount = 10;
    [SerializeField]
    private float spawnRadius = 100;

    private int lmGround;
    private int lmPowerUp;
    private Collider[] colliders;

    public void Initialize()
    {
        lmGround = 1 << Layers.GROUND;
        lmPowerUp = 1 << Layers.POWERUP;
        colliders = new Collider[1];
        ContactRadius = prefab.transform.localScale.x * 0.5f;
        for (int i = 0; i < amount; i++)
        {
            Instantiate(prefab).transform.parent = transform;
        }
    }

    public void Randomize()
    {
        float arc = Mathf.PI * 2f / (float)amount;
        float mod = Random.Range(2f, 10f);
        for (int i = 0; i < amount; i++)
        {
            float t = i * arc;
            float r = (0.6f + Mathf.Sin(t * mod) * 0.4f) * spawnRadius;
            Vector3 pos = new Vector3(r * Mathf.Cos(t), 10, r * Mathf.Sin(t));
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 20, lmGround))
            {
                transform.GetChild(i).transform.position = hit.point;
            }
        }
    }

    public bool HasProximity(Searcher agent, float radius, out Vector3 delta)
    {
        if (Physics.OverlapSphereNonAlloc(agent.Position, radius, colliders, lmPowerUp) > 0)
        {
            delta = colliders[0].transform.position - agent.Position;
            return true;
        }
        delta = Vector3.zero;
        return false;
    }
}
