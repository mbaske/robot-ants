using UnityEngine;

namespace RobotAnts
{
    public class Stats : MonoBehaviour
    {
        private Texture2D tex;
        private GUIStyle barStyle;
		private Robot[] robots;
        private Color bg;
        private Color eCol;
        private Color dCol;
        private int n;

        private void Start()
        {
            bg = new Color(0f, 0f, 0f, 0.5f);
            eCol = new Color(1f, 1f, 0f, 1f);
            dCol = new Color(0f, 0.5f, 1f, 1f);
            tex = Texture2D.whiteTexture;
            barStyle = new GUIStyle { normal = new GUIStyleState { background = tex } };         
            GameObject[] g = GameObject.FindGameObjectsWithTag("Agent");
            n = g.Length;
			robots = new Robot[n];
            for (int i = 0; i < n; i++)
				robots[i] = g[i].GetComponent<Robot>();
        }

		private void OnGUI()
		{
            float e = 0f;
            float eMean = 0f;
            float d = 0f;
            float dMean = 0f;

            for (int i = 0; i < n; i++)
            {
                DrawRect(new Rect(0f, i * 4, 80f, 4f), bg);

				e = robots[i].GetEnergy();
                eMean += e;
                SetColor(e);
                DrawRect(new Rect(0f, i * 4f, (e + 1f) * 40f, 2f), eCol);

				d = robots[i].GetDistanceCovered();
                dMean += d;
                DrawRect(new Rect(0f, i * 4f + 2f, d * 80f, 1f), dCol);
            }
            DrawRect(new Rect(0f, n * 4f, 80f, 10f), bg);

            eMean /= n;
            SetColor(eMean);
            DrawRect(new Rect(0f, n * 4f, (eMean + 1f) * 40f, 6f), eCol);
            dMean /= n;
            DrawRect(new Rect(0f, n * 4f + 6f, dMean * 80f, 4f), dCol);
		}

        private void DrawRect(Rect position, Color color)
        {
            GUI.backgroundColor = color;
            GUI.Box(position, GUIContent.none, barStyle);
        }

        private void SetColor(float e)
        {
            eCol.g = e > -0.5f ? (e + 0.5f) / 1.5f : 0f;
            eCol.r = e < 0.5f ? (-e + 0.5f) / 1.5f : 0f;
        }
	}
}