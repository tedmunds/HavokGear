using UnityEngine;
using System.Collections;

public class Proj_Grenade : ProjectileController {

    [SerializeField]
    private int maxBounces;

    [SerializeField]
    private float bounceElasticity = 0.5f;

    [SerializeField]
    private float explosionRadius;

    [SerializeField]
    private float baseDamage;


    private int numBounces;


    protected override void Update() {
        base.Update();
    }


    public override void LaunchProjectile(Vector3 direction, Weapon instigator) {
        base.LaunchProjectile(direction, instigator);
        numBounces = 0;
    }


    protected override void OnImpact(RaycastHit2D hit, Collider2D other) {
        const float hitOffsetDist = 0.5f;

        base.OnImpact(hit, other);

        numBounces += 1;

        if(ShouldExplode(hit, other)) {
            Explode();
            return;
        }

        // otherwise do a bounce
        velocity = hit.normal * (velocity.magnitude * bounceElasticity);
        transform.position = hit.point;
    }


    private bool ShouldExplode(RaycastHit2D hit, Collider2D other) {
        if(numBounces > maxBounces) {
            return true;
        }

        // If the hit is a mech
        if(other.gameObject.GetComponent<MechController>() != null ||
            (other.gameObject.transform.parent != null && other.gameObject.transform.parent.GetComponent<MechController>() != null)) {
            return true;
        }

        return false;
    }



    private void Explode() {
        Destroy(gameObject);

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        for(int i = 0; i < overlaps.Length; i++) {
            MechActor mech = overlaps[i].GetComponent<MechActor>();
            if(mech != null) {
                mech.TakeDamage(baseDamage, mech.GetComponent<MechController>(), sourceWeapon);

                Debug.DrawLine(transform.position, mech.transform.position, Color.red, 5.0f);
            }
        }
    }

}
