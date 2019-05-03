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
    public float walkSpeed, sprintSpeed, jumpForce, maxJumpCount, grappleLength, grappleSpeed, grappleDelayTime;
    public Transform Player, respawnPoint;
    public GameObject gameCanvas;
    public Text reticle;
    public Text winText;

    [SerializeField] private float att_rate = .5f;
    [SerializeField] private AudioClip swing, ding, hit, step;
    [SerializeField] private GameObject hitParticlePrefab;

    private AudioSource aud;
    private Camera cam;
    private int collectCount = 0;
    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -80f;
    private float maxPitch = 80f;
    private float horizontalMovement, verticalMovement, currentSpeed, currentJumpCount;
    private bool grappleUsed;
    private bool grappling;
    private Vector3 target;

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
        grappleUsed = false;
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

        if(Input.GetKey(KeyCode.LeftShift) && verticalMovement > 0)
        {
            currentSpeed = sprintSpeed;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
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
        if (Input.GetMouseButtonUp(1))
        {
            grappling = false;
            GrappleDelay();
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
            gameObject.transform.position = respawnPoint.position;
        }

        // shoot grapple
        if (Input.GetMouseButton(1))
        {
            if (grappling)
            {
                ReelGrapple(target);
            } else if (!grappleUsed)
            {
                target = LaunchGrapple();
            }
        }
        if (Input.GetMouseButton(1))
        {
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

        grappleUsed = true;
        grappling = false;
        GrappleDelay();

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
            rb.AddForce(new Vector3 (0, jumpForce, 0), ForceMode.Impulse);
            currentJumpCount -= 1;
        }
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
                Debug.Log(hitspot.point);
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

            if(Physics.Raycast(ray, out hitspot))
            {
                if (hitspot.point != null)
                {
                    Instantiate(hitParticlePrefab, hitspot.point, Quaternion.LookRotation(hitspot.normal)); // Spawn particles
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
        yield return new WaitForSeconds(grappleDelayTime);
        grappleUsed = false;
    }
}