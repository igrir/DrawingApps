using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffineTransformer : MonoBehaviour
{

    public float a;
    public float b;
    public float c;
    public float d;

    public Painter painter;

    // matrix:
    // a c
    // b d
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

    public void ExecuteAffineTransformation(float a, float b, float c, float d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;

        AffineTransform(this.a, this.b, this.c, this.d, painter.ShapeModels);
    }

    public void ExecuteAffineTransformation()
    {
        AffineTransform(this.a, this.b, this.c, this.d, painter.ShapeModels);
    }
}