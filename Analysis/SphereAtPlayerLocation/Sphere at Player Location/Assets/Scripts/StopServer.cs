using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StopServer : MonoBehaviour
{
    public NetworkManager networkManager;

    public void ServerStop()
    {
        networkManager = GameObject.Find("Network Manager").GetComponent<NetworkManager>();
        networkManager.StopServer();
    }
}
