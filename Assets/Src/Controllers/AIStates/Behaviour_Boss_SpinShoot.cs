using UnityEngine;
using System.Collections;

public class Behaviour_Boss_SpinShoot : BehaviourSM.BehaviourState {
        
    private float enteredShootTime;
    private float spinShootLength = 5.0f;
    private float chaseSpeedMult = 0.75f;
    private float minDistToTarget = 5.0f;


    private float lastShootTime;


    public override void EnterState(AIController controller) {
        // no pathing
        controller.InterruptPath();
        controller.EnablePathfinding(false);
        enteredShootTime = Time.time;
    }

    public override void ExitState(AIController controller) {
        controller.EnablePathfinding(true);

        ChargeBossController bossController = (ChargeBossController)controller;
        if(bossController.isWeakend) {
            bossController.CloseWeakSpot();
        }
    }




    public override BehaviourSM.StateResponse Update(AIController controller) {
        ChargeBossController bossController = (ChargeBossController)controller;

        float elapsedStun = Time.time - enteredShootTime;

        Vector3 currentFacing = controller.headTransform.up;
        currentFacing = Vector3.RotateTowards(currentFacing, controller.headTransform.right, bossController.shootingSpinRate * Time.deltaTime, 0.0f);

        controller.headTransform.up = currentFacing;

        // Try it with movement towards player
        Vector3 toTarget = controller.target.transform.position - controller.transform.position;
        if(toTarget.magnitude > minDistToTarget) {
            controller.MoveDirect(toTarget.normalized, controller.baseMoveSpeed * chaseSpeedMult);
        }
        
        // decide if it should shoot a bullet
        if(Time.time - lastShootTime > bossController.shootingFireDelay) {
            bossController.FireBullet();
            lastShootTime = Time.time;
        }

        if(elapsedStun > spinShootLength) {
            // TODO: wait state? just tracks the player for some time
            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.AbandonCurrent, new Behaviour_Boss_TelegrapthCharge());
        }

        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }
}
