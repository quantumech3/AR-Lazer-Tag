using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LazerBehavior : NetworkBehaviour
{
    // TODO: YOU MAY HAVE TO CHANGE HOW THIS COMPONENT WORKS IF THE LAZER POSITIONS ARE NOT PROPERLY SYNCED. YOU MIGHT NEED TO REPORT POSITION RELATIVE TO ORIGIN SIMILAR TO HOW PLAYERS DO IT
    // Maybe you may be able to use https://answers.unity.com/questions/1272882/can-i-get-networktransform-to-sync-local-coordinat.html to help you automatically sync local position.
    public float speed = 0.1f;

    private void FixedUpdate()
    {
        this.transform.position += this.transform.TransformDirection(new Vector3(0, 0, 1)).normalized * speed * Time.deltaTime;
    }
}
