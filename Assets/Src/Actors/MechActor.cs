using UnityEngine;
using System.Collections;


/// <summary>
/// MechActors are the main actors in the game. It contains all state for mechs like their health, armor
/// weapons and etc. The controller is the thing that actually uses these states however, and the Actor 
/// should never actually do stuff on its own.
/// </summary>
public class MechActor : Actor {

    public delegate void OnDamageHandler(float damageAmount);

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

    [SerializeField]
    public Weapon.EDamageType[] armorWeaknessList;

    [SerializeField]
    public float armorProtection;

    /// <summary>
    /// Attachment points for guns or whatever
    /// </summary>
    [SerializeField]
    public Transform leftAttachPoint;

    [SerializeField]
    public Transform rightAttachPoint;
    
    [SerializeField]
    public ParticleSystem brokenWeaponEffectPrototype;
    public ParticleSystem brokenEffect;

    [SerializeField]
    public Explosion deathExplosion;

    /// <summary>
    /// Cached weapon reference for the item attached to each side
    /// </summary>
    [HideInInspector]
    public Weapon leftWeapon;

    [HideInInspector]
    public Weapon rightWeapon;

    [HideInInspector]
    public OnDamageHandler damageHandlerCallback;

    private MechController controller;

    private bool isFalling;

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
        if(Time.time - lastReceivedDamage > shieldRechargeDelay) {
            if(currentEnergyLevel < maxEnergyLevel) {
                currentEnergyLevel += energyRechargeRate * Time.deltaTime;

                if(currentEnergyLevel > maxEnergyLevel) {
                    currentEnergyLevel = maxEnergyLevel;
                }
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

        // health regen
        float healthRegen = GetHealthRecharge() * Time.deltaTime;
        if(healthRegen > 0.0f) {
            AddHealth(healthRegen, this.gameObject);
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
        if(IsDead || isFalling) {
            return;
        }

        // Check if friendly fire is allowed
        if(weaponUsed != null) {
            if(instigator.mechTeam == controller.mechTeam && !WorldManager.instance.friendlyFire) {
                return;
            }
        }
        
        float modifiedDamage = instigator != null? instigator.ModifyBaseDamage(damageAmount, weaponUsed) : damageAmount;
        float reducedDamage = modifiedDamage;

        bool armorReducesDamage = true;

        // Account for the armor weaknesses from the weapon type
        if(armorWeaknessList.Length > 0 && weaponUsed != null) {
            // this armor has a weakness, check if the weapon has a strength against that weakness
            for(int i = 0; i < armorWeaknessList.Length; i++) {
                for(int j = 0; j < weaponUsed.damageTypeList.Length; j++) {
                    if(armorWeaknessList[i] == weaponUsed.damageTypeList[j]) {
                        // then the damage is not reduced any more, because this weapon is strong against this weakness
                        armorReducesDamage = false;
                    }
                }
            }
        }

        if(armorReducesDamage) {
            float priorDamage = reducedDamage;
            reducedDamage = (1.0f - armorProtection) * reducedDamage;
        }

        // Take the damage out of the shield first: changed system to only use energy bar
        //float shieldAbsorbtion = Mathf.Min(currentShield, reducedDamage);
        //currentShield -= shieldAbsorbtion;
        float shieldAbsorbtion = ConsumeEnergy(reducedDamage);

        // And reduce the damage that is done to health
        reducedDamage -= shieldAbsorbtion;

        if(damageHandlerCallback != null) {
            damageHandlerCallback(reducedDamage);
        }

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

        controller.NewWeaponAttached(weaponComponent);
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
                if(brokenEffect == null) {
                    brokenEffect = Instantiate(brokenWeaponEffectPrototype);
                }
                else {
                    brokenEffect.gameObject.SetActive(true);
                }
                
                brokenEffect.transform.position = detached.transform.position;
                brokenEffect.transform.parent = controller.headTransform;
                brokenEffect.Play();
            }

            // notifiy controller
            controller.WeaponDetached(weaponComponent);

            return detached;
        }

        return null;
    }



    /// <summary>
    /// Called when the mechs health goes below 0
    /// </summary>
    public override void Died() {
        base.Died();

        gameObject.SetActive(false);
        if(brokenEffect != null) {
            brokenEffect.gameObject.SetActive(false);
        }
        
        // TODO: death effects and stuff
    }




    /// <summary>
    /// Resets the state to a default
    /// </summary>
    public void ResetState(bool removeLeftWeapon = true, bool removeRightWeapon = true) {
        GameObject detached;

        if(removeLeftWeapon) {
            detached = Detach(leftWeapon != null ? leftWeapon.gameObject : null);
            if(detached != null) {
                detached.SetActive(false);
            }
        }
        
        if(removeRightWeapon) {
            detached = Detach(rightWeapon != null ? rightWeapon.gameObject : null);
            if(detached != null) {
                detached.SetActive(false);
            }
        }

        health = maxhealth;
        currentShield = maxShield;
        currentEnergyLevel = maxEnergyLevel;

        isDead = false;
        if(controller != null) {
            controller.SetControllerActive(true);
        }
    }






    /// <summary>
    /// Alternate mode of death, causes mech to fall down and die
    /// </summary>
    public void FalltoDeath(GameObject instigator) {
        const float fallLength = 0.3f;

        if(isDead) {
            return;
        }

        controller.SetControllerActive(false);
        
        if(!isFalling) {
            StartCoroutine(ScaleToZero(fallLength));
        }
    }

    /// <summary>
    /// Coroutine that scales the mech to zero and then kills it
    /// </summary>
    private IEnumerator ScaleToZero(float scaleTime) {
        Vector3 startScale = transform.localScale;
        isFalling = true;

        for(float t = 0.0f; t < scaleTime; t += Time.deltaTime) {
            // cubic scaling seems to look good
            float scaleFactor = 1.0f - ((t / scaleTime) * (t / scaleTime) * (t / scaleTime));
            transform.localScale = startScale * scaleFactor;
            
            yield return null;
        }
        
        // reset the scale once it's dead
        transform.localScale = startScale;
        isFalling = false;

        health = 0.0f;
        Died();
    }


    protected override float DoDeathSequence(MechController instigator, Weapon weaponUsed) {
        //if(instigator.GetType() != typeof(PlayerController)) {
        //    return 0.0f;
        //}

        const float delayLength = 0.0f;
        const float knowckbackForce = 0.0f;

        isDead = true;

        if(deathExplosion != null) {
            Explosion deathExplosionInstance = (Explosion)Instantiate(deathExplosion, transform.position, Quaternion.identity);
            if(deathExplosionInstance != null) {
                deathExplosionInstance.Explode(weaponUsed);
            }
        }

        // So it was killed by the player, do a fancy death sequence where it shoots away from the palyer
        AddForce((transform.position - instigator.transform.position).normalized, knowckbackForce);
        controller.SetControllerActive(false);

        return delayLength;
    }


    public override float GetBonusHealth() {
        return controller.GetHealthModifier();
    }

    public float GetHealthRecharge() {
        return controller.GetHealthRegen();
    }

}
