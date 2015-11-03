using UnityEngine;
using System.Collections;

public class Weapon_Melee : Weapon {

    [SerializeField]
    public float baseDamage;

    [SerializeField]
    public float hitRange;

    [SerializeField]
    public bool applyForce;

    [SerializeField]
    public float hitForce;

    [SerializeField]
    public int maxHitsPerAttack;

    [SerializeField]
    private AudioClip hitSound;

    protected override void Start() {
        base.Start();
    }



    protected override void OnDisable() {
        base.OnDisable();

    }


    protected override void Update() {
        base.Update();

    }


    protected override void Fire() {
        base.Fire();

        int numHits = 0;
        bool enemyWasHit = false;

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(firePoint.position, hitRange);
        foreach(Collider2D hit in overlaps) {
            // If max hits is zero, then dont worry about quitting out, otherwise check that it hasnt reached the limit
            if(maxHitsPerAttack > 0 && numHits >= maxHitsPerAttack) {
                break;
            }

            Actor target = hit.gameObject.GetComponent<Actor>();
            if(target == null) {
                continue;
            }

            // dont hit the owner!
            if(target.gameObject == owner.gameObject) {
                continue;
            }

            numHits += 1;
            enemyWasHit = true;

            Vector3 toTarget = target.transform.position - firePoint.position;
            Vector3 aimDirection = owner.GetAimLocation() - firePoint.position;
            float hitAngle = Vector3.Dot(aimDirection.normalized, toTarget.normalized);

            // If the dot is within the max arc, then do the damage
            if(hitAngle >= maxFireArc) {
                target.TakeDamage(baseDamage, owner, this);


                if(applyForce) {
                    target.AddForce(toTarget.normalized, hitForce);
                }
            }
        }

        // at least one enemy was hit
        if(enemyWasHit) {
            PlaySound(hitSound);
        }
    }
}
