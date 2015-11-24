using UnityEngine;
using System.Collections;


public class PlayerController : MechController {

    private const bool ALWAYS_DOES_BOOST = false;

    // TODO: control customization?
    [SerializeField]
    public string verticleInput = "Vertical";

    [SerializeField]
    public string horizontalInput = "Horizontal";

    [SerializeField]
    public string fireMainInput = "Fire1";

    [SerializeField]
    public string fireAuxInput = "Fire2";

    /// <summary>
    /// Input ramp remaps the engine input value to a custom curve for fine tuned movement input acceleration
    /// </summary>
    [SerializeField]
    public AnimationCurve inputRamp;

    [SerializeField]
    public KeyCode boostInput;

    [SerializeField]
    public KeyCode swapWeaponInput;

    [SerializeField]
    public float boostEnergy;

    [SerializeField]
    public float boostMoveForce;

    [SerializeField]
    public float weaponSwapDelay;

    [SerializeField]
    public AudioClip boostSound;

    [SerializeField]
    public AudioClip weaponSwapSound;

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
    /// The players whip attachment reference
    /// </summary>
    private Whip_PhotonWhip whipAttachment;

    private UpgradeManager upgradeManager;
    private PlayerState myState;

    /// <summary>
    /// Boost control vars
    /// </summary>
    private float lastBoostTime;
    private float boostLength;
    private bool isBoosting;
    public bool IsBoosting {
        get { return isBoosting; }
    }

    // Players secondary weapon 
    private Weapon backupWeapon;
    private float lastWeaponSwapTime;

    // Ref to the weapon that has been detached
    private Weapon detachedWeapon;

    
    protected override void Start () {
        base.Start();

        // Grab some ref to the instances of these global systems
        upgradeManager = UpgradeManager.instance;
        myState = PlayerState.instance;

        playerHUD = GetComponent<UI_PlayerHUD>();
        if(playerHUD == null) {
            Debug.LogWarning("Player does not have HUD component!");
        }

        laserSightRenderer = GetComponent<LineRenderer>();
        laserSightRenderer.enabled = false;

        mechComponent.RegisterDeathListener(OnDied);
    }


    protected override void OnEnable() {
        base.OnEnable();

        if(whipAttachment != null) {
            whipAttachment.ResetWhipEffect();
            if(whipAttachment.ValidLatchBoost) {
                whipAttachment.DetachFromSurface();
            }
        }

        if(laserSightRenderer != null) {
            laserSightRenderer.enabled = false;
        }
    }

    protected override void FirstFrameInitialization() {
        base.FirstFrameInitialization();

        mechComponent.AddHealth(GetHealthModifier(), gameObject);
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
                Debug.Log(mechComponent.leftWeapon.name + " fired by player");
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

        // Pause the game
        if(Input.GetKeyDown(KeyCode.Escape)) {
            WorldManager.instance.PauseGame(true);
        }

        // check for mouse scroll wheel or the swap button, for now at least
        if(Mathf.Abs(Input.mouseScrollDelta.y) > 0.0f || Input.GetKeyDown(swapWeaponInput)) {
            SwapWeapon();
        }

        // TEMP: a simple boost thingy TODO: how should boost actually be implemented
        if(Input.GetKeyDown(boostInput)) {
            StartBoosting(inputVector.normalized);
        }

        // On release of boost key
        if(Input.GetKeyUp(boostInput) && isBoosting) {
            EndBoosting();
        }

        // exit button
#if !UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F12)) {
            Application.Quit();
        }
#endif

        // Check if a boost has ended (only for a conventional non-latch boost, otherwise its left to the movement state)
        //if(isBoosting && Time.time - lastBoostTime > boostLength 
        //    && currentMoveState == moveState_Normal) {
        //    isBoosting = false;
        //}

