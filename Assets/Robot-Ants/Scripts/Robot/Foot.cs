using UnityEngine;

namespace RobotAnts
{
	/// <summary>
    /// There is no exclusive gameobject associated with the Foot class.
	/// It shares its rigidbody with the lowest limb L4.
	/// The foot's position is a point "tip" relative to L4's position.
    /// </summary>
    public class Foot : MonoBehaviour
    {
        [Range(0.0f, 10.0f)]
        public float gravityMultiplier = 1f;

        [Range(0.0f, 5000.0f)]
        public float lockStrengthMultiplier = 4000f;

        [Range(0.0f, 500.0f)]
        public float lockStrengthMax = 400f;
      
		private const float CONTACT_THRESHOLD = 0.001f;

		private float distanceToGround;
        private bool contact = false;
        private Vector3 lockPos;
        private Vector3 tip;
        private Rigidbody rb;

		/// <summary>
        /// Returns the foot's distance to the ground.
        /// </summary>
        /// <returns>
		/// <c>Float</c> - value between -1 and +1.
        /// </returns>
		internal float GetDistanceToGround()
        {
			return Mathf.Clamp(distanceToGround, -1f, 1f);
        }
              
        internal void Initialize()
        {
            rb = GetComponent<Rigidbody>();
            tip = new Vector3(0.2f, 0f, 0f);
        }
      
		/// <summary>
        /// Updates the physics. 
        /// Applies a directional "lock" force to keep the foot in place.
		/// Which gives the robot a stronger grip on steep terrain compared
		/// to using gravity + colliders + friction.
        /// </summary>
        internal void StepUpdate()
        {
            Vector3 pos = transform.TransformPoint(tip);
            Vector3 ground = pos.GroundPos();
            
			if (ground.IsValid())
            {
				distanceToGround = pos.y - ground.y;

				if (distanceToGround > CONTACT_THRESHOLD)
				{
					contact = false;
				}
				else if (!contact)
				{
					contact = true;
					lockPos = ground;
				}
            }
            else
            {
                contact = false;
				distanceToGround = -1;
            }
         
            if (contact)
            {
                Vector3 delta = lockPos - pos;
                float strength = Mathf.Min(Mathf.Sqrt(delta.magnitude) * lockStrengthMultiplier, lockStrengthMax);
                rb.AddForce(delta * strength, ForceMode.VelocityChange);
            }
            else
            {
                rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
            }
        }
    }
}