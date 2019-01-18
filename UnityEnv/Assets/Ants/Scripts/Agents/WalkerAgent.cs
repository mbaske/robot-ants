using UnityEngine;
using MLAgents;

public class WalkerAgent : Agent
{
    public bool EnableSearcher => mode == Mode.FollowTrails;

    private enum Mode
    {
        LearnGait = 0,
        FollowTarget = 1,
        FollowTrails = 2
    }
    [Space]
    [SerializeField]
    private Mode mode = 0;
    
    private float targetSpawnRadius = 100f;
    private float targetContactThreshold = 2f;
    private Vector3 walkDirection; // global
    private float walkAngle2D; // relative 

    private Ant ant;
    private GaitOsc osc;
    private Sandbox sandbox;
    private Transform target;
    private Resetter defaults;
    private System.Action onReset;
    private float[] prevActions;
    private int decisionInterval;

    private bool hasTrail;
    private bool hasTarget;
    private bool hasGaitOsc;

    public override void InitializeAgent()
    {
        decisionInterval = agentParameters.numberOfActionsBetweenDecisions;

        hasTrail = mode == Mode.FollowTrails;
        hasTarget = mode == Mode.FollowTarget;
        hasGaitOsc = mode == Mode.LearnGait;
    }

    // Called by Trainer.Initialize after trainer & ant have been cloned.
    public void Initialize(Vector3 localPos)
    {
        Transform sb = transform.parent.Find("Sandbox");
        sandbox = sb.GetComponent<Sandbox>();
        transform.position = Util.GetGroundPos(sb.position + localPos, Vector3.up * 0.5f);
        transform.LookAt(sb.position);
        defaults = new Resetter(transform);

        if (hasGaitOsc)
        {
            osc = new GaitOsc();
        }

        if (hasTarget)
        {
            target = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            target.GetComponent<Renderer>().material.color = Color.red;
            target.parent = transform.parent;
        }

        ant = GetComponent<Ant>();
        ant.Initialize(hasTrail);

        ReSet();
    }

    public void SetWalkTarget(Vector3 pos)
    {
        walkDirection = ant.GetDirectionTowards(pos);
        SetWalkAngle2D(ant.DirectionToRelativeAngle2D(walkDirection));
    }

    public void SetWalkAngle2D(float angle2D)
    {
        walkAngle2D = angle2D;
    }

    public void SetCallback(System.Action callback)
    {
        onReset = callback;
    }

    public override void AgentReset()
    {
        if (defaults != null)
        {
            ReSet();
        }
    }

    public override void CollectObservations()
    {
        CheckState();

        if (hasTrail)
        {
            ant.UpdateTrail();   
        }

        if (hasTarget)
        {
            SetWalkTarget(target.position);
            RewardMovingTowardsTarget();
            RewardFacingTarget();
        }

        AddVectorObs(ant.GetLegRotations()); // 36
        AddVectorObs(ant.GetFootHeights()); // 6
        // Divide velocities by 10 to get values in -1/+1 range.
        AddVectorObs(ant.GetRelativeDirection(ant.Velocity) / 10f); // 3
        AddVectorObs(ant.GetRelativeDirection(ant.AngularVelocity) / 10f); // 3
        AddVectorObs(walkAngle2D / 180f); // 1
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (hasGaitOsc)
        {
            // Optional preliminary training with hardcoded tripod gait.
            // Assuming stepInterval = 1. 
            // Movement speed/detail is controlled via Oscillator.freq.
            float[] rot = osc.GetRotations().ToArray();
            ant.SetLegTargetRotations(rot);
            // Train model to mimick the oscillator by setting the cumulative error as step 
            // penalty. Need to be careful not to overfit the model at this point, which  
            // would hamper subsequent training without oscillator.
            PenalizeActionDelta(rot, vectorAction);
        }
        else if (decisionInterval == 1)
        {
            ant.SetLegTargetRotations(vectorAction);
        }
        else
        {
            // decisionInterval > 1 results in identical vectorActions passed n times.
            // Interpolate vectorAction values between decision steps for smoother animation.
            int step = GetStepCount() % decisionInterval + 1;
            float t = step / (float)decisionInterval;
            float[] rot = new float[24];
            for (int i = 0; i < 24; i++)
            {
                rot[i] = Mathf.Lerp(prevActions[i], vectorAction[i], t);
            }
            if (step == decisionInterval)
            {
                System.Array.Copy(vectorAction, prevActions, 24);
            }
            ant.SetLegTargetRotations(rot);
        }
    }

    private void CheckState()
    {
        if (hasTarget && Vector3.Distance(ant.Position, target.position) < targetContactThreshold)
        {
            RandomizeTarget();
        }
        else if (sandbox.IsOutOfBounds(ant.Position))
        {
            ReSet();
        }
        else if (ant.Tilt < 0f)
        {
            // Ant fell over.
            AddReward(-1f);
            ReSet();
        }
    }

    private void ReSet()
    {
        defaults.Reset();
        walkDirection = Vector3.zero;
        walkAngle2D = 0f;
        prevActions = new float[24];

        if (hasTarget)
        {
            RandomizeTarget();
        }

        if (hasTrail)
        {
            ant.ResetTrail();
            onReset();
        }
    }

    private void RandomizeTarget()
    {
        target.position = sandbox.GetPosition(Random.value * targetSpawnRadius, 1f);
        target.name = transform.name + " Target";
    }

    private void RewardMovingTowardsTarget()
    {
        float r = Vector3.Dot(ant.Velocity, walkDirection) * 0.1f;
        // Debug.Log(r);
        AddReward(r);
    }

    private void RewardFacingTarget()
    {
        float r = Vector3.Dot(ant.Forward, walkDirection);
        r = Mathf.Pow(Mathf.Abs(r), 4f) * Mathf.Sign(r) * 0.75f; // focus
        // Debug.Log(r);
        AddReward(r);
    }

    private void PenalizeActionDelta(float[] oscRot, float[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            AddReward(-Mathf.Abs(oscRot[i] - actions[i]));
        }
    }
}
