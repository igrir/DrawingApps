using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ScanLineFill : MonoBehaviour
{

    // Target texture yang akan digambar
    public Texture2D targetTex;

    // Struktur data penampung nilai edge
    [System.Serializable]
    public class EdgeBucket
    {
        public int yMax = 0;
        public int yMin = 0;
        public int xMax = 0;
        public int xMin = 0;
        public float sign = 0;
        public float dX = 0;
        public float dY = 0;
        public int sum = 0;
    }

    public List<EdgeBucket> EdgeTable = new List<EdgeBucket>();
    public List<EdgeBucket> ActiveList = new List<EdgeBucket>();

    public Gradient gradient;

    // Membersihkan edge table
    public void Clear()
    {
        EdgeTable.Clear();
    }


    // menambah data edge
    public void AddEdge(int x1, int y1, int x2, int y2)
    {
        EdgeBucket edgeBucket = new EdgeBucket();

        // menentukan titik maksimum dan minimum
        if (y1 > y2)
        {
            edgeBucket.yMax = y1;
            edgeBucket.xMax = x1;

            edgeBucket.yMin = y2;
            edgeBucket.xMin = x2;
        }
        else
        {
            edgeBucket.yMax = y2;
            edgeBucket.xMax = x2;

            edgeBucket.yMin = y1;
            edgeBucket.xMin = x1;
        }

        edgeBucket.sum = x1;

        float dx = (float)x2 - (float)x1;
        float dy = (float)y2 - (float)y1;

        float m = (float)dy / (float)dx;

        // menentukan arah garis berdasarkan nilai gradiennya
        if (m < 0)
        {
            edgeBucket.sign = -1;
        }
        else
        {
            edgeBucket.sign = 1;
        }

        edgeBucket.dX = Mathf.Abs(dx);
        edgeBucket.dY = Mathf.Abs(dy);

        // hanya garis yang memiliki jarak y yang dimasukkan,
        // garis horizontal tidak perlu dimasukkan
        if (Mathf.Abs(edgeBucket.dY) > 0)
        {
            this.EdgeTable.Add(edgeBucket);
        }
    }



    public void ProcessEdgeTable()
    {
        if (EdgeTable.Count <= 0)
            return;

        // menyusun edge table berdasarkan nilai yMin
        EdgeTable.Sort((a, b) => a.yMin.CompareTo(b.yMin));

        int y = EdgeTable[0].yMin;

        // mendapatkan posisi minimum dan maksimum semua edge
        // untuk menentukan batas scan
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        for (int itET = 0; itET < EdgeTable.Count; itET++)
        {
            EdgeBucket bucket = EdgeTable[itET];

            if (bucket.xMin <= minX)
                minX = bucket.xMin;

            if (bucket.xMax <= minX)
                minX = bucket.xMax;

            if (bucket.xMax >= maxX)
                maxX = bucket.xMax;

            if (bucket.xMin >= maxX)
                maxX = bucket.xMin;

            // mencari batas y
            if (bucket.yMin <= minY)
                minY = bucket.yMin;

            if (bucket.yMax <= minY)
                minY = bucket.yMax;

            if (bucket.yMax >= maxY)
                maxY = bucket.yMax;

            if (bucket.yMin >= maxY)
                maxY = bucket.yMin;
        }

        // proses scan
        while (EdgeTable.Count > 0)
        {
            // masukkan edge ke active list jika posisi yMin == y
            for (int itET = 0; itET < EdgeTable.Count; itET++)
            {
                EdgeBucket currentBucket = EdgeTable[itET];

                if (currentBucket.yMin == y && currentBucket.dY != 0)
                {
                    ActiveList.Add(currentBucket);
                }
            }

            // sort edges di Active List berdasarkan nilai x
            ActiveList.Sort((a, b) => a.xMin.CompareTo(b.xMin));

            // update active list x
            for (int itAL = 0; itAL < ActiveList.Count; itAL++)
            {
                EdgeBucket currentBucket = ActiveList[itAL];

                // ubah nilai x berdasarkan gradiennya
                if (currentBucket.dX != 0)
                {
                    float calcX = (float)(y - currentBucket.yMin) / (currentBucket.dY / currentBucket.dX);

                    currentBucket.sum = (int)(currentBucket.xMin + currentBucket.sign * calcX);
                }
            }

            // isi scanline di antara pasangan edges dalam Active List
            List<Vector2Int> edgePair = new List<Vector2Int>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int i = 0; i < ActiveList.Count; i++)
                {
                    EdgeBucket activeBucket = ActiveList[i];
                    if (activeBucket.sum == x)
                    {
                        // masukkan posisi titik perpotongan scanline
                        edgePair.Add(new Vector2Int(x, y));
                    }


                    if (edgePair.Count >= 2)
                    {
                        var pair1 = edgePair[0];
                        var pair2 = edgePair[1];

                        for (int itPair = pair1.x; itPair <= pair2.x; itPair++)
                        {
                            Color previousColor = targetTex.GetPixel(itPair, y);

                            // warnai kolom di antara dua garis potong
                            Color color = gradient.Evaluate(Mathf.InverseLerp(minY, maxY, y));
                            color = color + previousColor * (1f - color.a);
                            targetTex.SetPixel(itPair, y, color);
                        }
                        edgePair.Clear();
                    }
                }
            }

            // lakukan proses ke baris selanjutnya
            y++;

            // hilangkan dari list jika sudah menyentuh titik ujung paling atas
            for (int itAL = ActiveList.Count - 1; itAL >= 0; itAL--)
            {
                EdgeBucket activeEdge = ActiveList[itAL];
                if (y == activeEdge.yMax)
                {
                    EdgeTable.Remove(activeEdge);
                    ActiveList.Remove(activeEdge);
                }
            }
        }
    }

}
