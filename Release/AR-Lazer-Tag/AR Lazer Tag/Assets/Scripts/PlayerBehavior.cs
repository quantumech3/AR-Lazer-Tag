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
    private CSVLogger csvLogger;

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

    [Command]
    public void CmdLogData(NetworkInstanceId networkInstanceId, float timeStamp, Vector3 vioPosition, Vector3 rfPosition, Vector3 fusionPosition, Vector3 vioRotation, Vector3 accelerometer, Vector3 gyroscope, Vector3 magnetometer)
    {
        if (csvLogger == null)
        {
            csvLogger = new CSVLogger(new string[]{"Client ID", "Timestamp",
                                       "VIO Position X", "VIO Position Y", "VIO Position Z",
                                       "RF Position X", "RF Position Y", "RF Position Z",
                                       "Fusion Position X", "Fusion Position Y", "Fusion Position Z",
                                       "VIO Rotation X", "VIO Rotation Y", "VIO Rotation Z",
                                       "Accelerometer X", "Accelerometer Y", "Accelerometer Z",
                                       "Gyroscope X", "Gyroscope Y", "Gyroscope Z",
                                       "Magnetometer X", "Magnetometer Y", "Magnetometer Z"}, "C:/temp/arLog.csv");
        }
        csvLogger.Write(new string[] {networkInstanceId.Value.ToString(), timeStamp.ToString(),
                                      vioPosition.x.ToString(), vioPosition.y.ToString(), vioPosition.z.ToString(),
                                      rfPosition.x.ToString(), rfPosition.y.ToString(), rfPosition.z.ToString(),
                                      fusionPosition.x.ToString(), fusionPosition.y.ToString(), fusionPosition.z.ToString(),
                                      vioRotation.x.ToString(), vioRotation.y.ToString(), vioRotation.z.ToString(),
                                      accelerometer.x.ToString(), accelerometer.y.ToString(), accelerometer.z.ToString(),
                                      gyroscope.x.ToString(), gyroscope.y.ToString(), gyroscope.z.ToString(),
                                      magnetometer.x.ToString(), magnetometer.y.ToString(), magnetometer.z.ToString()});
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
        this.camera.GetComponent<SetToPlayerPosition>().player = this.gameObject; // PATCH: AR camera now sets itself to players position. Therefore it needs a handle to the player game object
        this.raycastManager = sessionOrigin.GetComponent<ARRaycastManager>();
        this.anchorManager = sessionOrigin.GetComponent<ARAnchorManager>();

        // Instantiate ARCoreExtensions object
        ARCoreExtensions arCoreExtensions = Instantiate(arExtensionsPrefab).GetComponent<ARCoreExtensions>();
        arCoreExtensions.name = "ARCore Extensions";
        arCoreExtensions.Session = session.GetComponent<ARSession>();
        arCoreExtensions.SessionOrigin = sessionOrigin.GetComponent<ARSessionOrigin>();
        arCoreExtensions.CameraManager = this.camera.GetComponent<ARCameraManager>();

        // Set VIO Origin to RF origin in ARPlayerPoseTracker (The origin isnt actually used)
        ARPlayerPoseTracker.vioOrigin = GameObject.Find("RF Origin");
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

                // Enable gyroscope and compass
                Input.gyro.enabled = true;
                Input.compass.enabled = true;
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

        // TODO: Make this set camera position to the position of the player in fusion space
    }

    public void PregameUpdate()
    {
        if(isLocalPlayer && isClient)
        {
            CmdSetServerState(GameState.Waiting);
            CmdSetPlayerState(GameState.Waiting);            
        }
    }

    public void WaitingUpdate()
    {
        if (isClient)
        {
            CmdSetServerState(GameState.Alive);
            CmdSetPlayerState(GameState.Alive);
        }
    }

    public void AliveUpdate()
    {
        if(isClient)
        {
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
                if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                {
                    Transform lazerTransform = this.camera.transform; // PATCH: Made init lazer position independant of VIO

                    CmdSpawnLazerAt(lazerTransform.position + lazerTransform.forward * 0.5f, lazerTransform.rotation);
                }

                CmdLogData(GetComponent<NetworkIdentity>().netId, Time.time, this.GetComponent<ARPlayerPoseTracker>().vioPosition, this.GetComponent<ARPlayerPoseTracker>().rfPosition, this.camera.transform.position, this.camera.transform.rotation.eulerAngles, Input.acceleration, Input.gyro.attitude.eulerAngles, Input.compass.rawVector);
            }

            // Set this.playerHitbox to an instance of playerHitboxPrefab instantiated at the position of the player
            this.playerHitbox = Instantiate(playerHitboxPrefab, this.transform.position, Quaternion.identity); // PATCH: Made PlayerBehavior independant of VIO

            // Scale this.playerHitbox's Y dimension by this.height/39.3701
            this.playerHitbox.transform.localScale = new Vector3(0.45f, (float)this.height / 39.3701f, 0.45f);
        }
    }

    public void DeadUpdate()
    {

    }
}
