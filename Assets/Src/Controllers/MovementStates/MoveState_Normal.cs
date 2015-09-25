using UnityEngine;
using System.Collections;

public class MoveState_Normal : MovementState {

    public MoveState_Normal(MechController controller) 
        : base(controller) {
        
    }

    public override Vector3 GetMovementVector(Vector2 inputVector) {
        return inputVector * controller.baseMoveSpeed;
    }
}
