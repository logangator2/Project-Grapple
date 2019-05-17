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
    public float alertDistance;
    public Behavior behaviorStatus;
    public Transform patrolpointA, patrolpointB;
    
    // private variables
    protected float patrolDistance = 2f;

    // Unity variables
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected RaycastHit detected;

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

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Nail")
        {
            gameObject.SetActive(false);
        }
    }

    // Custom Functions

    protected void Patrol()
    {
        if (Vector3.Distance(rb.position, patrolpointA.position) <= patrolDistance)
        {
            agent.destination = patrolpointB.position;
        }
        else if (Vector3.Distance(rb.position, patrolpointB.position) <= patrolDistance)
        {
            agent.destination = patrolpointA.position;
        }

        // if player detected, engage
    }

    protected void Engage()
    {
        // empty, to be defined in child classes
    }   

    protected void Cool()
    {

    }

    protected void Search()
    {

    }
}
