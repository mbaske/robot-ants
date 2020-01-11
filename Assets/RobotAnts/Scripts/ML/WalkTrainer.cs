using UnityEngine;

public class WalkTrainer : Walker
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float radius;

    public override void AgentReset()
    {
        base.AgentReset();
        RandomizeTarget();
    }

    public override void CollectObservations()
    {
        Vector3 delta = target.position - body.transform.position;
        Vector3 dirXZ = Vector3.ProjectOnPlane(delta, Vector3.up).normalized;
        WalkDirection = Vector3.SignedAngle(dirXZ, body.ForwardXZ, Vector3.up) / 180f;

        base.CollectObservations();

        // Penalize facing away from target.
        AddReward(-Mathf.Abs(WalkDirection));

        // Reward moving towards target.
        float speed = Vector3.Dot(body.VelocityXZ, dirXZ);
        AddReward(speed * 0.1f);

        if (delta.sqrMagnitude < 25)
        {
            RandomizeTarget();
        }
    }

    private void RandomizeTarget()
    {
        Vector3 pos = transform.position + Random.insideUnitSphere * radius;
        pos.y = 10;
        Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 20, 1 << Layers.GROUND);
        target.position = hit.point;
    }
}
