using UnityEngine;
using System.Collections;



public class BossInitiateZone : MonoBehaviour {

    [SerializeField]
    public AIController bossController;

    private bool hasBeenTriggered = false;

    public void OnTriggerEnter2D(Collider2D other) {
        if(!hasBeenTriggered && other.GetComponent<PlayerController>() != null) {
            bossController.AiStartSensing();
            hasBeenTriggered = true;
        }
    }
}
