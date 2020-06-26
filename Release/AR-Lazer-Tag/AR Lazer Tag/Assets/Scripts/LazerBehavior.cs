using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LazerBehavior : NetworkBehaviour
{
    public float speed = 0.1f;

    private void FixedUpdate()
    {
        this.transform.position += this.transform.TransformDirection(new Vector3(0, 0, 1)).normalized * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }
}
