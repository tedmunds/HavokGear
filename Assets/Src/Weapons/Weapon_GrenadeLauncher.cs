using UnityEngine;
using System.Collections;

public class Weapon_GrenadeLauncher : Weapon {

    [SerializeField]
    public GameObject projectilePrototype;
    

    protected override void Start() {
        base.Start();
    }


    protected override void Update() {
        base.Update();
    }


    protected override void Fire() {
        base.Fire();

        Vector3 fireDirection = GetAimDirection();

        // Spawn and launch the projectile
        GameObject spawnedProjectile = WorldManager.instance.SpawnObject(projectilePrototype, firePoint.position);// (GameObject)Instantiate(projectilePrototype, firePoint.position, Quaternion.identity);
        ProjectileController projController = spawnedProjectile.GetComponent<ProjectileController>();
        if(projController != null) {
            projController.LaunchProjectile(fireDirection, this);
        }
    }

}
