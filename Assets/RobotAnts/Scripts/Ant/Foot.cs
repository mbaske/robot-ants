using UnityEngine;

public class Foot : MonoBehaviour
{
    public Vector3 Velocity => rb.velocity;

    [SerializeField]
    private Transform snap;
    private Transform body;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    private RaycastHit hit;
    private float distance;
    private readonly int lmGround = 1 << Layers.GROUND; 
    private const float thresh = 0.08f;

    public void Initialize(Transform body, int solverIterations)
    {
        this.body = body;
        rb = GetComponent<Rigidbody>();
        rb.solverIterations = solverIterations;
        joint = GetComponent<ConfigurableJoint>();
        CastDownRay();
    }

    public void StepUpdate()
    {
        joint.targetPosition = Quaternion.Inverse(body.rotation) * (body.position - snap.position);

        CastDownRay();
        // Apply up force in case foot slips through ground mesh collider.
        // Otherwise down force for stabilizing overall ant posture.
        rb.AddForce(Vector3.up * (distance < thresh ? 2f: -1f), ForceMode.VelocityChange);
    }

    public float GetNormalizedGroundDistance()
    {
        return Mathf.Min(2f, distance) - 1f;
    }

    private void CastDownRay()
    {
        distance = Physics.Raycast(transform.position, Vector3.down, out hit, 20, lmGround) 
                 ? hit.distance : 0;
    }
}
