using UnityEngine;

public class Ant : MonoBehaviour, ITrailOwner
{
    public Vector3 Velocity => body.velocity;
    public Vector3 AngularVelocity => body.angularVelocity;
    public Vector3 Forward => self.forward;
    public Vector3 Position => self.position;
    public Vector3 LocalPosition => self.localPosition;
    public float Tilt => self.up.y;
    public float Strength { get; private set; }

    [SerializeField]
    [Range(1f, 60f)]
    private float depletionTime = 10f;
    [SerializeField]
    private Color color0 = Color.red;
    [SerializeField]
    private Color color1 = Color.green;

    private TrailManager trailManager;
    private PowerUps powerUps;
    private Vector3 prevPos;
    private bool tmpPowerUp;
    private float time;
    private Material mat;

    private Leg[] innerLegs = new Leg[6];
    private Leg[] outerLegs = new Leg[6];
    // Leg rotation ranges (+/-degrees)
    private float yInner = 30f;
    private float zInner = 30f;
    private float yOuter = 20f;
    private float zOuter = 30f;

    private Transform self;
    private Rigidbody body;

    public void Initialize(bool hasTrail)
    {
        self = transform.Find("Thorax");
        body = self.GetComponent<Rigidbody>();

        int j = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tf = transform.GetChild(i);
            if (tf.name.Contains("Leg"))
            {
                innerLegs[j] = tf.Find("Inner").GetComponent<Leg>();
                innerLegs[j].Initialize();
                outerLegs[j] = tf.Find("Outer").GetComponent<Leg>();
                outerLegs[j].Initialize();
                j++;
            }
        }

        if (hasTrail)
        {
            powerUps = transform.parent.GetComponentInChildren<PowerUps>();
            powerUps.Initialize();
            trailManager = transform.parent.GetComponentInChildren<TrailManager>();
            trailManager.Initialize();
            ResetTrail();
            // Strength value -> material color.
            mat = transform.Find("Abdomen").Find("Model").GetComponent<Renderer>().material;  
        }
    }

    public void SetLegTargetRotations(float[] rot)
    {
        for (int i = 0; i < 6; i++)
        {
            // Need to alternate sign when using osc values due to how the joints are set up.
            float zSign = i % 2 == 0 ? -1f : 1f;
            innerLegs[i].SetTargetRotation(rot[i] * yInner, rot[i + 6] * zInner * zSign);
            outerLegs[i].SetTargetRotation(rot[i + 12] * yOuter, rot[i + 18] * zOuter * zSign);
        }
    }

    public float[] GetLegRotations()
    {
        int j = 0;
        float[] rot = new float[36];
        for (int i = 0; i < 6; i++)
        {
            Vector3 r = innerLegs[i].GetRotation();
            rot[j++] = r.x;
            rot[j++] = r.y;
            rot[j++] = r.z;
            r = outerLegs[i].GetRotation();
            rot[j++] = r.x;
            rot[j++] = r.y;
            rot[j++] = r.z;
        }
        return rot; // -1/+1
    }

    public float[] GetFootHeights()
    {
        float[] result = new float[6];
        for (int i = 0; i < 6; i++)
        {
            result[i] = outerLegs[i].GetFootHeight();
        }
        return result;
    }

    public Vector4 SearchVicinity(int radius)
    {
        return trailManager.SearchVicinity(radius, this);
    }

    public Vector3 GetDirectionTowards(Vector3 target)
    {
        return (target - Position).normalized;
    }

    public float DirectionToRelativeAngle2D(Vector3 worldDirection)
    {
        return Vector2.SignedAngle(Util.Flatten(worldDirection), Util.Flatten(Forward));
    }

    public Vector3 GetRelativeDirection(Vector3 worldDirection)
    {
        return self.InverseTransformDirection(worldDirection);
    }

    public void UpdateTrail()
    {
        bool powerUp = powerUps.HasPowerUpAt(Position);
        Strength = powerUp ? 1f : Mathf.Max(0f, Strength - (Time.time - time) / depletionTime);
        mat.color = Color.Lerp(color0, color1, Strength);
        time = Time.time;

        if (Strength > 0f)
        {
            bool step = Vector3.Distance(Position, prevPos) > trailManager.Spacing;
            bool newPowerUp = powerUp && !tmpPowerUp;

            if (step || newPowerUp)
            {
                if (newPowerUp)
                {
                    // Always create new trail at power-up.
                    trailManager.OrphanTrail(this);
                }

                trailManager.AddTrailPoint(this);
                tmpPowerUp = powerUp;
                prevPos = Position;
            }
        }
    }

    public void ResetTrail()
    {
        time = Time.time;
        Strength = 0f;
        prevPos = Position;
        tmpPowerUp = false;
        trailManager.OrphanTrail(this);
    }
}
