using UnityEngine;

namespace RobotAnts
{
    public class TrailBlob : MonoBehaviour
    {
        // Seconds
        public int maxLifeSpan = 40;

        private float energy;
        private Material mat;
        private Color col;
      
        /// <summary>
        /// Sets the blob's initial energy.
        /// </summary>
        /// <param name="e">A float value between 0 and +1.</param>
        internal void SetEnergy(float e = 1f)
        {
            energy = e * maxLifeSpan;
            DisplayEnergy();
        }
        
        /// <summary>
        /// Returns the blob's energy.
        /// </summary>
        /// <returns>
        /// <c>Float</c> - value between 0 and +1.
        /// </returns>
        internal float GetEnergy()
        {
            return energy / maxLifeSpan;
        }

        private void DisplayEnergy()
        {
            col.a = GetEnergy();
            mat.color = col;
        }
        
        private void Awake()
        {
            mat = GetComponent<Renderer>().material;
            col = new Color(0f, 1f, 0.5f, 1f);
        }

        private void Update()
        {
            energy -= Time.deltaTime;

            if (energy > 0f)
                DisplayEnergy();
            else
                PoolManager.ReleaseObject(gameObject);
        }
    }
}
