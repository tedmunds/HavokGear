using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour {


    [SerializeField]
    public PickupableItem itemToSpawn;

    [SerializeField]
    public bool spawnOnStart = true;


    private PickupableItem spawnedItem;


	private void Start() {
        if(spawnOnStart) {
            SpawnItem();
        }
	}
	

    public void SpawnItem() {
        if(spawnedItem == null) {
            spawnedItem = (PickupableItem)Instantiate(itemToSpawn, transform.position, Quaternion.identity);
        }

        if(!spawnedItem.gameObject.activeSelf) {
            spawnedItem.gameObject.SetActive(true);
            spawnedItem.Reset();
        }
    }
}
