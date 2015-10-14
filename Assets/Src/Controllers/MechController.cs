﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Base controller class for players and AI entities. Requires a mechActor component, which contains all of the 
/// state stuff for mechs. Controllers state should be based around its behaviour in some way. The actor is the 
/// actual "model" of a mech, the controller is more like its pilot.
/// </summary>
[RequireComponent(typeof(MechActor))]
public class MechController : MonoBehaviour {

    /// <summary>
    /// Transform for the head sprite that rotates towards aim direction
    /// </summary>
    [SerializeField]
    public Transform headTransform;

    [SerializeField]
    public Transform legTransform;

    [SerializeField]
    public Animator legAnimator;
    
    [SerializeField]
    public float baseMoveSpeed = 5.0f;

    [SerializeField]
    public float baseAimRotSpeed = 1.0f;


    protected MovementController2D movementComponent;
    protected MechActor mechComponent;
    public MechActor MechComponent {
        get { return mechComponent; }
    }

    /// <summary>
    /// Governs the current movement method
    /// </summary>
    protected MovementState currentMoveState;

    // Movement states that mechs can do
    protected MoveState_Normal moveState_Normal;

    /// <summary>
    /// If false the controller will not update
    /// </summary>
    protected bool controllerActive;

    
    // Use this for initialization
    protected virtual void Start () {
        controllerActive = true;

        // Just some initialization and warnings
        movementComponent = GetComponent<MovementController2D>();
        if(movementComponent == null) {
            Debug.LogWarning("MechController: <" + name + "> Does not have a CharacterController2D component, movement will not work!");
        }
        
        if(headTransform == null) {
            Debug.LogWarning("MechController: <" + name + "> Head Transform is not set!");
        }

        // Init the different move states
        moveState_Normal = new MoveState_Normal(this);

        // Default to the normal move state
        currentMoveState = moveState_Normal;
    }

    protected virtual void OnEnable() {
        mechComponent = GetComponent<MechActor>();
    }


    /// <summary>
    /// Called by world manager when a mech is spawned
    /// </summary>
    public virtual void OnSpawnInitialization() {
        // proagate initialization to the actor component
        mechComponent.OnSpawnInitialization();
    }


    /// <summary>
    /// Stops all control activity from this controller
    /// </summary>
    public void SetControllerActive(bool newFrozen) {
        controllerActive = newFrozen;
    }

    // Update is called once per frame
    protected virtual void Update () {
        
	}


    public virtual Vector3 GetAimLocation() {
        return transform.up;
    }



    /// <summary>
    /// Calculates a damage bonus for the input weapon
    /// </summary>
    /// <returns></returns>
    public virtual float ModifyBaseDamage(float baseDamage, Weapon weaponType) {
        return baseDamage;
    }


    public virtual bool UsesAmmo() {
        return true;
    }

}
