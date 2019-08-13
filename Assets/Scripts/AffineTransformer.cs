using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffineTransformer : MonoBehaviour
{

    [Tooltip("Target bidang gambar")]
    public MeshRenderer targetRender;

    public bool ScaleX;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ScaleX)
        {
            ScaleX = false;
            Texture2D targetTex = (Texture2D)targetRender.material.mainTexture;
            Texture2D newTex = new Texture2D(targetTex.width, targetTex.height);
            newTex.filterMode = FilterMode.Point;
            newTex.wrapMode = TextureWrapMode.Clamp;

            //float rotation = 10 * Mathf.Deg2Rad;
            //AffineTransform(Mathf.Cos(rotation), Mathf.Sin(rotation), -1 * Mathf.Sin(rotation), Mathf.Cos(rotation), ref targetTex, ref newTex);
            AffineTransform(1, 0, 0, 2, ref targetTex, ref newTex);
            targetRender.material.mainTexture = newTex;
        }
    }

    void AffineTransform(float a, float b, float c, float d, ref Texture2D originTex, ref Texture2D targetTex)
    {
        for (int x = 0; x < originTex.width; ++x)
        {
            for (int y = 0; y < originTex.height; ++y)
            {
                int targetX = (int)(a * (float)x) + (int)(c * (float)y);
                int targetY = (int)(b * (float)x) + (int)(d * (float)y);

                if (targetX >= 0 && targetX < originTex.width && targetY >= 0 && targetY < originTex.height)
                {
                    targetTex.SetPixel(x, y, originTex.GetPixel(targetX, targetY));
                }
                else
                {
                    targetTex.SetPixel(x, y, Color.white);
                }
            }
        }
        targetTex.Apply();
    }
}
