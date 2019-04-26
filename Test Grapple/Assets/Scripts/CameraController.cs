using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    Camera Camera;

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
        transform.rotation = Quaternion.Euler(Input.mousePosition);
    }
}