using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;

public class Player_PlaceCones : NetworkBehaviour
{
    public ARAnchorManager anchorManager;
    public ARRaycastManager raycastManager;
    public GameObject sessionOrigin;
    public GameObject cube; // This variable is set in the inspector

    public bool verboseLog;

    public SyncListString allCloudIds; // Cloud anchor IDs registered by all players

    public List<string> spawnedCloudIds; // Cloud anchor IDs that have been registered by this client

    public List<ARCloudAnchor> unregisteredCloudAnchors = new List<ARCloudAnchor>();

    void Start()
    {
        // Get references to nessasary components and game objects
        anchorManager = GameObject.Find("AR Session Origin").GetComponent<ARAnchorManager>();
        raycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();
        sessionOrigin = GameObject.Find("AR Session Origin");
    }


    void Update()
    {
        if(isServer)
            ServerUpdate();
        else
            ClientUpdate();
    }

    /**
    * I know this is deadcode, but im keeping it anyway :P
    **/
    void ServerUpdate()
    {
        /*if(verboseLog)
        {
            foreach(string i in allCloudIds)
                Debug.Log("[allCloudIds] " + Time.time.ToString() + i);
        }*/
    }

    void ClientUpdate()
    {
        /*if(verboseLog)
        {
            foreach(string i in allCloudIds)
                Debug.Log("[allCloudIds] " + i);
        }*/

        // If the player touches the screen
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Let touchPos = the position that the player touches in world space
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            raycastManager.Raycast(Input.GetTouch(0).position, hitResults);
            Pose touchPose = hitResults[0].pose;

            instantiateCloudAnchorAt(touchPose);
        }

        // Register all successfully initialized cloud anchors with the server
        for(int i = 0; i < unregisteredCloudAnchors.Count; i++)
        {
            // If the anchor is registered
            if(unregisteredCloudAnchors[i].cloudAnchorState == CloudAnchorState.Success)
            {
                // Add the anchor's ID to allCloudIds
                CmdPushToAllCloudIds(unregisteredCloudAnchors[i].cloudAnchorId);

                // Remove this anchor from unregisteredCloudAnchors[] because it is now a registered anchor
                unregisteredCloudAnchors.RemoveAt(i);
            }
            else if(unregisteredCloudAnchors[i].cloudAnchorState != CloudAnchorState.TaskInProgress) // If the cloud anchor had a problem being registered, throw an error
            {
                // Throw error
                Debug.LogError("Removing a cloud anchor. Could not register cloud anchor with error: " + unregisteredCloudAnchors[i].cloudAnchorState);

                // Remove this anchor from unregisteredCloudAnchors[] because it cannot become a registered anchor
                unregisteredCloudAnchors.RemoveAt(i);
            }
        }

        // Spawn a cube at the location of any new anchors that have not been spawned in yet
        for(int i = spawnedCloudIds.Count; i < allCloudIds.Count; i++)
        {
            if(verboseLog)
            {
                Debug.Log("Attempted to spawn cube at the cloud anchor id: " + allCloudIds[i]);
            }
            
            spawnCubeAtAnchorWithId(allCloudIds[i]);

            // Push the current cloud ID to spawnedCloudIds so it doesnt get spawned in the next frame
            spawnedCloudIds.Add(allCloudIds[i]);
        }
    }

    void instantiateCloudAnchorAt(Pose pose)
    {
        // Let localAnchor = new anchor placed at "pose"
        ARAnchor localAnchor = anchorManager.AddAnchor(pose);

        // Log position of new local anchor
        if(verboseLog)
        {
            Debug.Log("Placed local anchor at: " + pose.position.ToString());
        }

        // Instantiate a new cloud anchor based off of localAnchor and push to unregisteredCloudAnchors
        unregisteredCloudAnchors.Add(anchorManager.HostCloudAnchor(localAnchor));
    }

    [Command]
    void CmdPushToAllCloudIds(string id)
    {
        allCloudIds.Add(id);
    }

    async void spawnCubeAtAnchorWithId(string id)
    {
        await Task.Run(() =>
        {   
            ARCloudAnchor anchor = anchorManager.ResolveCloudAnchorId(id);

            while(anchor.cloudAnchorState == CloudAnchorState.TaskInProgress){} //Pause until the client gets a response from the cloud anchor service

            if(anchor.cloudAnchorState == CloudAnchorState.Success)
            {
                // Spawn a cube at the location of the cloud anchor
                GameObject newCube = Instantiate(cube, Vector3.zero, Quaternion.identity);
                newCube.transform.SetParent(anchor.transform, false);

                Debug.Log("Successfully instantiated cube at cloud anchor with id: " + id);
            }
            else
            {
                Debug.LogError("Failed to resolve cloud anchor with error: " + anchor.cloudAnchorState);
            }
        });
    }
}
