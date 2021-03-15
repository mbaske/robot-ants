using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

namespace MBaske.RobotAnts
{
    public class SearcherAgentTrain : SearcherAgent
    {
        [SerializeField]
        [Tooltip("Multiplied with energy.")]
        private float m_EnergyRewardFactor = 0.5f;

        [SerializeField]
        [Tooltip("Max. reward 1 for distance travelled\n= Num Waypoints x Trail Spacing.")]
        private int m_NumWaypoints = 25;
        private float m_MaxDistanceSqr;
        private float m_DistanceSqrRatio;
        private Queue<Vector3> m_Waypoints;

        private StatsRecorder m_Stats;
        [SerializeField]
        [Tooltip("Step interval for writing stats to Tensorboard.")]
        private int m_StatsInterval = 100;

        public override void Initialize()
        {
            base.Initialize();
            m_MaxDistanceSqr = m_NumWaypoints * m_TrailSpacing;
            m_MaxDistanceSqr *= m_MaxDistanceSqr;
            m_Waypoints = new Queue<Vector3>(m_NumWaypoints);
            m_Stats = Academy.Instance.StatsRecorder;
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            base.OnActionReceived(actionBuffers);

            if (StepCount % m_StatsInterval == 0)
            {
                m_Stats.Add("Searcher/Energy", m_Energy);
                m_Stats.Add("Searcher/Distance", m_DistanceSqrRatio);
            }
        }


        protected override void ResetAgent()
        {
            base.ResetAgent();
            m_Waypoints.Clear();
            m_DistanceSqrRatio = 0;
        }

        protected override void OnTrailStep(Vector3 pos)
        {
            base.OnTrailStep(pos);

            m_Waypoints.Enqueue(pos);
            m_DistanceSqrRatio = (pos - m_Waypoints.Peek()).sqrMagnitude / m_MaxDistanceSqr;
            AddReward(m_DistanceSqrRatio);
            AddReward(m_Energy * m_EnergyRewardFactor);

            if (m_Waypoints.Count == m_NumWaypoints)
            {
                m_Waypoints.Dequeue();
            }
        }
    }
}