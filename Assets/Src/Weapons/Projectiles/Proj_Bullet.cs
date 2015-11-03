using UnityEngine;
using System.Collections;



/// <summary>
/// General bullet controller
/// </summary>
public class Proj_Bullet : ProjectileController {

    [SerializeField]
    public float baseDamage;

    [SerializeField]
    public Explosion explosionPrototype;


    protected override void OnEnable() {
        base.OnEnable();


    }

    protected override void Start() {
        base.Start();
    }


    protected override void Update() {
        base.Update();

        transform.up = velocity.normalized;
    }


    protected override void OnImpact(RaycastHit2D hit, Collider2D other) {
        base.OnImpact(hit, other);

        if(explosionPrototype != null) {
            Instantiate(explosionPrototype, hit.point, Quaternion.identity);
        }

        // Try to damage the things that was hit
        Actor victim = other.gameObject.GetComponent<Actor>();
        if(victim != null) {
            victim.TakeDamage(baseDamage, sourceWeapon != null? sourceWeapon.owner : null, sourceWeapon);
        }

        // TODO: make bullets able to bounce
        transform.position = hit.point;
        gameObject.SetActive(false);
    }
}
