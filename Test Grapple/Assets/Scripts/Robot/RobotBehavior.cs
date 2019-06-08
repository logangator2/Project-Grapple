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
    public float alertDistance, fallSpeed, deathX, deathY, deathZ;
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
    [SerializeField] protected Vector3 targetAngle = new Vector3(90f, 0f, 0f);
    protected Vector3 currentAngle;
    protected Quaternion from, to;
    protected static Vector3 originalPosition;

    // Unity Functions

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); 
        rb = GetComponent<Rigidbody>();
        from = transform.GetChild(1).rotation;
        to = new Quaternion(90f, 90f, 90f, 0f);

        originalBehavior = behaviorStatus;
        originalPosition = transform.position;
    }

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        currentAngle = transform.eulerAngles;
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
            Die();
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

    protected void Die()
    {
        // Lerp fall
        // transform.Rotate (targetAngle * (fallSpeed * Time.deltaTime));
        // transform.Rotate(deathX, deathY, deathZ, Space.World);
        transform.GetChild(1).rotation = Quaternion.Lerp(from, to, Time.time * fallSpeed);
        // currentAngle = new Vector3(
        //      Mathf.LerpAngle(currentAngle.x, targetAngle.x, Time.deltaTime * fallSpeed),
        //      Mathf.LerpAngle(currentAngle.y, targetAngle.y, Time.deltaTime * fallSpeed),
        //      Mathf.LerpAngle(currentAngle.z, targetAngle.z, Time.deltaTime * fallSpeed));
 
        //  transform.eulerAngles = currentAngle;
        // destroy everything except the model
        gameObject.SetActive(false);
    }
}
