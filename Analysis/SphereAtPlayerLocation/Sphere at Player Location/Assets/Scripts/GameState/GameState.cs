using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour
{
    public enum States
    {
        PlaceOrigin,
        WaitingForOriginRegistration,
        OriginRegistered
    }

    [SyncVar(hook="onStateChanged")]
    public States state = States.PlaceOrigin;

    [SyncVar]
    public string originId;

    public void onStateChanged(GameState.States newState)
    {
        Debug.Log("Game state changed to " + newState.ToString());
        state = newState;
    }
}
