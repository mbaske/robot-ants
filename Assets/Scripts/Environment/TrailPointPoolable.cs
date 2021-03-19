using UnityEngine;

namespace MBaske.RobotAnts
{
    public class TrailPointPoolable : Poolable
    {
        public float Energy() => m_Energy;

        private float m_Energy;

        [SerializeField]
        private Gradient m_Gradient;
        private Renderer m_Renderer;
        private MaterialPropertyBlock m_MPB;

        private void Awake()
        {
            m_MPB = new MaterialPropertyBlock();
            m_Renderer = GetComponentInChildren<Renderer>();
        }

        public void SetEnergy(float value)
        {
            m_Energy = value;
            UpdateColor();
        }

        public void UpdateEnergy(float decrement)
        {
            m_Energy -= decrement;

            if (m_Energy > 0)
            {
                UpdateColor();
            }
            else
            {
                m_Energy = 0;
                Discard();
            }
        }

        private void UpdateColor()
        {
            m_Renderer.GetPropertyBlock(m_MPB);
            m_MPB.SetColor("_Color", m_Gradient.Evaluate(m_Energy));
            m_Renderer.SetPropertyBlock(m_MPB);
        }
    }
}
