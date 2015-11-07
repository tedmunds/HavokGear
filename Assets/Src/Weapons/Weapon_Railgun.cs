using UnityEngine;
using System.Collections;

public class Weapon_Railgun : Weapon {

    [SerializeField]
    public int maxPenetrations;

    [SerializeField]
    public float baseDamage;

    [SerializeField] // how much does the damage get scaled by each penetration
    public float damageFalloffPerHit;

    [SerializeField]
    public ParticleSystem shootEffectPrototype;

    [SerializeField]
    private AudioClip fireSound;

    [SerializeField]
    private AudioClip hitSound;

    [SerializeField]
    private float trailFadeOutTime;

    [SerializeField]
    private Color startTrailColor;

    [SerializeField]
    private Color endTrailColor;


    private LineRenderer lineRenderer;

	
    protected override void Start() {
        base.Start();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
	}


    protected override void Update() {
        base.Update();

        float elapsed = Time.time - lastFireTime;
        if(elapsed < trailFadeOutTime && lineRenderer != null) {
            Color startColor = startTrailColor;
            Color endColor = endTrailColor;

            startColor.a = startTrailColor.a * (1.0f - elapsed / trailFadeOutTime);
            endColor.a = endTrailColor.a * (1.0f - elapsed / trailFadeOutTime);

            lineRenderer.SetColors(startColor, endColor);
        }
        else if(lineRenderer.enabled) {
            lineRenderer.enabled = false;
        }
    }


    protected override void Fire() {
        base.Fire();

        Vector3 fireOrigin = GetFireOrigin();
        Vector3 fireDirection = owner.GetAimLocation() - fireOrigin;

        Vector3 endPoint = fireOrigin + fireDirection.normalized * maxRange;

        int numPenetrations = 0;
        float currentDamage = baseDamage;

        // Cast out and get all of the hits, so that it can sount through them for penetrations
        RaycastHit2D[] hits = WeaponRayCast(fireOrigin, fireDirection, maxRange, detectLayers);
        for(int i = 0; i < hits.Length; i++) {
            Actor validHit = hits[i].collider.GetComponent<Actor>();

            if(validHit != null) {
                validHit.TakeDamage(currentDamage, owner, this);

                numPenetrations += 1;
                currentDamage = currentDamage * damageFalloffPerHit;

                // Check if it has reached the max penetrations
                if(numPenetrations >= maxPenetrations) {
                    endPoint = hits[i].point;
                    break;
                }
            }
            else {
                // Not an actor, can t penetrate it so it stops here
                endPoint = hits[i].point;
                Debug.Log(hits[i].collider.gameObject.name);
                break;
            }
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, fireOrigin);
        lineRenderer.SetPosition(1, endPoint);

        PlaySound(fireSound, 1.0f, Random.Range(0.85f, 1.0f));
    }
}
