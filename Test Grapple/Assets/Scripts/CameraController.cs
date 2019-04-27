using UnityEngine;
using System.Collections;

// Performs a mouse look.

public class CameraController : MonoBehaviour
{
    public float minX = -80f;
    public float maxX = 80f;
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;

    private float h, v;

    void Update()
    {
        // Get horizontal and vertical movement
        h = horizontalSpeed * Input.GetAxis("Mouse X");
        v = verticalSpeed * Input.GetAxis("Mouse Y");

        // lock x rotation
        v = Mathf.Clamp(v, minX, maxX);

        transform.Rotate(-v, h, 0);
    }
}