using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBot : RobotBehavior
{
    public float laserDuration, laserDelay, laserDistance, engageDistance;
    public Vector3 laserOffset, engageBuffer;

    private LineRenderer laserLine;

    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
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
    }

    new void Patrol()
    {
        // Debug.Log("Patrolling");
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
        Debug.Log("Engaging");
        behaviorStatus = Behavior.Engaging;
        // follow player
        agent.destination = Player.transform.position - engageBuffer;
        // fire laser
        StartCoroutine("Fire");
        // set engagement destination, offset by engagementbuffer
        if (Vector3.Distance(transform.position, agent.destination) <= 10)
        {
            agent.destination = Player.transform.position - engageBuffer;
        }
    }

    IEnumerator Fire()
    {
        // Debug.Log("Firing");
        // set starting point of laser, using the laserOffset
        laserLine.SetPosition(0, transform.position + laserOffset);

        RaycastHit line;
        if (Physics.Raycast(transform.position, transform.forward, out line, laserDistance))
        {
            if (line.collider)
            {
                laserLine.SetPosition(1, line.point);
                laserLine.enabled = true;
                if (line.collider.tag == "Player")
                {
                    // Debug.Log("Player hit by laser!");
                    PlayerController x = Player.GetComponent<PlayerController>();
                    x.Respawn();

                    // reset player position
                    laserLine.enabled = false;
                    behaviorStatus = originalBehavior;
                    transform.position = originalPosition;
                }
                else 
                {
                    // hits the wall
                    yield return new WaitForSeconds(laserDuration);
                    laserLine.enabled = false;
                }
            }
        }
    }    
}