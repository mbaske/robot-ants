using UnityEngine;

namespace MBaske.RobotAnts
{
    public class AntFoot : MonoBehaviour
    {
        public Vector3 Velocity => m_Rigidbody.velocity;
        public bool IsGrounded { get; private set; }

        [SerializeField]
        private Transform m_Target;
        private Transform m_AntBody;

        private Rigidbody m_Rigidbody;
        private ConfigurableJoint m_Joint;

        public void Initialize(Transform antBody)
        {
            m_AntBody = antBody;
            m_Joint = GetComponent<ConfigurableJoint>();
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public void ManagedReset()
        {
            IsGrounded = false;
            m_Rigidbody.position = m_Target.position;
            ManagedUpdate();
        }

        public void ManagedUpdate()
        {
            // Keep rigidbody at target transform position.
            m_Rigidbody.centerOfMass = transform.InverseTransformPoint(m_AntBody.position);
            m_Joint.targetPosition = Quaternion.Inverse(m_AntBody.rotation)
                * (m_AntBody.position - m_Target.position);
        }

        private void OnCollisionEnter(Collision collision)
        {
            IsGrounded = true;
        }

        private void OnCollisionStay(Collision collision)
        {
            IsGrounded = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            IsGrounded = false;
        }
    }
}