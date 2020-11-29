using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SerialTerminal : MonoBehaviour
{
    AndroidJavaObject serialTerminal;
    bool connectionStatus = false;
    string buffer = "";
    string currentValue = ""; 
    void Start()
    {
#if UNITY_ANDROID 
        //Get Unity context
        var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        var unityContext = unityActivity.Call<AndroidJavaObject>("getApplicationContext");
        //Initalize serial terminal
        serialTerminal = new AndroidJavaObject("com.example.unityserialplugin.SerialTerm", new object[] { unityContext, gameObject.transform.name, "1366", "0105" });
#endif
    }

    void Update()
    {
#if UNITY_ANDROID
        if (connectionStatus) 
        {
            gameObject.GetComponent<Text>().text = currentValue;
        }
#endif
    }
    void OnDeviceFailure(string s)
    {
        Debug.Log("Device not found");
    }
    void OnDeviceSuccess(string s)
    {
        Debug.Log("Device found");
    }
    void OnPermissionFailure(string s)
    {
        Debug.Log("Permission not granted");
    }
    void OnPermissionSuccess(string s)
    {
        Debug.Log("Permission granted");
    }
    void OnInitException(string s)
    {

    }
    void OnRunException(string s)
    {

    }
    void OnDataException(string s)
    {

    }
    void OnConnection(string s)
    {
        Debug.Log("Connected");
        connectionStatus = true;
    }
    void OnData(string data)
    {
        buffer += data;
        if(buffer.Length > 0)
        {
            //Check if there is any new complete data in the buffer
            int end = buffer.LastIndexOf("\n");
            if(end > 0) {
                int start = buffer.LastIndexOf("\n", end - 1);
                if (start >= 0)
                {
                    string latestOutput = buffer.Substring(start + 1, end - start - 1);

                    //Remove everything from the buffer before the latestOutput. Incomplete outputs later on in the buffer will be kept.
                    buffer = buffer.Substring(end);

                    //Pass the data into the parser, might want to make this async
                    currentValue = latestOutput;
                }
            }

        }  
    }
    void Write(string s)
    {
        serialTerminal.Call("write", s);
    }
    void OnWriteException(string s)
    {
        Debug.Log("Write Exception");
    }
}
