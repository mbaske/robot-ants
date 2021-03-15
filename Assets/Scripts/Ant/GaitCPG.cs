using UnityEngine;

// Source: https://github.com/etienne-p/UnityHexapodSimulator

namespace MBaske.RobotAnts
{
    public class GaitCPG : MonoBehaviour
    {
        public enum Gait
        {
            None = 0,
            Metachronal = 1,
            Ripple = 2,
            Tripod = 3
        }

        /*
        Oscillator network structure:
        (knee oscillators are not represented as each knee oscillator is only linked to its shoulder)
        1 -- 2
        |    |
        3 -- 4
        |    |
        5 -- 6
        */

        readonly float[,] WEIGHT = new float[,]
        {
        //1, 2, 3, 4, 5, 6
        { 0, 1, 1, 0, 0, 0 },//1
        { 1, 0, 0, 1, 0, 0 },//2
        { 1, 0, 0, 1, 1, 0 },//3
        { 0, 1, 1, 0, 0, 1 },//4
        { 0, 0, 1, 0, 0, 1 },//5
        { 0, 0, 0, 1, 1, 0 } //6
        };

        const int N_OSCILLATORS = 6;

        // Note that all oscillators are assumed to have the same natural frequency
        public float freq = 8;
        public Gait gait = Gait.Tripod;

        public float[] phase { get; private set; }

        float[,] phaseBias;

        void Awake()
        {
            phase = new float[N_OSCILLATORS];
            for (int i = 0; i < N_OSCILLATORS; ++i)
            {
                phase[i] = Random.Range(0, 2 * Mathf.PI);
            }
            UpdateGait();
        }

        void OnEnable()
        {
            UpdateGait();
        }

        void OnValidate()
        {
            UpdateGait();
        }

        void Update()
        {
            UpdateKuramoto(Time.deltaTime);
        }

        void UpdateGait()
        {
            float[] theta = GetGaitPhaseBias(gait);

            for (int i = 0; i < 7; ++i)
            {
                theta[i] *= Mathf.PI;
            }

            phaseBias = new float[,]
            {
            //1,        2,        3,        4,        5,        6
            { 0.00000f, theta[0], theta[1], 0.00000f, 0.00000f, 0.00000f },//1
            { theta[0], 0.00000f, 0.00000f, theta[2], 0.00000f, 0.00000f },//2
            { theta[1], 0.00000f, 0.00000f, theta[3], theta[4], 0.00000f },//3
            { 0.00000f, theta[2], theta[3], 0.00000f, 0.00000f, theta[5] },//4
            { 0.00000f, 0.00000f, theta[4], 0.00000f, 0.00000f, theta[6] },//5
            { 0.00000f, 0.00000f, 0.00000f, theta[5], theta[6], 0.00000f } //6
            };
        }

        void UpdateKuramoto(float dt)
        {
            float[] newPhase = new float[N_OSCILLATORS];
            for (int i = 0; i < N_OSCILLATORS; ++i)
            {
                float dPhase = freq;
                for (int j = 0; j < N_OSCILLATORS; ++j)
                {
                    dPhase += WEIGHT[i, j] * Mathf.Sin(phase[j] - phase[i] - phaseBias[i, j]);
                }
                newPhase[i] = phase[i] + dt * dPhase;
            }
            phase = newPhase;
        }

        static float[] GetGaitPhaseBias(Gait gait)
        {
            // 7 elements representing links:
            // (1,2) (1, 3) (2, 4) (3, 4) (3, 5) (4, 6) (5, 6)
            switch (gait)
            {
                case Gait.Metachronal:
                    return new float[] { 1.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f };
                case Gait.Ripple:
                    return new float[] { -1.0f, -3.0f / 2.0f, 1.0f / 2.0f, 1.0f, 1.0f / 2.0f, 1.0f / 2.0f, 1.0f };
                case Gait.Tripod:
                    return new float[] { 1.0f, 1.0f, -1.0f, -1.0f, -1.0f, 1.0f, 1.0f };
                default:
                    Debug.LogWarning("Using default gait");
                    return new float[] { 0, 0, 0, 0, 0, 0, 0 };
            }
        }
    }
}