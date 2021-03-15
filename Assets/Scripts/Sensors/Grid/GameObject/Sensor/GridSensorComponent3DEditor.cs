#if (UNITY_EDITOR)
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using MBaske.Sensors.Util;

namespace MBaske.Sensors.Grid
{
    [CustomEditor(typeof(GridSensorComponent3D)), CanEditMultipleObjects]
    public class GridSensorComponent3DEditor : Editor
    {
        private GridSensorComponent3D m_Comp;
        private bool m_UpdateFlag;

        private ArcHandle m_ArcHandleLatN;
        private ArcHandle m_ArcHandleLatS;
        private ArcHandle m_ArcHandleLon;

        private static readonly Color s_WireColorA = new Color(0f, 0.5f, 1f, 0.3f);
        private static readonly Color s_WireColorB = new Color(0f, 0.5f, 1f, 0.1f);

        private void OnEnable()
        {
            m_Comp = (GridSensorComponent3D)target;
            Undo.undoRedoPerformed += OnUndoRedo;

            m_ArcHandleLatN = new ArcHandle();
            m_ArcHandleLatN.SetColorWithoutRadiusHandle(new Color32(152, 237, 67, 255), 0.1f);
            m_ArcHandleLatN.radiusHandleColor = Color.white;
            
            m_ArcHandleLatS = new ArcHandle();
            m_ArcHandleLatS.SetColorWithoutRadiusHandle(new Color32(152, 237, 67, 255), 0.1f);
            m_ArcHandleLatS.radiusHandleColor = Color.white;
           
            m_ArcHandleLon = new ArcHandle();
            m_ArcHandleLon.SetColorWithoutRadiusHandle(new Color32(237, 67, 30, 255), 0.1f);
            m_ArcHandleLon.radiusHandleColor = Color.white;
        }

        private void RecordUpdate()
        {
            Undo.RecordObject(m_Comp, "Gridsensor Update");
        }

        private void OnUndoRedo()
        {
            // TODO target is null here after undo in playmode?
            m_UpdateFlag = true;
        }

        private void OnSceneGUI()
        {
            if (m_UpdateFlag)
            {
                m_UpdateFlag = false;
                m_Comp.Validate();
            }

            if (m_Comp)
            {
                DrawHandles();
                DrawWireFrame();
            }
        }

        private void DrawHandles()
        {
            var tf = m_Comp.transform;

            // Latitude North & Max. Distance.

            Vector3 fwd = tf.forward;
            Vector3 normal = Vector3.Cross(fwd, tf.up);
            Matrix4x4 matrix = Matrix4x4.TRS(
                tf.position,
                Quaternion.LookRotation(fwd, normal),
                Vector3.one
            );

            using (new Handles.DrawingScope(matrix))
            {
                EditorGUI.BeginChangeCheck();
                {
                    m_ArcHandleLatN.angle = m_Comp.LatAngleNorth;
                    m_ArcHandleLatN.radius = m_Comp.MaxDistance;
                    m_ArcHandleLatN.DrawHandle();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    RecordUpdate();

                    if (m_Comp.LatAngleNorth != m_ArcHandleLatN.angle)
                    {
                        m_Comp.LatAngleNorth = m_ArcHandleLatN.angle;
                    }
                    else if (m_Comp.MaxDistance != m_ArcHandleLatN.radius)
                    {
                        m_Comp.MaxDistance = m_ArcHandleLatN.radius;
                    }
                }
            }

            // Latitude South & Max. Distance.

            normal = Vector3.Cross(fwd, -tf.up);
            matrix = Matrix4x4.TRS(
                tf.position,
                Quaternion.LookRotation(fwd, normal),
                Vector3.one
            );

            using (new Handles.DrawingScope(matrix))
            {
                EditorGUI.BeginChangeCheck();
                {
                    m_ArcHandleLatS.angle = m_Comp.LatAngleSouth;
                    m_ArcHandleLatS.radius = m_Comp.MaxDistance;
                    m_ArcHandleLatS.DrawHandle();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    RecordUpdate();

                    if (m_Comp.LatAngleSouth != m_ArcHandleLatS.angle)
                    {
                        m_Comp.LatAngleSouth = m_ArcHandleLatS.angle;
                    }
                    else if (m_Comp.MaxDistance != m_ArcHandleLatS.radius)
                    {
                        m_Comp.MaxDistance = m_ArcHandleLatS.radius;
                    }
                }
            }

            // Longitude & Max. Distance.

            normal = Vector3.Cross(fwd, tf.right);
            matrix = Matrix4x4.TRS(
                tf.position,
                Quaternion.LookRotation(fwd, normal),
                Vector3.one
            );

            using (new Handles.DrawingScope(matrix))
            {
                EditorGUI.BeginChangeCheck();
                {
                    m_ArcHandleLon.angle = m_Comp.LonAngle;
                    m_ArcHandleLon.radius = m_Comp.MaxDistance;
                    m_ArcHandleLon.DrawHandle();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    RecordUpdate();

                    if (m_Comp.LonAngle != m_ArcHandleLon.angle)
                    {
                        m_Comp.LonAngle = m_ArcHandleLon.angle;
                    }
                    else if (m_Comp.MaxDistance != m_ArcHandleLon.radius)
                    {
                        m_Comp.MaxDistance = m_ArcHandleLon.radius;
                    }
                }
            }

            // Minimum Distance.

            float min = m_Comp.MinDistance;
            EditorGUI.BeginChangeCheck();
            {
                min = Handles.RadiusHandle(tf.rotation, tf.position, min);
            }
            if (EditorGUI.EndChangeCheck())
            {
                RecordUpdate();
                m_Comp.MinDistance = min;
            }
        }

