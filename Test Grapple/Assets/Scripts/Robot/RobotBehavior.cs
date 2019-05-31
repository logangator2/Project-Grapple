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
        Searching = 3
    }

    // public variables
    public float alertDistance;
    public Behavior behaviorStatus;
    public Transform patrolpointA, patrolpointB;
    
    // protected variables
    protected float patrolDistance = 2f;
    protected static Behavior originalBehavior;

    // Unity variables
    protected GameObject Player;
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected RaycastHit detected;
    protected static Vector3 originalPosition;

    // Unity Functions

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); 
        rb = GetComponent<Rigidbody>();

        originalBehavior = behaviorStatus;
        originalPosition = transform.position;
    }

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (behaviorStatus != Behavior.Idle)
        {
            // check behavior
            if (behaviorStatus == Behavior.Patrolling) {Patrol();}
            else if (behaviorStatus == Behavior.Engaging) {Engage();}
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
        // alert doors
    }

    // Custom Functions

    protected void Patrol()
    {
        if (Vector3.Distance(rb.position, patrolpointA.position) <= patrolDistance)
        {
            // ADD: search here
            agent.destination = patrolpointB.position;
        }
        else if (Vector3.Distance(rb.position, patrolpointB.position) <= patrolDistance)
        {
            // ADD: search here
            agent.destination = patrolpointA.position;
        }
    }

    protected void Engage()
    {
        // empty, to be defined in child classes
    }   

    protected void Search()
    {

    }
}
