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
    public float ai_targetAquireTime;
    private float ai_AquireTargetPct;

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


    public override bool AI_AllowFire(AIController controller) {
        // If the AI is not even tracking (cant see target), dont even try
        if(!controller.isTrackingTarget) {
            return false;
        }

        // compare to either the last fire time or the last time the target was seen, whatever is more recent
        float lastRelevantTime = lastFireTime > controller.lastAquiredTargetTime ? lastFireTime : controller.lastAquiredTargetTime;

        // Railgun must be aimed at the player for some time before it will be shot by the AI
        float timeInSights = Time.time - lastRelevantTime;

        ai_AquireTargetPct = timeInSights / ai_targetAquireTime;
        
        if(timeInSights > ai_targetAquireTime) {
            return true;
        }

        return false;
    }


    public override void UpdateAIAttackState(AIController controller) {
        base.UpdateAIAttackState(controller);
        if(!controller.isTrackingTarget) {
            controller.aiLaserSight.enabled = false;
        }
        else {
            controller.aiLaserSight.enabled = true;
        }

        if(!CanRefire() || !controller.HasLOSTarget()) {
            controller.aiLaserSight.enabled = false;
        }

        controller.aiLaserSight.SetPosition(0, firePoint.position);
        controller.aiLaserSight.SetPosition(1, controller.target.transform.position);

        const float maxSightWidth = 1.0f;
        const float minSightWidth = 0.1f;

        float sightWidth = Mathf.Lerp(maxSightWidth, minSightWidth, ai_AquireTargetPct);
        controller.aiLaserSight.SetWidth(sightWidth, sightWidth);
    }
}
