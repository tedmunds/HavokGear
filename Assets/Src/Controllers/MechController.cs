using UnityEngine;
using System.Collections;

/// <summary>
/// Base controller class for players and AI entities
/// </summary>
[RequireComponent(typeof(MechActor))]
public class MechController : MonoBehaviour {

    /// <summary>
    /// Transform for the head sprite that rotates towards aim direction
    /// </summary>
    [SerializeField]
    public Transform headTransform;

    [SerializeField]
    public Transform legTransform;
    
    [SerializeField]
    public float baseMoveSpeed = 5.0f;

    [SerializeField]
    public float baseAimRotSpeed = 1.0f;


    protected MovementController2D movementComponent;
    protected MechActor mechComponent;

    /// <summary>
    /// Governs the current movement method
    /// </summary>
    protected MovementState currentMoveState;

    // Movement states that mechs can do
    protected MoveState_Normal moveState_Normal;

    // Use this for initialization
    protected virtual void Start () {
        // Just some initialization and warnings
        movementComponent = GetComponent<MovementController2D>();
        if(movementComponent == null) {
            Debug.LogWarning("MechController: <" + name + "> Does not have a CharacterController2D component, movement will not work!");
        }

        mechComponent = GetComponent<MechActor>();

        if(headTransform == null) {
            Debug.LogWarning("MechController: <" + name + "> Head Transform is not set!");
        }

        // Init the different move states
        moveState_Normal = new MoveState_Normal(this);

        // Default to the normal move state
        currentMoveState = moveState_Normal;
    }

    // Update is called once per frame
    protected virtual void Update () {
	    
	}


    public virtual Vector3 GetAimLocation() {
        return transform.up;
    }



}
