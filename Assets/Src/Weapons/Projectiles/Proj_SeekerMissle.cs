using UnityEngine;
using System.Collections;

public class Proj_SeekerMissle : ProjectileController {


    [SerializeField]
    private Explosion explosionPrefab;

    [HideInInspector]
    public MechController targetMech;

	private void Start() {
        Actor actorComp = GetComponent<Actor>();
        if(actorComp != null) {
            actorComp.RegisterDeathListener(OnProjectileKilled);
        }
	}


    protected override void Update() {
        base.Update();

        if(targetMech != null && targetMech.gameObject.activeSelf && !targetMech.MechComponent.IsDead) {
            Vector3 toTarget = targetMech.transform.position - transform.position;

            velocity = toTarget.normalized * moveSpeed;
            transform.up = velocity.normalized;
        }
        else {
            Explode();
        }
    }


    protected override void OnImpact(RaycastHit2D hit, Collider2D other) {
        if(ShouldExplode(hit, other)) {
            Explode();
            return;
        }
    }


    private bool ShouldExplode(RaycastHit2D hit, Collider2D other) {
        // If the hit is an actor
        if(other.gameObject.GetComponent<Actor>() != null ||
           (other.gameObject.transform.parent != null && other.gameObject.transform.parent.GetComponent<Actor>() != null)) {

            // Only explode on target mech
            MechController otherMech = other.gameObject.GetComponent<MechController>();
            if(otherMech != null && other.gameObject != targetMech.gameObject) {
                return false;
            }

            return true;
        }
        else { // did not hit an actor, explode!
            return true;
        }
    }


    private void Explode() {
        Explosion spawnedExplosion = (Explosion)Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if(spawnedExplosion != null) {
            spawnedExplosion.Explode(sourceWeapon);
        }

        gameObject.SetActive(false);
    }


    public void OnProjectileKilled(Actor actorComp) {
        Explode();
    }
}
