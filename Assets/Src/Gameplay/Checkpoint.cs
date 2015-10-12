using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour {

    [HideInInspector]
    public bool isCurrentCheckpoint;

    private WorldManager world;

	
	private void Start() {
        world = FindObjectOfType<WorldManager>();
        isCurrentCheckpoint = false;
    }


    public void OnTriggerEnter2D(Collider2D other) {

        // If player interacts with the checkpoint, set it to be the most recently activated one
        PlayerController player = other.GetComponent<PlayerController>();
        if(player != null && !isCurrentCheckpoint) {
            world.SetActiveCheckpoint(this);

            // TODO: checkpoint animations
            GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
	
	
}
