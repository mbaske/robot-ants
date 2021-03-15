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
        private Material m_Material;

        private void Awake()
        {
            m_Material = GetComponentInChildren<Renderer>().material;
        }

        public void SetEnergy(float value)
        {
            m_Energy = value;
            m_Material.SetColor("_Color", m_Gradient.Evaluate(m_Energy));
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