using UnityEngine;

namespace RobotAnts
{
    public class AgentExplore : Agent
    {
        private const float VALUE_RANGE = 10f;

        private Robot robot;
      
        public override void InitializeAgent()
        {
            robot = GetComponent<Robot>();
            robot.AddCallbacks(OnSuccess, OnFail);
        }

        // 12 continuous values between -1 and +1
        public override void CollectObservations()
        {
            // x 1
            AddVectorObs(robot.GetEnergy());
            // x 1
            AddVectorObs(robot.GetOrientation());
            // x 2
            AddVectorObs(robot.GetTrailInfo());
            // x 8
            AddVectorObs(robot.GetElevation());         
        }
              
        public override void AgentAction(float[] vectorAction, string textAction)
        {         
            // angle, distance
            robot.SetTargetPosition(vectorAction[0] / VALUE_RANGE, vectorAction[1] / VALUE_RANGE);            
            // constrain vectorAction to VALUE_RANGE
            AddReward(Mathf.Abs(vectorAction[0]) > VALUE_RANGE ? -Mathf.Abs(vectorAction[0]) + VALUE_RANGE : 0f);
            AddReward(Mathf.Abs(vectorAction[1]) > VALUE_RANGE ? -Mathf.Abs(vectorAction[1]) + VALUE_RANGE : 0f);
        }

        public override void AgentReset()
        {
        }

        public override void AgentOnDone()
        {
        }

        private void OnSuccess()
        {
            AddReward(robot.GetRewardExplore());
            RequestDecision();
        }

        internal void OnFail()
        {
            AddReward(-1f);
            RequestDecision();
        }
    }
}
