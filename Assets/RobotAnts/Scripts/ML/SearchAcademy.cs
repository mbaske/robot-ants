using MLAgents;
using UnityEngine;
using Unity.Collections;

public class SearchAcademy : Academy
{
    [Space]
    [SerializeField]
    private int resetInterval = 50000;
    [SerializeField]
    private Terrain terrain;
    [SerializeField]
    private PowerUps powerUps;
    private Searcher[] agents;
    private Vector3[] tmpPositions;

    [SerializeField]
    private float trailLifeTime = 45f;
    private float trailAttenuation;
    private float time;

    [SerializeField]
    private float trailSpacing = 1f;
    private float trailSpacingSqr;
    private DrawTrails drawTrails;
    private NativeList<TrailPoint> trailPoints;

    [SerializeField]
    private float trailDetectionRadius = 5f;
    [SerializeField]
    private float powerupDetectionRadius = 5f;

    private DetectTrails[] detectTrails;
    private NativeArray<float>[] detectionResults;


    public override void InitializeAcademy()
    {
        terrain.Initialize();
        powerUps.Initialize();

        agents = FindObjectsOfType<Searcher>();
        int n = agents.Length;
        tmpPositions = new Vector3[n];

        trailAttenuation = 1f / trailLifeTime;
        trailSpacingSqr = trailSpacing * trailSpacing;
        drawTrails = new DrawTrails();
        trailPoints = new NativeList<TrailPoint>(256 * n, Allocator.Persistent);

        detectTrails = new DetectTrails[n];
        detectionResults = new NativeArray<float>[n];
        for (int i = 0; i < n; i++)
        {
            detectTrails[i] = new DetectTrails();
            detectionResults[i] = new NativeArray<float>(3, Allocator.Persistent);
        }
    }

    public override void AcademyReset()
    {
        trailPoints.Clear();
        terrain.ClearTexture();
        powerUps.Randomize();
        time = Time.time;
    }

    public override void AcademyStep()
    {
        // Assuming agent decision interval = 5.
        int sc = GetStepCount();
        int step = sc % 5;

        switch (step)
        {
            case 0:
                CreateTrailPoints();
                UpdateTrailPoints(Time.time - time);
                time = Time.time;
                break;

            case 1:
                drawTrails.ScheduleJob(terrain, trailPoints);
                break;

            case 2:
                drawTrails.CompleteJob(terrain);

                for (int i = 0; i < agents.Length; i++)
                {
                    if (!HasPowerUpProximity(i))
                    {
                        detectTrails[i].ScheduleJob(
                            terrain, agents[i], trailDetectionRadius, detectionResults[i]);
                    }
                }
                break;

            case 4:
                for (int i = 0; i < agents.Length; i++)
                {
                    detectTrails[i].CompleteJob(agents[i]);
                }
                break;
        }

        if (sc % resetInterval == 0 && sc > 0)
        {
            AcademyReset();
        }
    }

    private void CreateTrailPoints()
    {
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].Energy > 0)
            {
                Vector3 p = agents[i].Position;
                if ((tmpPositions[i] - p).sqrMagnitude >= trailSpacingSqr)
                {
                    tmpPositions[i] = p;
                    trailPoints.Add(new TrailPoint(agents[i]));
                }
            }
        }
    }

    private void UpdateTrailPoints(float deltaTime)
    {
        for (int i = trailPoints.Length - 1; i >= 0; i--)
        {
            TrailPoint p = trailPoints[i];
            if (p.isObsolete)
            {
                trailPoints.RemoveAtSwapBack(i);
            }
            else
            {
                p.Attenuate(trailAttenuation * deltaTime);
                trailPoints[i] = p;
            }
        }
    }

    private bool HasPowerUpProximity(int i)
    {
        if (powerUps.HasProximity(agents[i], powerupDetectionRadius, out Vector3 delta))
        {
            detectionResults[i][0] = delta.x;
            detectionResults[i][1] = delta.z;
            detectionResults[i][2] = 1f; // max strength
            agents[i].OnDetectionResult(detectionResults[i]);

            if (delta.magnitude <= powerUps.ContactRadius)
            {
                agents[i].OnPowerUpContact();
            }
            return true;
        }
        return false;
    }

    protected override void OnDestroy()
    {
        drawTrails.Dispose(trailPoints);
        for (int i = 0; i < agents.Length; i++)
        {
            detectTrails[i].Dispose(detectionResults[i]);
        }
        base.OnDestroy();
    }
}
