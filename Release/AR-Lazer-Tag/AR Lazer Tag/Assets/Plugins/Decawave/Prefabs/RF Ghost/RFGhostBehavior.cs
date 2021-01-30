using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Decawave.HighLevel;
using Decawave.Common;

public class RFGhostBehavior : MonoBehaviour
{
    public static Dictionary<int, Vector3> ANCHORS = new Dictionary<int, Vector3>();

    public int[] anchorIds = new int[] { 0xA92, 0x8B08, 0xC70C, 0xCC93 };
    public GameObject rfOrigin; 

    public Vector3[] anchorPositions = new Vector3[] // Paralell array containing positions of each anchor with an ID above
    {
        new Vector3(3f, 1.5f, 6f),
        new Vector3(3f, 1.5f, 0),
        new Vector3(0, 1.5f, 6f),
        new Vector3(0, 0, 0)
    };

    /// <summary>
    /// Returns transform of RF ghost taking into account initial rotation offset
    /// </summary>
    /// <param name="initRotation">Initial theta0 of player</param>
    public Transform GetTransform()
    {
        Transform thisTransform = (new GameObject()).transform;
        thisTransform.position = transform.position;
        thisTransform.rotation = thisTransform.rotation * rfOrigin.transform.rotation; // Quaternion.Euler(rfOrigin.transform.rotation.eulerAngles.x + this.transform.rotation.eulerAngles.x, initRotation.y + transform.rotation.eulerAngles.y, initRotation.z + transform.rotation.eulerAngles.z);

        return thisTransform;
    }

    void Start()
    {
        // Construct dictionary associating anchor IDs with their corrosponding positions
        ANCHORS.Add(anchorIds[0], anchorPositions[0]);
        ANCHORS.Add(anchorIds[1], anchorPositions[1]);
        ANCHORS.Add(anchorIds[2], anchorPositions[2]);
        ANCHORS.Add(anchorIds[3], anchorPositions[3]);
    }

    void Update()
    {
        // Set this game object's local position to player's position in RF space
        AnchorData[] distances = Decawave.LowLevel.Interface.anchors;
        if(distances.Length == 4)
            this.transform.localPosition = RF.Trilaterate(ANCHORS, distances);
    }
}
