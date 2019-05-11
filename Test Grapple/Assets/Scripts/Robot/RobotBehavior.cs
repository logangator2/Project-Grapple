using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Behavior behaviorStatus;

    void Patrol()
    {
        behaviorStatus = Behavior.Patrolling;
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
