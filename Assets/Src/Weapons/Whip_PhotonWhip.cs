using UnityEngine;
using System.Collections;

public class Whip_PhotonWhip : Weapon {

    // Should the old weapon get destroyed when a new one is stolen
    private bool destroyOldWeapon = true;

    /// <summary>
    /// The effect system that makes up the actual whip trail
    /// </summary>
    private LineRenderer whipEffect;
    private const int whipSegments = 10;
    private Vector3[] segmentPositions = new Vector3[whipSegments];

    // Whip movement properties
    [SerializeField]
    private float segmentRandomization;

    [SerializeField]
    private float whipEasingSpeed;

    [SerializeField]
    private float travelTime;

    [SerializeField]
    private float snapRange = 1.0f;

    [SerializeField]
    private LayerMask weaponDetectionLayers;

    // the location selected for where the whip will shoot to
    private Vector3 targetLocation;
    private Vector3 lerpedEndPoint;

    // Weapon that the whip is currently targeting: Can be null
    private Weapon targetWeapon;

    // Time that the fire was started
    private float fireTime;

    // Is the whiup expanding or retracting
    private bool isExpanding;



    protected override void Start() {
        base.Start();
        whipEffect = firePoint.GetComponent<LineRenderer>();
        whipEffect.enabled = false;
    }


    protected override void Update() {
        base.Update();


        //if(isFiring) {
        //    float timeSinceFire = Time.time - fireTime;

        //    Vector3 aimPoint = targetWeapon == null? owner.GetAimLocation() : targetWeapon.transform.position;
        //    Vector3 toAimPoint = aimPoint - transform.position;
        //    Vector3 dirToAimPoint = toAimPoint.normalized;
        //    float segmentSpacing = toAimPoint.magnitude / whipSegments;

        //    // start at second point b/c first point is always at origin
        //    segmentPositions[0] = firePoint.position;
        //    whipEffect.SetPosition(0, segmentPositions[0]);

        //    for(int i = 1; i < whipSegments - 1; i++) {
        //        Vector3 currentPos = segmentPositions[i];

        //        // Lerp towrads new position faster at earlier segment to ease towards a roughly straight line
        //        float lerpFactor = (1.0f - i / whipSegments) * Time.deltaTime * whipEasingSpeed;
        //        Vector3 newPos = Vector3.Lerp(currentPos, firePoint.position + dirToAimPoint * i * segmentSpacing, lerpFactor);

        //        // add some randomization to the movement
        //        segmentPositions[i] = newPos +(Vector3)Random.insideUnitCircle * segmentRandomization;

        //        whipEffect.SetPosition(i, segmentPositions[i]);
        //    }

        //    Vector3 endPoint = aimPoint;
        //    whipEffect.SetPosition(whipSegments - 1, endPoint);

        //    // Check for weapons to steal
        //    CheckNearbyWeapons();
        //}

        float timeSinceFire = Time.time - fireTime;
        float fireDistance = (targetLocation - firePoint.position).magnitude;

        // Whip is moving towards the target location
        if(timeSinceFire < travelTime) {
            lerpedEndPoint = Vector3.Lerp(lerpedEndPoint, targetLocation, (1.0f / fireDistance) / travelTime);
        }
        else if(isExpanding) {
            // Switch to retracting
            isExpanding = false;
        }

        if(!isExpanding && timeSinceFire < (travelTime * 2)) {
            lerpedEndPoint = Vector3.Lerp(lerpedEndPoint, firePoint.position, (1.0f / fireDistance) / travelTime);
        }
        
        // Whip sequence is done
        if(timeSinceFire > (travelTime * 2)) {
            EndWhipSequence();
        }

        // start at second point b/c first point is always at origin
        segmentPositions[0] = firePoint.position;
        whipEffect.SetPosition(0, segmentPositions[0]);

        Vector3 toAimPoint = lerpedEndPoint - transform.position;
        Vector3 dirToAimPoint = toAimPoint.normalized;
        float segmentSpacing = toAimPoint.magnitude / whipSegments;

        for(int i = 1; i < whipSegments - 1; i++) {
                Vector3 currentPos = segmentPositions[i];

                // Lerp towrads new position faster at earlier segment to ease towards a roughly straight line
                float lerpFactor = (1.0f - i / whipSegments) * Time.deltaTime * whipEasingSpeed;
                Vector3 newPos = Vector3.Lerp(currentPos, firePoint.position + dirToAimPoint * i * segmentSpacing, lerpFactor);

                // add some randomization to the movement
                segmentPositions[i] = newPos +(Vector3)Random.insideUnitCircle * segmentRandomization;

                whipEffect.SetPosition(i, segmentPositions[i]);
        }

        Vector3 endPoint = lerpedEndPoint;
        whipEffect.SetPosition(whipSegments - 1, endPoint);
        
    }



    public override bool BeginFire() {
        bool beganFire = base.BeginFire();

        fireTime = Time.time;
        isExpanding = true;

        // Before activating the effect, reset all points
        for(int i = 0; i < whipSegments; i++) {
            segmentPositions[i] = firePoint.position;
            whipEffect.SetPosition(i, segmentPositions[i]);
        }

        CheckNearbyWeapons();

        targetLocation = targetWeapon != null? targetWeapon.transform.position : owner.GetAimLocation();
        lerpedEndPoint = firePoint.position;

        whipEffect.enabled = true;
        return beganFire;
    }
    

    public override void EndFire() {
        base.EndFire();

        //whipEffect.enabled = false;
        
        // If there is a weapon snalled, detach and reattach to owner
        if(targetWeapon != null && targetWeapon.owner != null) {

            MechActor targetActor = targetWeapon.owner.MechComponent;
            if(targetActor != null) {

                // Detach the weapon from target
                GameObject detached = targetActor.Detach(targetWeapon.gameObject);

                // Cache teh old weapon
                GameObject oldWeapon = owner.MechComponent.leftWeapon != null? owner.MechComponent.leftWeapon.gameObject : null;

                // and attach it to the owner on the left side
                owner.MechComponent.DoAttachment(MechActor.EAttachSide.Left, detached, Vector3.zero);

                if(destroyOldWeapon && oldWeapon != null) {
                    Destroy(oldWeapon);
                }

                // Also, if it was an AI controller, tell it that its weapon was stolen
                if(targetWeapon.owner.GetType() == typeof(AIController)) {
                    ((AIController)targetWeapon.owner).WeaponWasStolen();
                }
            }
        }
    }

    private void EndWhipSequence() {
        whipEffect.enabled = false;
    }


    private void CheckNearbyWeapons() {
        Vector3 ownerAimPoint = owner.GetAimLocation();
        
        RaycastHit2D[] overlaps = Physics2D.CircleCastAll(ownerAimPoint, snapRange, Vector2.zero, 0.0f, weaponDetectionLayers);
        for(int i = 0; i < overlaps.Length; i++) {
            // Was overlapping a weapon
            Weapon overlapWeapon = overlaps[i].collider.gameObject.GetComponent<Weapon>();

            // check that the owner doesnt match the whip's, so taht player doesn;t steal their own weapon
            if(overlapWeapon != null && overlapWeapon.owner != owner) {
                targetWeapon = overlapWeapon;
                return;
            }
        }

        targetWeapon = null;
    }

}
