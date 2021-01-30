using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[NetworkSettings(sendInterval = 0.01f)]
public class LazerBehavior : NetworkBehaviour
{
    public float speed = 0.1f;
    private bool transformIsLocalized = false;
    private GameObject origin = null;
    public float despawnRadius = 10;

    private void Start()
    {
        /* if (isClient && GameObject.Find("Origin(Clone)") != null)
        {
            this.origin = GameObject.Find("Origin(Clone)");
            this.transform.SetParent(origin.transform, false);
            transformIsLocalized = true;
        } */ //PATCH: Made lazer position independant of VIO
    }

    private void Update()
    {
        /* if (isClient && !transformIsLocalized && GameObject.Find("Origin(Clone)") != null)
        {
            this.origin = GameObject.Find("Origin(Clone)");
            this.transform.SetParent(origin.transform, false);
            transformIsLocalized = true;
        } */
    }

    private void FixedUpdate()
    {
        if(isServer)
        {
            // Spawn a new lazer with its position shifted then delete this lazer.
            // If this lazer is not over the despawn radius then move
            if (this.transform.position.magnitude < despawnRadius)
            {
                GameObject _lazer = Instantiate(this.gameObject);
                _lazer.transform.position += _lazer.transform.forward * speed * Time.deltaTime;
                NetworkServer.Spawn(_lazer);
            }
                
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