        // Update the laser sight renderer
        if(laserSightRenderer != null && laserSightRenderer.enabled && mechComponent.leftWeapon != null) {
            laserSightRenderer.SetPosition(0, mechComponent.leftWeapon.firePoint.position);

            Vector3 toAimPoint = currentAimLoc - mechComponent.leftWeapon.firePoint.position;

            // Get the endpoint for the laser sight
            RaycastHit2D hit = Physics2D.Raycast(mechComponent.leftWeapon.firePoint.position, toAimPoint.normalized, toAimPoint.magnitude, 
                                                 movementComponent.platformMask); // just use the platform mask as an approx. for los blockers

            laserSightRenderer.SetPosition(1, hit? (Vector3)hit.point : currentAimLoc);
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
        if(isBoosting) {
            return;
        }

        lastBoostTime = Time.time;
        isBoosting = true;

        // Check if it should do a latch boost and boost towards the whips attachment point 
        if(whipAttachment != null && whipAttachment.ValidLatchBoost) {
            float energyUsed = mechComponent.ConsumeEnergy(boostEnergy);
            GotoNewMoveState(new MoveState_Boosting(this));

            WorldManager.instance.PlayGlobalSound(boostSound);
        }
        else if(ALWAYS_DOES_BOOST) {
            float energyUsed = mechComponent.ConsumeEnergy(boostEnergy);
            // default boost move is just a linear dash 
            // If there was not enough energy to do a boost, it does a weakened version
            float boostForce = energyUsed / boostEnergy;
            mechComponent.AddForce(direction, boostForce * boostMoveForce);
            

            // calc the time it will take to return to normal speed after boosting
            boostLength = movementComponent.PhysicsSpeed * movementComponent.PhysicsDecel;
            boostLength = boostLength * Time.deltaTime;
        }
    }


    // Leave boost move state
    private void EndBoosting() {
        if(whipAttachment != null && whipAttachment.ValidLatchBoost) {
            whipAttachment.DetachFromSurface();
            GotoPreviousMoveState();
        }

        isBoosting = false;
    }


    public Vector3 GetBoostTarget() {
        if(whipAttachment == null) {
            return transform.position;
        }

        return whipAttachment.LatchLocation;
    }

    public void EndLatchBoost() {
        isBoosting = false;
        if(whipAttachment != null) {
            // If there is a surface object that the whip is attached to, check if it is damageable
            if(whipAttachment.latchedToSurface != null) {
                GameObject surf = whipAttachment.latchedToSurface;
                Actor damageable = surf.GetComponent<Actor>();

                // There is a damageable object that was atached to, so kill it
                if(damageable != null) {
                    damageable.TakeDamage(99999.9f, this, whipAttachment);
                }
            }

            whipAttachment.DetachFromSurface();
        }
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
        detachedWeapon = mechComponent.leftWeapon;
        playerHUD.SetWhipRecharge(whipUsed.stealCoolDown);

        // Check for life steal upgrade
        float lifeStealAmount = GetLifeSteal();
        if(lifeStealAmount > 0.0f) {
            mechComponent.AddHealth(lifeStealAmount, gameObject);
        }
    }



    public override void NewWeaponAttached(Weapon attached) {
        base.NewWeaponAttached(attached);

        attached.ResetRefireDelay();

        // Cache ref to photon whip when it is attached (on spawn usually)
        if(attached != null && attached.GetType() == typeof(Whip_PhotonWhip)) {
            whipAttachment = (Whip_PhotonWhip)attached;
            Debug.Log("Player has photon whip attachment");
        }

        // check if the weapon wants a laser sight or not
        if(attached != null && attached.GetType() != typeof(Whip_PhotonWhip)) {
            if(attached.hasLaserSight && laserSightRenderer != null) {
                laserSightRenderer.enabled = true;
            }
            else if(laserSightRenderer != null) {
                laserSightRenderer.enabled = false;
            }
        }

        // update the hud images
        if(playerHUD != null && playerHUD.equippedWeaponElement != null) {
            Sprite weaponSprite = attached.GetWeaponSprite(); ;
            playerHUD.equippedWeaponElement.sprite = weaponSprite;
            playerHUD.equippedWeaponElement.enabled = weaponSprite != null? true : false;
        }
        if(playerHUD != null && playerHUD.damageTypeElement != null) {
            playerHUD.SetDamageTypeDisplay(attached.damageTypeList);
        }

        // if there was a detached eapon, to attach this one, then swap it into the seconrady slot if there is nothing there
        if(backupWeapon == null && detachedWeapon != null) {
            // need to bring the detatched weapon back fromn the dead
            detachedWeapon.transform.SetParent(mechComponent.leftAttachPoint);
            detachedWeapon.gameObject.SetActive(true);
            detachedWeapon.GetRenderer().enabled = false;
            
            // cahche the weapon
            backupWeapon = detachedWeapon;
            
            // and update hud
            playerHUD.secondarydWeaponElement.enabled = true;
            playerHUD.secondarydWeaponElement.sprite = backupWeapon.GetWeaponSprite();
        }
        
        detachedWeapon = null;
    }


