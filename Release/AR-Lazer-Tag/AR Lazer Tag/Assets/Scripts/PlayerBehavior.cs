using Google.XR.ARCoreExtensions;
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

    private void InstantiateARObjects()
    {
        // Instantiate ARSession object
        GameObject session = Instantiate(arSessionPrefab);
        session.name = "AR Session";

        // Instantiate ARSessionOrigin object
        GameObject sessionOrigin = Instantiate(arSessionOriginPrefab);
        sessionOrigin.name = "AR Session Origin";

        // Set references
        this.camera = GameObject.Find("AR Camera");
        this.raycastManager = sessionOrigin.GetComponent<ARRaycastManager>();
        this.anchorManager = sessionOrigin.GetComponent<ARAnchorManager>();

        // Instantiate ARCoreExtensions object
        ARCoreExtensions arCoreExtensions = Instantiate(arExtensionsPrefab).GetComponent<ARCoreExtensions>();
        arCoreExtensions.name = "ARCore Extensions";
        arCoreExtensions.Session = session.GetComponent<ARSession>();
        arCoreExtensions.SessionOrigin = sessionOrigin.GetComponent<ARSessionOrigin>();
        arCoreExtensions.CameraManager = this.camera.GetComponent<ARCameraManager>();
    }

    void Start()
    {
        // Set this.networkManager to the ARNetworkManager in the scene
        this.networkManager = GameObject.Find("AR Network Manager").GetComponent<ARNetworkManager>();
        // Set this.serverInfo to the ServerInfo component of the Server Info game object in the scene
        this.serverInfo = GameObject.Find("Server Info").GetComponent<ServerInfo>();

        if(isClient)
        {
            // Set the player's initial game state based off of the state of the server
            if (serverInfo.state == GameState.Pregame)
            {
                CmdSetPlayerState(GameState.Pregame);
            }
            else
            {
                CmdSetPlayerState(GameState.Waiting);
            }

            if(isLocalPlayer)
            {
                // Set this.playerInfo to the PlayerInfo component of the Player Info game object in the scene
                this.playerInfo = GameObject.Find("Player Info").GetComponent<PlayerInfo>();

                // Set the height of this player
                CmdSetHeight(playerInfo.height);

                // Instantiate AR Objects in the player's scene
                InstantiateARObjects();

                // TODO: Implement the rest of Start() starting from line 61 and on of the design
            }
        }
    }
}
