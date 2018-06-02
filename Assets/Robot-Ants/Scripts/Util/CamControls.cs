// Original code by https://github.com/Syomus/ProceduralToolkit
// Added keyboard commands
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RobotAnts
{
    [RequireComponent(typeof(Image))]
    public class CamControls : UIBehaviour, IDragHandler
    {      
        public Transform cam;

        private Robot[] robots;
        private int index = 0;

        [Header("Position")]
        public float distanceMin = 10;
        public float distanceMax = 30;
        public float yOffset = 0;
        public float scrollSensitivity = 1000;
        public float scrollSmoothing = 10;
        [Header("Rotation")]
        public float tiltMin = -85;
        public float tiltMax = 85;
        public float rotationSensitivity = 0.5f;
        public float rotationSpeed = 20;

        private float distance;
        private float scrollDistance;
        private float velocity;
        private float lookAngle;
        private float tiltAngle;
        private Quaternion rotation;
        private Vector3 pos;
        private bool freeze = false;
      
        protected override void Awake()
        {
            base.Awake();

            GameObject[] g = GameObject.FindGameObjectsWithTag("Agent");
            int n = g.Length;
            robots = new Robot[n];
            for (int i = 0; i < n; i++)
                robots[i] = g[i].GetComponent<Robot>();
            
            tiltAngle = (tiltMin + tiltMax) / 2;
            distance = scrollDistance = (distanceMax + distanceMin) / 2;
         
            cam.rotation = rotation = Quaternion.Euler(tiltAngle, lookAngle, 0);
            cam.position = CalculateCameraPosition();
        }

        private void LateUpdate()
        {
            float f = Time.deltaTime * 65f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // swap target
                index = (index < robots.Length - 1) ? index + 1 : 0;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                // toggle freeze
                freeze = !freeze;
            }

            // tilt / zoom
            if (Input.GetKey(KeyCode.W))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    scrollDistance -= 0.15f * f;
                }
                else
                {
                    tiltAngle += 0.7f * f;
                }            
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    scrollDistance += 0.15f * f;
                }
                else
                {
                    tiltAngle -= 0.7f * f;
                }   
            }

            // rotate
            if (Input.GetKey(KeyCode.A))
            {
                lookAngle += 0.7f * f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                lookAngle -= 0.7f * f;
            }

            rotation = Quaternion.Euler(tiltAngle, lookAngle, 0);

            if (cam.rotation != rotation)
            {
                cam.rotation = Quaternion.Lerp(cam.rotation, rotation,
                    Time.deltaTime * rotationSpeed);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                scrollDistance -= scroll * Time.deltaTime * scrollSensitivity;
                scrollDistance = Mathf.Clamp(scrollDistance, distanceMin, distanceMax);
            }
            

            if (distance != scrollDistance)
            {
                distance = Mathf.SmoothDamp(distance, scrollDistance, ref velocity, Time.deltaTime * scrollSmoothing);
            }
            
            // block
            if (!Input.GetKey(KeyCode.LeftCommand) && !Input.GetKey(KeyCode.LeftAlt))
            {
                cam.position = CalculateCameraPosition();
            }

            // pan
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                cam.Translate(cam.transform.TransformDirection(Vector3.left) * 0.075f * f, Space.World);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                cam.Translate(cam.transform.TransformDirection(Vector3.right) * 0.075f * f, Space.World);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                cam.Translate(cam.transform.TransformDirection(Vector3.up) * 0.075f * f, Space.World);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                cam.Translate(cam.transform.TransformDirection(Vector3.down) * 0.075f * f, Space.World);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {         
            lookAngle += eventData.delta.x * rotationSensitivity;
            tiltAngle -= eventData.delta.y * rotationSensitivity;
            tiltAngle = Mathf.Clamp(tiltAngle, tiltMin, tiltMax);
            rotation = Quaternion.Euler(tiltAngle, lookAngle, 0);
        }

        private Vector3 CalculateCameraPosition()
        {
            if (!freeze)
            {
                pos = robots[index].GetCamTarget().position;
            }
            return pos + cam.rotation * (Vector3.back * distance) + Vector3.up * yOffset;
        }
    }
}
