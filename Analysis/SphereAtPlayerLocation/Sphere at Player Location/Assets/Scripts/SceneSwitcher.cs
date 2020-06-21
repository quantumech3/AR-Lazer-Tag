using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
public class SceneSwitcher : NetworkBehaviour
{
    [ClientRpc]
    public void RpcSwitchToARScene()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void Start()
    {
        RpcSwitchToARScene();
    }
}
