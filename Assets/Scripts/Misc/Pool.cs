using System.Collections.Generic;
using UnityEngine;

namespace MBaske
{
    public abstract class Pool<T> : MonoBehaviour where T : Poolable
    {
        [SerializeField]
        private T[] m_Prefabs;
        [SerializeField]
        private List<int> m_Capacities = new List<int>();

        private Stack<T>[] m_Inactive;
        protected IList<T>[] m_Active;

        private void OnValidate()
        {
            if (m_Prefabs != null)
            {
                int nP = m_Prefabs.Length;
                int nC = m_Capacities.Count;

                if (nP > nC)
                {
                    for (int i = nC; i < nP; i++)
                    {
                        m_Capacities.Add(64);
                    }
                }
                else if (nC > nP)
                {
                    for (int i = nC; i > nP; i--)
                    {
                        m_Capacities.RemoveAt(m_Capacities.Count - 1);
                    }
                }
            }
        }

        private void Awake()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            int n = m_Prefabs.Length;
            m_Active = new IList<T>[n];
            m_Inactive = new Stack<T>[n];

            for (int i = 0; i < n; i++)
            {
                int c = m_Capacities[i];
                m_Active[i] = new List<T>(c);
                m_Inactive[i] = new Stack<T>(c);

                for (int j = 0; j < c; j++)
                {
                    m_Inactive[i].Push(NewInstance(i));
                }
            }
        }

        public T Spawn(Vector3 position, int prefabIndex = 0)
        {
            T obj = Spawn(prefabIndex);
            obj.transform.position = position;

            return obj;
        }

        public T Spawn(Vector3 position, Quaternion rotation, int prefabIndex = 0)
        {
            T obj = Spawn(prefabIndex);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            return obj;
        }

        public T Spawn(int prefabIndex = 0)
        {
            T obj = m_Inactive[prefabIndex].Count > 0
                ? m_Inactive[prefabIndex].Pop()
                : NewInstance(prefabIndex);

            obj.gameObject.SetActive(true);
            m_Active[prefabIndex].Add(obj);
            obj.OnSpawn();

            return obj;
        }

        public void DiscardAll()
        {
            for (int i = 0; i < m_Active.Length; i++)
            {
                DiscardAll(i);
            }
        }

        public void DiscardAll(int prefabIndex)
        {
            var tmp = new List<T>(m_Active[prefabIndex]);
            foreach (var obj in tmp)
            {
                Discard(obj);
            }
        }

        public void Discard(T obj)
        {
            obj.Discard();
        }

        protected void OnDiscard(Poolable obj)
        {
            obj.gameObject.SetActive(false);
            m_Inactive[obj.PrefabIndex].Push((T)obj);
            m_Active[obj.PrefabIndex].Remove((T)obj);
        }

        private T NewInstance(int prefabIndex)
        {
            T obj = Instantiate(m_Prefabs[prefabIndex], transform);
            obj.gameObject.SetActive(false);
            obj.DiscardEvent += OnDiscard;
            obj.PrefabIndex = prefabIndex;
            return obj;
        }
    }
}