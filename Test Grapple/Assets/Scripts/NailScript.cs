using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Robot")
        {
            gameObject.transform.parent = other.transform;
        }

        Destroy(GetComponent<MeshCollider>());
        Destroy(gameObject, 20f);
        Destroy(this);
    }
}
