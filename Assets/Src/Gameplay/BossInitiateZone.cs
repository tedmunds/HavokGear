using UnityEngine;
using System.Collections;



public class BossInitiateZone : MonoBehaviour {

    [SerializeField]
    public AIController bossController;

    public void OnTriggerEnter2D(Collider2D other) {
        if(other.GetComponent<PlayerController>() != null) {
            bossController.AiStartSensing();
        }
    }
}
