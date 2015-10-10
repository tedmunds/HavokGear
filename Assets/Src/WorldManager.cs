using UnityEngine;
using System.Collections;


/// <summary>
/// Controlls world level logic, like enemy spawning and level generation / loading 
/// </summary>
public class WorldManager : MonoBehaviour {

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

    [SerializeField] // The weapont hat the player will be spawned with
    public GameObject defaultPlayerWeaponPrefab;

    [SerializeField] // The photon whip item
    public GameObject playerPhotonWhipPrefab;

    [SerializeField]
    public Transform spawnPoint;


    /// <summary>
    /// Cached reference to the player object
    /// </summary>
    [HideInInspector]
    public PlayerController playerCharacter;


	private void Start() {
        instance = this;
        SpawnPlayer();
    }
	

	private void Update() {
	    
	}



    /// <summary>
    /// Instantiates the player and camera prefab and initializes it with starting weapons, including photon whip
    /// </summary>
    public void SpawnPlayer() {
        if(playerPrefab == null) {
            Debug.LogError("WorldManager does not have a player prefab set! Cannot spawn player!");
            return;
        }

        // Spawn the player object
        Vector3 spawnLocation = transform.position;
        if(spawnPoint != null) {
            spawnLocation = spawnPoint.position;
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
        GameObject spawned = (GameObject)Instantiate(botPrefab, spawnLocation, Quaternion.identity);
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

        return botCharacter;
    }


    /// <summary>
    /// Spawn and init a weapon from tehinput prefab
    /// </summary>
    public GameObject SpawnWeapon(GameObject prefab) {
        GameObject spawnedWeapon = Instantiate(prefab);

        Weapon weaponComponent = spawnedWeapon.GetComponent<Weapon>();
        if(weaponComponent != null) {
            weaponComponent.OnSpawn();
        }

        return spawnedWeapon;
    }

}
