using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace MBaske.RobotAnts
{
    public class WalkerAgentTrain : WalkerAgent
    {
        [SerializeField]
        [Tooltip("Multiplied with velocity towards target.")]
        private float m_SpeedRewardFactor = 0.1f;
        [SerializeField]
        [Tooltip("Multiplied with -abs(Normalized Target Angle), agent must face target.")]
        private float m_TargetPenaltyFactor = 0.1f;
        private float m_Time;

        private StatsRecorder m_Stats;
        [SerializeField]
        [Tooltip("Step interval for writing stats to Tensorboard.")]
        private int m_StatsInterval = 100;

        [SerializeField]
        private float m_TargetRadius = 64;
        [SerializeField]
        private Transform m_Target;
        private Vector3 m_TargetPos;
        private Vector3 m_TargetDirXZ;
        private Vector3 m_DefPos;

        public override void Initialize()
        {
            m_DefPos = transform.position;
            m_Stats = Academy.Instance.StatsRecorder;
            base.Initialize();
        }

        public override void OnEpisodeBegin()
        {
            m_Time = Time.time;
            RandomizeTarget();
            base.OnEpisodeBegin();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            ManagedUpdate();
            base.CollectObservations(sensor);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            base.OnActionReceived(actionBuffers);

            ManagedUpdate();

            float speed = Vector3.Dot(m_TargetDirXZ, m_AntPhysics.Body.Velocity);
            AddReward(speed * m_SpeedRewardFactor);
            float error = Mathf.Abs(NormalizedTargetAngle);
            AddReward(error * -m_TargetPenaltyFactor);

            if (StepCount % m_StatsInterval == 0)
            {
                m_Stats.Add("Walker/Speed", speed);
                m_Stats.Add("Walker/Error", error);
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var actions = actionsOut.ContinuousActions.Array;
            var ctrl = m_AntPhysics.GaitCtrl;
            ctrl.left = 1 - Mathf.Clamp01(-NormalizedTargetAngle) * 2;
            ctrl.right = 1 - Mathf.Clamp01(NormalizedTargetAngle) * 2;
            ctrl.Heuristic(actions, Time.time - m_Time);
            m_Time = Time.time;
        }

        private void ManagedUpdate()
        {
            var body = m_AntPhysics.Body;
            Vector3 delta = m_TargetPos - body.Position;
            m_TargetDirXZ = Vector3.ProjectOnPlane(delta, Vector3.up).normalized;
            NormalizedTargetAngle = Vector3.SignedAngle(body.ForwardXZ, m_TargetDirXZ, Vector3.up) / 180f;

            if (delta.sqrMagnitude < 4)
            {
                RandomizeTarget();
            }
        }
            
        private void RandomizeTarget()
        {
            Vector2 p = Random.insideUnitCircle * m_TargetRadius;
            m_TargetPos = GroundPos.Under(m_DefPos + new Vector3(p.x, 0, p.y));
            m_Target.position = m_TargetPos;
        }
    }
}