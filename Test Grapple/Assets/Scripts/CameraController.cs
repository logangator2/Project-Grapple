using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    Camera Camera;
    Quaternion mouseMovement;

    void Start ()
    {
        
    }
    
    void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // if grapple not activated, shoot grapple
            // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); - unity doc
        }
        // print(Input.mousePosition);

        mouseMovement = Quaternion.LookRotation(Input.mousePosition, Vector3.up);
        transform.rotation = mouseMovement;
        // transform.LookAt(Input.mousePosition);
    }
}