/**
I know this code is garbage but it doesn't matter. This program will not work with hosts, only remote clients and dedicated servers
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KillEveryoneOnLeftClick : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && isClient)
        {
            Debug.Log("Click registered");
            // If user left clicks then kill all other players if the player is not already dead (GUI text is red if the player is dead already)
            if(GameObject.FindGameObjectsWithTag("UI")[0].GetComponentInChildren<Text>().text != "You died. Press left click to respawn")
            {
                CmdSpawnDeathGUI();
            }
            else
            {
                Camera.main.backgroundColor = new Color(1, 1, 1);
                Text text = GameObject.FindGameObjectsWithTag("UI")[0].GetComponentInChildren<Text>();
                text.text = "You are alive. Left click anywhere to kill everyone";
                text.color = new Color(0f, 0.5849056f, 0.06020758f);
            }
        }
    }

    [Command]
    void CmdSpawnDeathGUI()
    {
        // Tell all clients that are not the killer to change to the death GUI
        RpcSpawnDeathGUI();
    }

    [ClientRpc]
    void RpcSpawnDeathGUI()
    {
        // Change the environment to look like the death GUI if the client is not the killer
        if(!isLocalPlayer)
        {
            Camera.main.backgroundColor = new Color(0f, 0f, 0f);
            Text text = GameObject.FindGameObjectsWithTag("UI")[0].GetComponentInChildren<Text>();
            text.text = "You died. Press left click to respawn";
            text.color = new Color(1, 0, 0);
        }
    }
}
