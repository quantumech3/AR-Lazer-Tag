using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiTransitionHandler : MonoBehaviour
{
    public GameObject player;
    public GameObject nextGui;
    public GameState transitionState = GameState.Pregame;

    void Update()
    {
        PlayerBehavior playerBehavior = player.GetComponent<PlayerBehavior>();

        if (playerBehavior.state == transitionState)
        {
            GameObject gui = Instantiate(nextGui);
            gui.GetComponent<GuiTransitionHandler>().player = this.player;
            Destroy(this.gameObject);
        }
    }
}
