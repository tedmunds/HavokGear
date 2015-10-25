using UnityEngine;
using System.Collections;

public class Behaviour_Beserk : BehaviourSM.BehaviourState {

    private const float suicideRadius = 4.0f;

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
}
