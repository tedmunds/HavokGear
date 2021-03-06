﻿using UnityEngine;
using System.Collections;

public class Weapon_Laser : Weapon {

    [SerializeField]
    public float damagePerSecond;

    [SerializeField] // how many segments are in the line renderer for the laser
    public int bounces = 1;

    [SerializeField]
    public Vector2 laserWidthRange;

    [SerializeField]
    public float chargeUpTime;

    [SerializeField] // player can take a different ammoutn of time to charge
    public float playerChargeTimeMultiplier;

    [SerializeField]
    public ParticleSystem endpointEffectPrototype;

    [SerializeField]
    public AudioClip loopingLaserSound;

    private float currentEnergy;

    private LineRenderer laserRenderer;

    private int numLaserVerts;

    private ParticleSystem endpointEffect;

    private float startedFireTime;

	protected override void Start() {
        base.Start();

        laserRenderer = transform.GetComponentInChildren<LineRenderer>();
        if(laserRenderer == null) {
            Debug.LogWarning("Weapon_Laser didn't find a line renderer in the children objects... Maybe the structure of the weapon has changed?");
        }

        laserRenderer.enabled = false;
        numLaserVerts = (bounces * 2) + 2;
        laserRenderer.SetVertexCount(numLaserVerts);

        if(endpointEffectPrototype != null) {
            endpointEffect = Instantiate(endpointEffectPrototype);
            endpointEffect.gameObject.SetActive(false);
        }
    }


    protected override void Update() {
        base.Update();

    }


    /// <summary>
    /// Overrided to only look at the energy left
    /// </summary>
    protected override bool CanRefire() {
        if(currentEnergy > 0.0f) {
            return true;
        }

        EndFire();
        PlaySound(outOfAmmoSound);
        return false;
    }


    protected override void Fire() {
        // Do not call base fire b/c ammo consumtion is different!
        lastFireTime = Time.time;

        currentEnergy -= ammoPerShot * Time.deltaTime;
        if(currentEnergy <= 0.0f) {
            currentEnergy = 0.0f;
        }

        currentAmmo = (int)Mathf.Ceil(currentEnergy);

        bool isFullyCharged = false;

        // figure how much time it has been chargin for, and how long it needs to go
        float elapsedFire = Time.time - startedFireTime;
        float chargetimeMult = owner.GetType() == typeof(PlayerController) ? playerChargeTimeMultiplier : 1.0f;
        float chargePct = elapsedFire / (chargeUpTime * chargetimeMult);

        if(elapsedFire > (chargeUpTime * chargetimeMult)) {
            isFullyCharged = true;
            chargePct = 1.0f;
        }

        // set effects on fully charged
        if(loopingLaserSound != null && audioPlayer != null && audioPlayer.enabled &&
            isFullyCharged && !audioPlayer.isPlaying) {
            audioPlayer.clip = loopingLaserSound;
            audioPlayer.Play();
        }

        if(endpointEffect != null && !endpointEffect.gameObject.activeSelf && isFullyCharged) {
            endpointEffect.gameObject.SetActive(true);
        }

        float chargeWidth = Mathf.Lerp(laserWidthRange.x, laserWidthRange.y, chargePct);
        laserRenderer.SetWidth(chargeWidth, chargeWidth);

        //laserRenderer.SetPosition(0, firePoint.position);
        for(int i = 0; i < numLaserVerts; i++) {
            laserRenderer.SetPosition(i, firePoint.position);
        }

        Vector3 toAimPoint = owner.GetAimLocation() - firePoint.position;
        Vector3 endPoint;

        Vector3 fireDirection = toAimPoint.normalized;
        Vector3 castOrigin = firePoint.position;
        float castDist = maxRange;

        Vector3 finalEndPoint = Vector3.zero;

        for(int i = 0; i <= numLaserVerts - 2; i++) {
            RaycastHit2D[] hits = WeaponRayCast(castOrigin, fireDirection, castDist, detectLayers);

            if(hits.Length > 0) {
                endPoint = hits[0].point;
                laserRenderer.SetPosition(i + 1, endPoint);

                // check if any enemy was hit
                Actor target = hits[0].collider.gameObject.GetComponent<Actor>();
                if(target != null) {
                    // Only do damage if the laser is ful;ly charged
                    if(isFullyCharged) {
                        target.TakeDamage(damagePerSecond * Time.deltaTime, owner, this);
                    }
                    
                    for(int j = i + 2; j < numLaserVerts; j++) {
                        laserRenderer.SetPosition(j, endPoint + fireDirection * (0.01f * j));
                    }

                    finalEndPoint = endPoint;

                    break;
                }
                else {
                    if(i + 2 < numLaserVerts) {
                        laserRenderer.SetPosition(i + 2, endPoint + fireDirection * 0.1f);
                        i++;
                    }
                }

                //float distUsed = (castOrigin - endPoint).magnitude;

                // reflect off the surface
                Vector3 bounceDir = fireDirection - (2.0f * Vector3.Dot(fireDirection, hits[0].normal)) * (Vector3)hits[0].normal;

                fireDirection = bounceDir.normalized;
                //castDist -= distUsed;
                castOrigin = endPoint + bounceDir * 0.1f;
            }
            else {
                endPoint = castOrigin + fireDirection * castDist;
                laserRenderer.SetPosition(i + 1, endPoint);

                finalEndPoint = endPoint;

                // place all verts at the end
                if(bounces > 0) {
                    for(int j = i + 1; j < numLaserVerts; j++) {
                        laserRenderer.SetPosition(j, endPoint);
                    }
                }
            }
        }

        // set location of the effect
        if(endpointEffect != null) {
            endpointEffect.transform.position = finalEndPoint;
        }
    }

    public override bool BeginFire() {
        //bool beganFire = base.BeginFire();
        if(!CanRefire()) {
            return false;
        }
        isFiring = true;
        
        laserRenderer.enabled = true;

        // Do some camera shake, if its the player shooting. TOTO: more generalized way to do camera shake
        if(owner.GetType() == typeof(PlayerController)) {
            CameraController.CameraShake shakeData = new CameraController.CameraShake(0.1f, 0.05f, 5.0f, 1.0f, true);

            PlayerController playerOwner = (PlayerController)owner;
            playerOwner.PlayerCamera.GetComponent<CameraController>().StartCameraShake(ref shakeData, -GetAimDirection().normalized);
        }

        startedFireTime = Time.time;
        
        return true;
    }


    public override void EndFire() {
        base.EndFire();

        // Stop elaser effects
        laserRenderer.enabled = false;

        if(owner.GetType() == typeof(PlayerController)) {
            PlayerController playerOwner = (PlayerController)owner;
            playerOwner.PlayerCamera.GetComponent<CameraController>().ForceEndShake();
        }

        if(endpointEffect != null) {
            endpointEffect.gameObject.SetActive(false);
        }

        if(loopingLaserSound != null && audioPlayer != null) {
            audioPlayer.Stop();
            audioPlayer.clip = null;            
        }
    }



    public override void OnEquip(MechController controller, bool usesAmmo = true) {
        base.OnEquip(controller, usesAmmo);

        currentEnergy = baseAmmo;
    }


    public override float AIBurstLength() {
        return Random.Range(1.5f, 2.0f);
    }


    protected override void OnDisable() {
        base.OnDisable();

        if(endpointEffect != null) {
            endpointEffect.gameObject.SetActive(false);
        }

        if(loopingLaserSound != null && audioPlayer != null) {
            audioPlayer.Stop();
            audioPlayer.clip = null;
        }
    }
}
