using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RoomAccess_TriggerArea : RoomAccess {


    public void OnTriggerEnter2D(Collider2D other) {
        // If the player enters trigger area, then activate 

        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController != null) {
            Activate(playerController);
        }
    }

}
