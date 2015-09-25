#define debug_fire

using UnityEngine;
using System.Collections;

public class Weapon_MachineGun : Weapon {

    [SerializeField]
    public float maxRange;


	
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

        RaycastHit2D hitresult = Physics2D.Raycast(firePoint.position, fireDirection, maxRange, detectLayers);

        // Decide on the endpoint for effects and stuff
        Vector3 endPoint;
        if(hitresult.collider == null) {
            endPoint = firePoint.position + fireDirection * maxRange;
        }
        else {
            endPoint = hitresult.point;
        }

#if debug_fire
        Debug.DrawLine(firePoint.position, endPoint, Color.yellow, 1.0f);
#endif
    }

}
