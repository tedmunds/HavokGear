using UnityEngine;
using System.Collections;


/// <summary>
/// Controlls world level logic, like enemy spawning and level generation / loading 
/// </summary>
public class WorldManager : MonoBehaviour {

    public delegate void TimedFunction();

    public static WorldManager instance;

    /// <summary>
    /// Damage scale for enemies that kind of dictates difficulty scale
    /// </summary>
    [SerializeField]
    public float enemyDamageScale = 1.0f;

    [SerializeField]
    public GameObject playerCameraPrefab;

    [SerializeField]
    public GameObject playerPrefab;

    //[SerializeField] // the player state stores the sdata session for the player
    //public GameObject playerStatePrefab;

    [SerializeField] // The weapont hat the player will be spawned with
    public GameObject defaultPlayerWeaponPrefab;

    [SerializeField] // The photon whip item
    public GameObject playerPhotonWhipPrefab;

    [SerializeField]
    public Transform initialSpawnPoint;

    [SerializeField]
    public bool friendlyFire;

    [SerializeField]
    public UI_EnemyHealthBar enemyHealthBarPrototype;

    [SerializeField]
    public GameObject pauseMenuObject;


    /// <summary>
    /// Cached reference to the player object
    /// </summary>
    [HideInInspector]
    public PlayerController playerCharacter;

    /// <summary>
    /// The players current state object: stores their session data
    /// </summary>
    [HideInInspector]
    public PlayerState playerState;

    /// <summary>
    /// The checkpoint that the player has set and will be spawned at if they die
    /// </summary>
    private Checkpoint activeCheckpoint;

    /// <summary>
    /// Object pool used for enemies and weapons to save instantiating them constantly as they tend to have lots of components and 
    /// get re-used very often
    /// </summary>
    private ObjectPool objectPool;

    private AudioSource globalAudioPlayer;

    private bool isPaused = false;


	private void Start() {
        instance = this;
        objectPool = new ObjectPool();

        SpawnInitialPlayer();

        playerState = FindObjectOfType<PlayerState>();
        globalAudioPlayer = GetComponent<AudioSource>();

        if(pauseMenuObject == null) {
            Debug.LogError("Please assign an object to WorldManager::Pause Menu Object");
        }
    }
	

	private void Update() {
	    
	}



    /// <summary>
    /// Instantiates the player and camera prefab and initializes it with starting weapons, including photon whip
    /// </summary>
    public void SpawnInitialPlayer() {
        if(playerPrefab == null) {
            Debug.LogError("WorldManager does not have a player prefab set! Cannot spawn player!");
            return;
        }

        // Spawn the player object
        Vector3 spawnLocation = transform.position;
        if(initialSpawnPoint != null) {
            spawnLocation = initialSpawnPoint.position;
        }
        else {
            Debug.LogWarning("WorldManager::SpawnPlayer() - No Spawn Point set, using default location!");
        }

        GameObject spawned = (GameObject)Instantiate(playerPrefab, spawnLocation, Quaternion.identity);
        playerCharacter = spawned.GetComponent<PlayerController>();
        if(playerCharacter == null) {
            Debug.LogWarning("WorldManager::SpawnPlayer() - Spawned player has no PlayerController component!");
        }

        // Call controller initialization, which progagates to the actor and grabs vital component references
        playerCharacter.OnSpawnInitialization();

        playerCharacter.MechComponent.RegisterDeathListener(OnPlayerDeath);

        // spawn the players camera
        Vector3 CamSpawnLoc = spawnLocation;
        CamSpawnLoc.z = -10.0f;
        GameObject camera = (GameObject)Instantiate(playerCameraPrefab, CamSpawnLoc, Quaternion.identity);
        CameraController cameraController = camera.GetComponent<CameraController>();
        if(cameraController == null) {
            Debug.LogWarning("WorldManager::SpawnPlayer() - Spawned camera has no CameraController component!");
        }

        // Tie the camera and palyer together
        cameraController.playerTarget = playerCharacter;
        playerCharacter.PlayerCamera = cameraController.GetComponent<Camera>();

        // Spawn at attach the players photn whip item
        if(playerPhotonWhipPrefab != null) {
            GameObject spawnedWhip = (GameObject)Instantiate(playerPhotonWhipPrefab);

            MechActor mechComponent = playerCharacter.GetComponent<MechActor>();
            if(mechComponent != null) {
                mechComponent.DoAttachment(MechActor.EAttachSide.Right, spawnedWhip, Vector3.zero);
            }
        }
        else {
            Debug.LogWarning("WorldManager::SpawnPlayer() - There was no photon whip prefab set, so player will not have one!");
        }

        // And now, entirely optional starting weapon
        if(defaultPlayerWeaponPrefab != null) {
            GameObject spawnedWeapon = SpawnWeapon(defaultPlayerWeaponPrefab);

            MechActor mechComponent = playerCharacter.GetComponent<MechActor>();
            if(mechComponent != null) {
                mechComponent.DoAttachment(MechActor.EAttachSide.Left, spawnedWeapon, Vector3.zero);
            }
        }
    }



