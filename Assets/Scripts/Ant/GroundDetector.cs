using UnityEngine;

namespace MBaske.RobotAnts
{
    public class GroundDetector : MonoBehaviour
    {
        private const int c_Mask = 1 << 6;

        public float GetNormalizedDistance()
        {
            return Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1, c_Mask)
                ? hit.distance * 2 - 1 : 1;
        }

        //private void OnDrawGizmos()
        //{
        //    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1, LayerMasks.Ground))
        //    {
        //        Debug.DrawLine(transform.position, hit.point, Color.red);
        //    }
        //    else
        //    {
        //        Debug.DrawRay(transform.position, Vector3.down, Color.green);
        //    }
        //}
    }
}