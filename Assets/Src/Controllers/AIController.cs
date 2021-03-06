﻿//#define debug_los

using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class AIController : MechController {

    [SerializeField]
    public float maxDetectRange = 5.0f;

    [SerializeField]
    public float maxAimError = 2.0f;

    [SerializeField]
    public bool doesntShootOffScreen = false;

    [SerializeField]
    public float beserkSpeedModifier = 2.0f;

    [SerializeField]
    public LayerMask blockLOSLayers;

    [SerializeField]
    public Explosion suicideExplosion;

    [SerializeField]
    public AudioClip beserkSound;

    /// <summary>
    /// The mech that this Ai is targetting for shooting etc
    /// </summary>
    [HideInInspector]
    public MechController target;
    
    /// <summary>
    /// State machine that governs this ai's behaviour. The controller handles the exectution of all the 
    /// instructions sent by the behaviour machine, like moving to a location or shooting etc.
    /// </summary>
    protected BehaviourSM stateMachine;

    /// <summary>
    /// A* Pathfinding integration point: used by ai to navigate to target positions
    /// </summary>
    protected Seeker seekerComponent;

    /// <summary>
    /// The location that the AI wants to move to: generally set by the state machine
    /// </summary>
    protected Vector3 moveToTarget;

    /// <summary>
    /// Pathfinding state
    /// </summary>
    private int currentPathWaypoint;
    private Path currentPath;
    private bool waitingForPath;
    private bool doesPathfinding;

    // Pathing interrupt: forces a new path calculation
    private bool wantsNewPath;

    private Explosion normalExplosion;

    /// <summary>
    /// the spawn point this AI was spawned from
    /// </summary>
    private EnemySpawner spawnPoint;
    public EnemySpawner SourceSpawn {
        get { return spawnPoint; }
    }

    [HideInInspector] // The last game time that a target was first seen
    public float lastAquiredTargetTime;
    [HideInInspector] // Is the AI currently seeing a target
    public bool isTrackingTarget;

    // enemy is some cases will have a laser sight of its own
    [HideInInspector]
    public LineRenderer aiLaserSight;

    protected bool allowMovement;
    private bool isBeserking;
    
    protected override void Start() {
        base.Start();
        aiLaserSight = GetComponent<LineRenderer>();
        mechComponent.RegisterDeathListener(OnMechDeath);
        doesPathfinding = true;
    }


    // When created from an object pool
    protected override void OnEnable() {
        base.OnEnable();
        MechComponent.ResetState(true, true);

        if(mechComponent.deathExplosion != normalExplosion && normalExplosion != null) {
            mechComponent.deathExplosion = normalExplosion;
        }
    }


    public void OnMechDeath(Actor victim) {
        if(aiLaserSight != null) {
            aiLaserSight.enabled = false;
        }        
    }

    /// <summary>
    /// Called when this controller is spawned
    /// </summary>
    public override void OnSpawnInitialization() {
        base.OnSpawnInitialization();

        if(stateMachine == null) {
            stateMachine = new BehaviourSM(this);
        }
        else {
            stateMachine.ResetDefault();
        }

        if(seekerComponent == null) {
            seekerComponent = GetComponent<Seeker>();
        }
        
        currentPath = null;
        waitingForPath = false;
        wantsNewPath = true;
        isBeserking = false;
        currentPathWaypoint = 0;
        SetMovetoTarget(transform.position);
    }


    public void SpawnedFromPoint(EnemySpawner source) {
        spawnPoint = source;
    }


    /// <summary>
    /// Instructs the ai to start sensing for a target (does not mean it will actually start chasing and shooting)
    /// </summary>
    public virtual void AiStartSensing() {
        // TODO: better way of assigning target
        target = GameObject.FindObjectOfType<PlayerController>();
        SetCanMove(true);
    }

    // NEcessary to degerigister our delegate
    public void OnDisable() {
        if(seekerComponent != null) {
            seekerComponent.pathCallback -= OnPathComplete;
        }
    }
    

    protected override void Update() {
        const float reachedGoalError = 0.5f;

        base.Update();

        if(!controllerActive) {
            return;
        }

        // Update the state machine, which governs the controlls the current behaviour
        stateMachine.UpdateState();
        
        if(waitingForPath) {
            return;
        }

        if(doesPathfinding) {
            // Start looking for a path
            if(currentPath == null && !waitingForPath || wantsNewPath) {
                FindNewPath();
                return;
            }

            // for animating
            Vector3 moveDirection = Vector3.zero;
            float moveSpeed = 0.0f;

            // Check if its reached the end of the path
            if(currentPathWaypoint >= currentPath.vectorPath.Count) {
                // Reached end of current path
                currentPath = null;
            }
            else if(allowMovement) {
                // Move towards next waypoint
                Vector3 toWaypoint = (currentPath.vectorPath[currentPathWaypoint] - transform.position).normalized;
                movementComponent.Move(toWaypoint * baseMoveSpeed * Time.deltaTime);

                moveDirection = toWaypoint.normalized;
                moveSpeed = 1.0f;

                Debug.DrawLine(transform.position, currentPath.vectorPath[currentPathWaypoint], Color.blue);

                if(Vector3.Distance(transform.position, currentPath.vectorPath[currentPathWaypoint]) < reachedGoalError) {
                    currentPathWaypoint++;
                    return;
                }
            }

            // Set legs to walk direction
            if(legTransform != null && moveDirection.magnitude > 0.0f) {
                legTransform.up = moveDirection;
            }

            // Update the animator
            if(legAnimator != null && legAnimator.isInitialized) {
                legAnimator.SetFloat("MoveSpeed", moveSpeed);
            }
        }

        // when beserking, flash the damage screen!
        if(isBeserking && damageFlashScreen != null) {
            Color currentCol = damageFlashScreen.color;

            const float flashRate = 15.0f;
            float alpha = Mathf.Sin(Time.time * flashRate);
            alpha = (alpha + 1.0f) / 2.0f;

            currentCol.a = alpha;
            damageFlashScreen.color = currentCol;
        }
    }


    /// <summary>
    /// Causes this ai to start looking for a new path
    /// </summary>
    private void FindNewPath() {
        waitingForPath = true;
        wantsNewPath = false;
        seekerComponent.StartPath(transform.position, moveToTarget, OnPathComplete);
    }

    /// <summary>
    /// Force the Ai to stop pathing 
    /// </summary>
    public void InterruptPath() {
        wantsNewPath = true;
        waitingForPath = false;
        currentPath = null;
    }


    /// <summary>
    /// Tells teh AI whether or not it can move (ie follow its path or not)
    /// </summary>
    public void SetCanMove(bool newCanMove) {
        allowMovement = newCanMove;
    }


    public void EnablePathfinding(bool newEnabled) {
        doesPathfinding = newEnabled;
    }

    /// <summary>
    /// Called when a path to the target destination has been created
    /// </summary>
    public void OnPathComplete(Path p) {
        if(!p.error && waitingForPath) {
            currentPath = p;
            currentPathWaypoint = 0;
            waitingForPath = false;
        }
        else {
            Debug.LogError(name + " - PATHING ERROR: " + p.error);
        }
    }


    // Called by photon whip when it steals a weapon belongiong to this AI
    public void WeaponWasStolen() {
        // It has no weapons left!
        if(MechComponent.leftWeapon == null && MechComponent.rightWeapon == null) {
            Debug.Log("[" + name + "] Weapon stolen - Going beserk!");
            InterruptPath();
            stateMachine.GotoNewState(new Behaviour_Beserk(), BehaviourSM.TransitionMode.AbandonCurrent);
            
            isBeserking = true;

            // play a sound to notify player
            if(beserkSound != null) {
                //WorldManager.instance.PlayGlobalSound(beserkSound, 1.0f, 0.5f);
            }
        }

        aiLaserSight.enabled = false;
    }

    /// <summary>
    /// Manually move the ai without pathing
    /// </summary>
    public void MoveDirect(Vector3 direction, float speed) {
        movementComponent.Move(direction * speed * Time.deltaTime);
    }


    /// <summary>
    /// Checks if there is a LOS to the ai's target
    /// </summary>
    public bool HasLOSTarget() {
        if(target == null) {
            return false;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, toTarget.normalized, maxDetectRange, blockLOSLayers);
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

    public bool CheckLOSFrom(Vector3 origin, Vector3 to, float maxRange) {
        Vector3 toTarget = to - origin;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, toTarget.normalized, maxRange, blockLOSLayers);
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
        moveToTarget = moveTo;

        const float minAiSpacing = 1.5f;

        AIController[] ais = FindObjectsOfType<AIController>();
        foreach(AIController ai in ais) {
            Vector3 toAI = (ai.transform.position - transform.position);
            if(toAI.magnitude < minAiSpacing && ai.gameObject != this.gameObject) {
                moveToTarget -= toAI;
            }
        }
    }

    /// <summary>
    /// Overriden to aim at target + some randomization
    /// </summary>
    /// <returns></returns>
    public override Vector3 GetAimLocation() {
        if(target == null) {
            return transform.position + headTransform.up * 50.0f;
        }

        Vector2 randAimOffset = Random.insideUnitCircle;
        float aimError = Random.Range(0.0f, maxAimError);

        if(mechComponent.leftWeapon != null &&
           mechComponent.leftWeapon.GetType() == typeof(Weapon_Laser)) {
            aimError = 0.0f;
        }

        return target.transform.position + (Vector3)randAimOffset.normalized * aimError;
    }

    /// <summary>
    /// AI doesn't consume ammo when shooting, so that they don't awkwardly run out
    /// </summary>
    public override bool UsesAmmo() {
        return false;
    }


    public override float ModifyBaseDamage(float baseDamage, Weapon weaponType) {
        float dmg = base.ModifyBaseDamage(baseDamage, weaponType);

        dmg = dmg * WorldManager.instance.enemyDamageScale;
        return dmg;
    }



    public float GetAttackRange() {
        if(mechComponent.leftWeapon != null) {
            return mechComponent.leftWeapon.maxRange;
        }
        
        // If there is no weapon, try to get up close and personal
        return 0.0f;
    }


    /// <summary>
    /// Causes this mech to explode, damaging neaby mechs
    /// </summary>
    public void CommitSuicide() {
        Debug.Log("[" + name + "] Beserk Suicide! Killing self...");
        
        normalExplosion = mechComponent.deathExplosion;
        mechComponent.deathExplosion = suicideExplosion;

        mechComponent.TakeDamage(99999.9f, this, null);
    }


    /// <summary>
    /// Decides how long the enemy wants to shoot their gun for, in a single burst
    /// </summary>
    public float GetFireBurstLength() {
        int numShotsInBurst = 1;

        if(mechComponent.leftWeapon != null) {
            return mechComponent.leftWeapon.AIBurstLength();
        }

        return 0.2f;
    }


    /// <summary>
    /// Called when the AI sees a target
    /// </summary>
    public void OnAquireTarget() {
        lastAquiredTargetTime = Time.time; 
        isTrackingTarget = true;
    }


    /// <summary>
    /// Called when the AI looses sight of the target, not that they have given up attacking though
    /// </summary>
    public void OnLostSightOfTarget() {
        isTrackingTarget = false;
    }

}
