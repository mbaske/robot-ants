using UnityEngine;
using System.Collections;
using System;

namespace RobotAnts
{
	public class Robot : MonoBehaviour
	{
		public Body body;
        public Leg[] legs = new Leg[6];
      
        // Seconds
		public float timeout = 5f; 
		// From energy +1 to 0
        public float depletionTime = 15f;

        // Unity standard units
		public float searchRadius = 3f;
        public float targetThreshold = 0.5f;
        public float powerupThreshold = 0.5f;      
		public float trailDensity = 0.5f; 

        public int numWayPoints = 10;

        // Divider will yield deltas between approx. -0.5 to +0.5
		// Deltas of -1 are used to flag off terrain scans.
        private const float DIV_ELEVATION_DELTA = 10f;

		private const int N_SCAN_POINTS = 8;

        // Y at ground level
		private Vector3 position;      
        private Vector3 targetPosition;
		private Vector3 powerupPos;
        private Vector3 prevTrailPos;      

        // 2D
		private Vector2 forwardAxis;

		// 2D
		private float targetAngle; 
		// 3D
		private float targetDistance; 
		private float prevTargetDistance;   

		// 2D
		private float distanceCovered;
		private Queue wayPoints;

		private float trailStrength;
		// 2D
		private float trailDirection;     

		private float[] elevationDeltas;           
		private float degScan;
        private float energy;      
		private bool isAtTarget;
        private bool foundPowerup;
        private Collider[] colliders;
		// Callbacks on AgentExplore
		private Action callbackSuccess;
		private Action callbackFail;

		#region Observations
		/// <summary>
        /// Returns the robot's orientation in 2D space.
        /// </summary>
        /// <returns>
        /// <c>Float</c> - value between -1 and +1.
        /// </returns>
		internal float GetOrientation()
        {
			return body.GetOrientation();
        }

		/// <summary>
        /// Returns the robot's distance to the current target position.
        /// </summary>
        /// <returns>
        /// <c>Float</c> - value between -1 and +1.
		/// +1 -> at target, 0 -> search radius away from target (on level ground).
        /// </returns>
        internal float GetTargetDistance()
        {
			return Mathf.Max(-1f, 1f - targetDistance / searchRadius);
        }

		/// <summary>
		/// Returns the robot's relative angle (2D) towards the current target position.
        /// </summary>
        /// <returns>
        /// <c>Float</c> - value between -1 and +1.
        /// </returns>
        internal float GetTargetAngle()
        {
            return targetAngle;
        }

		/// <summary>
        /// Returns the robot's energy level.
        /// </summary>
        /// <returns>
        /// <c>Float</c> - value between -1 and +1.
        /// </returns>
        internal float GetEnergy()
		{
			return Mathf.Max(-1f, energy / depletionTime);
		}
        
		/// <summary>
        /// Returns trail info if blobs or power-ups were detected.
        /// </summary>
        /// <returns>
        /// <c>Float[]</c> - values between -1 and +1.
		/// Trail strength -1 flags no trail/power-up present.
		/// Trail direction is the robot's relative angle (2D) towards the trail	
		/// center or power-up position.
        /// </returns>
		internal float[] GetTrailInfo()
        {
			return new float[] { trailStrength, trailDirection };
        }
      
		/// <summary>
		/// Returns the elevation differences between the robot's position
		/// and 8 points on the boundary of its search radius.
        /// </summary>
        /// <returns>
        /// <c>Float[]</c> - values between -1 and +1.
		/// -1 flags off terrain scan (no raycast hit).
        /// </returns>
		internal float[] GetElevation()
        {
			return elevationDeltas;
        }
        
		/// <summary>
		/// Returns the distance covered by the robot as a fraction:
		/// Summed distances between n consecutive waypoints / (n - 1) * search radius.
        /// </summary>
        /// <returns>
        /// <c>Float[]</c> - value between 0 and +1.
        /// </returns>
		internal float GetDistanceCovered()
        {
            return distanceCovered;
        }
		#endregion
      
		internal float GetRewardWalk()
        {
			float penaltyDistance = (targetDistance / searchRadius) * 0.05f;

            float rewardMovement = 0f;
            if (prevTargetDistance > 0f)
            {
                rewardMovement = Mathf.Clamp((prevTargetDistance - targetDistance) * 5f, -1f, 1f);
                // Focus direction
                rewardMovement *= (rewardMovement > 0f ? 1f - Mathf.Abs(targetAngle) : 1f);
            }
			prevTargetDistance = targetDistance;

            return rewardMovement - penaltyDistance;
        }

