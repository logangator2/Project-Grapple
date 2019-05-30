using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{

    [SerializeField] private GameObject crosshairs;
    [SerializeField] private Image anchorHair;
    [SerializeField] private Image vignette;

    private PlayerController pc;
    private int VIGOPACITY = 40;
    private int VIGGRAPPLINGOPACITY = 200;
    private int ROTSPEED = 180;
    private float OPACITYDECELERATION = 2f;

    void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (pc.grappleCharging)
        {
            vignette.color = new Color(255, 255, 255, VIGGRAPPLINGOPACITY);
            crosshairs.gameObject.transform.Rotate(new Vector3(0, 0, 1) * Time.deltaTime * ROTSPEED);

            if (pc.aimedAtAnchor())
            {
                anchorHair.gameObject.SetActive(true);
            }
            else
            {
                anchorHair.gameObject.SetActive(false);
            }

            if (Input.GetMouseButtonUp(1))
            {
                anchorHair.gameObject.SetActive(false);
                crosshairs.gameObject.transform.rotation = Quaternion.identity;

            }
        }
        else
        { 
            vignette.color = new Color(255, 255, 255, VIGGRAPPLINGOPACITY);
        }
    }

}
