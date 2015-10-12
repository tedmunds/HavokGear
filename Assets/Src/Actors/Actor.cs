﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void OnDeathHandler(Actor victim);

/// <summary>
/// Actor is the base class for all objects in the game that can take damage and stuff.
/// In other words Actors are a collection of State, and should have minimal behaviour
/// </summary>
public class Actor : MonoBehaviour {
    
    [SerializeField]
    public float maxhealth;

    // Current health
    protected float health;
    public float Health {
        get { return health; }
    }

    protected float lastReceivedDamage;

    protected bool isDead;
    public bool IsDead {
        get { return isDead; }
    }


    private List<OnDeathHandler> deathListeners;

	protected virtual void Start() {
        health = maxhealth;
        isDead = false;
    }


    /// <summary>
    /// Register a delegate to listen for this actors death
    /// </summary>
    public void RegisterDeathListener(OnDeathHandler listener) {
        if(deathListeners == null) {
            deathListeners = new List<OnDeathHandler>();
        }

        deathListeners.Add(listener);
    }


    protected virtual void Update() {
	    
	}




    /// <summary>
    /// Applies the damage to the actor
    /// </summary>
    public virtual void TakeDamage(float damageAmount, MechController instigator, Weapon weaponUsed) {
        if(isDead) {
            return;
        }
        
        health -= damageAmount;
        
        if(health <= 0.0f) {
            health = 0.0f;
            Died();
        }

        lastReceivedDamage = Time.time;
    }


    public void AddForce(Vector3 forceDirection, float force) {
        MovementController2D movementComponent = GetComponent<MovementController2D>();

        if(movementComponent != null) {
            movementComponent.ApplyForce(forceDirection.normalized * force);
        }
    }




    public virtual void Died() {
        isDead = true;

        // Callback to all lsiteners
        if(deathListeners != null) {
            foreach(OnDeathHandler handler in deathListeners) {
                handler(this);
            }
        }
    }

}