		internal float GetRewardExplore()
        {
			return distanceCovered + GetEnergy() * 0.3f;
        }

		internal Transform GetCamTarget()
        {
            return body.transform;
        }

		   
		// Callbacks in AgentExplore
		internal void AddCallbacks(Action success, Action fail)
        {
            callbackSuccess = success;
            callbackFail = fail;
        }
      

		/// <summary>
		/// Called by AgentExplore.
        /// Sets the new target position in relation to the robot.
        /// </summary>
		/// <param name="angle">Float value between -1 and +1, relative angle 
		/// towards the target, 0 -> straight ahead.</param>
		/// <param name="distance">Float value between -1 and +1 (sign is ignored), 
		/// relative distance to target, 0 -> search radius.</param>
        internal void SetTargetPosition(float angle, float distance)
        {         
			angle = (body.GetOrientation() + Mathf.Clamp(angle, -1f, 1f)) * Mathf.PI;
            distance = (1f - Mathf.Min(Mathf.Abs(distance), 1f)) * searchRadius; 
         
            Vector3 pos = position;
            pos.x += distance * Mathf.Cos(angle);
            pos.z += distance * Mathf.Sin(angle);
            pos = pos.GroundPos();
            
            if (pos.IsValid())
            {
                isAtTarget = false;
                Invoke("NotifyFail", timeout);
                SetTargetPosition(pos);
            }
            else
            {
                // Target is off terrain.
                NotifyFail();
            }
        }

		/// <summary>
        /// Sets the new target position.
        /// </summary>
        /// <param name="pos">Vector3, y is assumed to be at ground level.</param>
        internal void SetTargetPosition(Vector3 pos)
        {
            targetPosition = pos;
            Debug.DrawLine(position, pos, Color.red, 1f);
        }


		/// <summary>
		/// Called by AgentWalk.
		/// StepUpdate is used instead of FixedUpdate for explicit
		/// control over the invocation order: Agent -> Robot -> Body/Feet.
        /// </summary>
		internal bool StepUpdate()
        {
            body.StepUpdate();
            for (int i = 0; i < 6; i++)
                legs[i].foot.StepUpdate();

			if (body.FellOver())
			{
				ResetDefaults(true);
				NotifyFail();
				return false;
			}

			position = body.GetPosition();
			forwardAxis = body.GetForwardAxis();
			targetDistance = Vector3.Distance(position, targetPosition);
			targetAngle = VectorUtil.NormAngle2D(position, targetPosition, forwardAxis);

			if (foundPowerup && Vector3.Distance(position, powerupPos) < powerupThreshold)
            {
                SetEnergy(1f);
            }

            if (targetDistance > targetThreshold)
            {
                isAtTarget = false;
            }
            else if (!isAtTarget)
            {                  
                isAtTarget = true;
                // Cancel timeout
                CancelInvoke();
                ScanElevation();
                TrackDistance();
                SearchVicinity();
                // Callback AgentExplore
                callbackSuccess();
            }         

			return true;
		}
      
		private void NotifyFail()
        {
			// Callback AgentExplore
			// Robot fell over or timed out or target position was off terrain.
			callbackFail();
        }


		private void ScanElevation()
        {
            Vector3 pos;
			float angle = body.GetOrientation() * Mathf.PI;
            for (int i = 0; i < N_SCAN_POINTS; i++)
            {
                pos = position;
				pos = new Vector3(pos.x + searchRadius * Mathf.Cos(angle + i * degScan * Mathf.Deg2Rad),
                                  pos.y,
				                  pos.z + searchRadius * Mathf.Sin(angle + i * degScan * Mathf.Deg2Rad));
                pos = pos.GroundPos();
            
                if (pos.IsValid())
                {
                    float delta = (pos.y - position.y) / DIV_ELEVATION_DELTA;
                    elevationDeltas[i] = Mathf.Clamp(delta, -1f, 1f);
                }
                else
                {
                    elevationDeltas[i] = -1;
                }
            }
        }

