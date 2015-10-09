﻿using UnityEngine;
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

    [SerializeField]
    public float cameraRecoil = 0.1f;

    [SerializeField]
    public int currentAmmo;

    [SerializeField]
    public int ammoPerShot = 1;

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
        
        // Do some camera shake, if its the player shooting. TOTO: more generalized way to do camera shake
        if(owner.GetType() == typeof(PlayerController)) {
            CameraController.CameraShake shakeData = new CameraController.CameraShake(0.1f, cameraRecoil, 5.0f, 1.0f, false);

            PlayerController playerOwner = (PlayerController)owner;
            playerOwner.PlayerCamera.GetComponent<CameraController>().StartCameraShake(ref shakeData, -GetAimDirection());
        }

        isFiring = true;
        return true;
    }


    /// <summary>
    /// Indicates whther or not this weapon can currently do another shot.
    /// overloaded per weapon for ammo and whatever else it may check
    /// </summary>
    protected virtual bool CanRefire() {
        if(Time.time - lastFireTime > refireDelay) {

            // check that the weapon has enough ammo
            if(currentAmmo >= ammoPerShot) {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Call to end weapon fire 
    /// </summary>
    public virtual void EndFire() {
        isFiring = false;
    }

    

    protected virtual void Fire() {
        lastFireTime = Time.time;

        // use up some amount of ammo
        currentAmmo -= ammoPerShot;
        if(currentAmmo <= 0) {
            currentAmmo = 0;
        }
    }



    /// <summary>
    /// Tries to find a mech actor component attached to this object somehow, either
    /// directly or on a parent
    /// </summary>
    public static Actor CheckIsActor(GameObject hitObject) {
        Actor victim = hitObject.GetComponent<Actor>();

        if(victim == null) {
            while(hitObject != null) {
                Transform objParent = hitObject.transform.parent;
                if(objParent != null) {
                    hitObject = hitObject.transform.parent.gameObject;

                    victim = hitObject.GetComponent<Actor>();
                    if(victim != null) {
                        break;
                    }
                }
                else {
                    hitObject = null;
                }
            }
        }

        return victim;
    }


    /// <summary>
    /// Gets the direction to the aim location, constrained within the aim arc range
    /// </summary>
    protected Vector3 GetAimDirection() {
        Vector3 aimLoc = owner.GetAimLocation();
        Vector3 fireDirection = aimLoc - firePoint.position;

        // How far from forward is the fire direction, if its over than the aim point is way off of mech facing, so default to facing
        float aimArcOffset = Vector3.Dot(transform.up, fireDirection);
        if(aimArcOffset < maxFireArc) {
            fireDirection = transform.up;
        }

        return fireDirection;
    }



}