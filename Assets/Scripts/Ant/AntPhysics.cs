using UnityEngine;

namespace MBaske.RobotAnts
{
    public class AntPhysics : MonoBehaviour
    {
        public AntBody Body { get; private set; }
        public AntLeg[] Legs { get; private set; }
        public AntFoot[] Feet { get; private set; }
        public GaitCtrl GaitCtrl { get; private set; }

        private Resetter m_Resetter;
        private bool m_IsActive;

        public void Initialize()
        {
            m_Resetter = new Resetter(transform);

            Body = GetComponentInChildren<AntBody>();
            Legs = GetComponentsInChildren<AntLeg>();
            Feet = GetComponentsInChildren<AntFoot>();
            GaitCtrl = GetComponent<GaitCtrl>();

            Body.Initialize();
            for (int i = 0; i < 6; i++)
            {
                Legs[i].Initialize();
                Feet[i].Initialize(Body.transform);
            }
        }

        public void ManagedReset()
        {
            m_Resetter.Reset();
            m_IsActive = false;
            Body.ToggleFreeze(true);

            for (int i = 0; i < 6; i++)
            {
                Legs[i].ManagedReset();
                Feet[i].ManagedReset();
            }
        }

        public void ApplyActions(float[] actions)
        {
            bool isGrounded = true;

            for (int i = 0, j = 0; i < 6; i++)
            {
                if (m_IsActive)
                {
                    Legs[i].ApplyActions(actions[j++], actions[j++]);
                    Feet[i].ManagedUpdate();
                }

                isGrounded = !m_IsActive && isGrounded && Feet[i].IsGrounded;
            }
            // Waits for all feet to be grounded.
            //
            // TODO Better way of preventing the agent from falling 
            // over after reset. Problem is likely the feet snapping
            // into place initially, throwing the body off balance.
            if (!m_IsActive && isGrounded)
            {
                m_IsActive = true;
                Body.ToggleFreeze(false);
            }
        }
    }
}