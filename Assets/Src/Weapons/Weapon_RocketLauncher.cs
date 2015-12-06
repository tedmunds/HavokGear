using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon_RocketLauncher : Weapon {

    [SerializeField]
    public int maxShotsInSalvo;

    [SerializeField]
    public float salvoFireDelay;

    [SerializeField]
    public GameObject rocketPrototype;

    [SerializeField]
    private AudioClip shootSound;

    [SerializeField]
    private GameObject ui_TargetLock;

    private int numShotsThisSalvo;
    private bool isLockingOn;

    private List<MechController> lockedTargets;
    private List<GameObject> lockSymbols;


    protected override void Start() {
        base.Start();
        lockedTargets = new List<MechController>(maxShotsInSalvo);

        // Create a bunch of UI elements and set their parent to tha canvas so they can render
        Canvas canvas = FindObjectOfType<Canvas>();

        lockSymbols = new List<GameObject>(maxShotsInSalvo);
        for(int i = 0; i < maxShotsInSalvo; i++) {
            lockSymbols.Add(Instantiate(ui_TargetLock));

            lockSymbols[i].transform.SetParent(canvas.transform);
            lockSymbols[i].SetActive(false);
        }
	}



    protected override void Update() {
        base.Update();

        // Check if the actor is locking onto to an enemy,
        if(isLockingOn && numShotsThisSalvo < maxShotsInSalvo) {

            // check that there is enough ammo to do this many lock ons
            if(numShotsThisSalvo * ammoPerShot < currentAmmo) {
                Vector3 aimLocation = owner.GetAimLocation();
                Collider2D[] overlaps = Physics2D.OverlapPointAll(aimLocation, detectLayers);

                // check is overlapping a mech
                foreach(Collider2D coll in overlaps) {
                    MechController mech = coll.gameObject.GetComponent<MechController>();

                    // check this isnt the owner, and it isnt already locked
                    if(mech != null && coll.gameObject != owner.gameObject &&
                        !lockedTargets.Contains(mech)) {

                        lockedTargets.Add(mech);
                        
                        // if the player is weilding this, then activate another target lock
                        if(owner.GetType() == typeof(PlayerController)) {
                            lockSymbols[numShotsThisSalvo].SetActive(true);
                        }

                        numShotsThisSalvo += 1;
                    }
                }
            }
        }

        // update the UI symbols for the player
        if(owner.GetType() == typeof(PlayerController)) {
            for(int i = 0; i < numShotsThisSalvo; i++) {                
                Vector3 targPos = lockedTargets[i].transform.position;
                Vector3 screenPos = ((PlayerController)owner).PlayerCamera.WorldToScreenPoint(targPos);

                lockSymbols[i].transform.position = screenPos;

                // if its gone inactive, remove it from locks
                if(!lockedTargets[i].gameObject.activeSelf) {
                    lockSymbols[numShotsThisSalvo - 1].SetActive(false);
                    lockedTargets.RemoveAt(i);
                    numShotsThisSalvo -= 1;
                    break;
                }
            }
        }
	}


    protected override void Fire() {
        // dont actually fire here, and dont consumea ammo or anythig
    }

    public override bool BeginFire() {
        if(!CanRefire()) {
            PlaySound(outOfAmmoSound);

            return false;
        }

        isLockingOn = true;
        return true;
    }


    public override void EndFire() {
        base.EndFire();

        if(isLockingOn) {
            // launch the salvo of rockets that were locked on during firing phase
            StartCoroutine(FireSalvo());
            lastFireTime = Time.time;
        }

        isLockingOn = false;
    }


    private void ShootMissile(int missleIdx) {
        Vector3 fireDirection = GetAimDirection();

        if(consumesAmmo) {
            currentAmmo -= ammoPerShot;
        }
        
        GameObject spawnedProjectile = WorldManager.instance.SpawnObject(rocketPrototype, GetFireOrigin());
        Proj_SeekerMissle projController = spawnedProjectile.GetComponent<Proj_SeekerMissle>();
        if(projController != null) {
            projController.LaunchProjectile(fireDirection, this);

            projController.targetMech = lockedTargets[missleIdx];

            PlaySound(shootSound, 0.5f, Random.Range(0.6f, 1.0f));
        }

    }


    private IEnumerator FireSalvo() {
        for(int i = 0; i < numShotsThisSalvo; i++) {
            ShootMissile(i);
            yield return new WaitForSeconds(salvoFireDelay);
        }

        SalvoCompleted();
    }


    private void SalvoCompleted() {
        // reset locking state
        if(lockedTargets != null) {
            lockedTargets.Clear();
        }
        
        numShotsThisSalvo = 0;

        if(lockSymbols != null) {
            foreach(GameObject symbol in lockSymbols) {
                if(symbol != null && symbol.activeSelf) {
                    symbol.SetActive(false);
                }
            }
        }
    }



    protected override void OnDisable() {
        base.OnDisable();
        
        SalvoCompleted();
    }


}
