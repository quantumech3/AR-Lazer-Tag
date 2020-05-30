using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class KillEveryone : NetworkBehaviour
{
    [SyncVar(hook="onValueChange")]
    public bool everyoneIsDead = false;

    public GameObject networkManagerObject;

    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
        {
            everyoneIsDead = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(everyoneIsDead && isClient)
        {
            networkManagerObject.GetComponent<NetworkManager>().StopClient();
        }
    }

    void onValueChange(bool everyoneIsDead)
    {
        Debug.Log("The value was changed");
    }
}
