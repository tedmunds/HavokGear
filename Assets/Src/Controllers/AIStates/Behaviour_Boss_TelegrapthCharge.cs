using UnityEngine;
using System.Collections;

public class Behaviour_Boss_TelegrapthCharge : BehaviourSM.BehaviourState {

    //public float telegraphLength = 3.0f;
    public float shakeMag = 0.1f;

    private float startedTelegraph;



    public override void EnterState(AIController controller) {
        // no pathing
        controller.InterruptPath();
        controller.EnablePathfinding(false);
        startedTelegraph = Time.time;

        ChargeBossController bossController = (ChargeBossController)controller;
        bossController.BeginTelegraphAttack();
    }

    public override void ExitState(AIController controller) {
        controller.EnablePathfinding(true);
        controller.headTransform.localPosition = Vector3.zero;
    }



    public override BehaviourSM.StateResponse Update(AIController controller) {
        ChargeBossController bossController = (ChargeBossController)controller;

        float elapsedStun = Time.time - startedTelegraph;

        Vector3 lookDirection = controller.target != null ? controller.target.transform.position - controller.transform.position : controller.transform.forward;
        lookDirection.Normalize();

        Vector3 currentFacing = controller.headTransform.up;
        controller.headTransform.up = Vector3.RotateTowards(currentFacing, lookDirection, controller.baseAimRotSpeed * Time.deltaTime, 0.0f);

        controller.headTransform.localPosition = Random.insideUnitCircle * shakeMag;


        if(elapsedStun > bossController.telegraphLength) {
            // TODO: go to telegraph state
            //ChargeBossController bossController = (ChargeBossController)controller;
            BehaviourSM.BehaviourState nextAttack = bossController.GetNextAttackState();

            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.AbandonCurrent, nextAttack);
        }

        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }
}
