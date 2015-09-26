#define debug_fire

using UnityEngine;
using System.Collections;

public class Weapon_MachineGun : Weapon {

    [SerializeField]
    public float maxRange;

    [SerializeField]
    public float baseDamage;

    [SerializeField]
    public bool applyForce;
	
	protected override void Start() {
        base.Start();
	}


    protected override void Update() {
        base.Update();   
	}


    /// <summary>
    /// Overloaded for machine gun fireing algorithm: basically just a raycast
    /// </summary>
    protected override void Fire() {
        base.Fire();

        Vector3 aimLoc = owner.GetAimLocation();
        Vector3 fireDirection = aimLoc - firePoint.position;

        // How far from forward is the fire direction, if its over than the aim point is way off of mech facing, so default to facing
        float aimArcOffset = Vector3.Dot(transform.up, fireDirection);
        if(aimArcOffset < maxFireArc) {
            fireDirection = transform.up;
        }

        RaycastHit2D hitResult = Physics2D.Raycast(firePoint.position, fireDirection, maxRange, detectLayers);

        // Decide on the endpoint for effects and stuff
        Vector3 endPoint;
        if(hitResult.collider == null) {
            endPoint = firePoint.position + fireDirection * maxRange;
        }
        else {
            endPoint = hitResult.point;
        }

        // check what was hit, and apply the damage to it if its an Actor
        if(hitResult.collider != null && hitResult.collider.gameObject != null) {
            Actor victim = CheckIsActor(hitResult.collider.gameObject);
            if(victim != null && victim != owner) {
                victim.TakeDamage(baseDamage, owner, this);

                if(applyForce) {
                    victim.AddForce(fireDirection, 10.0f);
                }
            }
        }
        
#if debug_fire
        Debug.DrawLine(firePoint.position, endPoint, Color.yellow, 1.0f);
#endif

        // Do some camera shake, if its the player shooting. TOTO: more generalized way to do camera shake
        if(owner.GetType() == typeof(PlayerController)) {
            CameraController.CameraShake shakeData = new CameraController.CameraShake(0.1f, 0.5f, 5.0f, 1.0f, false);
            
            PlayerController playerOwner = (PlayerController)owner;
            playerOwner.PlayerCamera.GetComponent<CameraController>().StartCameraShake(ref shakeData, -fireDirection);
        }
    }

}
