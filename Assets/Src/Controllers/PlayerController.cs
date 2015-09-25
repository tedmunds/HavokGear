using UnityEngine;
using System.Collections;


public class PlayerController : MechController {
    
    [SerializeField]
    public string verticleInput = "Vertical";

    [SerializeField]
    public string horizontalInput = "Horizontal";

    [SerializeField]
    public string fireMainInput = "Fire1";

    [SerializeField]
    public string fireAuxInput = "Fire2";

    [SerializeField]
    public AnimationCurve inputRamp;
    

    public Weapon testWeapon;
    
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



    protected override void Start () {
        base.Start();

        mechComponent.DoAttachment(MechActor.EAttachSide.Left, testWeapon.gameObject, Vector3.zero);
    }


    protected override void Update () {
        base.Update();

        Vector2 inputVector = GetInputVector();
        
        // Do base character movement, using the state based movement system
        movementComponent.Move(currentMoveState.GetMovementVector(inputVector) * Time.deltaTime);

        // and rotate the head to face the current aim location
        currentAimLoc = GetAimLocation();
        aimDirection = (currentAimLoc - transform.position).normalized;

        // Interpolate the rotation for a smooth transition, at a max speed
        if(headTransform != null) {
            Vector3 currentFacing = headTransform.up;
            
            headTransform.up = Vector3.RotateTowards(currentFacing, aimDirection, baseAimRotSpeed * Time.deltaTime, 0.0f);
        }

        // Do the fireing input checks
        if(Input.GetButtonDown(fireMainInput)) {
            if(mechComponent.leftWeapon != null) {
                mechComponent.leftWeapon.BeginFire();
            }
        }

        if(Input.GetButtonUp(fireMainInput)) {
            if(mechComponent.leftWeapon != null) {
                mechComponent.leftWeapon.EndFire();
            }
        }

        if(Input.GetButtonDown(fireAuxInput)) {
            if(mechComponent.rightWeapon != null) {
                mechComponent.rightWeapon.BeginFire();
            }
        }

        if(Input.GetButtonUp(fireAuxInput)) {
            if(mechComponent.rightWeapon != null) {
                mechComponent.rightWeapon.EndFire();
            }
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
    public override Vector3 GetAimLocation() {
        if(playerCamera == null) {
            return Vector3.up;
        }

        Vector2 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(mouseScreenPos);

        mouseWorldPos.z = 0.0f;

        return mouseWorldPos;
    }




}
