using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxBehavior : MonoBehaviour
{
    public bool hasBeenHit = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Lazer")
            hasBeenHit = true;
    }
}
