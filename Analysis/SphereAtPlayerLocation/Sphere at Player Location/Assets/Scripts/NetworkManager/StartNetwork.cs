using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartNetwork : MonoBehaviour
{
    public NetworkManager networkManager;
    public InputField ipInputField;

    public void StartServer()
    {
        Debug.Log("Attempting to start server at " + networkManager.networkAddress + ":" + networkManager.networkPort);
        networkManager.StartServer();
    }

    public void StartClient()
    {
        networkManager.networkAddress = ipInputField.text;
        Debug.Log("Attempting to connect to server at " + networkManager.networkAddress + ":" + networkManager.networkPort);

        networkManager.StartClient();
    }
}
