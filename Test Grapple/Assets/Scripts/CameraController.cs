﻿using UnityEngine;
using System.Collections;

// Performs a mouse look.
// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class CameraController : MonoBehaviour
{

    public float SpeedH = 10f;
    public float SpeedV = 10f;
    public float walkSpeed;
    public Transform Player;

    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -30f;
    private float maxPitch = 60f;
    private float horizontalMovement, verticalMovement;

    Rigidbody rb;
    Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Non-Physics steps

        CameraRotate();

        // compute movement direction
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;
        // to prevent accidental "flying"
        moveDirection.y = 0;
    }

    void FixedUpdate()
    {
        // Physics steps
        Move();
    }

    void CameraRotate()
    {
        yaw += Input.GetAxis("Mouse X") * SpeedH;
        pitch -= Input.GetAxis("Mouse Y") * SpeedV;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
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