using UnityEngine;

public class Trainer : MonoBehaviour
{
    // Trainer clones itself.
    private static bool flagRunOnce;

    [SerializeField]
    private Vector2Int trainerInstances = Vector2Int.one;
    [SerializeField]
    private int trainerSpacing = 260;
    [Space]
    [SerializeField]
    private int agentInstances = 1;
    [SerializeField]
    private int minSpawnRadius = 50;
    [SerializeField]
    private int maxSpawnRadius = 110;
    [Space]
    [SerializeField]
    private float sandboxHeightRange = 15; // +/-

    private bool clone;
    private int counter;
    private float heightIncr;

    private void Start()
    {
        if (!flagRunOnce)
        {
            flagRunOnce = true;
            Ant[] tmp = new Ant[agentInstances];
            int num = trainerInstances.x * trainerInstances.y;
            clone = num > 1;
            if (clone)
            {
                heightIncr = (sandboxHeightRange * 2f) / (float)(num - 1);
                transform.name = "Trainer 0x0";
                for (int x = 0; x < trainerInstances.x; x++)
                {
                    for (int z = 0; z < trainerInstances.y; z++)
                    {
                        if (x + z > 0)
                        {
                            Vector3 pos = new Vector3(x * trainerSpacing, 0, z * trainerSpacing);
                            GameObject trainer = Instantiate(gameObject, pos, Quaternion.identity);
                            trainer.transform.name = "Trainer " + x + "x" + z;
                            Initialize(trainer.transform, tmp);
                        }
                    }
                }
            }
            Initialize(transform, tmp);
            FindObjectOfType<CamTarget>()?.Initialize(tmp);
        }
    }

    private void Initialize(Transform trainer, Ant[] tmp)
    {
        trainer.GetComponentInChildren<Sandbox>().Initialize(
            clone, -sandboxHeightRange + heightIncr * (float)counter++);

        float angle = 0f;
        float incr = Mathf.PI * 2f / (float)agentInstances;
        // Clone ant present in scene.
        Ant ant = trainer.GetComponentInChildren<Ant>();
        for (int i = 0; i < agentInstances; i++)
        {
            ant = i > 0 ? Instantiate(ant.gameObject, trainer).GetComponent<Ant>() : ant;
            ant.name = "Ant " + i;
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
            ant.GetComponent<WalkerAgent>().Initialize(new Vector3(
                radius * Mathf.Cos(angle),  0f, radius * Mathf.Sin(angle)));
            angle += incr;
            tmp[i] = ant;
        }
    }
}