using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerInfo : NetworkBehaviour
{
    [SyncVar]
    public GameState state;

    [SyncVar]
    public string originId;
}
