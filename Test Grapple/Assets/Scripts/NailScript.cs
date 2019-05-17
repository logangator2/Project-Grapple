using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailScript : MonoBehaviour
{
    private float flySpeed = 30f;

    void FixedUpdate()
    {
        transform.position += transform.up * Time.deltaTime * flySpeed;
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<MeshCollider>());
        Destroy(gameObject, 20f);
        Destroy(this);
    }
}
