using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {


    /// <summary>
    /// Transform for the head sprite that rotates towards aim direction
    /// </summary>
    [SerializeField]
    public Transform headTransform;

    [SerializeField]
    public Transform legTransform;

    [SerializeField]
    public string verticleInput = "Vertical";

    [SerializeField]
    public string horizontalInput = "Horizontal";

    [SerializeField]
    public AnimationCurve inputRamp;
    
    [SerializeField]
    public float baseMoveSpeed = 5.0f;

    [SerializeField]
    public float baseAimRotSpeed = 1.0f;
    
    /// <summary>
    /// Main camera that is tracking player / scene camera depending on what we decide
    /// </summary>
    private Camera playerCamera;
    public Camera PlayerCamera {
        get { return playerCamera; }
        set { playerCamera = value; }
    }


    /// <summary>
    /// Where the player was last recorded aiming at
    /// </summary>
    private Vector3 currentAimLoc;
    private Vector3 aimDirection;


    private CharacterController2D movementComponent;



    private void Start () {
        // Just some initialization and warnings
        movementComponent = GetComponent<CharacterController2D>();
        if(movementComponent == null) {
            Debug.LogWarning("Player Controller: <" + name + "> Does not have a CharacterController2D component, movement will not work!");
        }

        if(headTransform == null) {
            Debug.LogWarning("Player Controller: <" + name +"> Head Transform is not set!");
        }
    }
	
	
	private void Update () {
        Vector2 inputVector = GetInputVector();

        //transform.position += (Vector3)inputVector * baseMoveSpeed * Time.deltaTime;
        if(movementComponent != null) {
            movementComponent.Move(inputVector * baseMoveSpeed * Time.deltaTime);
        }

        // and rotate the head to face the current aim location
        currentAimLoc = GetAimLocation();
        aimDirection = (currentAimLoc - transform.position).normalized;

        // Interpolate the rotation for a smooth transition, at a max speed
        if(headTransform != null) {
            Vector3 currentFacing = headTransform.up;
            
            headTransform.up = Vector3.RotateTowards(currentFacing, aimDirection, baseAimRotSpeed * Time.deltaTime, 0.0f);
        }
    }



    private Vector2 GetInputVector() {
        float vert_axis = Input.GetAxis(verticleInput);
        float horz_axis = Input.GetAxis(horizontalInput);

        // Alter the input magnitude along the input ramp curve
        Vector2 inputVector = new Vector2();

        inputVector.x = inputRamp.Evaluate(Mathf.Abs(horz_axis)) * Mathf.Sign(horz_axis);
        inputVector.y = inputRamp.Evaluate(Mathf.Abs(vert_axis)) * Mathf.Sign(vert_axis);

        // Only normalize if the magnitude is over 1, since that means diagonal input
        if(inputVector.sqrMagnitude > 1.0f) {
            inputVector.Normalize();
        }

        return inputVector;
    }


    
    /// <summary>
    /// Aim location along the 0 Z plane
    /// </summary>
    public Vector3 GetAimLocation() {
        if(playerCamera == null) {
            return Vector3.up;
        }

        Vector2 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(mouseScreenPos);

        mouseWorldPos.z = 0.0f;

        return mouseWorldPos;
    }




}
