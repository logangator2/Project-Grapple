using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie

    public float walkSpeed;

    Rigidbody rb;
    Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Non-Physics steps
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;
    }

    void FixedUpdate()
    {
        // Physics steps
        Move();
    }

    void Move()
    {
        // to fix falling slowly
        Vector3 yVelFix = new Vector3(0, rb.velocity.y, 0);
        // to move normally
        rb.velocity = moveDirection * walkSpeed * Time.deltaTime;
        // adding fixed fall velocity
        rb.velocity += yVelFix;
    }
}
