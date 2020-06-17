using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager_AR : NetworkManager
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("A client connected!");
    }
}
