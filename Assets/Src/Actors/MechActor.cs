using UnityEngine;
using System.Collections;


/// <summary>
/// MechActors are the main actors in the game. It contains all state for mechs like their health, armor
/// weapons and etc. The controller is the thing that actually uses these states however, and the Actor 
/// should never actually do stuff on its own.
/// </summary>
public class MechActor : Actor {

    public enum EAttachSide {
        Left, Right
    }

    [SerializeField]
    public float maxShield;

    [SerializeField]
    public float shieldRechargeDelay;

    [SerializeField]
    public float shieldRechargeRate;

    [SerializeField]
    public float shieldEnergyDrainRatio;

    [SerializeField]
    public float maxEnergyLevel;

    [SerializeField]
    public float energyRechargeRate;

    /// <summary>
    /// Attachment points for guns or whatever
    /// </summary>
    [SerializeField]
    public Transform leftAttachPoint;

    [SerializeField]
    public Transform rightAttachPoint;
    
    [SerializeField]
    public ParticleSystem brokenWeaponEffectPrototype;

    /// <summary>
    /// Cached weapon reference for the item attached to each side
    /// </summary>
    [HideInInspector]
    public Weapon leftWeapon;

    [HideInInspector]
    public Weapon rightWeapon;
    
    private MechController controller;

    private float currentShield;
    public float CurrentShield {
        get { return currentShield; }
    }

    private float currentEnergyLevel;
    public float EnergyLevel {
        get { return currentEnergyLevel;  }
    }

    protected override void Start() {
        base.Start();

        currentShield = maxShield;
        currentEnergyLevel = maxEnergyLevel;
    }
    

    /// <summary>
    /// Called from controller component when this mech is spawned by world manager
    /// </summary>
    public virtual void OnSpawnInitialization() {
        controller = GetComponent<MechController>();
    }


    protected override void Update() {
        base.Update();


        // energy regeneration
        if(currentEnergyLevel < maxEnergyLevel) {
            currentEnergyLevel += energyRechargeRate * Time.deltaTime;

            if(currentEnergyLevel > maxEnergyLevel) {
                currentEnergyLevel = maxEnergyLevel;
            }
        }

        // Can start recharging shield
        if(Time.time - lastReceivedDamage > shieldRechargeDelay) {
            if(currentShield < maxShield) {
                // recharge the shield by consuming energy
                float rechargeTick = ConsumeEnergy(shieldRechargeRate * Time.deltaTime * shieldEnergyDrainRatio);
                currentShield += rechargeTick / shieldEnergyDrainRatio;

                if(currentShield > maxShield) {
                    currentShield = maxShield;
                }
            }
        }
	}


    /// <summary>
    /// Reduces current energy level byt he base amount, unless there is not enough energy.
    /// Returns how much enery was actually consumed
    /// </summary>
    public float ConsumeEnergy(float baseAmount) {
        float amountConsumed = Mathf.Min(baseAmount, currentEnergyLevel);

        currentEnergyLevel -= amountConsumed;
        return amountConsumed;
    }



    /// <summary>
    /// Overriden to account for shield absorbtion effect
    /// </summary>
    public override void TakeDamage(float damageAmount, MechController instigator, Weapon weaponUsed) {
        if(IsDead) {
            return;
        }

        float modifiedDamage = instigator != null? instigator.ModifyBaseDamage(damageAmount, weaponUsed) : damageAmount;
        float reducedDamage = modifiedDamage;

        // Take the damage out of the shield first
        float shieldAbsorbtion = Mathf.Min(currentShield, damageAmount);
        currentShield -= shieldAbsorbtion;

        // And reduce the damage that is done to health
        reducedDamage -= shieldAbsorbtion;

        base.TakeDamage(reducedDamage, instigator, weaponUsed);
    }


    /// <summary>
    /// Attaches the input item to the mechs attach point indicated
    /// </summary>
    public void DoAttachment(EAttachSide attachSide, GameObject attachment, Vector3 attachOffset) {
        if(attachment == null) {
            return;
        }

        Weapon weaponComponent = attachment.GetComponent<Weapon>();

        // do the attachment through parenting
        if(attachSide == EAttachSide.Left) {
            if(leftWeapon != null) {
                Detach(leftWeapon.gameObject);
            }

            attachment.transform.parent = leftAttachPoint;
            leftWeapon = weaponComponent;
        }
        else {
            if(rightWeapon != null) {
                Detach(rightWeapon.gameObject);
            }

            attachment.transform.parent = rightAttachPoint;
            rightWeapon = weaponComponent;
        }

        // add whatever offset to the object
        attachment.transform.localPosition = attachOffset;
        attachment.transform.localRotation = Quaternion.identity;

        // if it was a weapon that was attached, then set its owner to this mech
        if(weaponComponent != null) {
            weaponComponent.owner = controller;
            weaponComponent.OnEquip(controller, controller.UsesAmmo());
        }
    }
    

    /// <summary>
    /// Attempts to detach the input oobject if it is attached 
    /// </summary>
    public GameObject Detach(GameObject detachTarget, bool isBroken = false) {
        if(detachTarget == null) {
            return null;
        }
        
        // figure out which weapon was detached
        GameObject detached = null;
        if(leftWeapon != null && leftWeapon.gameObject == detachTarget) {
            detached = leftWeapon.gameObject;
            detached.transform.parent = null;
            leftWeapon = null;
        }
        else if(rightWeapon != null && rightWeapon.gameObject == detachTarget) {
            detached = rightWeapon.gameObject;
            detached.transform.parent = null;
            rightWeapon = null;
        }
        
        if(detached != null) {
            // make sure the detached weapon component is not still firing
            Weapon weaponComponent = detached.GetComponent<Weapon>();
            if(weaponComponent != null) {
                weaponComponent.EndFire();
            }

            // If it was breoken off, then add the weapon broken effects
            if(isBroken) {
                ParticleSystem brokenEffect = Instantiate(brokenWeaponEffectPrototype);
                brokenEffect.transform.position = detached.transform.position;
                brokenEffect.transform.parent = controller.headTransform;
                brokenEffect.Play();
            }

            return detached;
        }

        return null;
    }



    /// <summary>
    /// Called when the mechs health goes below 0
    /// </summary>
    public override void Died() {
        base.Died();

        // TODO: death effects and stuff
        Destroy(gameObject);
    }


    /// <summary>
    /// Alternate mode of death, causes mech to fall down and die
    /// </summary>
    public void FalltoDeath(GameObject instigator) {
        const float fallLength = 0.3f;
        controller.FreezeControl();

        StartCoroutine(ScaleToZero(fallLength));
    }

    /// <summary>
    /// Coroutine that scales teh mech to zero and then kills it
    /// </summary>
    private IEnumerator ScaleToZero(float scaleTime) {
        Vector3 startScale = transform.localScale;

        for(float t = 0.0f; t < scaleTime; t += Time.deltaTime) {
            // cubic scaling seems to look good
            float scaleFactor = 1.0f - ((t / scaleTime) * (t / scaleTime) * (t / scaleTime));
            transform.localScale = startScale * scaleFactor;

            yield return null;
        }

        health = 0.0f;
        Died();
    }
}
