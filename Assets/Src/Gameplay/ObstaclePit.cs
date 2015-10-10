using UnityEngine;
using System.Collections;

public class ObstaclePit : MonoBehaviour {


    private Collider2D[] colliders;

	
	private void Start() {
        colliders = GetComponents<Collider2D>();
    }
	
	
	private void Update() {
	
	}


    public void OnTriggerStay2D(Collider2D other) {
        MechActor mech = other.gameObject.GetComponent<MechActor>();
        if(mech != null) {
            // Kill it
            PlayerController player = mech.GetComponent<PlayerController>();
            if(player != null && player.IsBoosting) {
                return;
            }
            
            // Only kill the mech if it is actually totally inside the pit
            for(int i = 0; i < colliders.Length; i++) {
                if(colliders[i].OverlapPoint(mech.transform.position)) {
                    mech.FalltoDeath(gameObject);
                    return;
                }
            }

        }
    }


}
