using UnityEngine;
using Unity.Collections;
using System.Linq;

public class Terrain : MonoBehaviour
{
    public NativeArray<Color32> Colors;
    public int Length { get; private set; }
    public float Scale { get; private set; }

    private const int meshLength = 256;

    private Texture2D tex;
    private Material mat;

    public void Initialize()
    {
        mat = GetComponent<Renderer>().material;
        Length = mat.mainTexture.width;
        Scale = Length / meshLength;
        tex = new Texture2D(Length, Length, TextureFormat.RGBA32, false);
    }

    public void ClearTexture()
    {
        tex.SetPixels32(Enumerable.Repeat(new Color32(255, 255, 255, 255), Length * Length).ToArray(), 0);
        mat.mainTexture = tex;
        Colors = tex.GetRawTextureData<Color32>();
    }

    public void UpdateTexture()
    {
        tex.Apply();
    }
}
