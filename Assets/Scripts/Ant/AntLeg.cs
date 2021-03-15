using UnityEngine;

namespace MBaske.RobotAnts
{
    public class AntLeg : MonoBehaviour
    {
        private Transform m_UpperLeg;
        private Transform m_LowerLeg;

        [SerializeField]
        private float m_ExtentA = 15;
        [SerializeField]
        private float m_ExtentB = 5;
        [SerializeField]
        private float m_OffsetB = -3;

        private Quaternion m_UpperLegDefRot;
        private Vector3 m_AxisA = Vector3.up;
        private Vector3 m_AxisB;

        public void Initialize()
        {
            m_UpperLeg = transform;
            m_LowerLeg = transform.GetChild(1);

            m_UpperLegDefRot = m_UpperLeg.localRotation;
            Transform pivotA = m_UpperLeg.GetChild(0);
            Transform pivotB = m_LowerLeg.GetChild(0);
            Vector3 delta = pivotB.position - pivotA.position;
            m_AxisB = Vector3.Cross(delta, Vector3.up).normalized;
        }

        public void ManagedReset()
        {
            ApplyActions(0, 0);
        }

        public void ApplyActions(float actionA, float actionB)
        {
            m_UpperLeg.localRotation = m_UpperLegDefRot
                * Quaternion.AngleAxis(actionA * m_ExtentA, m_AxisA)
                * Quaternion.AngleAxis(actionB * m_ExtentB + m_OffsetB, m_AxisB);
            m_LowerLeg.localRotation = Quaternion.AngleAxis(actionB * m_ExtentB + m_OffsetB, m_AxisB);
        }
    }
}