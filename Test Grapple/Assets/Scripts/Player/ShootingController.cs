using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShootingController : MonoBehaviour
{
    [SerializeField] private GameObject Gun, Grapple, NailPrefab, MuzzleFlash;
    [SerializeField] private Transform grappleHole;
    [SerializeField] private AudioClip fireSound;

    [SerializeField] private float FIRERATE = .9f;

    private float GunStartPos;
    private float GrappleStartPos;
    private float GrappleChargePos;

    private bool gunLoaded = true;

    private AudioSource auds;
    private ParticleSystem muzzle;
    private ParticleSystem g_muzzle;
    private PlayerController pc;
    private LineRenderer line;
    private Camera cam;
    private Rigidbody rb;


    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        auds = GetComponent<AudioSource>();
        pc = GetComponent<PlayerController>();
        line = GetComponent<LineRenderer>();
        muzzle = MuzzleFlash.GetComponent<ParticleSystem>();

        GunStartPos = Gun.transform.localPosition.z;
        GrappleStartPos = Grapple.transform.localPosition.z;
        GrappleChargePos = Grapple.transform.localPosition.z + .7f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {

            if (gunLoaded && !pc.grappleCharging)
            {
                fire();
            }
        }

        if (Input.GetMouseButton(1) && pc.grappleCharging || pc.grappling)
        {
            Vector3 grapplePos = new Vector3(Grapple.transform.localPosition.x, Grapple.transform.localPosition.y, Mathf.Lerp(Grapple.transform.localPosition.z, GrappleChargePos, .05f));
            Grapple.transform.localPosition = grapplePos;
        }
        else
        {
            Vector3 grapplePos = new Vector3(Grapple.transform.localPosition.x, Grapple.transform.localPosition.y, Mathf.Lerp(Grapple.transform.localPosition.z, GrappleStartPos, .4f));
            Grapple.transform.localPosition = grapplePos;
        }

        if (pc.grappling)
        {
            line.enabled = true;
            line.SetPosition(0, grappleHole.position);
            line.SetPosition(1, pc.grappleTarget);
        } else {
            line.enabled = false;
        }

        Vector3 gunPos = new Vector3(Gun.transform.localPosition.x, Gun.transform.localPosition.y, Mathf.Lerp(Gun.transform.localPosition.z, GunStartPos, 0.02f));
        Gun.transform.localPosition = gunPos;
    }

    void fire()
    {
        
        RaycastHit hitspot;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        int mask = 1 << 10;
        mask = ~mask; 
        if (Physics.Raycast(ray, out hitspot, Mathf.Infinity, mask))
        {
            if (hitspot.point != null)
            {
                Instantiate(NailPrefab, hitspot.point, cam.transform.rotation * Quaternion.Euler(90f, 0, 0));
            }
        }

        auds.PlayOneShot(fireSound, 1.0f);
        muzzle.Play();
        Gun.transform.localPosition += Vector3.back * .4f;
        StartCoroutine(reload());
    }

    IEnumerator reload()
    {
        gunLoaded = false;
        yield return new WaitForSeconds(FIRERATE);
        gunLoaded = true;
    }
}
