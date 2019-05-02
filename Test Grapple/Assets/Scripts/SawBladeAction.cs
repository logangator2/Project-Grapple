using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawBladeAction : MonoBehaviour
{
    public int LevelFloor = 0;
    public float timeToDestroy = 3.0f;
    public Vector3 respawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y <= LevelFloor)
        {
            Destroy(gameObject);
        }
        Destroy(gameObject, timeToDestroy);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.transform.position = respawnPoint;
        }
    }
}
