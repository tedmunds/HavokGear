using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider2D))]
public class DoorTriggerButton : MonoBehaviour {

    [SerializeField]
    public RoomAccess_Door targetDoor;

    private bool hasBeenPressed = false;



    public void OnTriggerEnter2D(Collider2D other) {
        if(hasBeenPressed) {
            return;
        }
        
        // If the player enters trigger area, then open the target door
        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController != null && targetDoor != null) {
            targetDoor.Activate(playerController);
            hasBeenPressed = true;

            gameObject.SetActive(false);
        }
    }
}
