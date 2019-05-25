using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// some movement from source: https://www.mvcode.com/lessons/first-person-camera-and-controller-jamie
// FPS camera movement from: https://answers.unity.com/questions/1087351/limit-vertical-rotation-of-camera.html

public class PlayerController : MonoBehaviour
{


    [SerializeField] private AudioClip ding, step, thunk, aimSound, grappleLaunch, reelIn, windSound;
    [SerializeField] private GameObject crosshairs;
    [SerializeField] private Image anchorHair;
    [SerializeField] private Camera vmCam;

    public float HSENS = 1f;
    public float VSENS = .8f;
    private float WALKSPEED = 5f;
    private float SPRINTSPEED = 10f;
    private float JUMPFORCE = 10f;
    private float MAXJUMPCOUNT = 2f;
    private float GRAPPLELEN = 500f;
    private float GRAPPLESPEED = .9f;
    private float GRAPPLEDELAY = 0.4f;
    private float STEPRATE = 0.5f;
    private float BASEFOV = 70f;
    private float SPRINTFOV = 60f;
    private float GRAPPLEFOV = 45f;
    private float MINPITCH = -89.9f;
    private float MAXPITCH = 89.9f;
    private float FOVDECELERATION = 0.1f;
    private float ROTSPEED = 180f;
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
    private bool grounded = true;

    public Vector3 grappleOrigin;
    public Vector3 grappleTarget;
    private RaycastHit grappleHit;

    private AudioSource aud;
    private Camera cam;
    private Rigidbody rb;
    private Vector3 moveDirection;

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
        currentSpeed = WALKSPEED;
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
            moveDirection *= GRAPPLINGMOVEMENTMODIFIER;
        }

        if (grounded && canStep && horizontalMovement + verticalMovement != 0)
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
                crosshairs.gameObject.transform.Rotate(new Vector3(0, 0, 1) * Time.deltaTime * ROTSPEED);
                currentGrappleLength = Mathf.Lerp(currentGrappleLength, MAXGRAPPLELENGTH, GRAPPLECHARGESPEED);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, GRAPPLEFOV, FOVDECELERATION);
            }
        } else {
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
            } else {
                aud.PlayOneShot(aimSound, .1f);
                grappleCharging = true;
            }
        }

        if (Input.GetMouseButtonUp(1) && grappleCharging)
        {
            anchorHair.gameObject.SetActive(false);
            grappleCharging = false;
            if (aimedAtAnchor())
            {
                grappleTarget = LaunchGrapple();
                grappleOrigin = rb.position;
                grappling = true;
            } else {
                // play whiff sound effect
            }
            crosshairs.gameObject.transform.rotation = Quaternion.identity;
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
        vmCam.fieldOfView = cam.fieldOfView - 5;
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

        // Physics steps

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
            upForce = RESIDUALGRAPPLEFORCE;
            aud.PlayOneShot(thunk, 0.2f);
            grappling = false;
        }

        // if (col.gameObject.name == "LineRenderer")
        // {
        //     // reset player to last spawn point
        //     Debug.Log("Player hit by laser!");
        // }

        if (col.gameObject.tag == "Respawn")
        {
            transform.position = new Vector3(69, 175, 81);
        }
    }

    void OnTriggerEnter(Collider col)
    {

        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Anchor")
        {
            grounded = true;
            currentJumpCount = MAXJUMPCOUNT;
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
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Anchor")
        {
            grounded = false;
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
    bool aimedAtAnchor()
    {
        RaycastHit hitspot;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hitspot))
        {
            if (hitspot.point != null && hitspot.collider.tag == "Anchor" && hitspot.distance <= currentGrappleLength)
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
        aud.PlayOneShot(step, 0.1f);
        canStep = false;
        
        if(currentSpeed == SPRINTSPEED)
        {
            yield return new WaitForSeconds(STEPRATE / 1.5f);
        } else
        {
            yield return new WaitForSeconds(STEPRATE);
        }

        canStep = true;
    }
}