using UnityEngine;
using System.Collections;

public class Behaviour_Boss_Stunned : BehaviourSM.BehaviourState {

    public float stunLength = 3.0f;


    private float enteredStunTime;

    public override void EnterState(AIController controller) {
        // no pathing
        controller.InterruptPath();
        controller.EnablePathfinding(false);
        enteredStunTime = Time.time;
    }

    public override void ExitState(AIController controller) {
        controller.EnablePathfinding(true);

        ChargeBossController bossController = (ChargeBossController)controller;
        if(bossController.isWeakend) {
            bossController.CloseWeakSpot();
        }
    }




    public override BehaviourSM.StateResponse Update(AIController controller) {
        float elapsedStun = Time.time - enteredStunTime;

        if(elapsedStun > stunLength) {
            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.AbandonCurrent, new Behaviour_Boss_TelegrapthCharge());
        }

        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }

}
