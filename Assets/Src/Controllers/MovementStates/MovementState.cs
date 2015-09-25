using UnityEngine;
using System.Collections;

/// <summary>
/// Movement state contains self sustained logic for doing movement
/// </summary>
public abstract class MovementState {

    // The controller whose movement this state is governing
    protected MechController controller;

    public MovementState(MechController controller) {
        this.controller = controller;
    }


    /// <summary>
    /// Calculates the movement vector for this frame: Non-framerate normalized
    /// </summary>
    public abstract Vector3 GetMovementVector(Vector2 inputVector);

}
