using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ARNetworkManager : NetworkManager
{
    override public void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("Client connected!");
    }
}
