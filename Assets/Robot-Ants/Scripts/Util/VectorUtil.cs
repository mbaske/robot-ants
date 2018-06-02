using UnityEngine;

namespace RobotAnts
{
    public static class VectorUtil
    {
        public static Vector2 Flatten(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }
      
        public static Vector3 OffsetY(this Vector3 vector, float offset)
        {
            return new Vector3(vector.x, vector.y + offset, vector.z);
        }
      
        public static Vector3 GroundPos(this Vector3 pos, float offsetIn = 10f, float offsetOut = 0f)
        {
            return new Ray(pos.OffsetY(offsetIn), Vector3.down).GroundPos(offsetOut);
        }

        public static Vector3 GroundPos(this Ray ray, float offsetOut = 0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 20f, 1 << 8))
                return hit.point.OffsetY(offsetOut);

            return Vector3.zero;
        }

        public static bool IsValid(this Vector3 pos)
        {
            return pos != Vector3.zero;
        }
      
        public static float NormAngle2D(Vector3 position, Vector3 target, Vector2 axis)
        {
            Vector2 delta = new Vector2(target.x - position.x, target.z - position.z);
            return Vector2.SignedAngle(delta, axis) / 180f;
        }
    }
}
