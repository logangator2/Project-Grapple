using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class PlayerController : MonoBehaviour
{

    public float SpeedH = 10f;
    public float SpeedV = 10f;
    public float OOBHeight = 0f;
    public float stepRate = 0.5f;
    public float walkSpeed, sprintSpeed, jumpForce, maxJumpCount, grappleLength, grappleSpeed, grappleDelayTime;
    
    public bool latchOn = true;

    private int minutes, seconds;
    private float ms;
    private bool timeStop = false;

    public Transform Player, respawnPoint;
    public GameObject gameCanvas;
    public Text reticle, winText, stopwatch;

    [SerializeField] private float att_rate = .5f;
    [SerializeField] private AudioClip swing, ding, hit, step, thunk;
    //[SerializeField] private GameObject hitParticlePrefab;

    private AudioSource aud;
    private Camera cam;
    private int collectCount = 0;
    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -80f;
    private float maxPitch = 80f;
    private float horizontalMovement, verticalMovement, currentSpeed, currentJumpCount;
    private bool grappleUsed = false;
    private bool grappling = false;
    private Vector3 target;
    private bool canStep = true;
    private bool grounded = true;

    Rigidbody rb;
    Vector3 moveDirection;
    RaycastHit grappleHit;

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentSpeed = walkSpeed;
    }

    // Non-Physics steps
    void Update()
    {
        CameraRotate();

        // compute movement direction
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;

        if (grounded && canStep && horizontalMovement + verticalMovement != 0)
        {
            StartCoroutine(StepDelay());
        } 

        if (Input.GetKey(KeyCode.LeftShift) && verticalMovement > 0)
        {
            currentSpeed = sprintSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = walkSpeed;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!grappling)
            {
                attack();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            target = LaunchGrapple();
        }
        if (Input.GetMouseButtonUp(1) && grappling)
        {
            StartCoroutine(GrappleDelay());
        }

        // get back cursor
        // FIXME: later on, get a way to remove the cursor again
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // FIXME: collectible stuff
        if (collectCount == 4)
        {
            timeStop = true;
            stopwatch.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, ms);
            stopwatch.color = new Color(0f, 0.75f, 0f, 1f);
            reticle.gameObject.SetActive(false);
            winText.gameObject.SetActive(true);
        }

        if (timeStop == false)
        {
            formatTime();
            stopwatch.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, ms);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

    }

    void FixedUpdate()
    {
        // Physics steps
        Move();

        // jumping
        if (gameObject.transform.position.y < OOBHeight)
        {
            gameObject.transform.position = respawnPoint.position;
        }

        // shoot grapple
        if (Input.GetMouseButton(1))
        {
            if (grappling)
            {
                ReelGrapple(target);
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Anchor")
        {
            aud.PlayOneShot(thunk, 0.2f);
            if (!latchOn)
            {
                grappling = false;
                StartCoroutine(GrappleDelay());
            }
        }

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
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            currentJumpCount -= 1;
        }
    }

    void formatTime()
    {
        int currentTime = (int)Time.time;
        minutes = currentTime / 60;
        seconds = currentTime % 60;
        ms = (Time.time * 1000) % 1000;
    }

    void ReelGrapple(Vector3 targetDir)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetDir, grappleSpeed);
    }

    Vector3 LaunchGrapple()
    {
        RaycastHit hitspot;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hitspot))
        {
            if (hitspot.point != null && hitspot.collider.tag == "Anchor")
            {
                //Lower the ice pick
                //Fire out the grapple
                grappling = true;
                aud.PlayOneShot(hit, 0.5F);
                return hitspot.point;
            }
        }
        return hitspot.point;
    }

    IEnumerator attack()
    {
        while (Input.GetMouseButton(0))
        {
            // Play Animation
            RaycastHit hitspot;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

            if (Physics.Raycast(ray, out hitspot))
            {
                if (hitspot.point != null)
                {
                    //Instantiate(hitParticlePrefab, hitspot.point, Quaternion.LookRotation(hitspot.normal)); // Spawn particles
                    aud.PlayOneShot(hit, 0.5F);
                }
                else
                {
                    aud.PlayOneShot(swing, 0.5F);
                }
            }
            yield return new WaitForSeconds(att_rate);
        }
    }

    IEnumerator GrappleDelay()
    {
        grappling = false;
        grappleUsed = true;
        yield return new WaitForSeconds(grappleDelayTime);
        grappleUsed = false;
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