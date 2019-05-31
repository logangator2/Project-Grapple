using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class PlayerController : MonoBehaviour
{

    public GameObject Spawn;
    [SerializeField]
    private Camera vmCam;
    [SerializeField]
    private AudioClip ding, step, thunk, aimSound, grappleLaunch, reelIn, windSound;
    private static PlayerController playerInstance;

    public float HSENS = 1f;
    public float VSENS = .8f;
    private float WALKSPEED = 10f;
    private float SPRINTSPEED = 18f;
    private float JUMPFORCE = 13f;
    private float MAXJUMPCOUNT = 2f;
    private float GRAPPLELEN = 500f;
    private float GRAPPLESPEED = .9f;
    private float GRAPPLEDELAY = 0.4f;
    private float STEPRATE = 0.5f;
    private float BASEFOV = 80f;
    private float SPRINTFOV = 65f;
    private float CHARGINGFOV = 45f;
    private float GRAPPLINGFOV = 120f;
    private float MINPITCH = -89.9f;
    private float MAXPITCH = 89.9f;
    private float FOVDECELERATION = 0.05f;
    private float GRAPPLECHARGESPEED = 1f;
    private float MAXGRAPPLELENGTH = 50f;
    private float RESIDUALGRAPPLEFORCE = 30f;
    private float GRAPPLINGMOVEMENTMODIFIER = 3.5f;
    private float horizontalMovement, verticalMovement, currentSpeed, currentJumpCount;
    private int collectCount = 0;
    private float currentGrappleLength = 0f;
    private float upForce = 0f;
    private float yaw = 0f;
    private float pitch = 0f;
    public bool grappling = false;
    public bool grappleCharging = false;
    private bool canStep = true;
    public bool multispawn = false;
    private int currentCollidingGrounds = 0;

    public Vector3 grappleTarget;
    public Vector3 grappleOrigin;
    private RaycastHit grappleHit;

    public Vector3 spawnPoint;

    private AudioSource aud;
    private Camera cam;
    private Rigidbody rb;
    private Vector3 moveDirection;

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
        if (multispawn == true)
        {
            DontDestroyOnLoad(this);
            if (playerInstance == null)
            {
                playerInstance = this;
            }
            else
            {
                //playerInstance.transform.position = spawnPoint;
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentSpeed = WALKSPEED;
        if (multispawn == true && spawnPoint != new Vector3(0,0,0))
        {
            playerInstance.transform.position = spawnPoint;
        }
    }

    void Update()
    {
        CameraRotate();
        // compute movement direction
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;
        if (grappling)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, GRAPPLINGFOV, FOVDECELERATION);
            moveDirection *= GRAPPLINGMOVEMENTMODIFIER;
        }

        if (currentCollidingGrounds > 0 && canStep && horizontalMovement + verticalMovement != 0)
        {
            StartCoroutine(StepDelay());
        }

        // sprinting
        if (Input.GetKey(KeyCode.LeftShift) && verticalMovement > 0)
        {
            currentSpeed = SPRINTSPEED;
            if (!grappleCharging)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, SPRINTFOV, FOVDECELERATION);
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = WALKSPEED;
        }

        if (Input.GetMouseButton(1))
        {
            if (grappleCharging)
            {
                currentGrappleLength = Mathf.Lerp(currentGrappleLength, MAXGRAPPLELENGTH, GRAPPLECHARGESPEED);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, CHARGINGFOV, FOVDECELERATION);
            }
        }
        else
        {
            if (currentSpeed != SPRINTSPEED)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, BASEFOV, FOVDECELERATION * 1.5f);
            }
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
                rb.AddForce(kickButt * RESIDUALGRAPPLEFORCE);
                Debug.Log(kickButt);
                grappling = false;
            }
            else
            {
                aud.PlayOneShot(aimSound, .1f);
                grappleCharging = true;
            }
        }

        if (Input.GetMouseButtonUp(1) && grappleCharging)
        {
            grappleCharging = false;
            if (aimedAtAnchor())
            {
                grappleTarget = LaunchGrapple();
                grappleOrigin = rb.position;
                grappling = true;
            }
            currentGrappleLength = 0f;
        }

        // get back cursor
        // FIXME: later on, get a way to remove the cursor again
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        vmCam.fieldOfView = cam.fieldOfView;
    }

    void FixedUpdate()
    {
        if (moveDirection != new Vector3(0, 0, 0) && rb.velocity.y == 0)
        {
            rb.velocity = new Vector3(0, 0, 0);
        }

        if (grappling)
        {
            transform.position = Vector3.MoveTowards(transform.position, grappleTarget, GRAPPLESPEED);
        }

        Move();

        // Physics steps

        if (upForce > 0)
        {
            rb.AddForce(new Vector3(0, upForce, 0));
            upForce -= Time.deltaTime * 30f;
        }

    }

    private void OnCollisionEnter(Collision col)
    {
        if ((col.gameObject.tag == "Anchor" || col.gameObject.tag == "Ground" || col.gameObject.tag == "Robot" || col.gameObject.tag == "Platform") && grappling)
        {
            upForce = RESIDUALGRAPPLEFORCE;
            aud.PlayOneShot(thunk, 0.2f);
            grappling = false;
        }

        if (col.gameObject.tag == "Respawn")
        {
            Respawn();
        }

        if (col.gameObject.tag == "Platform")
        {
            this.transform.parent = col.transform.parent;
        }
        else
        {
            this.transform.parent = null;
        }
        if (col.gameObject.tag == "Checkpoint")
        {
            spawnPoint = col.transform.parent.position;
        }
    }

    public void Respawn()
    {
        if (multispawn == false)
        {
            transform.position = Spawn.transform.position;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            transform.position = spawnPoint;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnTriggerEnter(Collider col)
    {

        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Anchor" || col.gameObject.tag == "Robot" || col.gameObject.tag == "Platform")
        {
            currentCollidingGrounds++;
            currentJumpCount = MAXJUMPCOUNT;
        }

        if (col.gameObject.CompareTag("Pick Up"))
        {
            col.gameObject.SetActive(false);
            aud.PlayOneShot(ding, 0.3f);
            if (SceneManager.GetActiveScene().buildIndex < 5)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Anchor" || other.gameObject.tag == "Robot")
        {
            currentCollidingGrounds--;
        }
    }

    void CameraRotate()
    {
        yaw += Input.GetAxis("Mouse X") * HSENS;
        pitch -= Input.GetAxis("Mouse Y") * VSENS;
        pitch = Mathf.Clamp(pitch, MINPITCH, MAXPITCH);
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
            rb.AddForce(new Vector3(0, JUMPFORCE, 0), ForceMode.Impulse);
            currentJumpCount -= 1;
        }
    }

    public bool aimedAtAnchor()
    {
        RaycastHit hitspot;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hitspot))
        {
            if (hitspot.point != null && (hitspot.collider.tag == "Anchor" || hitspot.collider.tag == "Robot") && hitspot.distance <= currentGrappleLength)
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
            Debug.Log(currentGrappleLength);
            if (hitspot.point != null && hitspot.collider.tag == "Anchor")
            {
                aud.PlayOneShot(grappleLaunch, 1.1F);
                return hitspot.point;
            }
        }
        return hitspot.point;
    }

    IEnumerator StepDelay()
    {
        aud.PlayOneShot(step, 0.4f);
        canStep = false;

        if (currentSpeed == SPRINTSPEED)
        {
            yield return new WaitForSeconds(STEPRATE / 2f);
        }
        else
        {
            yield return new WaitForSeconds(STEPRATE);
        }

        canStep = true;
    }
}