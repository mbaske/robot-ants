using UnityEngine;

namespace RobotAnts
{
    public class AgentWalk : Agent
    {   
		/// <summary>
        /// Use oscillator / heuristic walker for training a tripod gait.
		/// This can be helpful in order to kick-start learning before switching
		/// to environment based rewards.
        /// </summary>
		public bool useOscillator = false;

		private const float VALUE_RANGE = 10f;
      
		private Robot robot;
		private HeuristicWalker hw;     
		private float[] hwAction;

        public override void InitializeAgent()
        {         
			robot = GetComponent<Robot>();
         
			if (useOscillator)
            {
                hwAction = new float[24];
                hw = GetComponent<HeuristicWalker>();
            }
        }
      
        // 41 continuous values between -1 and +1
        public override void CollectObservations()
        {
			for (int j = 0; j < 6; j++)
            {
				for (int i = 0; i < 4; i++)
                {
					// x 24
					AddVectorObs(robot.legs[j].limbs[i].GetRotation());
                }
                // x 6
				AddVectorObs(robot.legs[j].foot.GetDistanceToGround());
            }
            // x 2
			AddVectorObs(robot.body.GetInclination());
            // x 1
			AddVectorObs(robot.body.GetDistanceToGround());
            // x 6
			AddVectorObs(robot.body.GetVelocity());
			// x 1
			AddVectorObs(robot.GetTargetDistance());
			// x 1
			AddVectorObs(robot.GetTargetAngle());     
        }

        public override void AgentAction(float[] vectorAction, string textAction)
        {
			int index = 0;
			float reward = 0f;  

			if (useOscillator)
			{
				UpdateHW();
				for (int j = 0; j < 6; j++)
                {
					for (int i = 0; i < 4; i++)
                    {
                        float v = hwAction[index];
                        robot.legs[j].limbs[i].SetRotation(v);
                        // train model to approximate oscillator values
                        reward -= Mathf.Abs(v - vectorAction[index] / VALUE_RANGE);
                        index++;
                    }
                }
				robot.StepUpdate();
			}
			else
			{
				for (int j = 0; j < 6; j++)
                {
					for (int i = 0; i < 4; i++)
                    {
						float v = vectorAction[index];                     
                        robot.legs[j].limbs[i].SetRotation(v / VALUE_RANGE);
						// constrain vectorAction to VALUE_RANGE
						reward -= (Mathf.Abs(v) > VALUE_RANGE ? Mathf.Abs(v) - VALUE_RANGE : 0f);             
                        index++;
                    }
                }

				if (robot.StepUpdate())
				{
					reward += robot.GetRewardWalk();
				}
				else
				{
					// fell over
					reward -= 1f;
				}
			}

			SetReward(reward);         
        }
              
		public override void AgentReset()
        {
        }

        public override void AgentOnDone()
        {
        }
      
		private void UpdateHW()
        {
			// adjust tripod gait params
			float angle = robot.GetTargetAngle();
			float xIncl = Mathf.Abs(robot.body.GetInclination().x);
			float zIncl = Mathf.Abs(robot.body.GetInclination().y); 
            hw.left = 1f - Mathf.Min(angle < 0f ? -angle * 4f : 0f, 2f);
            hw.right = 1f - Mathf.Min(angle > 0f ? angle * 4f : 0f, 2f);  
            hw.sig = 1f + Mathf.Min(0.1f, xIncl * zIncl);
            xIncl = 1f - Mathf.Min(0.5f, xIncl);
            zIncl = 1f - Mathf.Min(0.5f, zIncl);
            angle = 1f - Mathf.Sqrt(Mathf.Abs(angle));         
			float freq = 5f + 50f * angle * xIncl * zIncl * Mathf.Clamp(robot.body.GetSpeed() / 2f, 0.2f, 1f);
            hw.cpg.freq = Mathf.Lerp(hw.cpg.freq, freq, 0.2f);         
			JointRotations jr = hw.GetRotations();
			for (int i = 0; i < 6; i++)
            {
                hwAction[i * 4 + 0] = jr.rotShoulder[i];
                hwAction[i * 4 + 1] = jr.rotKnee[i];
                hwAction[i * 4 + 2] = jr.rotKnee[i] * 2f;
                hwAction[i * 4 + 3] = jr.rotKnee[i] * -2f;
            }
        }
    }
}