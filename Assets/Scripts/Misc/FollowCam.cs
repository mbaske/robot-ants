using UnityEngine;

namespace MBaske.RobotAnts
{
    public class FollowCam : MonoBehaviour
    {
        private AntBody[] m_Targets;
        private Vector3 m_LookPos;
        private int m_Index;

        [Header("Space: switch target")]

        [SerializeField]
        private float m_Distance = 5;
        [SerializeField]
        private Vector2 m_CamOffset = Vector2.up;
        [SerializeField]
        private Vector2 m_LookOffset;

        [SerializeField]
        private float m_MoveDamp = 0.25f;
        private Vector3 m_MoveVelo;
        [SerializeField]
        private float m_LookDamp = 0.25f;
        private Vector3 m_LookVelo;

        private void Awake()
        {
            m_Targets = FindObjectsOfType<AntBody>();
        }

        private void LateUpdate()
        {
            var camGroundPos = GroundPos.Under(transform.position);
            var targetGroundPos = GroundPos.Under(m_Targets[m_Index].Position);
            var perp = Vector3.Cross(targetGroundPos - camGroundPos, Vector3.up).normalized;

            var direction = (camGroundPos - targetGroundPos).normalized;
            var newCamPos = targetGroundPos
                + direction * m_Distance
                + perp * m_CamOffset.x 
                + Vector3.up * m_CamOffset.y;

            transform.position = Vector3.SmoothDamp(transform.position, newCamPos, ref m_MoveVelo, m_MoveDamp);

            var newLookPos = targetGroundPos 
                + perp * m_LookOffset.x 
                + Vector3.up * m_LookOffset.y;

            m_LookPos = Vector3.SmoothDamp(m_LookPos, newLookPos, ref m_LookVelo, m_LookDamp);
            transform.LookAt(m_LookPos);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Index = (m_Index + 1) % m_Targets.Length;
            }
        }
    }
}