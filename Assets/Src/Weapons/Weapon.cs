using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour {

    /// <summary>
    /// Time inbetween being able to shoot again
    /// </summary>
    [SerializeField]
    public float refireDelay;

    /// <summary>
    /// Dot product arc range that the weapon can fire in
    /// </summary>
    [SerializeField]
    public float maxFireArc;

    /// <summary>
    /// Does the weapon automatically fire after refire delay
    /// </summary>
    [SerializeField]
    public bool isAutomatic;

    /// <summary>
    /// the spacial location from which firing originates
    /// </summary>
    [SerializeField]
    public Transform firePoint;

    /// <summary>
    /// Which layers to interact with (ie. raycast hits etc)
    /// </summary>
    [SerializeField]
    public LayerMask detectLayers;

    /// <summary>
    /// The mech that owns this weapons currently
    /// </summary>
    [HideInInspector]
    public MechController owner;


    /// <summary>
    /// The weapon is currently firing
    /// </summary>
    protected bool isFiring;

    protected float lastFireTime;


    protected virtual void Start() {
	    if(firePoint == null) {
            Debug.LogError("Weapon: <" + name + "> does not have a fire point!");
        }
	}
	
	
	protected virtual void Update() {
	    // Update and check for refire on automatic weapons
        if(isFiring && isAutomatic) {
            if(CanRefire()) {
                Fire();
            }
        }
	}


    /// <summary>
    /// Call to start the weapon firing:
    /// returns true if it starts firing, false if it fails for some reason
    /// </summary>
    public virtual bool BeginFire() {
        if(!CanRefire()) {
            return false;
        }

        Fire();

        isFiring = true;
        return true;
    }


    /// <summary>
    /// Indicates whther or not this weapon can currently do another shot.
    /// overloaded per weapon for ammo and whatever else it may check
    /// </summary>
    protected virtual bool CanRefire() {
        return(Time.time - lastFireTime > refireDelay);
    }


    /// <summary>
    /// Call to end weapon fire 
    /// </summary>
    public virtual void EndFire() {
        isFiring = false;
    }

    

    protected virtual void Fire() {
        lastFireTime = Time.time;
    }


}
