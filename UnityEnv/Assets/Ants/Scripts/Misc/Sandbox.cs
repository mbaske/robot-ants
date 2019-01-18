using UnityEngine;

public class Sandbox : MonoBehaviour
{
    [SerializeField]
    private float noiseXZScale = 3f;
    [SerializeField]
    private Vector2 noiseXZOffset = Vector2.one * 5f;
    [SerializeField]
    private float noiseYScale = -15f;
    [SerializeField]
    [Range(0.1f, 3f)]
    private float slope = 0.5f;

    [InspectorButton("OnButtonClicked")]
    [SerializeField]
    private bool update;

    private float radiusBounds = 124f;
    private float radiusCutOffMesh = 127f;

    private void OnButtonClicked()
    {
        Initialize(false, noiseYScale);
    }

    // Called via Trainer.Initialize.
    public void Initialize(bool clone, float height = 0f)
    {
        noiseYScale = height;
        UpdateMesh(clone);
    }

    public Vector3 GetPosition(float angle, float radius, float yOffset)
    {
        Vector3 pos = new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
        return Util.GetGroundPos(transform.position + pos, Vector3.up * yOffset);
    }

    public Vector3 GetPosition(float radius, float yOffset)
    {
        return GetPosition(Random.Range(-Mathf.PI, Mathf.PI), radius, yOffset);
    }

    public bool IsOutOfBounds(Vector3 pos)
    {
        return Vector3.Distance(pos, transform.position) > radiusBounds;
    }

    private void UpdateMesh(bool clone)
    {
        Transform ground = transform.Find("Ground");
        Mesh mesh = ground.GetComponent<MeshFilter>().sharedMesh;
        if (clone)
        {
            mesh = (Mesh)Instantiate(mesh);
            mesh.name = "Height " + noiseYScale;
        }
        float r = mesh.bounds.size.x / 2f;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float d = Vector3.Distance(Vector3.zero, vertices[i]);
            if (d > radiusCutOffMesh)
            {
                vertices[i] = Vector3.Lerp(Vector3.zero, vertices[i], radiusCutOffMesh / d);
            }
            else
            {
                d = Mathf.Pow(1f - d / r, slope);
                float y = Mathf.PerlinNoise(
                    noiseXZOffset.x + vertices[i].x / r * noiseXZScale,
                    noiseXZOffset.y + vertices[i].z / r * noiseXZScale
                );
                vertices[i].y = y * noiseYScale * d;
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        ground.GetComponent<MeshCollider>().sharedMesh = mesh;
        ground.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
