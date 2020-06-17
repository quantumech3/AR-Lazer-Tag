using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NManager_UIHandler : MonoBehaviour
{
    public GameObject inputField;
    public NetworkManager networkManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInputFieldChanged()
    {
        string newAddress = inputField.GetComponent<InputField>().text;
        Debug.Log("Input field was changed to: " + newAddress);
        networkManager.networkAddress = newAddress;
        networkManager.StartClient();
    }

    public void OnButtonPressed()
    {
        Debug.Log("Attempting to start server");
        networkManager.StartServer();
    }
}
