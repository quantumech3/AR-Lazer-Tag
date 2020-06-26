using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HostGuiBehavior : NetworkBehaviour
{
    public ARNetworkManager networkManager;
    public ServerInfo serverInfo;
    public Text gameStateDisplay;

    void Update()
    {
        this.gameStateDisplay.text = serverInfo.state.ToString();
    }
}
