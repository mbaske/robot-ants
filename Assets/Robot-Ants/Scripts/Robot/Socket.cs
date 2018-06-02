using UnityEngine;

namespace RobotAnts
{
	public class Socket : MonoBehaviour
	{
		private Rigidbody rb;
		private Vector3 pos;
		private Quaternion rot;

		internal void Initialize()
		{
			rb = GetComponent<Rigidbody>();
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
		}
	}
}