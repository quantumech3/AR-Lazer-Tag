using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

public class PlayerBehavior : NetworkBehaviour
{
    [SyncVar]
    public GameState state;

    [SyncVar]
    public double height;

    [SyncVar]
    public Vector3 position;

    public GameObject pregameGuiPrefab;
    public GameObject waitingGuiPrefab;
    public GameObject aliveGuiPrefab;
    public GameObject deadGuiPrefab;
    public GameObject originPrefab;
    public GameObject playerHitboxPrefab;
    public GameObject lazerPrefab;
    public GameObject arSessionOriginPrefab;
    public GameObject arSessionPrefab;
    public GameObject arExtensionsPrefab;
    public ARNetworkManager networkManager;
    public ServerInfo serverInfo;

    private PlayerInfo playerInfo;
    private GameObject origin;
    private GameObject playerHitbox;
    private GameObject camera;
    private ARRaycastManager raycastManager;
    private ARAnchorManager anchorManager;

    [Command]
    public void CmdSetHeight(double height)
    {
        this.height = height;
    }

    [Command]
    public void CmdSetPlayerState(GameState state)
    {
        this.state = state;
    }

    [Command]
    public void CmdSetServerState(GameState state)
    {
        this.serverInfo.state = state;
    }

    [Command]
    public void CmdSetOriginId(string id)
    {
        this.serverInfo.originId = id;
    }

    [Command]
    public void CmdSetPosition(Vector3 position)
    {
        this.position = position;
    }

    [Command]
    public void CmdSpawnLazerAt(Vector3 position, Quaternion rotation)
    {
        GameObject lazer = Instantiate(lazerPrefab, position, rotation);
        NetworkServer.Spawn(lazer);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CmdSpawnLazerAt(this.transform.position, this.transform.rotation);
    }
}
