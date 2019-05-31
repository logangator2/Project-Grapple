using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMechanic : MonoBehaviour
{
    public bool doorOpen;
    public float speed;
    public Vector3 openPosition, closePosition;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        closePosition = rb.position;
        // FIXME: Causes the door to fly upwards in non-testing environment
        openPosition = new Vector3(0f, rb.position.y * 3, 0f);
    }

    void Start()
    {
        // if door set to open
        if (doorOpen)
        {
            rb.position = openPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // awaits for close trigger
        // check if robots are dead to open
        if (GameObject.FindGameObjectWithTag("Robot") == null)
        {
            Open();
        }
    }

    public void Open()
    {
        rb.position = Vector3.MoveTowards(rb.position, openPosition, Time.deltaTime * speed);
        // Debug.Log("Door Opened");
    }
    public void Close()
    {
        rb.position = closePosition;
        // Debug.Log("Door Closed");
    }

    // IEnumerator Wait()
    // {
    //     Open();
    //     yield return new WaitForSeconds(5f);
    //     Close();
    //     yield return new WaitForSeconds(20f);
    // }
}
