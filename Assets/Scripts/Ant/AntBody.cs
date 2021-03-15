using UnityEngine;

namespace MBaske.RobotAnts
{
    public class AntBody : MonoBehaviour
    {
        public Vector3 Position => transform.position;
        public Vector3 Velocity => m_Rigidbody.velocity;
        public Vector3 AngularVelocity => m_Rigidbody.angularVelocity;
        public Vector3 Inclination => new Vector3(transform.right.y, transform.up.y, transform.forward.y);
        public Vector3 ForwardXZ => Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        private Rigidbody m_Rigidbody;

        public void Initialize()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public Vector3 Localize(Vector3 v)
        {
            return transform.InverseTransformDirection(v);
        }

        public void ToggleFreeze(bool freeze)
        {
            m_Rigidbody.freezeRotation = freeze;
        }
    }
}