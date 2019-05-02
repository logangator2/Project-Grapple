using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class CameraController : MonoBehaviour
{

    public float SpeedH = 10f;
    public float SpeedV = 10f;
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public float maxJumpCount;
    public float OOBHeight = 0f;
    public Transform Player;
    public Transform respawnPoint;
    public GameObject gameCanvas;

    private int collectCount = 0;
    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -30f;
    private float maxPitch = 60f;
    private float horizontalMovement, verticalMovement, currentSpeed, currentJumpCount;

    Rigidbody rb;
    Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Non-Physics steps
    void Update()
    {
        CameraRotate();

        // compute movement direction
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;
        // to prevent accidental "flying"
        moveDirection.y = 0;

        // set player speed, doesn't allow player to sprint backwards
        currentSpeed = walkSpeed;
        if(Input.GetKey(KeyCode.LeftShift) && verticalMovement > 0)
        {
            currentSpeed = sprintSpeed;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = walkSpeed;
        }

        // jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump ();
        }
        if (gameObject.transform.position.y < OOBHeight)
        {
            Player.transform.position = respawnPoint.position;
        }
        if (collectCount == 4)
        {
            gameCanvas.SetActive(true);
        }
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

    void Jump()
    {
        if (currentJumpCount != 0) 
        {
            // from https://www.noob-programmer.com/unity3d/how-to-make-player-object-jump-in-unity-3d/
            rb.AddForce(new Vector3 (0, jumpForce, 0), ForceMode.Impulse);
            currentJumpCount -= 1;
        }
    }

    void Move()
    {
        // to fix falling slowly
        Vector3 yVelFix = new Vector3(0, rb.velocity.y, 0);
        // to move normally
        rb.velocity = moveDirection * currentSpeed * Time.deltaTime;
        // adding fixed fall velocity
        rb.velocity += yVelFix;
    }

    // partially from https://unity3d.com/learn/tutorials/topics/physics/detecting-collisions-oncollisionenter
    void OnCollisionEnter (Collision col)
    {
        // resets jumpCount
        if(col.gameObject.tag == "Ground")
        {
            currentJumpCount = maxJumpCount;
        }
        if (col.gameObject.tag == "Pick Up")
        {
            collectCount++;
        }
        // if (col.gameObject.name == "SawBlade")
        // {
        //     transform.position = new Vector3(0, 51, -47.5f);
        // }

        // // reset posiiton to original position - a "teleport" to the starting position
        // if (col.gameObject.name == "Reset")
        // {
        //     transform.position = new Vector3(0, 1, 0);
        // }
    }
}