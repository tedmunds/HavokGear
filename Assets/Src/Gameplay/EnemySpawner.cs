using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// For object that will spawn enemies when the player is in range
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemySpawner : MonoBehaviour {

    //[SerializeField]
    //public GameObject enemyPrototype;

    [SerializeField]
    public GameObject[] enemyPrototypes;

    [SerializeField]
    public List<Weapon> enemyWeaponList;

    [SerializeField]
    private Vector2 spawnIntervalRange;

    [SerializeField]
    private int maxEnemeiesAtOnce = 3;

    [SerializeField]
    private int totalEnemiesToSpawn = 5;

    [SerializeField] // A targhet object to open when this spawner is cleared
    private RoomAccess openTargetOnClear;
    
    // Spawn point can have a list of patrol points associated with it
    [SerializeField]
    public List<Transform> patrolPoints;

    // reference to the woprld maanger used for spawning enemies
    private WorldManager world;
    
    // Indicates whether this spawner should be spawning enemies over the interval
    private bool spawnerActive;

    // Spawn timing
    private float lastSpawnTime = 0.0f;
    private float nextSpawnInteral;

    private int numSpawned = 0;

    private bool hasBeenCleared;

    // List of the enemies that are currently spawned and active
    private List<AIController> activeEnemies;

	private void Start() {
        world = FindObjectOfType<WorldManager>();

        activeEnemies = new List<AIController>();

        //if(enemyPrototype.GetComponent<AIController>() == null) {
        //    Debug.LogWarning(name+" is trying to spawn an entity without an AI component!");
        //}

        numSpawned = 0;
        hasBeenCleared = false;
    }
	
	
	private void Update() {
        for(int i = activeEnemies.Count - 1; i >= 0; i--) {
            if(activeEnemies[i] == null || !activeEnemies[i].gameObject.activeSelf) {
                activeEnemies.RemoveAt(i);
            }
        }


	    if(spawnerActive) {
            // Do a spawn if enough time has passed and there isnt too many enemies out already, and it hasnt reached the spawn limit
            if(Time.time - lastSpawnTime > nextSpawnInteral && 
                activeEnemies.Count < maxEnemeiesAtOnce && 
                numSpawned < totalEnemiesToSpawn) {

                nextSpawnInteral = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
                lastSpawnTime = Time.time;

                SpawnNewEnemy();
            }
        }

        // check if it has been cleared: it has spawned its max enemies, and they are all dead
        if(!hasBeenCleared && numSpawned >= totalEnemiesToSpawn && activeEnemies.Count == 0) {
            hasBeenCleared = true;
            if(openTargetOnClear != null) {
                openTargetOnClear.Activate(WorldManager.instance.playerCharacter);
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

        // Dicide what enemy to spawn
        int randEnemyIdx = Random.Range(0, enemyPrototypes.Length);
        GameObject prototype = enemyPrototypes[randEnemyIdx];

        AIController spawned = world.SpawnMechBot(spawnLocation, prototype, true, weaponPrefab != null ? weaponPrefab.gameObject : null);
        if(spawned != null) {
            activeEnemies.Add(spawned);
            spawned.SpawnedFromPoint(this);
        }

        numSpawned += 1;
    }
}
