using UnityEngine;
using System.Collections.Generic;

public class Painter : MonoBehaviour
{

    const int TEXTURE_WIDTH = 100;
    const int TEXTURE_HEIGHT = 100;

    [Tooltip("Target bidang gambar")]
    public MeshRenderer targetRender;

    [Tooltip("Target bidang yang sedang digunakan")]
    public MeshRenderer tempTargetRender;

    // Camera utama
    Camera cam = default;

    Vector3 startDownPos;
    Vector3 lastPixelPosition;
    Vector3 lastMouseUpPos;
    Vector3 startTrianglePos;

    int lineCount = 0;

    public enum DrawingMode
    {
        Dot,
        Line,
        Triangle,
        Rectangle
    }
    public DrawingMode CurrentDrawingMode = DrawingMode.Dot;



    void Start()
    {
        // Mendapatkan camera utama
        cam = Camera.main;

        SetDefaultTexture();
        SetDefaultTemporaryTexture();
    }

    void SetDefaultTexture()
    {
        // Target texture yang akan digambar
        Texture2D targetTexture = null;

        // Setup texture yang akan digambar
        targetTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT);
        targetTexture.filterMode = FilterMode.Point;
        targetTexture.wrapMode = TextureWrapMode.Clamp;

        // Beri texture secara default berwarna putih
        Color[] cols = targetTexture.GetPixels();
        for (int i = 0; i < cols.Length; ++i)
        {
            cols[i] = Color.white;
        }

        // Set pengaturan texture
        targetTexture.SetPixels(cols);
        targetTexture.Apply();

