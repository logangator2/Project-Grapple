using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour
{
    Light lt;

    // Start is called before the first frame update
    void Start()
    {
        lt = GetComponent<Light>();   
    }

    // Update is called once per frame
    void Update()
    {
        // based off of Unity tutorial at unity3d.com/ScriptReference/Color.Lerp.html
        lt.color = Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1));
    }
}