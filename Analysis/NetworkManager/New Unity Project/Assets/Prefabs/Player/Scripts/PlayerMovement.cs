using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{   
    public GameObject gameObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isServer)
        {
           NetworkServer.Spawn(Instantiate(gameObject, new Vector3(0, 0, 0), Quaternion.identity));
        }
    }
}
