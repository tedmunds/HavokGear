﻿using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

    [SerializeField]
    public float moveSpeed;

    [SerializeField]
    protected float maxLifeTime = 0.0f;

    // Movement velocity
    protected Vector3 velocity;

    // previous position in movement simulation
    protected Vector3 previousPos;

    protected float launchTime;

    protected Weapon sourceWeapon;

    protected virtual void Start() {
	    
	}


    protected virtual void Update() {
        Move();
    }


    /// <summary>
    /// Start the projectiles simulation
    /// </summary>
    public virtual void LaunchProjectile(Vector3 direction, Weapon instigator) {
        velocity = direction.normalized * moveSpeed;
        launchTime = Time.time;
        sourceWeapon = instigator;
        previousPos = transform.position;
    }


    public void Move() {
        int ignoreLayer = 1 << gameObject.layer;
        ignoreLayer = ~ignoreLayer;

        // Update position from velocity
        transform.position += velocity * Time.deltaTime;

        // Interpolation hit check
        Vector3 moveDisplacement = transform.position - previousPos;

        RaycastHit2D hit;
        hit = Physics2D.Raycast(transform.position, moveDisplacement, moveDisplacement.magnitude, ignoreLayer);
        if(hit.collider != null && 
            (sourceWeapon != null && hit.collider.gameObject != sourceWeapon.gameObject) && 
            !hit.collider.isTrigger) {
            // A different thing has been hit, call impact handler
            OnImpact(hit, hit.collider);
        }

        previousPos = transform.position;

        // destroy atfter lifetime
        if(Time.time - launchTime > maxLifeTime && maxLifeTime != 0.0f) {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// Called when the projectile hits something, can be overloaded to do whatever, like explode or bounce etc.
    /// </summary>
    protected virtual void OnImpact(RaycastHit2D hit, Collider2D other) {

    }


}