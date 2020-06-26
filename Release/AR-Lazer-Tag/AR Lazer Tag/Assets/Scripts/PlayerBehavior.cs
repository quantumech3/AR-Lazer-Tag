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
    private GameObject origin = null;
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

                // Spawn an appropriate gui depending on the initial player state
                if (this.state == GameState.Pregame)
                {
                    GameObject gui = Instantiate(pregameGuiPrefab);
                    GuiTransitionHandler transitionHandler = gui.GetComponent<GuiTransitionHandler>();
                    transitionHandler.player = this.gameObject;
                }
                else if(this.state == GameState.Waiting)
                {
                    GameObject gui = Instantiate(waitingGuiPrefab);
                    GuiTransitionHandler transitionHandler = gui.GetComponent<GuiTransitionHandler>();
                    transitionHandler.player = this.gameObject;
                }
            }
        }
    }

    void Update()
    {
        switch (this.state)
        {
            case GameState.Pregame:
                PregameUpdate();
                break;
            case GameState.Waiting:
                WaitingUpdate();
                break;
            case GameState.Alive:
                AliveUpdate();
                break;
            case GameState.Dead:
                DeadUpdate();
                break;
        }
    }

    public void PregameUpdate()
    {
        if(isLocalPlayer && isClient)
        {
            // If the user taps the screen
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                List<ARRaycastHit> hitpoints = new List<ARRaycastHit>();
                raycastManager.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f), hitpoints);

                if(hitpoints.Count > 0)
                {
                    CmdSetServerState(GameState.Waiting);

                    // this.origin = an instantiated "originPrefab" game object located at the transform of hitpoints[0]
                    this.origin = Instantiate(originPrefab, hitpoints[0].pose.position, hitpoints[0].pose.rotation);

                    // ARCloudAnchor anchor = A cloud anchor instantiated at the transform of hitpoints[0]
                    ARCloudAnchor anchor = anchorManager.HostCloudAnchor(anchorManager.AddAnchor(hitpoints[0].pose));

                    // Set the "cloudAnchor" attribute of the "OriginBehavior" component of "origin" to "anchor"
                    this.origin.GetComponent<OriginBehavior>().cloudAnchor = anchor;
                }
            }

            if (serverInfo.state != GameState.Pregame)
                CmdSetPlayerState(GameState.Waiting);
        }
    }

    public void WaitingUpdate()
    {
        if(isClient)
        {
            if(this.origin != null)
            {
                OriginBehavior originBehavior = this.origin.GetComponent<OriginBehavior>();

                if(originBehavior.cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
                {
                    CmdSetOriginId(originBehavior.cloudAnchor.cloudAnchorId);
                    CmdSetServerState(GameState.Alive);
                    CmdSetPlayerState(GameState.Alive);
                }
                else if(originBehavior.cloudAnchor.cloudAnchorState != CloudAnchorState.TaskInProgress)
                {
                    Debug.LogError("Had trouble hosting cloud anchor: " + originBehavior.cloudAnchor.cloudAnchorState);
                    Debug.LogError("Trying again..");

                    Destroy(this.origin);
                    this.origin = null;
                }
            }
            else if(serverInfo.originId != string.Empty)
            {
                if(this.origin == null)
                {
                    this.origin = Instantiate(originPrefab);
                    OriginBehavior originBehavior = this.origin.GetComponent<OriginBehavior>();

                    originBehavior.cloudAnchor = anchorManager.ResolveCloudAnchorId(serverInfo.originId);
                }
            }
        }
    }

    public void AliveUpdate()
    {
        if(isClient)
        {
            // Tell ARCore that the initial position of the player is the origin, not where the player starts the program initially
            //GameObject sessionOrigin = GameObject.Find("AR Session Origin");
            //sessionOrigin.transform.position = Vector3.zero;
            //sessionOrigin.transform.rotation = Quaternion.identity;
            //sessionOrigin.transform.SetParent(this.origin.transform, false);

            bool playerHasBeenHit = false;

            if (this.playerHitbox != null)
            {
                playerHasBeenHit = this.playerHitbox.GetComponent<PlayerHitboxBehavior>().hasBeenHit;
                Destroy(this.playerHitbox);
                playerHitbox = null;
            }

            if (isLocalPlayer)
            {
                // If this player's hitbox has been hit
                if (playerHasBeenHit)
                {
                    CmdSetPlayerState(GameState.Dead);
                    return;
                }

                // CmdSetPosition(local position of "this" relative to this.origin)
                CmdSetPosition(this.origin.transform.InverseTransformPoint(this.camera.transform.position));

                if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                {
                    Transform lazerTransform = this.camera.transform;

                    // Translate lazerTransform out 0.65 units in the direction it is facing
                    lazerTransform.position += lazerTransform.forward * 0.65f;

                    CmdSpawnLazerAt(lazerTransform.position, lazerTransform.rotation);
                }
            }

            // Set this.playerHitbox to an instance of playerHitboxPrefab instantiated at "position" relative to "origin"
            this.playerHitbox = Instantiate(playerHitboxPrefab, this.position, Quaternion.identity);
            this.playerHitbox.transform.SetParent(this.origin.transform, false);

            // Scale this.playerHitbox's Y dimension by this.height/39.3701
            this.playerHitbox.transform.localScale = new Vector3(0.45f, (float)this.height / 39.3701f, 0.45f);
        }
    }

    public void DeadUpdate()
    {

    }
}
