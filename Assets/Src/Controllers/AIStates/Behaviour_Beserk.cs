using UnityEngine;
using System.Collections;

public class Behaviour_Beserk : BehaviourSM.BehaviourState {

    private const float suicideRadius = 1.5f;

    private const float moveSpeedMultiplier = 2.0f;
    private float baseMoveSpeed;

    public override BehaviourSM.StateResponse Update(AIController controller) {
        
        Vector3 targetLoc = controller.target.transform.position;
        bool hasLos = controller.HasLOSTarget();
        if(hasLos) {
            controller.SetMovetoTarget(targetLoc);
        }

        // In range to explode
        if((targetLoc - controller.transform.position).magnitude < suicideRadius) {
            controller.CommitSuicide();
        }

        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }


    public override void EnterState(AIController controller) {
        base.EnterState(controller);

        baseMoveSpeed = controller.baseMoveSpeed;
        controller.baseMoveSpeed = baseMoveSpeed * moveSpeedMultiplier;
    }

    public override void ExitState(AIController controller) {
        base.ExitState(controller);

        controller.baseMoveSpeed = baseMoveSpeed;
    }
}
