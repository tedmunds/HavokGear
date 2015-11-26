using UnityEngine;
using System.Collections;

public class Proj_Grenade : ProjectileController {

    [SerializeField]
    private int maxBounces;

    [SerializeField]
    private float bounceElasticity = 0.5f;

    [SerializeField]
    private Explosion explosionPrefab;

    [SerializeField]
    private AudioClip bounceSound;

    private int numBounces;

    private AudioSource audioPlayer;


    protected override void Update() {
        base.Update();

        if(!WorldManager.instance.ObjectOnScreen(this.gameObject)) {
            gameObject.SetActive(false);
        }
    }


    protected override void OnEnable() {
        base.OnEnable();
        numBounces = 0;

        audioPlayer = GetComponent<AudioSource>();
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if(trail != null) {
            //trail.time = -1;
        }
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

        if(ShouldExplode(hit, other) && explosionPrefab != null) {
            Explosion spawnedExplosion = (Explosion)Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if(spawnedExplosion != null) {
                spawnedExplosion.Explode(sourceWeapon);
            }

            gameObject.SetActive(false);
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

        // bounced, so play bounce sound
        if(audioPlayer != null && bounceSound != null) {
            audioPlayer.PlayOneShot(bounceSound);
        }
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


   
}
