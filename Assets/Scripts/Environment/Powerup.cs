using UnityEngine;
using MBaske.Sensors.Grid;

namespace MBaske.RobotAnts
{
    public class Powerup : DetectableGameObject2D
    {
        [SerializeField]
        private float m_RandomPositionRadius = 16;
        private Vector3 m_DefPos;

        private void Awake()
        {
            m_DefPos = transform.position;
            RandomizePosition();
        }

        public void RandomizePosition()
        {
            Vector2 p = Random.insideUnitCircle * m_RandomPositionRadius;
            transform.position = GroundPos.Under(m_DefPos + new Vector3(p.x, 0, p.y));
        }
    }
}
