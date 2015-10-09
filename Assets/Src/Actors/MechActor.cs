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

    protected override void Start() {
        base.Start();

        currentShield = maxShield;
    }
    

    /// <summary>
    /// Called from controller component when this mech is spawned by world manager
    /// </summary>
    public virtual void OnSpawnInitialization() {
        controller = GetComponent<MechController>();
    }


    protected override void Update() {
        base.Update();

        // Can start recharging shield
        if(Time.time - lastReceivedDamage > shieldRechargeDelay) {
            if(currentShield < maxShield) {
                currentShield += shieldRechargeRate * Time.deltaTime;

                if(currentShield > maxShield) {
                    currentShield = maxShield;
                }
            }
        }
	}

    /// <summary>
    /// Overriden to account for shield absorbtion effect
    /// </summary>
    public override void TakeDamage(float damageAmount, MechController instigator, Weapon weaponUsed) {
        if(IsDead) {
            return;
        }

        float reducedDamage = damageAmount;

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
        if(leftWeapon.gameObject == detachTarget) {
            detached = leftWeapon.gameObject;
            detached.transform.parent = null;
            leftWeapon = null;
        }
        else if(rightWeapon.gameObject == detachTarget) {
            detached = rightWeapon.gameObject;
            detached.transform.parent = null;
            rightWeapon = null;
        }
        
        if(detached != null) {
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

}
