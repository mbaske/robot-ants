using UnityEngine;

public class Leg : MonoBehaviour
{
    [SerializeField]
    private Vector3 constraintsA = Vector3.one * 15;
    private Vector3 eulersA;

    [SerializeField]
    private Vector3 constraintsB = Vector3.one * 15;
    private Vector3 eulersB;

    [SerializeField]
    private Transform legB;

    public void Initialize()
    {
        eulersA = transform.localEulerAngles;
        eulersB = legB.localEulerAngles;
    }

    public void StepUpdate(float p0, float p1, float p2, float p3, float p4, float p5)
    {
        transform.localRotation = Quaternion.Euler(
            eulersA.x + p0 * constraintsA.x,
            eulersA.y + p1 * constraintsA.y,
            eulersA.z + p2 * constraintsA.z
        );
        legB.localRotation = Quaternion.Euler(
            eulersB.x + p3 * constraintsB.x,
            eulersB.y + p4 * constraintsB.y,
            eulersB.z + p5 * constraintsB.z
        );
    }
}
