using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBot : RobotBehavior
{
    public Vector3 engageBuffer;
    public GameObject Player;
    private LineRenderer laserLine;

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
    }

    new void Patrol()
    {
        // patrolling from A to B
        if (Vector3.Distance(rb.position, patrolpointA.position) <= patrolDistance)
        {
            agent.destination = patrolpointB.position;
        }
        else if (Vector3.Distance(rb.position, patrolpointB.position) <= patrolDistance)
        {
            agent.destination = patrolpointA.position;
        }

        // if raycast hits player, engage
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out detected, alertDistance))
        {
            if (detected.collider.tag == "Player")
            {
                Engage();
            }
        }
    }

    new void Engage()
    {
        agent.destination = Player.transform.position - engageBuffer;
        if (Vector3.Distance(transform.position, agent.destination) <= 10)
        {
            agent.isStopped = true;
            rb.velocity = Vector3.zero;
        }
    }

    void Fire()
    {
        
    }
}