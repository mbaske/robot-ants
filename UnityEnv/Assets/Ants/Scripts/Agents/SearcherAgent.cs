using UnityEngine;
using MLAgents;
using System.Collections.Generic;

public class SearcherAgent : Agent
{
    [SerializeField]
    [Range(1, 25)]
    private int searchRadius = 10;
    [SerializeField]
    [Range(1, 1000)]
    private int numWaypoints = 500; // nominal
    private int nWaypoints;
    private Queue<Vector3> waypoints;
    private float distanceCovered;
    private float maxRewardDistanceCovered;
    private float prevAngle2D;
    private int decisionInterval;
    private WalkerAgent walker;
    private Ant ant;

    public override void InitializeAgent()
    {
        walker = GetComponent<WalkerAgent>();
        this.enabled = walker.EnableSearcher;

        if (enabled)
        {
            decisionInterval = agentParameters.numberOfActionsBetweenDecisions;
            walker.SetCallback(Done);
            nWaypoints = numWaypoints / decisionInterval;
            waypoints = new Queue<Vector3>();
            ant = GetComponent<Ant>();
        }
    }

    public override void AgentReset()
    {
        waypoints.Clear();
        distanceCovered = 0f;
        prevAngle2D = 0f;
    }

    public override void CollectObservations()
    {
        Vector4 searchResult = ant.SearchVicinity(searchRadius);
        Vector3 clusterDirection = ant.GetDirectionTowards(searchResult);
        float clusterStrength = searchResult.w;

        RewardDistanceCovered();
        RewardStrength(clusterStrength);

        // Divide velocities by 10 to get values in -1/+1 range.
        AddVectorObs(ant.GetRelativeDirection(ant.Velocity) / 10f); // 3
        AddVectorObs(ant.GetRelativeDirection(ant.AngularVelocity) / 10f); // 3
        AddVectorObs(ant.GetRelativeDirection(clusterDirection)); // 3
        AddVectorObs(clusterStrength); // 1 
        AddVectorObs(ant.Strength); // 1
        AddVectorObs(distanceCovered); // 1
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int step = GetStepCount() % decisionInterval + 1;
        float t = step / (float)decisionInterval;
        float angle2D = vectorAction[0] * 180f;

        walker.SetWalkAngle2D(Mathf.Lerp(prevAngle2D, angle2D, t));

        if (step == decisionInterval)
        {
            prevAngle2D = angle2D;
        }
    }

    private void RewardStrength(float clusterStrength)
    {
        // Need to balance rewards for strength vs distance...
        float r = (ant.Strength + clusterStrength) * maxRewardDistanceCovered * 0.25f;
        // Debug.Log(r);
        AddReward(r);
    }

    private void RewardDistanceCovered()
    {
        UpdateWaypoints();
        float r = distanceCovered / (float)numWaypoints;
        maxRewardDistanceCovered = Mathf.Max(r, maxRewardDistanceCovered);
        // Debug.Log(r);
        AddReward(r);
    }

    private void UpdateWaypoints()
    {
        Vector3 pos = ant.Position;
        waypoints.Enqueue(pos);
        if (waypoints.Count > nWaypoints)
        {
            waypoints.Dequeue();
        }

        Vector3 start = waypoints.Peek();
        distanceCovered = Vector3.Distance(start, pos);

        // Debug.DrawLine(start, pos, Color.cyan);
        // foreach (Vector3 p in waypoints)
        // {
        //     Debug.DrawLine(start, p, Color.blue);
        //     start = p;
        // }
    }
}
