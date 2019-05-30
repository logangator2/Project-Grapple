using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointRotator : MonoBehaviour
{

    public GameObject aroundThis;
    public int speed = 50;
    public bool clockwise = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clockwise)
        {
            transform.RotateAround(aroundThis.transform.position, aroundThis.transform.up, speed * Time.deltaTime);
        }
        else
        {
            transform.RotateAround(aroundThis.transform.position, -aroundThis.transform.up, speed * Time.deltaTime);
        }
    }
}
