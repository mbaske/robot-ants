using UnityEngine;
using System.Collections.Generic;

namespace MBaske
{
    public class ResettableItem
    {
        private readonly Vector3 m_Position;
        private readonly Quaternion m_Rotation;
        private readonly Transform m_Transform;
        private readonly Rigidbody m_Rigidbody;
        private readonly ConfigurableJoint m_Joint;

        public ResettableItem(Transform tf)
        {
            m_Transform = tf;
            m_Position = tf.localPosition;
            m_Rotation = tf.localRotation;
            m_Rigidbody = tf.GetComponent<Rigidbody>();
            m_Joint = tf.GetComponent<ConfigurableJoint>();
        }

        public void Reset()
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
                m_Rigidbody.Sleep();
            }

            if (m_Joint != null)
            {
                m_Joint.targetPosition = Vector3.zero;
                m_Joint.targetRotation = Quaternion.identity;
            }

            m_Transform.localPosition = m_Position;
            m_Transform.localRotation = m_Rotation;
        }
    }

    public class Resetter
    {
        private List<ResettableItem> items;

        public Resetter(Transform tf)
        {
            items = new List<ResettableItem>();
            Add(tf);
        }

        public void Reset()
        {
            foreach (ResettableItem item in items)
            {
                item.Reset();
            }
        }

        private void Add(Transform tf)
        {
            items.Add(new ResettableItem(tf));

            for (int i = 0; i < tf.childCount; i++)
            {
                Add(tf.GetChild(i));
            }
        }
    }
}