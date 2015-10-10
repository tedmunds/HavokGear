﻿using UnityEngine;
using System.Collections;


public class RoomAccess_Door : RoomAccess {

    [SerializeField]
    GameObject doorCollisionObject;
    


    public void OnTriggerEnter2D(Collider2D other) {
        // If the player reaches a door, then activate 

        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController != null) {
            DoorOpen();

            Activate(playerController);
        }
    }


    private void DoorOpen() {
        // TODO: door animation
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if(renderer != null) {
            renderer.enabled = false;
        }

        // Allow the player to walk through door by clearing its collision
        if(doorCollisionObject != null) {
            doorCollisionObject.SetActive(false);
        }
    }

}
