using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARNetworkManagerHandler : MonoBehaviour
{
    public ARNetworkManager networkManager;

    private void Start()
    {
        networkManager = GameObject.Find("AR Network Manager").GetComponent<ARNetworkManager>();
    }

    public void ClientStart()
    {
        networkManager.ClientStart();
    }

    public void ServerStart()
    {
        networkManager.ServerStart();
    }

    public void StopClient()
    {
        networkManager.StopClient();
    }

    public void StopServer()
    {
        networkManager.StopServer();
    }
}
