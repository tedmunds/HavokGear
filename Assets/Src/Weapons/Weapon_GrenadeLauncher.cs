using UnityEngine;
using System.Collections;

public class Weapon_GrenadeLauncher : Weapon {

    [SerializeField]
    public GameObject projectilePrototype;

    [SerializeField]
    private AudioClip shootSound;


    protected override void Start() {
        base.Start();
    }


    protected override void Update() {
        base.Update();
    }


    protected override void Fire() {
        base.Fire();

        Vector3 fireDirection = GetAimDirection();

        PlaySound(shootSound);

        // Spawn and launch the projectile
        GameObject spawnedProjectile = WorldManager.instance.SpawnObject(projectilePrototype, GetFireOrigin());
        ProjectileController projController = spawnedProjectile.GetComponent<ProjectileController>();
        if(projController != null) {
            projController.LaunchProjectile(fireDirection, this);
        }
    }

}
