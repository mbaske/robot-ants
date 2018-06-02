using UnityEngine;

namespace RobotAnts
{
    public class Leg : MonoBehaviour
    {
        public Limb[] limbs = new Limb[4];
        public Foot foot;
        public Socket socket;
   
        // TODO calculate these values based on connected limb angles
        private float[] relativeJointAngleMinimum =
        {
            -40f,
            -40f,
            20f,
            15f
        };

        internal void Initialize()
        {
            socket.Initialize();
            for (int i = 0; i < 4; i++)
                limbs[i].Initialize(relativeJointAngleMinimum[i]);
            foot.Initialize();
        }
      
        internal void SavePosition()
        {
            socket.SavePosition();
            for (int i = 0; i < 4; i++)
                limbs[i].SavePosition();
        }

        internal void ResetPosition()
        {
            socket.ResetPosition();
            for (int i = 0; i < 4; i++)
                limbs[i].ResetPosition();
        }
    }
}