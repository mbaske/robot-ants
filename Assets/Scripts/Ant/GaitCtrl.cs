using UnityEngine;

// Source: https://github.com/etienne-p/UnityHexapodSimulator

namespace MBaske.RobotAnts
{
    [RequireComponent(typeof(GaitCPG))]
    public class GaitCtrl : MonoBehaviour
    {
        const int N_LEGS = 6;

        [Range(0, 5)]
        public float sig = 2.0f;
        [Range(-1, 1)]
        public float right = 1.0f;
        [Range(-1, 1)]
        public float left = 1.0f;
        [Range(0, 2)]
        public float smoothing = 1f;

        GaitCPG cpg;
        // Knee joint phase offset
        float[] phaseOffset;
        // Knee joint phase offset derivative
        float[] dPhaseOffset;
        // Target knee joint phase offset derivative
        float[] tPhaseOffset;
        // Shoulder joint amp
        float[] amp;
        // Shoulder joint amp derivative
        float[] dAmp;
        // Target shoulder joint amp
        float[] tAmp;

        const float PI2 = Mathf.PI / 2f;

        readonly float[] PHASE_OFFSET = new float[]
        {
        0, Mathf.PI, 0, Mathf.PI, 0, Mathf.PI, 0
        };

        void OnEnable()
        {
            cpg = GetComponent<GaitCPG>();
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
        }

        public void Heuristic(float[] actions, float deltaTime)
        {
            GetKneeJointPhaseOffset(left, right, ref tPhaseOffset);
            GetShoulderJointAmp(left, right, ref tAmp);

            for (int i = 0; i < N_LEGS; ++i)
            {
                // Critically damped second order differential equation
                // http://mathproofs.blogspot.ca/2013/07/critically-damped-spring-smoothing.html
                float ddPhaseOffset = smoothing * ((smoothing / 4.0f) * (tPhaseOffset[i] - phaseOffset[i]) - dPhaseOffset[i]); // 2nd derivative
                dPhaseOffset[i] = dPhaseOffset[i] + deltaTime * ddPhaseOffset; // 1st derivative
                phaseOffset[i] = phaseOffset[i] + deltaTime * dPhaseOffset[i];

                float ddAmp = smoothing * ((smoothing / 4.0f) * (tAmp[i] - amp[i]) - dAmp[i]); // 2nd derivative
                dAmp[i] = dAmp[i] + deltaTime * ddAmp; // 1st derivative
                amp[i] = amp[i] + deltaTime * dAmp[i];
            }

            for (int i = 0; i < N_LEGS; ++i)
            {
                float osc = Mathf.Sin(cpg.phase[i] + PHASE_OFFSET[i]);
                osc = (1.0f / (1.0f + Mathf.Exp(-sig * osc)) - .5f) * 2.0f;
                actions[i * 2] = Mathf.Lerp(-1f, 1f, .5f * (osc + 1.0f)) * amp[i];
            }

            // legs stops moving if the shoulder does
            float a = 1 - Mathf.Exp(-4 * Mathf.Clamp01(Mathf.Max(Mathf.Abs(left), Mathf.Abs(right))));

            for (int i = 0; i < N_LEGS; ++i)
            {
                float osc = Mathf.Sin(cpg.phase[i] * 1.0f + phaseOffset[i] * PI2);
                osc = (1.0f / (1.0f + Mathf.Exp(-sig * osc)) - .5f) * 2.0f;
                actions[i * 2 + 1] = Mathf.Lerp(-1f, 1f, .5f * (osc + 1.0f)) * a;
            }
        }

        static void GetKneeJointPhaseOffset(float leftTrack, float rightTrack, ref float[] offset)
        {
            for (int i = 0; i < N_LEGS; ++i)
            {
                offset[i] = (i % 2 == 0 ? leftTrack : rightTrack);
            }
        }

        static void GetShoulderJointAmp(float leftTrack, float rightTrack, ref float[] amp)
        {
            for (int i = 0; i < N_LEGS; ++i)
            {
                amp[i] = Mathf.Abs(i % 2 == 0 ? leftTrack : rightTrack);
            }
        }
    }
}