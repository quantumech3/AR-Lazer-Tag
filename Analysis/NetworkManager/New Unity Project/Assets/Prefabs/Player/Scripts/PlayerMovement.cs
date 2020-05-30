using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(!isLocalPlayer)
        {
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position.Set(transform.position.x, transform.position.y, transform.position.z);
        //Debug.Log(transform.position);
    }
}
