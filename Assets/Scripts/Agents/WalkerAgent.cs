using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

namespace MBaske.RobotAnts
{
    public class WalkerAgent : Agent
    {
        [Tooltip("Set by SearcherAgent agent or WalkerAgentTrain.")]
        public float NormalizedTargetAngle;

        public event Action EndEpisodeEvent;
        public event Action<Vector3> TrailStepEvent;

        public float TrailSpacing
        {
            set 
            {
                m_LeavesTrail = true;
                m_TrailSpacingSqr = value * value;
            }
        }
        protected float m_TrailSpacingSqr;
        protected bool m_LeavesTrail;
        protected Vector3 m_TmpPos;

        protected AntPhysics m_AntPhysics;
        protected GroundDetector[] m_GroundDetectors;
        protected Vector3 m_Inclination;
        protected int m_DecisionInterval;
        protected float[] m_PrevActions;

        protected const int c_NumLegs = 6;
        protected const int c_NumActions = 12;

        public override void Initialize()
        {
            transform.position = GroundPos.Under(transform.position) + Vector3.up;

            m_PrevActions = new float[c_NumActions];
            m_DecisionInterval = GetComponent<DecisionRequester>().DecisionPeriod;
            m_GroundDetectors = GetComponentsInChildren<GroundDetector>();
            m_AntPhysics = GetComponentInChildren<AntPhysics>();
            m_AntPhysics.Initialize();
        }

        public override void OnEpisodeBegin()
        {
            NormalizedTargetAngle = 0;
            Array.Clear(m_PrevActions, 0, c_NumActions);
            m_AntPhysics.ManagedReset();
            StorePosition();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(NormalizedTargetAngle);

            var body = m_AntPhysics.Body;
            m_Inclination = body.Inclination;
            sensor.AddObservation(m_Inclination);
            sensor.AddObservation(Sigmoid(body.Localize(body.Velocity)));
            sensor.AddObservation(Sigmoid(body.Localize(body.AngularVelocity)));

            var feet = m_AntPhysics.Feet;
            for (int i = 0; i < c_NumLegs; i++)
            {
                sensor.AddObservation(feet[i].IsGrounded);
                sensor.AddObservation(Sigmoid(body.Localize(feet[i].Velocity)));
            }

            for (int i = 0; i < m_GroundDetectors.Length; i++)
            {
                sensor.AddObservation(m_GroundDetectors[i].GetNormalizedDistance());
            }

            for (int i = 0; i < c_NumActions; i++)
            {
                // Previous actions => current leg rotations.
                sensor.AddObservation(m_PrevActions[i]);
            }

            if (m_LeavesTrail)
            {
                Vector3 pos = body.Position;
                if ((pos - m_TmpPos).sqrMagnitude >= m_TrailSpacingSqr)
                {
                    TrailStepEvent.Invoke(pos);
                    StorePosition(pos);
                }
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var actions = actionBuffers.ContinuousActions.Array;
            int step = StepCount % m_DecisionInterval;
            
            if (step == 0)
            {
                // Pre-decision step: apply and store current actions.
                Array.Copy(actions, 0, m_PrevActions, 0, c_NumActions);
            }
            else
            {
                // Interpolate from previous to current actions.
                float t = step / (float)m_DecisionInterval;
                for (int i = 0; i < c_NumActions; i++)
                {
                    actions[i] = Mathf.Lerp(m_PrevActions[i], actions[i], t);
                }
            }

            m_AntPhysics.ApplyActions(actions);

            if (m_Inclination.y < 0)
            {
                // Fell over.
                EndEpisode();
                EndEpisodeEvent?.Invoke();
                
            }
        }

        protected void StorePosition(Vector3 pos)
        {
            m_TmpPos = pos;
        }

        protected void StorePosition()
        {
            StorePosition(m_AntPhysics.Body.Position);
        }

        protected static float Sigmoid(float val, float scale = 1f)
        {
            val *= scale;
            return val / (1f + Mathf.Abs(val));
        }

        protected static Vector3 Sigmoid(Vector3 v3, float scale = 1f)
        {
            v3.x = Sigmoid(v3.x, scale);
            v3.y = Sigmoid(v3.y, scale);
            v3.z = Sigmoid(v3.z, scale);
            return v3;
        }
    }
}