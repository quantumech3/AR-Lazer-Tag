using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SwitchClientToScene : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(isClient && isLocalPlayer)
        {
            Debug.Log("Switching scene to ARScene");
            SceneManager.LoadScene("ARScene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
