using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBot : RobotBehavior
{
    // public variables
    
    // private variables

    // Unity variables
    public GameObject Player;
    private Vector3 _angles;
    private AudioSource aud;

    // Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
        aud.loop = true;
        _angles = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        if (behaviorStatus != Behavior.Idle)
        {
            // check behavior
            if (behaviorStatus == Behavior.Patrolling) {Patrol();}
            else if (behaviorStatus == Behavior.Engaging) {Engage();}
            else if (behaviorStatus == Behavior.Searching) {Alert();}
        }

        // trigger alarm if player gets too close
        if (Vector3.Distance(Player.transform.position, transform.position) < alertDistance)
        {
            // Alert();
            if (!aud.isPlaying){aud.Play();}
            // aud.Play();
            behaviorStatus = Behavior.Searching;
        }
        else
        {
            aud.Stop();
        }
    }

    // For Physics steps
    void FixedUpdate()
    {

    }

    // Custom Methods

    void Alert()
    {
        // look at player - from Wills example
        // Calculate vector from pickup to player.
        Vector3 d = Player.transform.position - transform.position;

        // Normalize to a direction.
        d.Normalize();

        float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.forward, d));

        // Textbook, but *not* most efficient solution - think scalar projection.
        Vector3 cross = Vector3.Cross (Vector3.forward, d);
        if (cross.y < 0.0f) {
            angle = -angle;
        }

        _angles.y = angle;
        transform.eulerAngles = _angles;

        // start following player

        // play alert sound
        // aud.Play();

        // alert other robots

    }
}
