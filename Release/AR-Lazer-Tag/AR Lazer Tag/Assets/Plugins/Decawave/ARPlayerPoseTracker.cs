using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Networking;

public class ARPlayerPoseTracker : NetworkBehaviour
{
    public static GameObject vioOrigin; // Set at Start or Update
    public string rfOriginName; // Used to find RF origin in scene when player loads into the game
    public GameObject rfOrigin; // Set at Start
    public GameObject rfGhostPrefab;
    public GameObject vioGhostPrefab;

    [SyncVar]
    public Vector3 rfPosition;

    [SyncVar]
    public Vector3 vioPosition;

    public GameObject rfGhost;
    public GameObject vioGhost;

    [Command]
    public void CmdSetVIOPosition(Vector3 vioPosition)
    {
        this.vioPosition = vioPosition;
    }

    [Command]
    public void CmdSetRFPosition(Vector3 rfPosition)
    {
        this.rfPosition = rfPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(isLocalPlayer)
            Decawave.LowLevel.Interface.Initialize(); 
        
        // Find RF Origin in scene
        rfOrigin = GameObject.Find(rfOriginName);
        
        if (isLocalPlayer && isClient)
        {
            // Instantiate RF Ghost
            rfGhost = Instantiate(rfGhostPrefab);
            rfGhost.GetComponent<RFGhostBehavior>().rfOrigin = rfOrigin;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isClient)
        {
            if (isLocalPlayer)
                LocalPlayerUpdate();

            ClientUpdate();
        }
        else
        {
            ServerUpdate();
        }
    }

    void LocalPlayerUpdate()
    {
        // Obtain RF transform
        Transform rfTransform = rfGhost.GetComponent<RFGhostBehavior>().GetTransform(); // Special method must be used to obtain coordinates

        // If vioOrigin exists then
        // Summon vioGhost if one doesnt already exist else sync vioPosition for this object
        if(vioOrigin)
        {
            if(vioGhost)
            {
                CmdSetVIOPosition(vioGhost.transform.position); // Sync VIO position
            }
            else
            {
                vioGhost = Instantiate(vioGhostPrefab, rfTransform); // Summon VIO Ghost at current RF transform
            }
        }

        // Sync RF position with all players
        CmdSetRFPosition(rfTransform.position);

        // Set rotation of this object specifically on clientside
        transform.rotation = Quaternion.identity * rfTransform.rotation;
    }

    void ClientUpdate()
    {
        // Set transform of this object to RF transform
        transform.position = rfPosition;

        // If vioOrigin and vioPosition exist then set this game object's position to fusion coordinates
        if(vioOrigin != null && vioPosition != null)
            transform.position = Decawave.HighLevel.RF.ToFusion(rfPosition, vioPosition);
    }

    void ServerUpdate()
    {

    }
}
