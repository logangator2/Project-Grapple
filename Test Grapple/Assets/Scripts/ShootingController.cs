using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShootingController : MonoBehaviour
{
    [SerializeField] private GameObject Gun, Grapple, NailPrefab, MuzzleFlash;
    [SerializeField] private Transform grappleHole;
    [SerializeField] private float firerate = 5f;
    [SerializeField] private AudioClip BANG;

    private Camera cam;
    private Rigidbody rb;
    private float GunStartPos;
    private float GrappleStartPos;
    private float GrappleChargePos;

    private bool gunLoaded = true;

    private AudioSource auds;
    private ParticleSystem muzzle;
    private PlayerController pc;


    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        auds = GetComponent<AudioSource>();
        pc = GetComponent<PlayerController>();
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
                Instantiate(NailPrefab, cam.transform.position + (cam.transform.forward * 1.5f), cam.transform.rotation * Quaternion.Euler(90f, 0, 0));
                auds.PlayOneShot(BANG, 1.4f);
                muzzle.Play();
                Gun.transform.localPosition += Vector3.back * .6f;
                StartCoroutine(reload());
            }
        }

        if (Input.GetMouseButton(1) && pc.grappleCharging)
        {
            Vector3 grapplePos = new Vector3(Grapple.transform.localPosition.x, Grapple.transform.localPosition.y, Mathf.Lerp(Grapple.transform.localPosition.z, GrappleChargePos, .1f));
            Grapple.transform.localPosition = grapplePos;

        }
        else
        {
            Vector3 grapplePos = new Vector3(Grapple.transform.localPosition.x, Grapple.transform.localPosition.y, Mathf.Lerp(Grapple.transform.localPosition.z, GrappleStartPos, .7f));
            Grapple.transform.localPosition = grapplePos;
        }

        if (pc.grappling)
        {
            Debug.DrawRay(grappleHole.position, pc.grappleTarget, Color.blue);
        }

        Vector3 gunPos = new Vector3(Gun.transform.localPosition.x, Gun.transform.localPosition.y, Mathf.Lerp(Gun.transform.localPosition.z, GunStartPos, 0.02f));
        Gun.transform.localPosition = gunPos;
    }
    IEnumerator reload()
    {
        gunLoaded = false;
        yield return new WaitForSeconds(firerate);
        gunLoaded = true;
    }
}
