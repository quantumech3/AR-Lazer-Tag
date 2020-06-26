using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginBehavior : MonoBehaviour
{
    public ARCloudAnchor cloudAnchor;

    void Update()
    {
        // If cloudAnchor exists, then set the transform of this game object to the cloud anchors transform
        if(cloudAnchor != null && cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            this.transform.position = cloudAnchor.pose.position;
            this.transform.rotation = cloudAnchor.pose.rotation;
        }
    }
}
