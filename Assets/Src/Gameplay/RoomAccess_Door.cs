using UnityEngine;
using System.Collections;


public class RoomAccess_Door : RoomAccess {

    [SerializeField]
    GameObject doorCollisionObject;
    


    public void OnTriggerEnter2D(Collider2D other) {
        // If the player reaches a door, then activate 

        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController != null) {
            Activate(playerController);
        }
    }

    public override void Activate(MechController activator) {
        base.Activate(activator);

        PlayerController playerController = (PlayerController)activator;
        DoorOpen(playerController);
    }


    public void DoorOpen(PlayerController playerController) {
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
