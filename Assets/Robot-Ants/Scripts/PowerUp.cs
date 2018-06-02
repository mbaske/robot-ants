using UnityEngine;

namespace RobotAnts
{
    public class PowerUp : MonoBehaviour
    {
        internal static float OFFSET = -0.25f;

        public bool animate = true;

        private Material mat;
        private Color col;
        private Vector3 pos;
        private float y;

        void Start()
        {
            mat = GetComponent<Renderer>().material;
            col = Color.green;
            pos = transform.position.GroundPos(0f, OFFSET);
            transform.position = pos;
            y = pos.y;
        }

        void Update()
        {
            if (animate)
            {
                float pp = Mathf.PingPong(Time.time, 0.5f);
                col.g = pp + 0.2f;
                pos.y = y + pp / 3f;
                transform.position = pos;
                mat.SetColor("_EmissionColor", col);
            }
        }
    }
}