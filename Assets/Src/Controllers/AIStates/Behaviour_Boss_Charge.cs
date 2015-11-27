using UnityEngine;
using System.Collections;

public class Behaviour_Boss_Charge : BehaviourSM.BehaviourState {

    public float damageRadius = 1.0f;


    public override void EnterState(AIController controller) {
        // Charge directly at player, no pathing
        controller.InterruptPath();
        controller.EnablePathfinding(false);
    }

    public override void ExitState(AIController controller) {
        controller.EnablePathfinding(true);
    }


    public override BehaviourSM.StateResponse Update(AIController controller) {
        ChargeBossController bossController = (ChargeBossController)controller;

        Vector3 lookDirection = controller.target != null ? controller.target.transform.position - controller.transform.position : controller.transform.forward;
        lookDirection.z = 0.0f;
        lookDirection.Normalize();

        Vector3 currentFacing = controller.headTransform.up;

        float turnRateMultiplier = (controller.target.transform.position - controller.transform.position).magnitude / bossController.chargeTurnRateFalloffDist;
        turnRateMultiplier = Mathf.Min(turnRateMultiplier, 1.0f);

        Vector3 chargeDirection = Vector3.RotateTowards(currentFacing, lookDirection, controller.baseAimRotSpeed * Time.deltaTime * turnRateMultiplier, 0.0f);
        chargeDirection.z = 0.0f;

        controller.headTransform.up = chargeDirection;

        // Move direct so that it doesnt path
        controller.MoveDirect(chargeDirection, controller.baseMoveSpeed * bossController.chargeSpeedModifier);

        Vector3 toTarget = controller.target.transform.position - controller.transform.position;
        if(toTarget.magnitude < damageRadius) {
            // Damage the player, go to the stunned state, knock back the player
            controller.target.MovementComponent.ApplyForce(toTarget * bossController.chargeKnockbackForce);
            controller.target.MechComponent.TakeDamage(bossController.chargeHitDamage, controller, null);

            // A bit of a hack, but do a camera shake
            CameraController camera = GameObject.FindObjectOfType<CameraController>();
            if(camera != null) {
                CameraController.CameraShake shakeData = new CameraController.CameraShake(0.2f, 1.0f, 1.0f, 1.0f, false);
                camera.StartCameraShake(ref shakeData, Vector3.up);
            }

            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.AbandonCurrent, new Behaviour_Boss_Stunned());
        }

        // if it collided this frame, that means it hit a wall and should go into stun state
        if(controller.MovementComponent.collidedThisFrame) {
            bossController.OpenWeakSpot();

            // Again, do a smaller shake when the boss hits a wall
            CameraController camera = GameObject.FindObjectOfType<CameraController>();
            if(camera != null) {
                CameraController.CameraShake shakeData = new CameraController.CameraShake(0.3f, 0.3f, 5.0f, 1.0f, false);
                camera.StartCameraShake(ref shakeData, Vector3.up);
            }

            return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.AbandonCurrent, new Behaviour_Boss_Stunned());
        }

        return new BehaviourSM.StateResponse(BehaviourSM.TransitionMode.NoChange);
    }
	
}
