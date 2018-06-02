using UnityEngine;

namespace RobotAnts
{
	/// <summary>
	/// The Body contains all robot gameobjects except for the legs.
	/// "Body" is synonymous with "Robot" regarding most of the observed values.
    /// </summary>
    public class Body : MonoBehaviour
    {      
        [Range(0.0f, 10.0f)]
        public float angularVelocityDamper = 1f;

        [Range(0.0f, 1.0f)]
        public float anchorHeight = 0.2f;

        [Range(0.0f, 10.0f)]
        public float anchorStrength = 6f;
     
		private const float DIV_VELOCITY = 10f;

		private float xInclination;
        private float zInclination;
        private float distanceToGround;
		private float speed;
		private bool fellOver;

        private Rigidbody rb;
		private Vector3 pos;
		private Quaternion rot;
		private Material mat;
        private Color col;

		#region Observations
		/// <summary>
		/// Returns the ground coordinates at the body's position.
        /// </summary>
		/// <returns>
		/// <c>Vector3</c> - not normalized.
        /// </returns>
		internal Vector3 GetPosition()
        {
			return transform.position.GroundPos(); 
        }

		/// <summary>
		/// Returns the body's orientation in 2D space.
        /// </summary>
        /// <returns>
		/// <c>Float</c> - value between -1 and +1.
        /// </returns>
		internal float GetOrientation()
        {
			return Vector2.SignedAngle(GetForwardAxis(), Vector2.right) / -180f;
        }

		/// <summary>
		/// Returns the body's forward axis in 2D space.
        /// </summary>
        /// <returns>
		/// <c>Vector2</c> - not normalized.
        /// </returns>
		internal Vector2 GetForwardAxis()
        {
			return transform.TransformDirection(Vector3.forward).Flatten();
        }

		/// <summary>
        /// Returns the body's distance to the ground.
        /// </summary>
        /// <returns>
		/// <c>Float</c> - value between -1 and +1.
        /// </returns>
		internal float GetDistanceToGround()
        {
			return Mathf.Clamp(distanceToGround, -1f, 1f);
        }

		/// <summary>
		/// Returns the body's inclination (x/z-axis).
        /// </summary>
        /// <returns>
		/// <c>Vector2</c> - values between -2 and +2.
		/// Only values between -1 and +1 are observed since the robot
		/// is considered to have fallen over and gets reset when its
		/// inclination is beyond that range (> +/-90°).
        /// </returns>
		internal Vector2 GetInclination()
        {
			return new Vector2(xInclination, zInclination);
        }

		/// <summary>
        /// Returns the body's velocity and angular velocity in 3D space.
        /// </summary>
        /// <returns>
		/// <c>Float[]</c> - values between -1 and +1.
        /// </returns>
		internal float[] GetVelocity()
        {
			return new float[] {
                Mathf.Clamp(rb.velocity.x / DIV_VELOCITY, -1f, 1f),
                Mathf.Clamp(rb.velocity.y / DIV_VELOCITY, -1f, 1f),
                Mathf.Clamp(rb.velocity.z / DIV_VELOCITY, -1f, 1f),
                Mathf.Clamp(rb.angularVelocity.x / DIV_VELOCITY, -1f, 1f),
                Mathf.Clamp(rb.angularVelocity.y / DIV_VELOCITY, -1f, 1f),
                Mathf.Clamp(rb.angularVelocity.z / DIV_VELOCITY, -1f, 1f)
            };
        }

		/// <summary>
        /// Returns <c>true</c> if the body/robot fell over. 
        /// </summary>
        /// <returns>
		/// <c>bool</c>
        /// </returns>
		internal bool FellOver()
        {
			return fellOver || Mathf.Abs(xInclination) > 1f || Mathf.Abs(zInclination) > 1f;
        }
        
		/// <summary>
		/// Returns the body's speed (forward direction velocity).
        /// </summary>
        /// <returns>
		/// <c>Float</c> - not normalized.
        /// </returns>
        internal float GetSpeed()
        {
			// not normalized
            return Mathf.Max(0f, transform.InverseTransformDirection(rb.velocity).z);
        }
		#endregion  


        internal void Initialize()
        {
            rb = GetComponent<Rigidbody>();
            rb.solverVelocityIterations = 1;
            rb.solverIterations = 100;
			mat = transform.Find("Abdomen").GetComponent<Renderer>().material;
            col = mat.color;
        }
        
		internal void SavePosition()
        {         
			pos = transform.position;
            rot = transform.rotation;
        }

        internal void ResetPosition()
        {
            transform.position = pos;
            transform.rotation = rot;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
			fellOver = false;
        }

		internal void DisplayEnergy(float e)
		{
			col.g = e > -0.5f ? (e + 0.5f) / 1.5f : 0f;
			col.r = e < 0.5f ? (-e + 0.5f) / 1.5f : 0f;
			col.b = 0f;
            mat.color = col;
        }
              
		/// <summary>
        /// Updates the physics. 
		/// Applies a directional "anchor" force to stabilize the robot on steep terrain.
        /// </summary>
		internal void StepUpdate()
        {
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, 10f, 1 << 8))
            {
                distanceToGround = transform.position.y - hit.point.y;
                rb.AddTorque(-rb.angularVelocity * angularVelocityDamper, ForceMode.VelocityChange);
                float f = Mathf.Sqrt(Mathf.Max(0, distanceToGround - anchorHeight)) * anchorStrength;
                rb.AddForce(-hit.normal * f, ForceMode.VelocityChange);
				fellOver = false;
            }
            else
            {
				fellOver = true;
				distanceToGround = -1;
            }
            
            Vector3 r = transform.rotation.eulerAngles;
            xInclination = (r.x > 180f ? r.x - 360f : r.x) / 90f;
            zInclination = (r.z > 180f ? r.z - 360f : r.z) / 90f;         
        }
    }
}