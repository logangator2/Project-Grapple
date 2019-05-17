using UnityEngine;
using System.Collections;

public class CrosshairRotator : MonoBehaviour {
    public float rotSpeed = 1;

    void Update () 
    {
        transform.Rotate (new Vector3 (0, 0, 1) * Time.deltaTime * rotSpeed);
    }
}