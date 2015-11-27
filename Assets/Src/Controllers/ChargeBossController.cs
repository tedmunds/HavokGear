using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChargeBossController : AIController {

    [SerializeField]
    public string bossName;

    [SerializeField]
    public float chargeSpeedModifier;

    [SerializeField]
    public float chargeTurnRateFalloffDist;

    [SerializeField]
    public float chargeTurnRate;

    [SerializeField]
    public float chargeKnockbackForce;

    [SerializeField]
    public float chargeHitDamage;

    [SerializeField]
    public float weakSpotArc;

    [SerializeField]
    public GameObject bulletPrototype;

    [SerializeField]
    public Transform bulletFirePoint;

    [SerializeField]
    public float shootingSpinRate;

    [SerializeField]
    public float shootingFireDelay;

    [SerializeField]
    public AudioClip fireSound;

    [SerializeField]
    private float introBulletAttackRatio;

    [SerializeField]
    public Collider2D weakSpot;

    [SerializeField]
    public UI_BossHealthBar healthBar;

    /// <summary>
    /// Is the boss available for damage right now
    /// </summary>
    [HideInInspector]
    public bool isWeakend;

    private AudioSource audioPlayer;

    private bool wasLastAttackCharge;

    protected override void Start() {
        base.Start();

        OnSpawnInitialization();
        mechComponent.RegisterDeathListener(OnBossKilled);

        // Give the controller authority over when it takes damage
        mechComponent.canTakeDamageRequest = IsVulnerable;

        audioPlayer = GetComponent<AudioSource>();
    }

    public override void AiStartSensing() {
        base.AiStartSensing();

        stateMachine.GotoNewState(new Behaviour_Boss_TelegrapthCharge(), BehaviourSM.TransitionMode.PushCurrent);

        healthBar.gameObject.SetActive(true);

        Text nameField = healthBar.GetComponentInChildren<Text>();
        if(nameField != null) {
            nameField.enabled = true;
            nameField.text = bossName;
        }

    }


    protected override void OnEnable() {
        base.OnEnable();

    }


    protected override void Update() {
        base.Update();
	    
        if(healthBar != null) {
            float hpRatio = mechComponent.Health / mechComponent.maxhealth;
            healthBar.UpdateHealthBar(hpRatio);
        }
	}




    public void OpenWeakSpot() {
        isWeakend = true;
    }


    public void CloseWeakSpot() {
        isWeakend = false;
    }


    /// <summary>
    /// Can this boss take damage right now
    /// </summary>
    public bool IsVulnerable(GameObject damageInstigator) {
        Vector3 facing = headTransform.up;
        Vector3 damageDirection = (damageInstigator.transform.position - transform.position).normalized;

        // Make sure that the attack came from behind
        float angleOfAttack = Vector3.Dot(facing, damageDirection);

        if(isWeakend && angleOfAttack < weakSpotArc) {
            return true;
        }

        return false;
    }


    public void OnBossKilled(Actor victim) {
        // ensure that the hp bar shows its dead
        healthBar.UpdateHealthBar(0.0f);
    }




    public void FireBullet() {
        GameObject obj = WorldManager.instance.SpawnObject(bulletPrototype, bulletFirePoint.position);

        ProjectileController proj = obj.GetComponent<ProjectileController>();
        if(proj != null) {
            proj.LaunchProjectile(headTransform.up, null);
        }

        if(fireSound != null && audioPlayer != null) {
            audioPlayer.PlayOneShot(fireSound);
        }
    }



    public BehaviourSM.BehaviourState GetNextAttackState() {
        if(mechComponent.Health / mechComponent.maxhealth < introBulletAttackRatio) {            
            if(wasLastAttackCharge) {
                wasLastAttackCharge = false;
                return new Behaviour_Boss_SpinShoot();
            }
        }

        wasLastAttackCharge = true;
        return new Behaviour_Boss_Charge();
    }

}
