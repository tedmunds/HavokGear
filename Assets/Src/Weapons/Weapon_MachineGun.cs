//#define debug_fire

using UnityEngine;
using System.Collections;

public class Weapon_MachineGun : Weapon {

    [SerializeField]
    public float baseDamage;

    [SerializeField]
    public bool applyForce;

    [SerializeField]
    public ParticleSystem hitEffectPrototype;

    [SerializeField]
    public ParticleSystem shootEffectPrototype;

    [SerializeField]
    private Color trailStartColor;

    [SerializeField]
    private Color trailEndColor;

    [SerializeField]
    private AudioClip fireSound;

    private LineRenderer bulletTrailRenderer;
    private ParticleSystem hitEffect;
    private ParticleSystem shootEffect;

    protected override void Start() {
        base.Start();
        
        // Init the special effects for this weapon
        bulletTrailRenderer = GetComponent<LineRenderer>();
        bulletTrailRenderer.enabled = false;

        if(hitEffectPrototype != null && hitEffect == null) {
            hitEffect = Instantiate(hitEffectPrototype);
            hitEffect.gameObject.SetActive(false);
            hitEffect.transform.parent = transform;
        }

        if(shootEffectPrototype != null && shootEffect == null) {
            shootEffect = Instantiate(shootEffectPrototype);
            shootEffect.gameObject.SetActive(false);
            shootEffect.transform.parent = transform;
        }
    }


    protected override void OnDisable() {
        base.OnDisable();

        if(hitEffect != null) {
            hitEffect.gameObject.SetActive(false);
        }
        
        if(shootEffect != null) {
            shootEffect.gameObject.SetActive(false);
        }

        if(bulletTrailRenderer != null) {
            bulletTrailRenderer.enabled = false;
        }
    }

    protected override void Update() {
        base.Update();

        // Update bullet renderer
        if(bulletTrailRenderer != null) {
            float elapsed = Time.time - lastFireTime;
            if(elapsed < refireDelay) {
                Color startColor = trailStartColor;
                Color endColor = trailEndColor;
                startColor.a = trailStartColor.a * (1.0f - elapsed / refireDelay);
                endColor.a = trailEndColor.a * (1.0f - elapsed / refireDelay);

                bulletTrailRenderer.SetColors(startColor, endColor);
            }
            else {
                bulletTrailRenderer.enabled = false;
            }
        }
	}


    /// <summary>
    /// Overloaded for machine gun fireing algorithm: basically just a raycast
    /// </summary>
    protected override void Fire() {
        base.Fire();
        
        Vector3 fireDirection = GetAimDirection();

        //RaycastHit2D hitResult = Physics2D.Raycast(firePoint.position, fireDirection, maxRange, detectLayers);
        RaycastHit2D[] hitResults = Physics2D.RaycastAll(firePoint.position, fireDirection, maxRange, detectLayers);
        
        // default end point
        Vector3 endPoint = firePoint.position + fireDirection * maxRange; ;

        foreach(RaycastHit2D hit in hitResults) {
            if(hit.collider != null && hit.collider.gameObject != null) {
                Actor victim = CheckIsActor(hit.collider.gameObject);
                    
                if(victim != null && victim.tag != owner.tag) {
                    victim.TakeDamage(baseDamage, owner, this);

                    if(applyForce) {
                        victim.AddForce(fireDirection, 5.0f);
                    }

                    // stop at the first non-owner victim
                    endPoint = hit.point;
                    break;
                }
                else if(victim == null) {
                    endPoint = hit.point;
                    break;
                }
            }
        }
        
        PlaySound(fireSound, 1.0f, Random.Range(0.85f, 1.0f));

#if debug_fire
        Debug.DrawLine(firePoint.position, endPoint, Color.yellow, 1.0f);
#endif

        // Do bullet effect
        if(bulletTrailRenderer != null) {
            bulletTrailRenderer.SetPosition(0, firePoint.position);
            bulletTrailRenderer.SetPosition(1, endPoint);
            bulletTrailRenderer.enabled = true;
        }

        if(hitEffect != null) {
            hitEffect.transform.position = endPoint;
            hitEffect.gameObject.SetActive(true);
            hitEffect.Play();
        }

        if(shootEffect != null) {
            shootEffect.transform.position = firePoint.position;
            shootEffect.transform.up = fireDirection;
            shootEffect.gameObject.SetActive(true);
            shootEffect.Play();
        }
    }

}
