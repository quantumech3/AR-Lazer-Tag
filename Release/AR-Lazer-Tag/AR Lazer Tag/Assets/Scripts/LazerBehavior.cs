using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LazerBehavior : NetworkBehaviour
{
    // TODO: YOU MAY HAVE TO CHANGE HOW THIS COMPONENT WORKS IF THE LAZER POSITIONS ARE NOT PROPERLY SYNCED. YOU MIGHT NEED TO REPORT POSITION RELATIVE TO ORIGIN SIMILAR TO HOW PLAYERS DO IT
    public float speed = 0.1f;

    private void FixedUpdate()
    {
        this.transform.position += this.transform.TransformDirection(new Vector3(0, 0, 1)).normalized * speed * Time.deltaTime;
    }
}