    public override void WeaponDetached(Weapon detached) {
        base.WeaponDetached(detached);

        if(detached != null) {
            detached.gameObject.SetActive(false);
        }

        if(laserSightRenderer != null) {
            laserSightRenderer.enabled = false;
        }
        
        // update the hud images
        if(playerHUD != null && playerHUD.equippedWeaponElement != null) {
            playerHUD.equippedWeaponElement.enabled = false;
        }
        if(playerHUD != null && playerHUD.damageTypeElement != null) {
            playerHUD.SetDamageTypeDisplay(null);
        }
    }



    public void OnDied(Actor victim) {
        playerHUD.equippedWeaponElement.enabled = false;

        if(mechComponent.leftWeapon != null) {
            GameObject detached = mechComponent.Detach(mechComponent.leftWeapon.gameObject);
            if(detached != null) {
                detached.gameObject.SetActive(false);
            }
        }
        
        if(backupWeapon != null) {
            backupWeapon.gameObject.SetActive(false);
        }

        backupWeapon = null;

        // reset the whip cooldown as well
        if(whipAttachment != null) {
            whipAttachment.ResetRefireDelay();
        }

        detachedWeapon = null;

        Debug.Log("Player died, resetting weapons");
    }


    public void SwapWeapon() {
        if(Time.time - lastWeaponSwapTime > weaponSwapDelay) {
            // swap the currentyl equiped (null or not) with the backup
            Weapon currentMain = mechComponent.leftWeapon;
            if(currentMain != null) {
                currentMain.EndFire();
                currentMain.GetRenderer().enabled = false;
            }
            
            if(backupWeapon != null) {
                backupWeapon.GetRenderer().enabled = true;
            }

            if(laserSightRenderer != null) {
                laserSightRenderer.enabled = false;
            }
            
            mechComponent.leftWeapon = backupWeapon;
            backupWeapon = currentMain;
            
            // set backup image
            if(backupWeapon != null && playerHUD != null) {
                playerHUD.secondarydWeaponElement.enabled = true;
                playerHUD.secondarydWeaponElement.sprite = backupWeapon.GetWeaponSprite();
            }
            else if(playerHUD != null) {
                playerHUD.secondarydWeaponElement.enabled = false;
            }

            // set laser sight and main image
            if(mechComponent.leftWeapon != null) {
                NewWeaponAttached(mechComponent.leftWeapon);
            }
            else {
                playerHUD.equippedWeaponElement.enabled = false;
            }

            WorldManager.instance.PlayGlobalSound(weaponSwapSound, 1.0f, 0.5f);

            lastWeaponSwapTime = Time.time;
            Debug.Log("Weapon swapped to " + (mechComponent.leftWeapon != null? mechComponent.leftWeapon.name : "null"));
        }
    }





    /// <summary>
    /// Upgrade access points -------------------------------------------
    /// </summary>

    public override float GetHealthModifier() {
        // TODO: cache this level, since it wont change over the game?
        float bonusHealth = 0.0f;
        int[] equippedLevels = myState.GetEquippedLevelsFor(typeof(Upgrade_Health).Name);
        for(int i = 0; i < equippedLevels.Length; i++) {
            bonusHealth += upgradeManager.upgrade_Health.GetBonusHealth(equippedLevels[i]);
        }

        return bonusHealth;
    }

    public override float GetHealthRegen() {
        float regen = 0.0f;
        int[] equippedLevels = myState.GetEquippedLevelsFor(typeof(Upgrade_HealthRegen).Name);
        for(int i = 0; i < equippedLevels.Length; i++) {
            regen += upgradeManager.upgrade_HealthRegen.GetRegenPerSecond(equippedLevels[i]);
        }

        return regen;
    }


    public override float GetEnergyModifier() {
        float bonusEnergy = 0.0f;

        int[] equippedLevels = myState.GetEquippedLevelsFor(typeof(Upgrade_Energy).Name);
        for(int i = 0; i < equippedLevels.Length; i++) {
            bonusEnergy += upgradeManager.upgrade_Energy.GetBonusEnergy(equippedLevels[i]);
        }

        return bonusEnergy;
    }

    public float GetLifeSteal() {
        float lifeSteal = 0.0f;

        int[] equippedLevels = myState.GetEquippedLevelsFor(typeof(Upgrade_LifeSteal).Name);
        for(int i = 0; i < equippedLevels.Length; i++) {
            lifeSteal += upgradeManager.upgrade_LifeSteal.GetLifeStealAmmount(equippedLevels[i]);
        }

        Debug.Log("Life steal amount = " + lifeSteal + " :: num equipped = " + equippedLevels.Length);

        return lifeSteal;
    }


}
