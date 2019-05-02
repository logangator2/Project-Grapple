using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robotMover : MonoBehaviour
{
    public bool right = true;
    public float speed = 3.0f;
    public int paramLeft = 0;
    public int paramRight = 0;

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x >= paramRight)
        {
            right = false;
        }
        else if (transform.position.x <= paramLeft)
        {
            right = true;
        }
        if (right)
        {
            transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
        }
        else
        {
            transform.position += new Vector3(-speed * Time.deltaTime, 0, 0);

        }
    }
}
