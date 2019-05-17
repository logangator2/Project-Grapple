using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class PlayerController : MonoBehaviour
{

    public float SpeedH = 10f;
    public float SpeedV = 10f;
    public float stepRate = 0.5f;
    public float residualGrappleForce = 30f;
    public float walkSpeed, sprintSpeed, jumpForce, maxJumpCount, grappleLength, grappleSpeed, grappleDelayTime;

    [SerializeField] private float att_rate = .5f;
    [SerializeField] private AudioClip swing, ding, hit, step, thunk, aimSound;
    [SerializeField] private Image aimingHair, anchorHair;
    [SerializeField] private Camera vmCam;

    private AudioSource aud;
    private Camera cam;

    private int collectCount = 0;
    private float upForce = 0f;

    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -89.9f;
    private float maxPitch = 89.9f;
    private float FOVDeceleration = 0.2f;
    private float horizontalMovement, verticalMovement, currentSpeed, currentJumpCount;
    public bool grappleCharging = false;
    private bool canStep = true;
    private bool grounded = true;
    
    public float baseFov = 70f;
    public float sprintFov = 60f;
    public float grappleFov = 45f;

    public bool grappling = false;
    public Vector3 grappleOrigin;
    public Vector3 grappleTarget;

    Rigidbody rb;
    Vector3 moveDirection;
    RaycastHit grappleHit;

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        //vmcam = GetComponentInChildren<Camera>();

        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        CameraRotate();
        // compute movement direction
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;

        // step audio
        if (grounded && canStep && horizontalMovement + verticalMovement != 0)
        {
            StartCoroutine(StepDelay());
        }

        // sprinting
        if (Input.GetKey(KeyCode.LeftShift) && verticalMovement > 0)
        {
            currentSpeed = sprintSpeed;
            if (!grappleCharging)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFov, FOVDeceleration);
            }
        }

        // sprinting
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = walkSpeed;
        }

        // grappling
        
        if (Input.GetMouseButton(1))
        {
            if (grappleCharging)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, grappleFov, FOVDeceleration);
            }
        }
        else if (currentSpeed != sprintSpeed)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFov, FOVDeceleration * 1.5f);
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (grappling)
            {
                Vector3 kickButt = grappleTarget - grappleOrigin;
                if (kickButt.y < 0)
                {
                    kickButt.y *= -1;
                }
                rb.AddForce(kickButt * residualGrappleForce);
                Debug.Log(kickButt);
                grappling = false;
            } else {
                aud.PlayOneShot(aimSound, 1.3f);
                grappleCharging = true;
                aimingHair.gameObject.SetActive(true);
            }
        }

        if (Input.GetMouseButtonUp(1) && grappleCharging)
        {
            aimingHair.gameObject.SetActive(false);
            anchorHair.gameObject.SetActive(false);
            grappleCharging = false;

            if (aimedAtAnchor())
            {
                grappleTarget = LaunchGrapple();
                grappleOrigin = rb.position;
                grappling = true;
            } else
            {
                // play whiff sound effect
            }
        }

        // get back cursor
        // FIXME: later on, get a way to remove the cursor again
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // 
        vmCam.fieldOfView = cam.fieldOfView - 5;
    }

    void FixedUpdate()
    {
        // Physics steps shoudl be in FixedUpdate
        if (moveDirection != new Vector3(0, 0, 0) && rb.velocity.y == 0)
        {
            rb.velocity = new Vector3(0, 0, 0);
        }

        Move();

        if (grappling)
        {
            transform.position = Vector3.MoveTowards(transform.position, grappleTarget, grappleSpeed);
        }

        if (Input.GetMouseButton(1) && grappleCharging)
        {
            if (aimedAtAnchor())
            {
                anchorHair.gameObject.SetActive(true);
            } else
            {
                anchorHair.gameObject.SetActive(false);
            }
        }

        if(upForce > 0)
        {
            rb.AddForce(new Vector3(0, upForce, 0));
            upForce -= Time.deltaTime * 30f;
        }

    }

    private void OnCollisionEnter(Collision col)
    {
        if ((col.gameObject.tag == "Anchor" || col.gameObject.tag == "Ground") && grappling)
        {
            upForce = residualGrappleForce;
            aud.PlayOneShot(thunk, 0.2f);
            grappling = false;
        }

        // if (col.gameObject.name == "LineRenderer")
        // {
        //     // reset player to last spawn point
        //     Debug.Log("Player hit by laser!");
        // }

        // FIXME: 
        if (col.gameObject.tag == "Respawn")
        {
            transform.position = new Vector3(69, 175, 81);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        // Collision interactions
        if (col.gameObject.tag == "Ground")
        {
            grounded = true;
            currentJumpCount = maxJumpCount;
        }

        if (col.gameObject.CompareTag("Pick Up"))
        {
            col.gameObject.SetActive(false);
            aud.PlayOneShot(ding, 0.3f);
            collectCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            grounded = false;
        }
    }

    void CameraRotate()
    {
        yaw += Input.GetAxis("Mouse X") * SpeedH;
        pitch -= Input.GetAxis("Mouse Y") * SpeedV;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.eulerAngles = new Vector3(0f, yaw, 0f);
        cam.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    void Move()
    {
        rb.MovePosition(transform.position + moveDirection * currentSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if (currentJumpCount != 0)
        {
            // from https://www.noob-programmer.com/unity3d/how-to-make-player-object-jump-in-unity-3d/
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            currentJumpCount -= 1;
        }
    }
    bool aimedAtAnchor()
    {
        RaycastHit hitspot;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hitspot))
        {
            if (hitspot.point != null && hitspot.collider.tag == "Anchor")
            {
                return true;
            }
        }
        return false;
    }
    
    Vector3 LaunchGrapple()
    {
        RaycastHit hitspot;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hitspot))
        {
            if (hitspot.point != null && hitspot.collider.tag == "Anchor")
            {
                aud.PlayOneShot(hit, 0.5F);
                return hitspot.point;
            }
        }
        return hitspot.point;
    }

    IEnumerator StepDelay()
    {
        aud.PlayOneShot(step, 0.1f);
        canStep = false;
        
        if(currentSpeed == sprintSpeed)
        {
            yield return new WaitForSeconds(stepRate / 1.5f);
        } else
        {
            yield return new WaitForSeconds(stepRate);
        }

        canStep = true;
    }
}