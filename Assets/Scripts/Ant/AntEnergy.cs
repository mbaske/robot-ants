using UnityEngine;
using MBaske.Sensors.Grid;
using System;

namespace MBaske.RobotAnts
{
    public class AntEnergy : DetectableGameObject2D
    {
        public event Action PowerupEvent;

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

            m_Renderer.GetPropertyBlock(m_MPB);
            m_MPB.SetColor("_Color", m_Gradient.Evaluate(m_Energy));
            m_Renderer.SetPropertyBlock(m_MPB);
        }

        public override void AddObservations()
        {
            Observations.Add(Energy, "Energy");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Powerup"))
            {
                PowerupEvent.Invoke();
            }
        }
    }
}