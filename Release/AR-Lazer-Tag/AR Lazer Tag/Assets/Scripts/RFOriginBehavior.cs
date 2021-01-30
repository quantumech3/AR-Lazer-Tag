using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RFOriginBehavior : MonoBehaviour
{
    public Slider xRot, yRot, zRot;
    public string xRotSliderName, yRotSliderName, zRotSliderName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If all sliders exist, set rotation of this object to <xrot, yrot, zrot> in euler angles
        if (xRot && yRot && zRot)
        {
            this.transform.rotation = Quaternion.Euler(new Vector3(xRot.value, yRot.value, zRot.value));
        }
        else
        {
            // Else attempt to find sliders in level (Only happens for clients in Alive State)
            GameObject xSlider, ySlider, zSlider;
            xSlider = GameObject.Find(xRotSliderName);
            ySlider = GameObject.Find(yRotSliderName);
            zSlider = GameObject.Find(zRotSliderName);

            if (xSlider && ySlider && zSlider)
            {
                xRot = xSlider.GetComponent<Slider>();
                yRot = ySlider.GetComponent<Slider>();
                zRot = zSlider.GetComponent<Slider>();
            }
        }
    }
}
