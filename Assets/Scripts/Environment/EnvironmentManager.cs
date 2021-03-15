using UnityEngine;

namespace MBaske.RobotAnts
{
    public class EnvironmentManager : MonoBehaviour
    {
        [SerializeField]
        private int m_ResetInterval = 20000;
        private int m_StepCount;
        private Powerup[] m_Powerups;
        private TrailPool m_TrailPool;

        private void Awake()
        {
            m_Powerups = GetComponentsInChildren<Powerup>();
            m_TrailPool = FindObjectOfType<TrailPool>();
        }

        private void FixedUpdate()
        {
            if (++m_StepCount % m_ResetInterval == 0)
            {
                m_StepCount = 0;
                m_TrailPool.DiscardAll();

                for (int i = 0; i < m_Powerups.Length; i++)
                {
                    m_Powerups[i].RandomizePosition();
                }
            }
            else
            {
                m_TrailPool.ManagedUpdate(m_StepCount);
            }
        }
    }
}
