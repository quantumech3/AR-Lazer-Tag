using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class UINetwork : NetworkManager
{
    public const int PORT = 7001;
    public string clientAddr;

    public void StartServer_PC()
    {
        this.StartServer();
        Debug.Log("Server started on port: " + PORT.ToString());
    }

    public void StopServer_PC()
    {
        this.StopServer();
    }

    public void StartClient_Android()
    {
        clientAddr = gameObject.GetComponentsInChildren<Text>()[3].text;
        this.networkAddress = clientAddr;
        this.networkPort = PORT;
        this.StartClient();
        Debug.Log("Client started at address: " + this.networkAddress + ":" + this.networkPort);
    }

    public void StopClient_Android()
    {
        this.StopClient();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("[Server] Client logged in with address: " + conn.address);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("[Client] Client logged in with address: " + conn.address);
    }
}
