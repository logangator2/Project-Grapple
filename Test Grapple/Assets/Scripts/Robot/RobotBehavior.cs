using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotBehavior : MonoBehaviour
{
    public enum Behavior
    {
        Idle = 0,
        Patrolling = 1,
        Engaging = 2,
        Cooldown = 3,
        Searching = 4
    }

    // public variables
    public Behavior behaviorStatus;
    public Transform patrolpointA, patrolpointB;
    
    // private variables
    private float patrolDistance = 2f;

    // Unity variables
    NavMeshAgent agent;
    Rigidbody rb;

    // Unity Functions

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); 
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (behaviorStatus != Behavior.Idle)
        {
            // check behavior
            if (behaviorStatus == Behavior.Patrolling) {Patrol();}
            else if (behaviorStatus == Behavior.Engaging) {Engage();}
            else if (behaviorStatus == Behavior.Cooldown) {Cool();}
            else if (behaviorStatus == Behavior.Searching) {Patrol();}
        }
        // else 
        // {
        //     // await activation
        // }
    }

    // Custom Functions

    void Patrol()
    {
        // behaviorStatus = Behavior.Patrolling;
        if (Vector3.Distance(rb.position, patrolpointA.position) <= patrolDistance)
        {
            agent.destination = patrolpointB.position;
        }
        else if (Vector3.Distance(rb.position, patrolpointB.position) <= patrolDistance)
        {
            agent.destination = patrolpointA.position;
        }
    }

    void Engage()
    {
        behaviorStatus = Behavior.Engaging;
    }   

    void Cool()
    {
        behaviorStatus = Behavior.Cooldown;
    }

    void Search()
    {
        behaviorStatus = Behavior.Searching;
    }
}
