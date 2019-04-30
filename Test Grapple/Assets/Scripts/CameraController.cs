using UnityEngine;
using System.Collections;

// Performs a mouse look.

public class CameraController : MonoBehaviour
{
    public float minX = -80f;
    public float maxX = 80f;
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;
    public Transform Player;

    private float h, v, z;

    void Update()
    {
        // Get horizontal and vertical movement
        h = horizontalSpeed * Input.GetAxisRaw("Mouse X");
        v = -(verticalSpeed * Input.GetAxisRaw("Mouse Y"));
            
        // FIXME: Need some way to prevent the rotation of the x axis to go above 80 degrees or below -80
        // ... otherwise this messes up the camera
        transform.Rotate(v, h, 0);
        Player.transform.Rotate(0, h, 0);

        // from: https://forum.unity.com/threads/how-to-lock-or-set-the-cameras-z-rotation-to-zero.68932/
        // cancels out any z rotation; for some reason the transform above...
        // doesn't do the trick
        z = transform.eulerAngles.z;
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