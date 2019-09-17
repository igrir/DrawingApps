using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShapeCoordinateRenderer : MonoBehaviour
{

    // teks yang akan ditampilkan
    public GameObject textPrefab;

    // penampung data vertex gambar
    public Painter painter;

    // mesh filter canvas
    public MeshFilter drawMeshFilter;

    // mesh renderer canvas
    public MeshRenderer drawRenderer;

    // list objek teks yang ditampilkan
    List<GameObject> vertexPositions = new List<GameObject>();

    void Start()
    {
        // menyembunyikan teks template dari scene
        textPrefab.SetActive(false);
    }

    // memunculkan objek teks di scene
    public void ShowVertexPositions()
    {
        ClearVertexPositions();

        for (int i = 0; i < painter.ShapeModels.Count; i++)
        {
            for (int j = 0; j < painter.ShapeModels[i].Vertices.Count; j++)
            {
                var vertex = painter.ShapeModels[i].Vertices[j];

                // membuat objek teks baru
                GameObject newText = Instantiate(textPrefab) as GameObject;
                var tmp = newText.GetComponent<TextMeshPro>();
                string formattedString = string.Format("({0:0.00},{1:0.00})", vertex.x, vertex.y);
                tmp.SetText(formattedString);
                newText.SetActive(true);

                // mengatur posisi teks
                Vector3 worldPos = GetQuadPos(vertex);
                newText.transform.position = new Vector3(worldPos.x, worldPos.y, newText.transform.position.z);

                vertexPositions.Add(newText);
            }
        }
    }

    // menghapus objek teks di scene
    void ClearVertexPositions()
    {
        for (int i = 0; i < vertexPositions.Count; i++)
        {
            Destroy(vertexPositions[i]);
        }
        vertexPositions.Clear();
    }

    // mendapatkan posisi teks relatif posisi quad
    Vector2 GetQuadPos(Vector2 screenPos)
    {
        // mendapatkan vertex quad
        Vector3[] vertices = drawMeshFilter.mesh.vertices;
        Vector3 bottomLeft = drawMeshFilter.transform.TransformPoint(vertices[0]);
        Vector3 topRight = drawMeshFilter.transform.TransformPoint(vertices[1]);

        // mendapatkan besar quad
        float width = Mathf.Abs(bottomLeft.x - topRight.x);
        float height = Mathf.Abs(topRight.y - bottomLeft.y);

        // mengubah koordinat menjadi relatif dari 0 hingga 1
        float uv_x = (screenPos.x / (float)drawRenderer.material.mainTexture.width);
        float uv_y = (screenPos.y / (float)drawRenderer.material.mainTexture.height);

        // mendapatkan posisi titik relatif posisi quad
        float x = (width * uv_x) + bottomLeft.x;
        float y = (height * uv_y) + bottomLeft.y;
        return new Vector2(x, y);
    }

}