using UnityEngine;
using System.Collections;

public class Behaviour_Attack : BehaviourSM.BehaviourState {

    // the ideal distance from the target to sit at. TODO: based on the controllers weapons
    private float attackRange = 6.0f;

    // How long in between changing goal poition
    private float minMoveInterval = 0.5f;
    private float maxMoveInterval = 2.0f;

    private float currentMoveInterval;
    private float lastMoveTime;

    // how long after loosing sight of the target should it give up
    private float attentionSpan = 3.0f;

    // The time that the target was last seen
    private float lastSawTargetTime;
    private Vector3 lastKnownTargetPos;

    // Attacking behaviour
    private float maxFireArc = 0.5f;

    public float minBurstInterval = 1.0f;
    public float maxBurstInterval = 2.0f;

    private float lastBurstTime;
    private float currentBurstDelay;
    private float currentBurstLength;
    private bool isBursting;


    public override BehaviourSM.StateResponse Update(AIController controller) {
        Vector3 lookDirection = controller.target != null? controller.target.transform.position - controller.transform.position : controller.transform.forward;
        lookDirection.Normalize();

        // lerp towards the current look direction
        Vector3 currentFacing = controller.headTransform.up;
        controller.headTransform.up = Vector3.RotateTowards(currentFacing, lookDirection, controller.baseAimRotSpeed * Time.deltaTime, 0.0f);
        
        // Update the current weapon
        if(controller.MechComponent != null && controller.MechComponent.leftWeapon != null) {
            controller.MechComponent.leftWeapon.UpdateAIAttackState(controller);
        }

        bool hasLos = controller.HasLOSTarget();
        if(hasLos) {
            lastSawTargetTime = Time.time;
            lastKnownTargetPos = controller.target.transform.position;

            // move to new shooting position at random intervals
            if(Time.time - lastMoveTime > currentMoveInterval) {
                lastMoveTime = Time.time;
                currentMoveInterval = Random.Range(minMoveInterval, maxMoveInterval);

                float engagementRange = controller.GetAttackRange();

                // the ideal location is some distance away from target, slightly random direction offset though
                Vector3 offsetDirection = (lookDirection + (Vector3)Random.insideUnitCircle).normalized;
                Vector3 shootingPosition = controller.target.transform.position - offsetDirection * engagementRange;
                
                controller.SetMovetoTarget(shootingPosition);
            }
        }
        else {
            // If its run out of LOS, move towards the last known position of the target
            if(controller.target != null) {
                controller.SetMovetoTarget(controller.target.transform.position);
            }
        }

        // Handle firing at target
        AttackTarget(controller);

        // TRANSITIONS:
        // give up looking for target, go to previous state
        if(!hasLos && Time.time - lastSawTargetTime > attentionSpan ||controller.target == null) {
            controller.InterruptPath();
            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.PopPrevious);
        }
        
        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }



    /// <summary>
    /// Attempts to attack the current target with the equipped weapon
    /// </summary>
    private void AttackTarget(AIController controller) {
        if(controller.target == null) {
            return;
        }

        // Direction to the target
        Vector3 toTarget = (controller.transform.position - controller.target.transform.position);
        
        if(!isBursting) {
            // Check if enough delay has past to start another burst
            if(Time.time - lastBurstTime > currentBurstDelay && CanShootTarget(controller)) {

                lastBurstTime = Time.time;
                isBursting = true;

                currentBurstLength = controller.GetFireBurstLength();//Random.Range(0.1f, 1.0f);
                currentBurstDelay = currentBurstLength + Random.Range(minBurstInterval, maxBurstInterval);

                StartShooting(controller);
            }
        }
        else {
            // Is in the middle of shooting the weapon
            if(Time.time - lastBurstTime > currentBurstLength) {
                // Cancel the burst
                isBursting = false;
                StopShooting(controller);
            }
            else if(!CanShootTarget(controller)) {
                // if at any point in the burst, the ai cant see target, end the burst
                isBursting = false;
                StopShooting(controller);
            }

        }
    }



    private bool CanShootTarget(AIController controller) {
        // otherwise, check that there are no other mechs in the way
        Vector3 fireOrigin = GetWeaponOrigin(controller);

        // Some enemies do not shoot when they are not visible on screen
        if(controller.doesntShootOffScreen && 
           !WorldManager.instance.ActorOnScreen(controller.MechComponent)) {
            return false;
        }

        Vector3 toTarget = controller.target.transform.position - fireOrigin;
        float arcToTarget = Vector3.Dot(controller.headTransform.up, toTarget.normalized);

        // constrain the shooting to within an arc
        if(arcToTarget < maxFireArc || toTarget.magnitude > controller.GetAttackRange()) {
            return false;
        }

        if(!controller.CheckLOSFrom(fireOrigin, controller.target.transform.position, 100.0f)) {
            controller.OnLostSightOfTarget();
            return false;
        }
        else if(!controller.isTrackingTarget) {
            controller.OnAquireTarget();
        }

        // allow the weapon to decide if it can shoot yet, or if it is doing something
        if(controller.MechComponent != null && controller.MechComponent.leftWeapon != null &&
           !controller.MechComponent.leftWeapon.AI_AllowFire(controller)) {
               return false;
        }

        return true;
    }


    private Vector3 GetWeaponOrigin(AIController controller) {
        if(controller.MechComponent.leftWeapon != null) {
            return controller.MechComponent.leftWeapon.firePoint.position;
        }
        else if(controller.MechComponent.rightWeapon != null) { 
            return controller.MechComponent.rightWeapon.firePoint.position;
        }
        else {
            return controller.transform.position;
        }
    }


    /// <summary>
    /// Begin firing any weapon the mech has active
    /// </summary>
    private void StartShooting(AIController controller) {
        if(controller.MechComponent.leftWeapon != null) {
            controller.MechComponent.leftWeapon.BeginFire();
        }

        if(controller.MechComponent.rightWeapon != null) {
            controller.MechComponent.rightWeapon.BeginFire();
        }
    }

    /// <summary>
    /// Likewise, stop the shooting of any weapons
    /// </summary>
    private void StopShooting(AIController controller) {
        if(controller.MechComponent.leftWeapon != null) {
            controller.MechComponent.leftWeapon.EndFire();
        }

        if(controller.MechComponent.rightWeapon != null) {
            controller.MechComponent.rightWeapon.EndFire();
        }
    }

}
