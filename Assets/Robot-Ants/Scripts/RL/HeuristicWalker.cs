// Original code by Etienne Cella
// https://github.com/etienne-p/UnityHexapodSimulator
//
using UnityEngine;

namespace RobotAnts
{
    public class HeuristicWalker : MonoBehaviour
    {
        private const int N_LEGS = 6;
        
        internal float sig = 1.0f;
        internal float right = 1.0f;
        internal float left = 1.0f;
        internal float smoothing = 75.0f;

        internal Oscillator cpg;
        // Knee joint phase offset
        private float[] phaseOffset;
        // Knee joint phase offset derivative
        private float[] dPhaseOffset;
        // Target knee joint phase offset derivative
        private float[] tPhaseOffset;
        // Shoulder joint amp
        private float[] amp;
        // Shoulder joint amp derivative
        private float[] dAmp;
        // Target shoulder joint amp
        private float[] tAmp;

        private JointRotations jr;

        readonly float[] PHASE_OFFSET = new float[]
        {
            0, Mathf.PI, 0, Mathf.PI, 0, Mathf.PI, 0
        };

        private void Awake()
        {
            cpg = GetComponent<Oscillator>();
            phaseOffset = new float[N_LEGS];
            dPhaseOffset = new float[N_LEGS];
            tPhaseOffset = new float[N_LEGS];
            GetKneeJointPhaseOffset(left, right, ref phaseOffset);
            GetKneeJointPhaseOffset(left, right, ref tPhaseOffset);
            amp = new float[N_LEGS];
            dAmp = new float[N_LEGS];
            tAmp = new float[N_LEGS];
            GetShoulderJointAmp(left, right, ref amp);
            GetShoulderJointAmp(left, right, ref tAmp);
            jr = new JointRotations(N_LEGS);
        }

        internal JointRotations GetRotations()
        {
            GetKneeJointPhaseOffset(left, right, ref tPhaseOffset);
            GetShoulderJointAmp(left, right, ref tAmp);

            float dt = Time.deltaTime;
            cpg.UpdateKuramoto(dt);
            // legs stops moving if the shoulder does
            float a = 1 - Mathf.Exp(-4 * Mathf.Clamp01(Mathf.Max(Mathf.Abs(left), Mathf.Abs(right))));

            for (int i = 0; i < N_LEGS; ++i)
            {
                // Critically damped second order differential equation
                // http://mathproofs.blogspot.ca/2013/07/critically-damped-spring-smoothing.html
                float ddPhaseOffset = smoothing * ((smoothing / 4.0f) * (tPhaseOffset[i] - phaseOffset[i]) - dPhaseOffset[i]); // 2nd derivative
                dPhaseOffset[i] = dPhaseOffset[i] + dt * ddPhaseOffset; // 1st derivative
                phaseOffset[i] = phaseOffset[i] + dt * dPhaseOffset[i];

                float ddAmp = smoothing * ((smoothing / 4.0f) * (tAmp[i] - amp[i]) - dAmp[i]); // 2nd derivative
                dAmp[i] = dAmp[i] + dt * ddAmp; // 1st derivative
                amp[i] = amp[i] + dt * dAmp[i];

                float osc = Mathf.Sin(cpg.phase[i] + PHASE_OFFSET[i]);
                jr.rotShoulder[i] = (1.0f / (1.0f + Mathf.Exp(-sig * osc)) - .5f) * 2.0f * amp[i];

                osc = Mathf.Sin(cpg.phase[i] * 1.0f + phaseOffset[i] * Mathf.PI / 2.0f);
                jr.rotKnee[i] = (1.0f / (1.0f + Mathf.Exp(-sig * osc)) - .5f) * 2.0f * a;
            }

            return jr;
        }

        private void GetKneeJointPhaseOffset(float leftTrack, float rightTrack, ref float[] offset)
        {
            for (int i = 0; i < N_LEGS; ++i)
            {
                offset[i] = (i % 2 == 0 ? leftTrack : rightTrack);
            }
        }

        private void GetShoulderJointAmp(float leftTrack, float rightTrack, ref float[] amp)
        {
            for (int i = 0; i < N_LEGS; ++i)
            {
                amp[i] = Mathf.Abs(i % 2 == 0 ? leftTrack : rightTrack);
            }
        }
    }
    
    internal struct JointRotations
    {
        internal float[] rotShoulder;
        internal float[] rotKnee;

        internal JointRotations(int n)
        {
            rotShoulder = new float[n];
            rotKnee = new float[n];
        }
    }
}