using UnityEngine;

public class Leg : MonoBehaviour
{
    private ConfigurableJoint joint;
    private Transform foot;

    public void Initialize()
    {
        foot = transform.Find("Foot");
        Vector3 pivot = transform.Find("Pivot").localPosition;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.Lerp(rb.centerOfMass, pivot, 0.5f);
        rb.solverIterations = 36;

        joint = GetComponent<ConfigurableJoint>();
        joint.anchor = pivot;
    }

    public void SetTargetRotation(float y, float z = 0f)
    {
        joint.targetRotation = Quaternion.Euler(0f, y, z);
    }

    public Vector3 GetRotation()
    {
        Vector3 r = transform.localEulerAngles;
        r.x = Util.ClampAngle(r.x) / 180f;
        r.y = Util.ClampAngle(r.y) / 180f;
        r.z = Util.ClampAngle(r.z) / 180f;
        return r;
    }

    public float GetFootHeight()
    {
        RaycastHit hit;
        return Physics.Raycast(foot.position, Vector3.down, out hit, 1f, Layers.GROUND)
            ? foot.position.y - hit.point.y
            : -1f;
    }
}
