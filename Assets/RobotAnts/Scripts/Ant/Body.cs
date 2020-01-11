using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    public Vector3 VelocityXZ => Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
    public Vector3 ForwardXZ => Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
    public RaycastHit RaycastHit { get; private set; }

    [SerializeField]
    private Leg[] legs;
    [SerializeField]
    private Foot[] feet;

    private Rigidbody rb;
    private List<float> obs;
    private Vector3 offset;
    private readonly int lmGround = 1 << Layers.GROUND; 

    public void Initialize(Vector3 offset)
    {
        this.offset = offset;
        CastDownRay();

        rb = GetComponent<Rigidbody>();
        obs = new List<float>(34);
        for (int i = 0; i < 6; i++)
        {
            legs[i].Initialize();
            feet[i].Initialize(transform, 64); 
        }
    }

    public void StepUpdate(float[] va)
    {
        for (int i = 0, j = 0; i < 6; i++)
        {
            legs[i].StepUpdate(va[j++], va[j++], va[j++], va[j++], va[j++], va[j++]);
            feet[i].StepUpdate();
        }
    }

    public List<float> GetWalkerObs()
    {
        CastDownRay();

        obs.Clear();
        // Inclination
        obs.Add(transform.right.y);
        obs.Add(transform.up.y);
        obs.Add(transform.forward.y);

        obs.Add(GetNormalizedGroundDistance());
        AddLocalized(rb.velocity);
        AddLocalized(rb.angularVelocity);

        for (int i = 0; i < 6; i++)
        {
            AddLocalized(feet[i].Velocity);
            obs.Add(feet[i].GetNormalizedGroundDistance());
        }
        return obs;
    }

    private void AddLocalized(Vector3 v)
    {
        v = AgentUtil.Sigmoid(transform.InverseTransformVector(v));
        obs.Add(v.x);
        obs.Add(v.y);
        obs.Add(v.z);
    }

    private float GetNormalizedGroundDistance()
    {
        return Mathf.Min(2f, RaycastHit.distance) - 1f;
    }

    private void CastDownRay()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10, lmGround))
        {
            RaycastHit = hit;
        }
    }
}
