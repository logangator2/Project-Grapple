using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotSpawner : MonoBehaviour
{
    public GameObject[] spawners;
    public GameObject SpiderBot;
    void Awake()
    {
        foreach (var spawner in spawners)
        {
            Instantiate(SpiderBot, spawner.transform);
        }
    }
}
