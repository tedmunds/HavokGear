//#define debug_los

using UnityEngine;
using System.Collections;

public class AIController : MechController {

    [SerializeField]
    public float maxDetectRange = 5.0f;

    /// <summary>
    /// The mech that this Ai is targetting for shooting etc
    /// </summary>
    [HideInInspector]
    public MechController target;
    
    /// <summary>
    /// State machine that governs this ai's behaviour. The controller handles the exectution of all the 
    /// instructions sent by the behaviour machine, like moving to a location or shooting etc.
    /// </summary>
    private BehaviourSM stateMachine;


    private Vector3 moveToTarget;
    private bool isMovingToTarget;

    // Use this for initialization
    protected override void Start() {
        base.Start();

        // TODO: move to spawn init when I add ai spawning
        stateMachine = new BehaviourSM(this);
    }


    public override void OnSpawnInitialization() {
        base.OnSpawnInitialization();
    }


    /// <summary>
    /// Instructs the ai to start sensing for a target (does not mean it will actually start chasing and shooting)
    /// </summary>
    public void AiStartSensing() {
        // TODO: better way of assigning target
        target = GameObject.FindObjectOfType<PlayerController>();
        Debug.Log(name + " <AI Entity> Has started sensing! target = " + target.name);
    }


    // Update is called once per frame
    protected override void Update() {
        base.Update();

        // Update the state machine, which governs the controlls the current behaviour
        stateMachine.UpdateState();

        if(isMovingToTarget) {
            const float reachedGoalError = 0.5f;

            // Move at the current target
            Vector3 moveDirection = (moveToTarget - transform.position).normalized;
            movementComponent.Move(moveDirection * baseMoveSpeed * Time.deltaTime);

            // if its withint he error range, consider it at the goal target
            float remainingDist = (transform.position - moveToTarget).sqrMagnitude;
            if(remainingDist < reachedGoalError) {
                isMovingToTarget = false;
            }
        }
    }


    // Called by photon whip when it steals a weapon belongiong to this AI
    public void WeaponWasStolen() {
        // It has no weapons left!
        if(MechComponent.leftWeapon == null && MechComponent.rightWeapon == null) {
            // TODO: flee behaviour
        }
    }



    /// <summary>
    /// Checks if there is a LOS to the ai's target
    /// </summary>
    public bool HasLOSTarget() {
        if(target == null) {
            return false;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, toTarget.normalized, maxDetectRange);
        for(int i = 0; i < hits.Length; i++) {
            // ignore self
            if(hits[i].collider != null && hits[i].collider.gameObject == this.gameObject) {
                continue;
            }

            // also ignore if its a child of self
            if(hits[i].collider.transform.parent != null && hits[i].collider.transform.parent.gameObject == this.gameObject) {
                continue;
            }

            // ignore trigger obviously
            if(hits[i].collider.isTrigger) {
                continue;
            }

            if(hits[i].collider.gameObject.tag == gameObject.tag) {
                continue;
            }

            // the next thing that is hit, must be the target, sinse hits are sorted nearest first
            if(hits[i].collider.gameObject == target.gameObject ||
               (hits[i].collider.transform.parent != null && hits[i].collider.transform.parent.gameObject == target.gameObject)) {
#if debug_los
                Debug.DrawLine(transform.position, target.transform.position, Color.blue);
#endif
                return true;
            }
            else {
                break;
            }
        }
#if debug_los
        Debug.DrawLine(transform.position, target.transform.position, Color.red);
#endif
        return false;
    }


    /// <summary>
    /// Tells the ai to start moving to the input location. The location will be cached until it is overriden or abandoned.
    /// NOTE: Meant to be able to have some PathFinding injected here
    /// </summary>
    public void SetMovetoTarget(Vector3 moveTo) {
        moveTo.z = 0;

        // Only try to move there if it is actually in a different location to avoid little jerky start-stop movements
        if((moveTo - transform.position).magnitude > 0.5f) {
            moveToTarget = moveTo;
            isMovingToTarget = true;
        }
    }

    /// <summary>
    /// Stopos the AI trying to move to its current goal position
    /// </summary>
    public void AbandonMovetoTarget() {
        isMovingToTarget = false;
    }
}
