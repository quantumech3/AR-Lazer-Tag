using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetToPlayerPosition : MonoBehaviour
{
    public GameObject player; // This is set by the player

    void Start()
    {
        
    }

    void Update()
    {
        if(player)
        {
            this.transform.position = player.transform.position;
            this.transform.rotation = player.transform.rotation;
        }
        else
        {
            Debug.LogError("Cannot set position of this game object to player position. SetToPlayerPosition does not have handle to player");
        }
    }
}
