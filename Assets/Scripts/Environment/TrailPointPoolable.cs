using UnityEngine;

namespace MBaske.RobotAnts
{
    public class TrailPointPoolable : Poolable
    {
        public float Energy() => m_Energy;

        private float m_Energy;

        [SerializeField]
        private Gradient m_Gradient;
        private Material m_Material;

        public void SetEnergy(float value)
        {
            m_Energy = value;
            m_Material ??= GetComponent<Renderer>().material;
            m_Material.SetColor("_Color", m_Gradient.Evaluate(m_Energy));
        }

        public void UpdateEnergy(float decrement)
        {
            m_Energy -= decrement;

            if (m_Energy > 0)
            {
                m_Material.SetColor("_Color", m_Gradient.Evaluate(m_Energy));
            }
            else
            {
                m_Energy = 0;
                Discard();
            }
        }
    }
}
