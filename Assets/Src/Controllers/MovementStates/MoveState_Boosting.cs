using UnityEngine;
using System.Collections;



public class MoveState_Boosting : MovementState {


    private const float boostMoveSpeed = 26.0f;

    private PlayerController playerController;


    public MoveState_Boosting(MechController controller) 
        : base(controller) {
        
        if(controller.GetType() == typeof(PlayerController)) {
            playerController = (PlayerController)controller;
        }
    }




    public override Vector3 GetMovementVector(Vector2 inputVector) {
        const float minDistance = 1.0f;

        if(playerController == null) {
            return Vector3.zero;
        }

        Vector3 endPoint =  playerController.GetBoostTarget();
        Vector3 toEndpoint = endPoint - controller.transform.position;
        Vector3 moveDirection = toEndpoint.normalized;
        float remainingDist = toEndpoint.magnitude;

        // check if it has reached the target
        if(remainingDist < minDistance) {
            EndMoveState();
            playerController.EndLatchBoost();
            return Vector3.zero;
        }

        return moveDirection * boostMoveSpeed;
    }
}
