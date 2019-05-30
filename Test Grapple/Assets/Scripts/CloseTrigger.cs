using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseTrigger : MonoBehaviour
{
    public DoorMechanic door;

    void OnTriggerEnter()
    {
        // close door
        door.Close();
        // activate alert and red light
    }
}
