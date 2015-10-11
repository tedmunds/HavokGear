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

    [SerializeField]
    private Animator explosionEffectPrefab;


    private int numBounces;


    protected override void Update() {
        base.Update();
    }


    public override void LaunchProjectile(Vector3 direction, Weapon instigator) {
        base.LaunchProjectile(direction, instigator);
        numBounces = 0;
        transform.up = direction;
    }


    protected override void OnImpact(RaycastHit2D hit, Collider2D other) {
        const float maxDeflectAngle = 0.2f;

        base.OnImpact(hit, other);

        numBounces += 1;

        if(ShouldExplode(hit, other)) {
            Explode();
            return;
        }

        // otherwise do a bounce
        Vector3 randOffset = Random.insideUnitCircle;
        float randOffsetLerp = Random.Range(0.0f, maxDeflectAngle);
        
        Vector3 bounceDir = velocity - (2.0f * Vector3.Dot(velocity, hit.normal)) * (Vector3)hit.normal;
        bounceDir = Vector3.Lerp(bounceDir, randOffset, randOffsetLerp);

        velocity = bounceDir.normalized * (velocity.magnitude * bounceElasticity);
        transform.position = hit.point + hit.normal * 0.1f;
        transform.up = velocity.normalized;
    }


    private bool ShouldExplode(RaycastHit2D hit, Collider2D other) {
        if(numBounces > maxBounces) {
            return true;
        }

        // If the hit is an actor
        if(other.gameObject.GetComponent<Actor>() != null ||
            (other.gameObject.transform.parent != null && other.gameObject.transform.parent.GetComponent<Actor>() != null)) {
            return true;
        }

        return false;
    }



    private void Explode() {
        Destroy(gameObject);

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        for(int i = 0; i < overlaps.Length; i++) {
            Actor mech = overlaps[i].GetComponent<Actor>();
            if(mech != null) {
                mech.TakeDamage(baseDamage, sourceWeapon.owner.GetComponent<MechController>(), sourceWeapon);
            }
        }

        // Create the effect
        Animator explosionAnimator = (Animator)Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(explosionAnimator.gameObject, 0.3f);
    }

}
