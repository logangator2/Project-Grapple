using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shooter : MonoBehaviour
{
    public GameObject blade;
    public Transform makePoint;
    public float delay = 1.0f;
    public int sawForce = 100;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("ShotDelay");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator ShotDelay()
    {
        for(;;)
        {
            GameObject sawblade = Instantiate(blade, makePoint.position, Quaternion.identity) as GameObject;
            sawblade.GetComponent<Rigidbody>().AddForce(makePoint.forward * sawForce);
            yield return new WaitForSeconds(delay);
        }
    }
}
