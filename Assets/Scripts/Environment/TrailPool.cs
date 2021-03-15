using UnityEngine;

namespace MBaske.RobotAnts
{
    public class TrailPool : Pool<TrailPointPoolable>
    {
        [SerializeField, Range(1, 50)]
        private int m_UpdateInterval = 40;

        [SerializeField, Range(0f, 1f)]
        private float m_EnergyDecrement = 0.01f;

        public void ManagedUpdate(int stepCount)
        {
            if (stepCount % m_UpdateInterval == 0)
            {
                for (int i = m_Active[0].Count - 1; i >= 0; i--)
                {
                    m_Active[0][i].UpdateEnergy(m_EnergyDecrement);
                }
            }
        }
    }
}