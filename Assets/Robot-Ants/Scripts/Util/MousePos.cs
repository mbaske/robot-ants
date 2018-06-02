using UnityEngine;

namespace RobotAnts
{
    public class MousePos : MonoBehaviour
    {
        public Robot robot;
        private Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = cam.ScreenPointToRay(Input.mousePosition).GroundPos();
                if (pos.IsValid())
                    robot.SetTargetPosition(pos);
            }
        }
    }
}