        private void TrackDistance(bool reset = false)
        {
			if (reset)
			{
				wayPoints.Clear();
			}
            else if (wayPoints.Count == numWayPoints)
			{
				wayPoints.Dequeue();
			}

			Vector2 pos = targetPosition.Flatten();
            wayPoints.Enqueue(pos);
            distanceCovered = Mathf.Min(1f,
                                        Vector2.Distance((Vector2)wayPoints.Peek(), pos)
                                        / (searchRadius * wayPoints.Count));
        }

        private void SearchVicinity()
        {
            trailStrength = -1f;
            trailDirection = 0f;
            // Ignore trail when there's a power-up nearby.
            // Assuming there's never more than 1 power-up within searchRadius of the agent.
            int n = Physics.OverlapSphereNonAlloc(position, searchRadius, colliders, 1 << 10, QueryTriggerInteraction.Collide);
            foundPowerup = n > 0;
            if (foundPowerup)
            {
                powerupPos = colliders[0].transform.position.OffsetY(-PowerUp.OFFSET);
				trailDirection = VectorUtil.NormAngle2D(position, powerupPos, forwardAxis);
                trailStrength = 1f;
				Debug.DrawLine(position, powerupPos, Color.cyan, 1f);
            }
            else
            {
                n = Physics.OverlapSphereNonAlloc(position, searchRadius, colliders, 1 << 9, QueryTriggerInteraction.Collide);
                if (n > 0)
                {
                    float[] strength = new float[n];
                    float minStrength = Mathf.Infinity;
                    float maxStrength = Mathf.NegativeInfinity;
                    Vector3[] pos = new Vector3[n];
                    for (int i = 0; i < n; i++)
                    {
                        pos[i] = colliders[i].transform.position;
                        strength[i] = colliders[i].gameObject.GetComponent<TrailBlob>().GetEnergy();
                        minStrength = Mathf.Min(minStrength, strength[i]);
                        maxStrength = Mathf.Max(maxStrength, strength[i]);
                        trailStrength += strength[i];
                    }
                    // Spread out strength distribution to get a stronger sense of direction.
                    float spread = 1f / (maxStrength - minStrength);
                    Vector3 centroid = Vector3.zero;
                    for (int i = 0; i < n; i++)
                    {
                        Debug.DrawLine(position, pos[i], Color.blue, 1f);
                        centroid += Vector3.Lerp(position, pos[i], (strength[i] - minStrength) * spread);
                    }
                    centroid /= n;
					trailStrength /= n;               
					trailDirection = VectorUtil.NormAngle2D(position, centroid, forwardAxis);
					Debug.DrawLine(position, centroid, Color.cyan, 1f);
                }
            }
        }

		private void Update()
        {
            energy -= Time.deltaTime;    
			body.DisplayEnergy(GetEnergy());

            if (energy > 0f)
			{
				LeaveEnergyTrail();
			}         
        }

        private void SetEnergy(float e)
        {
            energy = e * depletionTime;
			body.DisplayEnergy(GetEnergy());
        }

        private void LeaveEnergyTrail()
        {
            if (Vector3.Distance(prevTrailPos, position) > trailDensity)
            {
                prevTrailPos = position;
                TrailBlob blob = PoolManager.SpawnBlob(position).GetComponent<TrailBlob>();
				blob.SetEnergy(GetEnergy());
            }
        }      

		private void Awake()
        {
            wayPoints = new Queue();
            colliders = new Collider[100];
            elevationDeltas = new float[N_SCAN_POINTS];
            degScan = 360f / N_SCAN_POINTS;

            body.Initialize();
            for (int i = 0; i < 6; i++)
                legs[i].Initialize();

            SavePosition();
            ResetDefaults();
        }

		private void ResetDefaults(bool resetPosition = false)
        {
            if (resetPosition)
            {
                body.ResetPosition();
                for (int i = 0; i < 6; i++)
                    legs[i].ResetPosition();
            }

			position = body.GetPosition();
			forwardAxis = body.GetForwardAxis();
            targetPosition = position;
            prevTrailPos = position;
            targetDistance = 0f;
            prevTargetDistance = 0f;
            targetAngle = 0f;
            isAtTarget = true;         
			SetEnergy(-1f);
            TrackDistance(true);
            ScanElevation();
            SearchVicinity();
        }

        private void SavePosition()
        {
            body.SavePosition();
            for (int i = 0; i < 6; i++)
                legs[i].SavePosition();
        }
	}
}