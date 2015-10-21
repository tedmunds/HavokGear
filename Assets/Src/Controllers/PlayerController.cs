using UnityEngine;
using System.Collections;


public class PlayerController : MechController {
    
    // TODO: control customization?
    [SerializeField]
    public string verticleInput = "Vertical";

    [SerializeField]
    public string horizontalInput = "Horizontal";

    [SerializeField]
    public string fireMainInput = "Fire1";

    [SerializeField]
    public string fireAuxInput = "Fire2";

    [SerializeField]
    public KeyCode boostInput;

    /// <summary>
    /// Input ramp remaps the engine input value to a custom curve for fine tuned movement input acceleration
    /// </summary>
    [SerializeField]
    public AnimationCurve inputRamp;

    [SerializeField]
    public float boostEnergy;

    [SerializeField]
    public float boostMoveForce;
    
    /// <summary>
    /// Main camera that is tracking player / the scene camera depending on what we decide (rooms or open level)
    /// </summary>
    private Camera playerCamera;
    public Camera PlayerCamera {
        get { return playerCamera; }
        set { playerCamera = value; }
    }

    private UI_PlayerHUD playerHUD;
    public UI_PlayerHUD PlayerHUD {
        get { return playerHUD; }
    }

    /// <summary>
    /// Where the player was last recorded aiming at & the cached direction to that point
    /// </summary>
    private Vector3 currentAimLoc;
    private Vector3 aimDirection;

    private LineRenderer laserSightRenderer;

    /// <summary>
    /// Boost control vars
    /// </summary>
    private float lastBoostTime;
    private float boostLength;
    private bool isBoosting;
    public bool IsBoosting {
        get { return isBoosting; }
    }
    
    protected override void Start () {
        base.Start();

        playerHUD = GetComponent<UI_PlayerHUD>();
        if(playerHUD == null) {
            Debug.LogWarning("Player does not have HUD component!");
        }

        laserSightRenderer = GetComponent<LineRenderer>();
    }


    protected override void Update () {
        base.Update();
        if(!controllerActive) {
            return;
        }

        Vector2 inputVector = GetInputVector();
        
        // Do base character movement, using the state based movement system
        movementComponent.Move(currentMoveState.GetMovementVector(inputVector) * Time.deltaTime);

        // and rotate the head to face the current aim location
        currentAimLoc = GetAimLocation();
        aimDirection = (currentAimLoc - transform.position).normalized;
        aimDirection.z = 0;

        // Interpolate the rotation for a smooth transition, at a max speed
        if(headTransform != null) {
            Vector3 currentFacing = headTransform.up;

            headTransform.up = Vector3.RotateTowards(currentFacing, aimDirection, baseAimRotSpeed * Time.deltaTime, 0.0f);
        }

        // Set leg direction
        if(legTransform != null && inputVector.magnitude > 0.1f) {
            legTransform.transform.up = inputVector.normalized;
        }

        // update leg animation
        if(legAnimator != null) {
            legAnimator.SetFloat("MoveSpeed", inputVector.magnitude);
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

        // TEMP: a simple boost thingy TODO: how should boost actually be implemented
        if(Input.GetKeyDown(boostInput)) {
            StartBoosting(inputVector.normalized);
        }

        // Check if a boost has ended
        if(isBoosting && Time.time - lastBoostTime > boostLength) {
            isBoosting = false;
        }

        // Update the laser sight renderer
        if(laserSightRenderer != null && mechComponent.leftWeapon != null) {
            laserSightRenderer.enabled = true;
            laserSightRenderer.SetPosition(0, mechComponent.leftWeapon.firePoint.position);
            laserSightRenderer.SetPosition(1, currentAimLoc);
        }
        else if(laserSightRenderer != null) {
            laserSightRenderer.enabled = false;
        }
    }
    

    /// <summary>
    /// Gets the current input axis for movement, and remaps them along the input ramp
    /// </summary>
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


    private void StartBoosting(Vector3 direction) {
        float energyUsed = mechComponent.ConsumeEnergy(boostEnergy);

        lastBoostTime = Time.time;

        // If there was not enough energy to do a boost, it does a weakened version
        float boostForce = energyUsed / boostEnergy;
        mechComponent.AddForce(direction, boostForce * boostMoveForce);
        isBoosting = true;
        
        // calc the time it will take to return to normal speed after boosting
        boostLength = movementComponent.PhysicsSpeed * movementComponent.PhysicsDecel;
        boostLength = boostLength * Time.deltaTime;
    }


    
    /// <summary>
    /// Aim location along the 0 Z plane (based on mouse poition). 
    /// NOTE: If we want to add controller suppor, this will need an alt method with just aim direction
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


    public override float ModifyBaseDamage(float baseDamage, Weapon weaponType) {
        float baseBonus = base.ModifyBaseDamage(baseDamage, weaponType);

        // TODO: Upgrade system will link in here

        return baseBonus;
    }


    /// <summary>
    /// Gets the amount of ammo remaining in current left gun
    /// </summary>
    public int GetCurrentAmmo() {
        if(mechComponent.leftWeapon == null) {
            return 0;
        }

        return mechComponent.leftWeapon.currentAmmo;
    }


    /// <summary>
    /// The player has stolen a weapon with the photon weapon
    /// </summary>
    public void SuccessfulWeaponSteal(Whip_PhotonWhip whipUsed) {
        playerHUD.SetWhipRecharge(whipUsed.stealCoolDown);
    }

}
