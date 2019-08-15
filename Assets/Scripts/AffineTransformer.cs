using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffineTransformer : MonoBehaviour
{

    public Painter painter;

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
            AffineTransform(1.1f, 0.1f, 0, 1, painter.ShapeModels);
        }
    }

    void AffineTransform(float a, float b, float c, float d, List<Painter.ShapeModel> shapeModels)
    {
        for (int i = 0; i < shapeModels.Count; i++)
        {
            Painter.ShapeModel shapeModel = shapeModels[i];

            // iterasi semua vertex
            for (int itVert = 0; itVert < shapeModel.Vertices.Count; itVert++)
            {
                float prevX = shapeModel.Vertices[itVert].x;
                float prevY = shapeModel.Vertices[itVert].y;

                float targetX = (a * prevX) + (c * prevY);
                float targetY = (b * prevX) + (d * prevY);

                shapeModel.Vertices[itVert] = new Vector2(targetX, targetY);
            }
        }

        painter.RenderShapes();

    }
}
