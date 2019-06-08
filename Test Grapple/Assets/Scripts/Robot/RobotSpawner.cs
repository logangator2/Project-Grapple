using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotSpawner : MonoBehaviour
{
    public float spawnDelay;
    public GameObject[] spawners;
    public GameObject SpiderBot;
    void Awake()
    {
        foreach (var spawner in spawners)
        {
            Instantiate(SpiderBot, spawner.transform);
        }
    }

    void Start()
    {
        InvokeRepeating("AwaitSpawn", spawnDelay, spawnDelay);
    }

    void AwaitSpawn()
    {
        foreach (var spawner in spawners)
        {
            Instantiate(SpiderBot, spawner.transform);
        }
    }
}
