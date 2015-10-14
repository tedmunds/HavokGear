using UnityEngine;
using System.Collections;

/// <summary>
/// In the idle state, the mech will look around in random direction at semi random intervals
/// </summary>
public class Behaviour_Idle : BehaviourSM.BehaviourState {

    const float minWaitInterval = 1.0f;
    const float maxWaitInterval = 3.0f;
    
    private Vector3 lookDirection;

    // Last time that the mech changed look direction
    private float lastLooktime;

    // time until the next look direction is chosen
    private float thisLookInterval;


    private float lookSpeed = 2.0f;

    public override BehaviourSM.StateResponse Update(AIController controller) {

        if(Time.time - lastLooktime > thisLookInterval) {
            lookDirection = Random.insideUnitCircle;
            thisLookInterval = Random.Range(minWaitInterval, maxWaitInterval);
            lastLooktime = Time.time;
        }

        controller.SetMovetoTarget(controller.transform.position);

        // lerp towards teh current look direction
        Vector3 currentFacing = controller.headTransform.up;
        controller.headTransform.up = Vector3.RotateTowards(currentFacing, lookDirection, lookSpeed * Time.deltaTime, 0.0f);

        // TRANSITIONS:
        if(controller.HasLOSTarget()) {
            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.PushCurrent, new Behaviour_Attack());
        }


        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }
    
}
