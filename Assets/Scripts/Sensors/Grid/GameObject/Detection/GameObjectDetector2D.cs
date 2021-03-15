using UnityEngine;

namespace MBaske.Sensors.Grid
{
    public class GameObjectDetector2D : GameObjectDetector
    {
        public bool Rotate
        {
            set { m_Rotate = value; }
        }
        private bool m_Rotate;

        public Quaternion WorldRotation
        {
            set { m_WorldRotation = value; }
        }
        private Quaternion m_WorldRotation;

        public Constraint2D Constraint
        {
            set
            {
                m_Constraint = value;
                m_BoundsCenter = value.Bounds.center;
                m_BoundsExtents = value.Bounds.extents;
            }
        }
        private Constraint2D m_Constraint;
        private Vector3 m_BoundsCenter;
        private Vector3 m_BoundsExtents;

        public override DetectionResult Update()
        {
            Result.Clear();

            Quaternion rot = m_Rotate 
                ? Quaternion.AngleAxis(m_Transform.eulerAngles.y, Vector3.up) 
                : m_WorldRotation;

            Vector3 pos = m_Transform.position;
            Matrix4x4 worldToLocalMatrix = Matrix4x4.TRS(pos, rot, Vector3.one).inverse;

            ParseColliders(
                Physics.OverlapBoxNonAlloc(pos + rot * m_BoundsCenter, 
                    m_BoundsExtents, m_Buffer, rot, m_LayerMask),
                m_Constraint, worldToLocalMatrix);

            return Result;
        }
    }

    public class Constraint2D : Constraint
    {
        public Bounds Bounds
        {
            set
            {
                m_Bounds = value;
                m_BoundRect = new Rect(value.min.x, value.min.z, value.size.x, value.size.z);
            }
            get { return m_Bounds; }
        }
        private Bounds m_Bounds;
        private Rect m_BoundRect;

        public override bool ContainsPoint(Vector3 localPoint, out Vector3 normalized)
        {
            if (m_Bounds.Contains(localPoint))
            {
                normalized = Rect.PointToNormalized(m_BoundRect, new Vector2(localPoint.x, localPoint.z));
                return true;
            }

            normalized = default;
            return false;
        }
    }
}
