using UnityEngine;
using System.Collections;

// Performs a mouse look.

public class CameraController : MonoBehaviour
{
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;
    public Transform Player;

    private float h, v, z;

    void Update()
    {
        // Get horizontal and vertical movement
        h = horizontalSpeed * Input.GetAxis("Mouse X");
        v = verticalSpeed * Input.GetAxis("Mouse Y");

        transform.Rotate(-v, h, 0);

        // from: https://forum.unity.com/threads/how-to-lock-or-set-the-cameras-z-rotation-to-zero.68932/
        // cancels out any z rotation; for some reason the transform above...
        // doesn't do the trick
        float z = transform.eulerAngles.z;
        transform.Rotate(0, 0, -z);
    }

    void LateUpdate()
    {
        // from: https://answers.unity.com/questions/884006/camera-not-rotate-when-following-player.html
        if (Player != null)
        {
            transform.position = Player.position;
        }
    }
}