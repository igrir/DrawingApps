using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Vector2 curDist;
    Vector2 prevDist;

    Camera mainCamera;
    Vector3 cameraInitialPosition;

    public float zoomSpeed = 0.5f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // mendeteksi ada touch
        if (Input.touchCount >= 2)
        {

            // proses ketika kedua jari bergerak
            if (Input.GetTouch(0).phase == TouchPhase.Moved ||
                Input.GetTouch(1).phase == TouchPhase.Moved)
            {

                curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
                prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
                float touchDelta = curDist.magnitude - prevDist.magnitude;

                // ubah ukuran kamera
                mainCamera.orthographicSize -= touchDelta * zoomSpeed * Time.deltaTime;
                mainCamera.orthographicSize = Mathf.Max(mainCamera.orthographicSize, 0.1f);

                // geser gambar berdasarkan perubahan touch 0
                transform.Translate(new Vector2(-Input.GetTouch(0).deltaPosition.x * Time.deltaTime, -Input.GetTouch(0).deltaPosition.y * Time.deltaTime));
            }
        }
    }
}
