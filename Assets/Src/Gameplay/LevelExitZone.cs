using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class LevelExitZone : MonoBehaviour {

    [SerializeField]
    public string targetScene;

    public void OnTriggerEnter2D(Collider2D other) {
        // If the player enters trigger area, then transition to next scene
        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController == null) {
            playerController = other.GetComponentInChildren<PlayerController>();
            if(playerController == null) {
                playerController = other.GetComponentInParent<PlayerController>();
            }
        }

        if(playerController != null) {
            PlayerState playerState = FindObjectOfType<PlayerState>();
            if(playerState != null) {
                Debug.Log("Begin Level Exit! Trying to save player state...");
                playerState.SaveState();
            }

            Debug.Log("Travelling to Level " + targetScene);
            Application.LoadLevel(targetScene);
        }
    }
}
