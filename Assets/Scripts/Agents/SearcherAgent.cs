using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

namespace MBaske.RobotAnts
{
    public class SearcherAgent : Agent
    {
        [SerializeField]
        [Tooltip("Distance between trail points.")]
        protected float m_TrailSpacing = 1;
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Decrement per decision step.")]
        protected float m_EnergyDecrement = 0.01f;
        protected float m_Energy;
        protected float m_PrevAction;
        protected int m_DecisionInterval;

        protected AntEnergy m_AntEnergy;
        protected WalkerAgent m_WalkerAgent;
        protected TrailPool m_TrailManager;

        public override void Initialize()
        {
            m_DecisionInterval = GetComponent<DecisionRequester>().DecisionPeriod;
            m_TrailManager = FindObjectOfType<TrailPool>();
            m_WalkerAgent = GetComponentInChildren<WalkerAgent>();
            m_WalkerAgent.TrailSpacing = m_TrailSpacing;
            m_WalkerAgent.TrailStepEvent += OnTrailStep;
            m_WalkerAgent.EndEpisodeEvent += ResetAgent;
            m_AntEnergy = GetComponentInChildren<AntEnergy>();
            m_AntEnergy.PowerupEvent += OnPowerup;
        }

        public override void OnEpisodeBegin()
        {
            ResetAgent();
            m_WalkerAgent.OnEpisodeBegin();
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            float action = actionBuffers.ContinuousActions[0];
            int step = StepCount % m_DecisionInterval;

            if (step == 0)
            {
                // Pre-decision step: apply and store current action.
                m_PrevAction = action;

                m_Energy = Mathf.Clamp01(m_Energy - m_EnergyDecrement);
                m_AntEnergy.SetEnergy(m_Energy);
            }
            else
            {
                // Interpolate from previous to current action.
                float t = step / (float)m_DecisionInterval;
                action = Mathf.Lerp(m_PrevAction, action, t);
            }

            m_WalkerAgent.NormalizedTargetAngle = action;
        }

        public override void Heuristic(in ActionBuffers actionsOut) { }


        protected virtual void ResetAgent()
        {
            m_Energy = 0;
            m_AntEnergy.SetEnergy(m_Energy);
            m_PrevAction = 0;
        }

        protected virtual void OnTrailStep(Vector3 pos)
        {
            if (m_Energy > 0)
            {
                var point = m_TrailManager.Spawn(GroundPos.Under(pos));
                point.SetEnergy(m_Energy);
            }
        }

        protected void OnPowerup()
        {
            m_Energy = 1;
            m_AntEnergy.SetEnergy(m_Energy);
        }

        private void OnDestroy()
        {
            m_WalkerAgent.TrailStepEvent -= OnTrailStep;
            m_WalkerAgent.EndEpisodeEvent -= ResetAgent;
            m_AntEnergy.PowerupEvent -= OnPowerup;
        }
    }
}