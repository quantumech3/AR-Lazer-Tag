using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogGlobalCoordinates : MonoBehaviour
{
    public GameObject origin;
    public GameObject objectToBePlaced;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(placeObject());
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("[Object] Global position: " + this.transform.position.ToString());
        Debug.Log("[Object] Local position: " + origin.transform.InverseTransformPoint(this.transform.position).ToString());

        
    }

    private IEnumerator placeObject()
    {
        // Place object at the position of the sphere using local coordinates relative to the cube
        Vector3 localPos = origin.transform.InverseTransformPoint(this.transform.position);
        Vector3 localOrientation = origin.transform.InverseTransformDirection(this.transform.eulerAngles);

        GameObject placedObject = Instantiate(objectToBePlaced, localPos, Quaternion.Euler(localOrientation));
        placedObject.transform.SetParent(origin.transform, false);

        yield return new WaitForSeconds(1.0f);
        StartCoroutine(placeObject());
    }
}
