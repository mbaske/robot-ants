using UnityEngine;
using MLAgents;

public class Walker : Agent
{
    [HideInInspector]
    public float WalkDirection; // -1/+1

    [SerializeField]
    protected Body body;

    private int interval;
    private float[] actionsLerp;
    private float[] actionsBuffer;
    private const int nActions = 36;
    private Resetter resetter;

    public override void InitializeAgent()
    {
        interval = agentParameters.numberOfActionsBetweenDecisions;
        actionsLerp = new float[nActions];
        actionsBuffer = new float[nActions];
        Transform container = transform.parent;
        container.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
        resetter = new Resetter(container);
        body.Initialize(container.position);
    }

    public override void AgentReset()
    {
        WalkDirection = 0;
        System.Array.Clear(actionsLerp, 0, nActions);
        System.Array.Clear(actionsBuffer, 0, nActions);
        resetter.Reset();
    }

    public override void CollectObservations()
    {
        AddVectorObs(actionsBuffer); // 36
        AddVectorObs(body.GetWalkerObs()); // 34
        AddVectorObs(WalkDirection); // 1
    }

    public override void AgentAction(float[] vectorAction)
    {
        // Interpolate action values between decision steps for smoother movement.
        int step = GetStepCount() % interval + 1;
        float t = step / (float)interval;
        for (int i = 0; i < nActions; i++)
        {
            actionsLerp[i] = Mathf.Lerp(actionsBuffer[i], vectorAction[i], t);
        }
        if (step == interval)
        {
            System.Array.Copy(vectorAction, actionsBuffer, nActions);
        }
        body.StepUpdate(actionsLerp);
    }
}
