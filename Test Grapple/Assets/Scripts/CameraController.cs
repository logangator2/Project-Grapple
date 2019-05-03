using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class CameraController : MonoBehaviour
{

    public float SpeedH = 10f;
    public float SpeedV = 10f;
    public float OOBHeight = 0f;
    public float walkSpeed, sprintSpeed, jumpForce, maxJumpCount, grappleLength, grappleSpeed, grappleDelayTime;
    public Transform Player, respawnPoint;
    public GameObject gameCanvas;
    public Text reticle;
    public Text winText;

    private int collectCount = 0;
    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -30f;
    private float maxPitch = 60f;
    private float horizontalMovement, verticalMovement, currentSpeed, currentJumpCount;
    private bool grappleUsed;

    Rigidbody rb;
    Vector3 moveDirection;
    RaycastHit grappleHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Cursor.visible = false;
        grappleUsed = false;
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

        // get back cursor
        // FIXME: later on, get a way to remove the cursor again
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
        }

        // FIXME: collectible stuff
        if (collectCount == 4)
        {
            reticle.gameObject.SetActive(false);
            winText.gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        // Physics steps
        Move();

        // jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump ();
        }
        if (gameObject.transform.position.y < OOBHeight)
        {
            Player.transform.position = respawnPoint.position;
        }

        // shoot grapple
        if (Input.GetMouseButtonDown(1))
        {
            Grapple();
            StartCoroutine("GrappleDelay");
        }
    }

    // partially from https://unity3d.com/learn/tutorials/topics/physics/detecting-collisions-oncollisionenter
    void OnCollisionEnter (Collision col)
    {
        // resets jumpCount
        if(col.gameObject.tag == "Ground")
        {
            currentJumpCount = maxJumpCount;
        }

        // if (col.gameObject.name == "SawBlade")
        // {
        //     transform.position = new Vector3(0, 51, -47.5f);
        // }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Pick Up"))
        {
            col.gameObject.SetActive(false);
            collectCount++;
        }
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
        rb.velocity = moveDirection * currentSpeed * Time.deltaTime;
        // adding fixed fall velocity
        rb.velocity += yVelFix;
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

    void Grapple()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out grappleHit, grappleLength))
        {
            // check for anchor status
            if (grappleHit.collider.tag == "Anchor" && !grappleUsed)
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * grappleHit.distance, Color.yellow);
                // send player towards point
                // transform.Translate(Vector3.forward * Time.deltaTime * grappleSpeed);
                transform.position = Vector3.Lerp(transform.position, grappleHit.transform.position , grappleSpeed * Time.deltaTime);
                grappleUsed = true;
            }
        }
    }

    IEnumerator GrappleDelay()
    {
        yield return new WaitForSeconds(grappleDelayTime);
        grappleUsed = false;
    }
}