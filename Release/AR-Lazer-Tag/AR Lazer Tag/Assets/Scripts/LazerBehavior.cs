using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//[NetworkSettings(sendInterval = 0.01f)]
public class LazerBehavior : NetworkBehaviour
{
    public float speed = 0.1f;

    private void Start()
    {
        if (isClient)
        {
            this.transform.SetParent(GameObject.Find("Origin(Clone)").transform, false);
        }
    }

    private void FixedUpdate()
    {
        if(isServer)
        {
            // Spawn a new lazer with its position shifted then delete this lazer.
            // I am doing this as a part of a work-around to localize position relative to origin
            GameObject _lazer = Instantiate(this.gameObject);
            _lazer.transform.position += _lazer.transform.forward * speed * Time.deltaTime;
            NetworkServer.Spawn(_lazer);
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