        private void DrawWireFrame()
        {
            int nLon = m_Comp.GridSize.x;
            int nLat = m_Comp.GridSize.y;
            Quaternion[,] wf = m_Comp.Wireframe;

            if ((nLon + 1) * (nLat + 1) != wf.Length)
            {
                return;
            }

            UI.GLMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(m_Comp.transform.localToWorldMatrix);

            Vector3 min = Vector3.forward * m_Comp.MinDistance;
            Vector3 max = Vector3.forward * m_Comp.MaxDistance;

            // Grid Cells

            for (int iLat = 0; iLat <= nLat; iLat++)
            {
                GL.Begin(GL.LINE_STRIP);
                GL.Color(s_WireColorA);

                for (int iLon = 0; iLon <= nLon; iLon++)
                {
                    var v = wf[iLat, iLon] * max;
                    GL.Vertex3(v.x, v.y, v.z);
                }
                GL.End();
            }

            for (int iLon = 0; iLon <= nLon; iLon++)
            {
                GL.Begin(GL.LINE_STRIP);
                GL.Color(s_WireColorA);

                for (int iLat = 0; iLat <= nLat; iLat++)
                {
                    var v = wf[iLat, iLon] * max;
                    GL.Vertex3(v.x, v.y, v.z);
                }
                GL.End();
            }

            // Angles

            if (m_Comp.LatAngleSouth < 90)
            {
                GL.Begin(GL.LINES);
                GL.Color(s_WireColorB);

                for (int iLon = 0; iLon <= nLon; iLon++)
                {
                    var a = wf[0, iLon] * min;
                    GL.Vertex3(a.x, a.y, a.z);
                    var b = wf[0, iLon] * max;
                    GL.Vertex3(b.x, b.y, b.z);
                }
                GL.End();
            }

            if (m_Comp.LatAngleNorth < 90)
            {
                GL.Begin(GL.LINES);
                GL.Color(s_WireColorB);

                for (int iLon = 0; iLon <= nLon; iLon++)
                {
                    var a = wf[nLat, iLon] * min;
                    GL.Vertex3(a.x, a.y, a.z);
                    var b = wf[nLat, iLon] * max;
                    GL.Vertex3(b.x, b.y, b.z);
                }
                GL.End();
            }

            if (m_Comp.LonAngle < 180)
            {
                for (int iLon = 0; iLon <= nLon; iLon += nLon)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(s_WireColorB);

                    for (int iLat = 0; iLat <= nLat; iLat++)
                    {
                        var a = wf[iLat, iLon] * min;
                        GL.Vertex3(a.x, a.y, a.z);
                        var b = wf[iLat, iLon] * max;
                        GL.Vertex3(b.x, b.y, b.z);
                    }
                    GL.End();
                }
            }

            GL.PopMatrix();
        }
    }
}
#endif
