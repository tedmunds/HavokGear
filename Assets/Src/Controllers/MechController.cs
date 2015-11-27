using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base controller class for players and AI entities. Requires a mechActor component, which contains all of the 
/// state stuff for mechs. Controllers state should be based around its behaviour in some way. The actor is the 
/// actual "model" of a mech, the controller is more like its pilot.
/// </summary>
[RequireComponent(typeof(MechActor))]
public class MechController : MonoBehaviour {

    public enum EMechTeam {
        Friendly, Neutral, Enemy
    }

    /// <summary>
    /// Transform for the head sprite that rotates towards aim direction
    /// </summary>
    [SerializeField]
    public Transform headTransform;

    [SerializeField]
    public Transform legTransform;

    [SerializeField]
    public SpriteRenderer damageFlashScreen;

    [SerializeField]
    public Animator legAnimator;
    
    [SerializeField]
    public float baseMoveSpeed = 5.0f;

    [SerializeField]
    public float baseAimRotSpeed = 1.0f;

    [SerializeField] // 0 = good guy, 1 = bad
    public EMechTeam mechTeam;


    protected MovementController2D movementComponent;
    public MovementController2D MovementComponent {
        get { return movementComponent; }
    }

    protected MechActor mechComponent;
    public MechActor MechComponent {
        get { return mechComponent; }
    }

    /// <summary>
    /// Governs the current movement method
    /// </summary>
    protected MovementState currentMoveState;

    /// <summary>
    /// Stack stores previous move states as they get applied
    /// </summary>
    protected Stack<MovementState> moveStateStack;

    // Movement states that mechs can do
    protected MoveState_Normal moveState_Normal;

    /// <summary>
    /// If false the controller will not update
    /// </summary>
    protected bool controllerActive;


    private bool hasDoneFirstFrameInit;

    
    // Use this for initialization
    protected virtual void Start () {
        controllerActive = true;
        moveStateStack = new Stack<MovementState>(3);

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
        moveStateStack.Push(moveState_Normal);

        // register to listen for damage events
        mechComponent.damageHandlerCallback = OnDamageHandler;
    }

    protected virtual void OnEnable() {
        mechComponent = GetComponent<MechActor>();

        if(damageFlashScreen != null) { 
            Color c = damageFlashScreen.color;
            c.a = 0.0f;
            damageFlashScreen.color = c;
        }

        hasDoneFirstFrameInit = false;
    }


    /// <summary>
    /// Called by world manager when a mech is spawned
    /// </summary>
    public virtual void OnSpawnInitialization() {
        // proagate initialization to the actor component
        mechComponent.OnSpawnInitialization();
    }

    protected virtual void FirstFrameInitialization() {
        hasDoneFirstFrameInit = true;
    }


    /// <summary>
    /// Stops all control activity from this controller
    /// </summary>
    public void SetControllerActive(bool newFrozen) {
        controllerActive = newFrozen;
    }

    // Update is called once per frame
    protected virtual void Update () {
        const float damageFlashTime = 0.2f;

        if(!hasDoneFirstFrameInit) {
            FirstFrameInitialization();
        }
        
        if(damageFlashScreen != null) {
            float timeSinceDamage = Time.time - mechComponent.LastDamageTime;

            if(timeSinceDamage < damageFlashTime) {
                Color c = damageFlashScreen.color;
                c.a = 1.0f - timeSinceDamage / damageFlashTime;
                damageFlashScreen.color = c;
            }
            else {
                Color c = damageFlashScreen.color;
                c.a = 0.0f;
                damageFlashScreen.color = c;
            }
        }
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


    /// <summary>
    /// Called when a new weapon is attached to the mechComponent this controller is controlling
    /// </summary>
    public virtual void NewWeaponAttached(Weapon attached) {

    }

    public virtual void WeaponDetached(Weapon detached) {
        
    }


    public virtual float GetHealthModifier() {
        return 0.0f;
    }

    public virtual float GetHealthRegen() {
        return 0.0f;
    }

    public virtual float GetEnergyModifier() {
        return 0.0f;
    }

    /// <summary>
    /// Forces the controller into a movement state. If the previous state is overloaded, it will be removed from the stack
    /// before the new state is applied
    /// </summary>
    public void GotoNewMoveState(MovementState newMoveState, bool overloadPrevious = false) {
        if(newMoveState == null || newMoveState == currentMoveState) {
            return;
        }

        if(!overloadPrevious) {
            // save the previous state
            moveStateStack.Push(currentMoveState);
        }

        currentMoveState = newMoveState;
    }

    /// <summary>
    /// Goes to previous move state
    /// </summary>
    public void GotoPreviousMoveState() {
        if(moveStateStack.Count > 0) {
            currentMoveState = moveStateStack.Pop();
        }
        else {
            // Default fallback behaviour in case the stack is empty is to just return to the normal state
            currentMoveState = moveState_Normal;
        }
    }



    public void OnDamageHandler(float amount) {
        if(damageFlashScreen != null) {
            // TODO: flash the damage screen

        }
    }

}
