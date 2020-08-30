using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;


public class ARNetworkManager : NetworkManager
{
    public GameObject playerInfoPrefab;
    public GameObject hostGuiPrefab;

    [HideInInspector]
    public InputField addressInput;

    [HideInInspector]
    public InputField portInput;

    [HideInInspector]
    public InputField heightInput;

    [HideInInspector]
    public GameObject playerInfo;


    /// <summary>
    /// Searches for the following components and defines references to them if they exist: [addressInput, heightInput, portInput]
    /// </summary>
    public void GetHandlesToOfflineGui()
    {
        try
        {
            this.addressInput = GameObject.Find("Address Input Box").GetComponent<InputField>();
            this.portInput = GameObject.Find("Port Input Box").GetComponent<InputField>();
            this.heightInput = GameObject.Find("Height Input Box").GetComponent<InputField>();
        }
        catch(Exception e)
        {

        }
    }

    public void ServerStart()
    {
        GetHandlesToOfflineGui();

        // Set the network address to port to the inputed text in the Offline Gui
        this.networkAddress = addressInput.text;

        // Attempt to set the network port to the input from the Offline Gui and throw an error if the input is non-numeric
        try
        {
            this.networkPort = int.Parse(portInput.text);
        }
        catch(Exception e)
        {
            Debug.LogError("Encountered exception while parsing port input: " + e.Message);
            return;
        }

        // Start the server
        this.StartServer();
    }

    public void ClientStart()
    {
        GetHandlesToOfflineGui();

        // Set the network address to port to the inputed text in the Offline Gui
        this.networkAddress = addressInput.text;

        // Attempt to set the network port to the input from the Offline Gui and throw an error if the input is non-numeric
        try
        {
            this.networkPort = int.Parse(portInput.text);
        }
        catch (Exception e)
        {
            Debug.LogError("Encountered exception while parsing port input: " + e.Message);
            return;
        }

        // Attempt to parse the height input
        double height;
        try
        {
            height = double.Parse(heightInput.text);
        }
        catch(Exception e)
        {
            Debug.LogError("Encountered exception while parsing height input: " + e.Message);
            return;
        }

        // Destroy this.playerInfo if it is not null
        if (this.playerInfo != null)
            Destroy(this.playerInfo);

        // Instantiate "Player Info"
        this.playerInfo = Instantiate(this.playerInfoPrefab);
        this.playerInfo.name = "Player Info";

        // Set the "height" attribute of the PlayerInfo component of playerInfo to the value of heightInput
        this.playerInfo.GetComponent<PlayerInfo>().height = height;

        DontDestroyOnLoad(playerInfo);

        this.StartClient();
    }
}
