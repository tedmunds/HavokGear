using UnityEngine;
using System.Collections;

public class Weapon_Shotgun : Weapon {

    [SerializeField]
    public float spreadArc;

    [SerializeField]
    public int bulletsInSpread;

    [SerializeField]
    public float chargeUpTime;

    [SerializeField]
    public float chargedSpreadArc;

    [SerializeField]
    public ProjectileController bulletPrefab;

    [SerializeField]
    public ParticleSystem shootEffectPrototype;

    [SerializeField]
    private AudioClip chargeUpSound;

    [SerializeField]
    private AudioClip fullyChargedSound;

    [SerializeField]
    private AudioClip fireSound;

    private ParticleSystem shootEffect;

    private bool beginCharging;



    protected override void Start() {
        base.Start();

        if(shootEffectPrototype != null && shootEffect == null) {
            shootEffect = Instantiate(shootEffectPrototype);
            shootEffect.gameObject.SetActive(false);
            shootEffect.transform.parent = transform;
        }
	}





    public override bool BeginFire() {
        bool beganFire = base.BeginFire();

        beginCharging = beganFire;

        if(beginCharging) {
            if(audioPlayer != null && chargeUpSound != null) {
                audioPlayer.clip = chargeUpSound;
                audioPlayer.Play();
            }
        }
        
        return beganFire;
    }


    /// <summary>
    /// Call to end weapon fire 
    /// </summary>
    public override void EndFire() {
        base.EndFire();
        
        if(!beginCharging) {
            return;
        }

        Vector3 fireDirection = GetAimDirection();

        // constrain the charged up time
        float chargedTime = Time.time - lastFireTime;
        if(chargedTime > chargeUpTime) {
            chargedTime = chargeUpTime;
        }

        float chargeRatio = chargedTime / chargeUpTime;

        float maxArc = Mathf.Lerp(chargedSpreadArc, spreadArc, 1.0f - chargeRatio);

        // Spawn the bullets out in a general arc
        for(int i = 0; i < bulletsInSpread; i++) {
            Vector3 randDirection = Random.insideUnitCircle;

            Vector3 bulletDirection = Vector3.Slerp(fireDirection, randDirection, maxArc);

            GameObject spawnedBullet = WorldManager.instance.SpawnObject(bulletPrefab.gameObject, firePoint.position);
            ProjectileController projController = spawnedBullet.GetComponent<ProjectileController>();
            if(projController != null) {
                projController.LaunchProjectile(bulletDirection, this);
            }
        }

        if(shootEffect != null) {
            shootEffect.transform.position = firePoint.position;
            shootEffect.transform.up = fireDirection;
            shootEffect.gameObject.SetActive(true);
            shootEffect.Play();
        }

        // clear looping sound
        if(audioPlayer != null) {
            audioPlayer.Stop();
            audioPlayer.clip = null;
        }

        // Then play the shot sound
        PlaySound(fireSound, 1.0f, Random.Range(0.85f, 1.0f));

        DoCameraRecoil(0.3f);

        beginCharging = false;
    }



    protected override void Update() {
        base.Update();

        // its chargin, update charge effects?
        if(beginCharging && audioPlayer != null) {
            float chargeTime = Time.time - lastFireTime;
            if(chargeTime > chargeUpTime && fullyChargedSound != null) {
                // Fully charged, change sound to the fully charged sound
                if(audioPlayer.clip != fullyChargedSound) {
                    audioPlayer.clip = fullyChargedSound;
                    audioPlayer.Play();
                }
            }
        }
    }


    protected override void Fire() {
        base.Fire();

    }
}
