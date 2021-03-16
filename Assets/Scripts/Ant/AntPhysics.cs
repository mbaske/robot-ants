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

            for (int i = 0; i < 6; i++)
            {
                Legs[i].ManagedReset();
                Feet[i].ManagedReset();
            }
        }

        public void ApplyActions(float[] actions)
        {
            for (int i = 0, j = 0; i < 6; i++)
            {
                Legs[i].ApplyActions(actions[j++], actions[j++]);
                Feet[i].ManagedUpdate();
            }
        }
    }
}