    /// <summary>
    /// Spawn a mech bot Ai entity. Will spawn an instance of the input prefab and attach input weapon prefabs:
    /// startSensing = should the bot immediatly start sensing, or wait for some other instruction to do so
    /// </summary>
    public AIController SpawnMechBot(Vector3 spawnLocation, GameObject botPrefab, bool startSensing,
                             GameObject weaponPrefabLeft = null, GameObject weaponPrefabRight = null) {
        if(botPrefab == null) {
            return null;
        }

        // spawn the bot
        //GameObject spawned = (GameObject)Instantiate(botPrefab, spawnLocation, Quaternion.identity);
        GameObject spawned = objectPool.GetInactiveGameObjectInstance(botPrefab.gameObject);//, spawnLocation, Quaternion.identity);
        spawned.transform.position = spawnLocation;
        spawned.SetActive(true);

        AIController botCharacter = spawned.GetComponent<AIController>();
        if(botCharacter != null) {
            // Call controller initialization, which progagates to the actor and grabs vital component references
            botCharacter.OnSpawnInitialization();

            if(startSensing) {
                botCharacter.AiStartSensing();
            }
        }
        
        // Now spawn and attach either left / right or both weapons
        if(weaponPrefabLeft != null) {
            GameObject spawnedWeapon = SpawnWeapon(weaponPrefabLeft);

            MechActor mechComponent = botCharacter.GetComponent<MechActor>();
            if(mechComponent != null) {
                mechComponent.DoAttachment(MechActor.EAttachSide.Left, spawnedWeapon, Vector3.zero);
            }
        }

        if(weaponPrefabRight != null) {
            GameObject spawnedWeapon = SpawnWeapon(weaponPrefabRight);

            MechActor mechComponent = botCharacter.GetComponent<MechActor>();
            if(mechComponent != null) {
                mechComponent.DoAttachment(MechActor.EAttachSide.Right, spawnedWeapon, Vector3.zero);
            }
        }

        // add the health bar to it
        if(enemyHealthBarPrototype != null) {
            GameObject healthBarObject = objectPool.GetInactiveGameObjectInstance(enemyHealthBarPrototype.gameObject);
            UI_EnemyHealthBar enemyHpBar = healthBarObject.GetComponent<UI_EnemyHealthBar>();
            if(enemyHpBar != null) {
                healthBarObject.SetActive(true);
                enemyHpBar.AssignToTarget(botCharacter.GetComponent<MechActor>());
            }
        }

        return botCharacter;
    }


    /// <summary>
    /// Spawn and init a weapon from tehinput prefab
    /// </summary>
    public GameObject SpawnWeapon(GameObject prefab) {
        GameObject spawnedWeapon = objectPool.GetInactiveGameObjectInstance(prefab);
        spawnedWeapon.SetActive(true);

        Weapon weaponComponent = spawnedWeapon.GetComponent<Weapon>();
        if(weaponComponent != null) {
            weaponComponent.OnSpawn();
        }

        return spawnedWeapon;
    }


    /// <summary>
    /// Sets the checkpoint that the player will spawn at
    /// </summary>
    public void SetActiveCheckpoint(Checkpoint newActive) {
        if(activeCheckpoint != null) {
            activeCheckpoint.isCurrentCheckpoint = false;
        }
        
        activeCheckpoint = newActive;
        activeCheckpoint.isCurrentCheckpoint = true;
    }


    /// <summary>
    /// Called whent he player dies, respawns them at a checkpoint
    /// </summary>
    public void OnPlayerDeath(Actor died) {
        const float playerRespawnDelay = 1.0f;
        SetTimer(playerRespawnDelay, RespawnPlayer);
    }


    public void RespawnPlayer() {
        Vector3 spawnLocation;

        // decide what spawn location to use
        if(activeCheckpoint == null) {
            spawnLocation = initialSpawnPoint != null ? initialSpawnPoint.position : transform.position;
        }
        else {
            spawnLocation = activeCheckpoint.transform.position;
        }

        // reset the players state
        playerCharacter.transform.position = spawnLocation;
        playerCharacter.MechComponent.ResetState(true, false);

        playerCharacter.gameObject.SetActive(true);
    }


    /// <summary>
    /// Sets a function to be called after some delay
    /// </summary>
    public void SetTimer(float delay, TimedFunction function) {
        StartCoroutine(TimerRoutine(delay, function));
    }



    
    private IEnumerator TimerRoutine(float delay, TimedFunction function) {
        for(float t = 0.0f; t < delay; t += Time.deltaTime) {
            yield return null;
        }

        function();
    }


    /// <summary>
    /// Spawns an arbitrary game object using the object pool, good for commonly re-used objects like projectiles etc
    /// </summary>
    public GameObject SpawnObject(GameObject prototype, Vector3 position) {
        GameObject obj = objectPool.GetInactiveGameObjectInstance(prototype);
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }


    /// <summary>
    /// Checks if the enemy is visible on the players camera
    /// </summary>
    public bool ActorOnScreen(Actor actor) {
        if(actor == null) {
            return false;
        }

        Vector3 viewportLocation = playerCharacter.PlayerCamera.WorldToViewportPoint(actor.transform.position);

        // Use the normalized viewport coords to check if the actor is on screen
        if(viewportLocation.x > 1.0f || viewportLocation.x < 0.0f || 
           viewportLocation.y > 1.0f || viewportLocation.y < 0.0f) {
               return false;
        }

        return true;
    }


    /// <summary>
    /// Player the inout clip through the global player
    /// </summary>
    public void PlayGlobalSound(AudioClip clip) {
        if(globalAudioPlayer != null && clip != null) {
            globalAudioPlayer.PlayOneShot(clip);
        }
    }



    public void PauseGame() {
        Time.timeScale = 0.0f;
        isPaused = true;
        pauseMenuObject.SetActive(true);
    }



    public void UnpauseGame() {
        Time.timeScale = 1.0f;
        isPaused = false;
        pauseMenuObject.SetActive(false);
    }


    // If the scene changes while paused, make sure to reset the time scale
    void OnDisable() {
        Time.timeScale = 1.0f;
    }

}
