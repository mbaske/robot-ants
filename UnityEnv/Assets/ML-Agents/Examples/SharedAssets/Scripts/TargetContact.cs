﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgents
{
    /// <summary>
    /// This class contains logic for locomotion agents with joints which might make contact with a target.
    /// By attaching this as a component to those joints, their contact with the ground can be used as
    /// an observation for that agent.
    /// </summary>
    [DisallowMultipleComponent]
    public class TargetContact : MonoBehaviour
    {
        [Header("Detect Targets")] public bool touchingTarget;
        private const string Target = "target"; // Tag on target object.

        /// <summary>
        /// Check for collision with a target.
        /// </summary>
        void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(Target))
            {
                touchingTarget = true;
            }
        }

        /// <summary>
        /// Check for end of ground collision and reset flag appropriately.
        /// </summary>
        void OnCollisionExit(Collision other)
        {
            if (other.transform.CompareTag(Target))
            {
                touchingTarget = false;
            }
        }
    }
}
