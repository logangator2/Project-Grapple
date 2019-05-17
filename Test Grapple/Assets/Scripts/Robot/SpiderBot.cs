using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBot : RobotBehavior
{
    public float laserDuration, laserDelay;
    public Vector3 fudge, laserOffset, engageBuffer;
    public GameObject Player;

    private LineRenderer laserLine;

    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
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
        behaviorStatus = Behavior.Engaging;
        // follow player
        agent.destination = Player.transform.position - engageBuffer;
        // fire laser
        // StartCoroutine("Fire");
        if (Vector3.Distance(transform.position, agent.destination) <= 10)
        {
            agent.destination = Player.transform.position = engageBuffer;
        }
        // FIXME: Add way for robot to exit "Engage"
    }

    IEnumerator Fire()
    {
        laserLine.SetPosition(0, transform.position + laserOffset);
        laserLine.enabled = true;
        laserLine.SetPosition(1, Player.transform.position + fudge); // FIXME: Needs correct fidge factor
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
        yield return new WaitForSeconds(laserDelay);
    }    
}