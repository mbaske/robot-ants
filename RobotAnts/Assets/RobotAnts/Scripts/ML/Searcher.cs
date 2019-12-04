using UnityEngine;
using MLAgents;
using Unity.Collections;

public class Searcher : Agent
{
    public Vector3 Position => body.transform.position;
    public Vector2 UVCoord => body.RaycastHit.textureCoord;
    public float Energy { get; private set; } // -1/+1

    protected float trailDirection;
    protected float trailStrength;

    [SerializeField]
    private float obstacleDetectionRadius = 3f;
    protected float cumlObstacleProximity;
    private int lmObstacles;

    [SerializeField]
    private float energyDepletionTime = 20f;
    private float energyAttenuation;
    private float time;

    [SerializeField]
    protected Walker walker;

    [SerializeField]
    protected Body body;
    [SerializeField]
    private Renderer energyRenderer;
    private Material energyMaterial;

    public override void InitializeAgent()
    {
        lmObstacles = (1 << Layers.BODY) | (1 << Layers.WALL);
        energyAttenuation = 1f / energyDepletionTime;

        energyMaterial = Instantiate(energyRenderer.material);
        energyRenderer.material = energyMaterial;
    }

    public override void AgentReset()
    {
        walker.AgentReset();

        Energy = 0;
        trailDirection = 0;
        trailStrength = 0;
        time = Time.time;
    }

    public void OnDetectionResult(NativeArray<float> result)
    {
        Vector3 trailDelta = new Vector3(result[0], 0, result[1]);
        trailDirection = Vector3.SignedAngle(body.HeadingXZ, trailDelta, Vector3.up) / 180f;
        trailStrength = result[2];
        Debug.DrawRay(Position, trailDelta, Color.Lerp(Color.blue, Color.cyan, trailStrength));
    }

    public void OnPowerUpContact()
    {
        Energy = 1f;
    }

    public override void CollectObservations()
    {
        UpdateEnergy(Time.time - time);
        time = Time.time;

        DetectObstacles(); // 8

        AddVectorObs(Energy);
        AddVectorObs(trailDirection);
        AddVectorObs(trailStrength > 0 ? trailStrength : -1);
        AddVectorObs(walker.WalkDirection);
    }

    public override void AgentAction(float[] vectorAction)
    {
        walker.WalkDirection = vectorAction[0];
    }

    private void UpdateEnergy(float deltaTime)
    {
        Energy = Mathf.Max(-1f, Energy - energyAttenuation * deltaTime);
        energyMaterial.color = Color.Lerp(Color.red, Color.green, Energy);
    }

    private void DetectObstacles()
    {
        cumlObstacleProximity = 0;
        Vector3 pos = body.RaycastHit.point + Vector3.up * 0.5f;
        Vector3 heading = body.HeadingXZ;
        for (int angle = 0; angle < 360; angle += 45)
        {
            Vector3 dir = Quaternion.Euler(0, angle, 0) * heading;
            if (Physics.SphereCast(pos, 1f, dir, out RaycastHit hit, obstacleDetectionRadius, lmObstacles))
            {
                // Debug.DrawLine(pos, hit.point, Color.red);
                float d = hit.distance / obstacleDetectionRadius;
                AddVectorObs(d * 2f - 1f);
                cumlObstacleProximity += (1f - d);
            }
            else
            {
                // Debug.DrawRay(pos, dir * obstacleDetectionRadius, Color.green);
                AddVectorObs(1f);
            }
        }
    }
}
