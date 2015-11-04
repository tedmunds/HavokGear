using UnityEngine;
using System.Collections;

public class Weapon_Laser : Weapon {

    [SerializeField]
    public float damagePerSecond;

    [SerializeField] // how many segments are in the line renderer for the laser
    public int bounces = 1;

    private float currentEnergy;

    private LineRenderer laserRenderer;

    private int numLaserVerts;

	protected override void Start() {
        base.Start();

        laserRenderer = transform.GetComponentInChildren<LineRenderer>();
        if(laserRenderer == null) {
            Debug.LogWarning("Weapon_Laser didn't find a line renderer in the children objects... Maybe the structure of the weapon has changed?");
        }

        laserRenderer.enabled = false;
        numLaserVerts = bounces + 2;
        laserRenderer.SetVertexCount(numLaserVerts);
    }



    protected override void OnDisable() {
        base.OnDisable();

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

        //laserRenderer.SetPosition(0, firePoint.position);
        for(int i = 0; i < numLaserVerts; i++) {
            laserRenderer.SetPosition(i, firePoint.position);
        }

        Vector3 toAimPoint = owner.GetAimLocation() - firePoint.position;
        Vector3 endPoint;


        Vector3 fireDirection = toAimPoint.normalized;
        Vector3 castOrigin = firePoint.position;
        float castDist = maxRange;

        for(int i = 0; i <= numLaserVerts - 2; i++) {
            RaycastHit2D[] hits = WeaponRayCast(castOrigin, fireDirection, castDist, detectLayers);

            if(hits.Length > 0) {
                endPoint = hits[0].point;
                laserRenderer.SetPosition(i + 1, endPoint);

                // check if any enemy was hit
                Actor target = hits[0].collider.gameObject.GetComponent<Actor>();
                if(target != null) {
                    target.TakeDamage(damagePerSecond * Time.deltaTime, owner, this);

                    for(int j = i + 2; j < numLaserVerts; j++) {
                        laserRenderer.SetPosition(j, endPoint);
                    }

                    break;
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

                // place all verts at the end
                if(bounces > 0) {
                    for(int j = i + 1; j < numLaserVerts; j++) {
                        laserRenderer.SetPosition(j, endPoint);
                    }
                }
            }
        }
    }

    public override bool BeginFire() {
        bool beganFire = base.BeginFire();
        
        if(beganFire) {
            laserRenderer.enabled = true;

            // Do some camera shake, if its the player shooting. TOTO: more generalized way to do camera shake
            if(owner.GetType() == typeof(PlayerController)) {
                CameraController.CameraShake shakeData = new CameraController.CameraShake(0.1f, 0.05f, 5.0f, 1.0f, true);

                PlayerController playerOwner = (PlayerController)owner;
                playerOwner.PlayerCamera.GetComponent<CameraController>().StartCameraShake(ref shakeData, -GetAimDirection().normalized);
            }
        }

        return beganFire;
    }


    public override void EndFire() {
        base.EndFire();

        // Stop elaser effects
        laserRenderer.enabled = false;

        if(owner.GetType() == typeof(PlayerController)) {
            PlayerController playerOwner = (PlayerController)owner;
            playerOwner.PlayerCamera.GetComponent<CameraController>().ForceEndShake();
        }
    }



    public override void OnEquip(MechController controller, bool usesAmmo = true) {
        base.OnEquip(controller, usesAmmo);

        currentEnergy = baseAmmo;
    }
}
