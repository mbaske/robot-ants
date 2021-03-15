using UnityEngine;

namespace MBaske
{
    public static class GroundPos
    {
        private const int c_Mask = 1 << 6;

        public static Vector3 Under(Vector3 pos)
        {
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit hit, 100, c_Mask))
            {
                return hit.point;
            }

            Debug.LogWarning("Ground detection failed.");
            return pos;
        }
    }
}