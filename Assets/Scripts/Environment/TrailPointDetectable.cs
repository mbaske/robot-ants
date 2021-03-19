using UnityEngine;
using MBaske.Sensors.Grid;

namespace MBaske.RobotAnts
{
    [RequireComponent(typeof(TrailPointPoolable))]
    public class TrailPointDetectable : DetectableGameObject2D
    {
        public override void AddObservations()
        {
            Observations.Add(GetComponent<TrailPointPoolable>().Energy, "Energy");
        }
    }
}