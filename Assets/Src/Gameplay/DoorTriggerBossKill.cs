using UnityEngine;
using System.Collections;

public class DoorTriggerBossKill : MonoBehaviour {

    [SerializeField]
    private MechActor bossActor;

    [SerializeField]
    public RoomAccess_Door targetDoor;

	private void Start() {
        bossActor.RegisterDeathListener(OnBossDeath);
	}



    public void OnBossDeath(Actor died) {
        targetDoor.Activate(FindObjectOfType<PlayerController>());
    }

}
