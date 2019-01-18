// Adapted from original code by Etienne Cella
// https://github.com/etienne-p/UnityHexapodSimulator
//
using UnityEngine;

public class Oscillator
{
    public float[] phase { get; private set; }

    // Note that all oscillators are assumed to have the same natural frequency
    private float freq = 3f;
    private enum GaitType
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

    private const int N_OSCILLATORS = 6;

    private GaitType gait;
    private float[,] phaseBias;

    public Oscillator()
    {
        gait = GaitType.Tripod;
        phase = new float[N_OSCILLATORS];
        for (int i = 0; i < N_OSCILLATORS; ++i)
        {
            phase[i] = Random.Range(0, 2 * Mathf.PI);
        }
        UpdateGait();
    }

    public void UpdateKuramoto(float dt)
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

    private void UpdateGait()
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

    private float[] GetGaitPhaseBias(GaitType g)
    {
        // 7 elements representing links:
        // (1,2) (1, 3) (2, 4) (3, 4) (3, 5) (4, 6) (5, 6)
        switch (g)
        {
            case GaitType.Metachronal:
                return new float[] { 1.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f };
            case GaitType.Ripple:
                return new float[] { -1.0f, -3.0f / 2.0f, 1.0f / 2.0f, 1.0f, 1.0f / 2.0f, 1.0f / 2.0f, 1.0f };
            case GaitType.Tripod:
                return new float[] { 1.0f, 1.0f, -1.0f, -1.0f, -1.0f, 1.0f, 1.0f };
            default:
                Debug.LogWarning("Using default gait");
                return new float[] { 0, 0, 0, 0, 0, 0, 0 };
        }
    }
}

public class GaitOsc
{
    private const int N_LEGS = 6;

    private float sig = 1f;
    private float right = 1.0f;
    private float left = 1.0f;
    private float smoothing = 50.0f;

    public Oscillator osc;
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

    public GaitOsc()
    {
        osc = new Oscillator();
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

    public JointRotations GetRotations()
    {
        GetKneeJointPhaseOffset(left, right, ref tPhaseOffset);
        GetShoulderJointAmp(left, right, ref tAmp);

        float dt = Time.deltaTime;
        osc.UpdateKuramoto(dt);
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

            float osc = Mathf.Sin(this.osc.phase[i] + PHASE_OFFSET[i]);
            jr.y[i] = (1.0f / (1.0f + Mathf.Exp(-sig * osc)) - .5f) * 2.0f * amp[i];

            osc = Mathf.Sin(this.osc.phase[i] * 1.0f + phaseOffset[i] * Mathf.PI / 2.0f);
            jr.z[i] = (1.0f / (1.0f + Mathf.Exp(-sig * osc)) - .5f) * 2.0f * a;
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

public struct JointRotations
{
    public float[] y;
    public float[] z;

    public JointRotations(int n)
    {
        y = new float[n];
        z = new float[n];
    }

    public float[] ToArray()
    {
        float[] r = new float[24];
        y.CopyTo(r, 0);
        z.CopyTo(r, 6);
        y.CopyTo(r, 12);
        z.CopyTo(r, 18);
        return r;
    }
}
