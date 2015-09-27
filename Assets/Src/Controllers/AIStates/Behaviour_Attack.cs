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


    public override BehaviourSM.StateResponse Update(AIController controller) {
        Vector3 lookDirection = controller.target.transform.position - controller.transform.position;
        lookDirection.Normalize();

        // lerp towards the current look direction
        Vector3 currentFacing = controller.headTransform.up;
        controller.headTransform.up = Vector3.RotateTowards(currentFacing, lookDirection, controller.baseAimRotSpeed * Time.deltaTime, 0.0f);
        

        bool hasLos = controller.HasLOSTarget();
        if(hasLos) {
            lastSawTargetTime = Time.time;
            lastKnownTargetPos = controller.target.transform.position;

            // move to new shooting position at random intervals
            if(Time.time - lastMoveTime > currentMoveInterval) {
                lastMoveTime = Time.time;
                currentMoveInterval = Random.Range(minMoveInterval, maxMoveInterval);

                // the ideal location is some distance away from target, slightly random direction offset though
                Vector3 offsetDirection = (lookDirection + (Vector3)Random.insideUnitCircle).normalized;
                Vector3 shootingPosition = controller.target.transform.position - offsetDirection * attackRange;
                controller.SetMovetoTarget(shootingPosition);
            }
        }
        else {
            // If its run out of LOS, move towards the last known position of the target
            controller.SetMovetoTarget(controller.target.transform.position);
        }
        
        // TRANSITIONS:
        // give up looking for target, go to previous state
        if(!hasLos && Time.time - lastSawTargetTime > attentionSpan) {
            controller.AbandonMovetoTarget();
            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.PopPrevious);
        }
        
        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }

}
