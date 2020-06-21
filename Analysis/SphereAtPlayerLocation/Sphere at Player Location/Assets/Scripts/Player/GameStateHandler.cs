using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;

public class GameStateHandler : NetworkBehaviour
{
    public GameState gameState;
    public GameObject PlaceOriginUI;
    public GameObject OriginRegisteredUI;
    public GameObject WaitingForOriginRegistrationUI;
    public ARAnchorManager anchorManager;
    public ARRaycastManager raycastManager;

    public GameObject playerCamera;

    public ARCloudAnchor origin=null;

    public GameObject testGetRidOfThis;

    private GameObject testObject = null;


    // Start is called before the first frame update
    void Start()
    {
        GameObject arSessionOrigin = GameObject.Find("AR Session Origin");

        this.gameState = GameObject.Find("GameState").GetComponent<GameState>();
        this.anchorManager = arSessionOrigin.GetComponent<ARAnchorManager>();
        this.raycastManager = arSessionOrigin.GetComponent<ARRaycastManager>();
        this.playerCamera = GameObject.Find("AR Camera");
    }

    void Update()
    {
        if(isClient)
        {
            //TODO: Implement GUI
            switch (gameState.state)
            {
                case GameState.States.PlaceOrigin:
                    onPlaceOrigin();
                    break;
                case GameState.States.WaitingForOriginRegistration:
                    onWaitingForOriginRegistration();
                    break;
                case GameState.States.OriginRegistered:
                    onOriginRegistered();
                    break;
            }
        }
    }

    public void onPlaceOrigin()
    {
        if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            raycastManager.Raycast(Input.touches[0].position, hitResults);

            if(hitResults.Count > 0)
            {
                CmdChangeGameState(GameState.States.WaitingForOriginRegistration);

                ARAnchor localAnchor = anchorManager.AddAnchor(hitResults[0].pose);
                origin = anchorManager.HostCloudAnchor(localAnchor);
            }
        }
    }

    public void onWaitingForOriginRegistration()
    {
        switch(origin.cloudAnchorState)
        {
            case CloudAnchorState.Success:
                CmdSetOriginID(origin.cloudAnchorId);
                CmdChangeGameState(GameState.States.OriginRegistered);
                break;
            case CloudAnchorState.TaskInProgress:
                break;
            default:
                CmdChangeGameState(GameState.States.PlaceOrigin);
                Debug.LogError("Could not host cloud anchor: " + origin.cloudAnchorState.ToString());
                break;
        }
    }

    public void onOriginRegistered()
    {
        if(this.origin == null)
        {
            origin = anchorManager.ResolveCloudAnchorId(gameState.originId);
        }
        else if(this.origin.cloudAnchorState == CloudAnchorState.Success)
        {
            Vector3 localCameraPos = origin.transform.InverseTransformPoint(playerCamera.transform.position);
            Vector3 localCameraRot = origin.transform.InverseTransformDirection(playerCamera.transform.rotation.eulerAngles);

            CmdInstantiateInOriginSpace(localCameraPos, localCameraRot);
        }
        else if(this.origin.cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            Debug.Log("Had trouble resolving the origin: " + this.origin.cloudAnchorState.ToString());
            Debug.Log("Trying again...");
            this.origin = null;
        }
    }

    [Command]
    public void CmdChangeGameState(GameState.States state)
    {
        this.gameState.state = state;
    }

    [Command]
    public void CmdSetOriginID(string originId)
    {
        this.gameState.originId = originId;
    }

    [Command]
    public void CmdInstantiateInOriginSpace(Vector3 pos, Vector3 rot)
    {
        RpcInstantiateInOriginSpace(pos, rot);
    }

    [ClientRpc]
    public void RpcInstantiateInOriginSpace(Vector3 pos, Vector3 rot)
    {
        if(testObject != null)
            GameObject.Destroy(testObject);

        testObject = Instantiate(testGetRidOfThis, pos, Quaternion.identity); // I don't know how rotation works with poses exactly but its irrelevent for a capsule shaped hitbox. Also, sorry for the bad scratch code usually I plan more before developing.
        testObject.transform.SetParent(origin.transform, false);
    }
}