        // Buat material gambar tidak terpengaruh oleh cahaya
        targetRender.material = new Material(Shader.Find("Unlit/Texture"));
        targetRender.material.mainTexture = targetTexture;
    }

    void SetDefaultTemporaryTexture()
    {
        // Target texture yang akan digambar
        Texture2D targetTexture = null;

        // Setup texture yang akan digambar
        targetTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT);
        targetTexture.filterMode = FilterMode.Point;
        targetTexture.wrapMode = TextureWrapMode.Clamp;

        // Beri texture secara default berwarna transparent
        Color transparentColor = new Color(1, 1, 1, 0);
        Color[] cols = targetTexture.GetPixels();
        for (int i = 0; i < cols.Length; ++i)
        {
            cols[i] = transparentColor;
        }

        // Set pengaturan texture
        targetTexture.SetPixels(cols);
        targetTexture.Apply();

        // Buat material gambar tidak terpengaruh oleh cahaya
        tempTargetRender.material = new Material(Shader.Find("Unlit/Transparent"));
        tempTargetRender.material.mainTexture = targetTexture;
    }


    void Update()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        // jangan proses jika posisi mouse masih sama dengan posisi terakhir
        if (Vector3.Equals(hit.textureCoord, lastPixelPosition))
            return;

        Vector2 pixelUV = hit.textureCoord;

        Texture2D tex = rend.material.mainTexture as Texture2D;

        Texture2D temporaryTexture = (Texture2D)tempTargetRender.material.mainTexture;
        Texture2D targetTexture = (Texture2D)targetRender.material.mainTexture;

        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;


        // Menerima input mouse pertama ditekan
        if (Input.GetMouseButtonDown(0))
        {
            startDownPos = pixelUV;

            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Triangle:

                    // posisi awal menekan mouse
                    if (lineCount == 0)
                    {
                        startTrianglePos = startDownPos;
                    }
                    break;
            }
        }

        // Menerima input mouse sedang ditekan
        if (Input.GetMouseButton(0))
        {

            switch (this.CurrentDrawingMode)
            {
                case DrawingMode.Dot:
                    // tidak perlu menggunakan temporary target, langsung render saja di objek tujuan
                    tempTargetRender.gameObject.SetActive(false);

                    DrawDot(ref tex, (int)pixelUV.x, (int)pixelUV.y);
                    break;
                case DrawingMode.Line:

                    tempTargetRender.gameObject.SetActive(true);

                    if (hit.transform == tempTargetRender.transform)
                    {
                        ClearColor(ref temporaryTexture);
                        DrawBresenhamLine(ref temporaryTexture, (int)startDownPos.x, (int)startDownPos.y, (int)pixelUV.x, (int)pixelUV.y);
                    }
                    break;
                case DrawingMode.Triangle:

                    tempTargetRender.gameObject.SetActive(true);

                    if (lineCount == 0 && (startDownPos.x != pixelUV.x && startDownPos.y != pixelUV.y))
                    {
                        lastMouseUpPos = startDownPos;
                        lineCount = 1;
                    }

                    ClearColor(ref temporaryTexture);
                    DrawBresenhamLine(ref temporaryTexture, (int)startDownPos.x, (int)startDownPos.y, (int)pixelUV.x, (int)pixelUV.y);
                    break;
                case DrawingMode.Rectangle:

                    tempTargetRender.gameObject.SetActive(true);

                    ClearColor(ref temporaryTexture);
                    DrawBresenhamLine(ref temporaryTexture, (int)startDownPos.x, (int)startDownPos.y, (int)pixelUV.x, (int)startDownPos.y);
                    DrawBresenhamLine(ref temporaryTexture, (int)pixelUV.x, (int)startDownPos.y, (int)pixelUV.x, (int)pixelUV.y);
                    DrawBresenhamLine(ref temporaryTexture, (int)pixelUV.x, (int)pixelUV.y, (int)startDownPos.x, (int)pixelUV.y);
                    DrawBresenhamLine(ref temporaryTexture, (int)startDownPos.x, (int)pixelUV.y, (int)startDownPos.x, (int)startDownPos.y);

                    break;
            }
        }


        // Menerima input ketika mouse diangkat
        if (Input.GetMouseButtonUp(0))
        {
            switch (this.CurrentDrawingMode)
            {
                // menggambar garis
                case DrawingMode.Line:
                    ApplyTemporaryTex(ref temporaryTexture, ref targetTexture);
                    targetTexture.Apply();
                    ClearColor(ref temporaryTexture);
                    break;
                // menggambar segitiga
                case DrawingMode.Triangle:

                    // menghitung jumlah garis yang sudah digambar
                    // indeks titik 0, 1, dan 2
                    if (lineCount < 2)
                    {

                        if (lineCount == 0 && (startDownPos.x != pixelUV.x && startDownPos.y != pixelUV.y))
                        {
                            lineCount = 1;
                        }
                        else
                        {
                            lineCount++;
                        }

                    }
                    else
                    {
                        //garis terakhir
                        DrawBresenhamLine(ref temporaryTexture, (int)pixelUV.x, (int)pixelUV.y, (int)startTrianglePos.x, (int)startTrianglePos.y);

                        // reset indeks garis
                        lineCount = 0;
                    }


                    ApplyTemporaryTex(ref temporaryTexture, ref targetTexture);
                    targetTexture.Apply();
                    ClearColor(ref temporaryTexture);

                    break;
                // menggambar segi empat
                case DrawingMode.Rectangle:
                    ApplyTemporaryTex(ref temporaryTexture, ref targetTexture);
                    targetTexture.Apply();
                    ClearColor(ref temporaryTexture);
                    break;
            }
            lastMouseUpPos = pixelUV;
        }

        // Input ketika update
        switch (this.CurrentDrawingMode)
        {
            case DrawingMode.Triangle:
                tempTargetRender.gameObject.SetActive(true);
                if (hit.transform == tempTargetRender.transform)
                {

                    if (lineCount > 0)
                    {
                        ClearColor(ref temporaryTexture);
                        DrawBresenhamLine(ref temporaryTexture, (int)lastMouseUpPos.x, (int)lastMouseUpPos.y, (int)pixelUV.x, (int)pixelUV.y);
                    }

                }
                break;
        }



        lastPixelPosition = hit.textureCoord;
        tex.Apply();
    }

    void DrawDot(ref Texture2D targetTex, int x, int y)
    {
        targetTex.SetPixel(x, y, Color.black);
    }

    void DrawBresenhamLine(ref Texture2D targetTex, int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;
        for (; ; )
        {
            targetTex.SetPixel(x0, y0, Color.black);
            if (x0 == x1 && y0 == y1) break;
            e2 = err;
            if (e2 > -dx) { err -= dy; x0 += sx; }
            if (e2 < dy) { err += dx; y0 += sy; }
        }
    }

    void ClearColor(ref Texture2D targetTex)
    {
        Color transparentColor = new Color(1, 1, 1, 0);
        Color[] cols = targetTex.GetPixels();
        for (int i = 0; i < cols.Length; ++i)
        {
            cols[i] = transparentColor;
        }
        targetTex.SetPixels(cols);
    }

    void ApplyTemporaryTex(ref Texture2D originTex, ref Texture2D targetTex)
    {
        Color[] originColors = originTex.GetPixels();
        Color[] targetColors = targetTex.GetPixels();
        for (int i = 0; i < targetColors.Length; ++i)
        {
            targetColors[i] = targetColors[i] * originColors[i];
        }
        targetTex.SetPixels(targetColors);
    }

    public void SetCurrentDrawingMode(string drawingMode)
    {
        switch (drawingMode.ToLower())
        {
            case "dot":
                this.CurrentDrawingMode = DrawingMode.Dot;
                break;
            case "line":
                this.CurrentDrawingMode = DrawingMode.Line;
                break;
            case "triangle":
                this.CurrentDrawingMode = DrawingMode.Triangle;
                break;
            case "rectangle":
                this.CurrentDrawingMode = DrawingMode.Rectangle;
                break;
        }
    }
}
