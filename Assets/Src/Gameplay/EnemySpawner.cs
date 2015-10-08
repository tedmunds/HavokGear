using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// For object that will spawn enemies when the player is in range
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemySpawner : MonoBehaviour {

    [SerializeField]
    public GameObject enemyPrototype;

    [SerializeField]
    public List<Weapon> enemyWeaponList;

    [SerializeField]
    private Vector2 spawnIntervalRange;

    [SerializeField]
    private int maxEnemeiesAtOnce = 3;

    // reference to the woprld maanger used for spawning enemies
    private WorldManager world;
    
    // Indicates whether this spawner should be spawning enemies over the interval
    private bool spawnerActive;

    // Spawn timing
    private float lastSpawnTime = 0.0f;
    private float nextSpawnInteral;

    // List of the enemies that are currently spawned and active
    private List<AIController> activeEnemies;

	private void Start() {
        world = GameObject.FindObjectOfType<WorldManager>();

        activeEnemies = new List<AIController>();

        if(enemyPrototype.GetComponent<AIController>() == null) {
            Debug.LogWarning(name+" is trying to spawn an entity without an AI component!");
        }
	}
	
	
	private void Update() {
        for(int i = activeEnemies.Count - 1; i >= 0; i--) {
            if(activeEnemies[i] == null) {
                activeEnemies.RemoveAt(i);
            }
        }


	    if(spawnerActive) {
            // Do a spawn if enough time has passed
            if(Time.time - lastSpawnTime > nextSpawnInteral && activeEnemies.Count < maxEnemeiesAtOnce) {
                nextSpawnInteral = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
                lastSpawnTime = Time.time;

                SpawnNewEnemy();
            }
        }
	}


    public void OnTriggerEnter2D(Collider2D other) {
        // player has entered trigger area
        if(other.GetComponent<PlayerController>() != null) {
            spawnerActive = true;

            //nextSpawnInteral = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
        }
    }

    public void OnTriggerExit2D(Collider2D other) {
        // player has exited trigger area
        if(other.GetComponent<PlayerController>() != null) {
            spawnerActive = false;
        }
    }



    /// <summary>
    /// Spawns an enemy at the spawn location
    /// </summary>
    private void SpawnNewEnemy() {
        Weapon weaponPrefab = null;

        if(enemyWeaponList.Count > 0) {
            int randWeaponIdx = Random.Range(0, enemyWeaponList.Count);
            weaponPrefab = enemyWeaponList[randWeaponIdx];
        }

        Vector3 spawnLocation = transform.position;
        spawnLocation.z = 0.0f;

        AIController spawned = world.SpawnMechBot(spawnLocation, enemyPrototype, true, weaponPrefab != null? weaponPrefab.gameObject : null);
        if(spawned != null) {
            activeEnemies.Add(spawned);
        }
    }
}
