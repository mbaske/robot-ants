using UnityEngine;

namespace RobotAnts
{
    public class Limb : MonoBehaviour
    {     
        private float min;
        private float max;
		private float mid;
        private float relMin;
        private float range;
		private float rotation;
		private bool isL2;
        private ConfigurableJoint joint;
        private Transform connected;
		private Rigidbody rb;
        private Vector3 pos;
        private Quaternion rot;

		internal void Initialize(float relativeMin)
        {
			rb = GetComponent<Rigidbody>();
			JointDrive jd = new JointDrive
			{
				positionSpring = 1e+5f,
				maximumForce = 1e+5f,
				positionDamper = 0f
			};
			joint = GetComponent<ConfigurableJoint>();
            joint.angularXDrive = jd;
            connected = joint.connectedBody.transform;
            isL2 = joint.axis == Vector3.up;
         
            min = joint.lowAngularXLimit.limit;
            max = joint.highAngularXLimit.limit;
            relMin = relativeMin;
            range = (max - min) / 2f;
			mid = min + range;         
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
			joint.targetRotation = Quaternion.AngleAxis(mid, Vector3.right);
        }
      
		/// <summary>
		/// Returns the limb's rotation.
        /// </summary>
        /// <returns>
		/// <c>Float</c> - value between -1 and +1.
        /// </returns>
		internal float GetRotation()
        {
			Quaternion r = Quaternion.Inverse(transform.rotation) * connected.rotation;
            float angle = isL2 ? r.eulerAngles.y : r.eulerAngles.z;
            angle = (angle > 180f ? angle - 360f : angle) - relMin;
            return Mathf.Clamp(angle / range - 1f, -1f, 1f);
        } 

		/// <summary>
        /// Sets the limb's rotation.
        /// </summary>
        /// <param name="r">Float value between -1 and +1.</param>
        internal void SetRotation(float r)
        {
            rotation = mid + Mathf.Clamp(r, -1f, 1f) * range;
            joint.targetRotation = Quaternion.AngleAxis(rotation, Vector3.right);
        }


		//public float maxDegreesPerFrame = 10f;

        //internal void SetTorque(float t)
        //{
        //    rotation = Mathf.Clamp(rotation + Mathf.Clamp(t, -1f, 1f) * maxDegreesPerFrame, min, max);
        //    joint.targetRotation = Quaternion.AngleAxis(rotation, Vector3.right);
        //}
    }
}