using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMechanic : MonoBehaviour
{
    public float speed;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // check for signal to open or close
        // if (GameObject.Find("Flybot") == null)
        // {
        //     Open();
        // }
    }

    private void OnCollisionEnter(Collision col)
    {
        // just for testing
        if (col.gameObject.tag == "Nail")
        {
            Open();
        }
    }

    void Open()
    {
        rb.position = Vector3.Slerp(rb.position, rb.position * 3, Time.deltaTime * speed);
        Debug.Log("Opened");
    }
    void Close()
    {

    }
}
