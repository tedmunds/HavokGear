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

    private bool whipActive;



    protected override void Start() {
        base.Start();
        whipEffect = firePoint.GetComponent<LineRenderer>();
        whipEffect.enabled = false;
    }


    protected override void Update() {
        base.Update();
        
        float timeSinceFire = Time.time - fireTime;
        float fireDistance = (targetLocation - firePoint.position).magnitude;
        
        // Whip is moving towards the target location
        if(timeSinceFire < travelTime) {
            Vector3 lerpTarget = targetWeapon != null? targetWeapon.transform.position : targetLocation;

            lerpedEndPoint = Vector3.Lerp(lerpedEndPoint, lerpTarget, (1.0f / fireDistance) / travelTime);
        }
        else if(isExpanding) {
            // Switch to retracting
            isExpanding = false;

            // Detach the weapon, so it can fly back!
            if(targetWeapon != null && targetWeapon.owner != null) {
                MechActor targetActor = targetWeapon.owner.MechComponent;
                // Break off the weapon to create particles
                GameObject detached = targetActor.Detach(targetWeapon.gameObject, true);
            }
        }

        if(!isExpanding && timeSinceFire < (travelTime * 2)) {
            // The end point is either this weapons fire point or the left weapon, if a weapon was stolen
            Vector3 endLocation = targetWeapon != null? owner.MechComponent.leftAttachPoint.position : firePoint.position;

            lerpedEndPoint = Vector3.Lerp(lerpedEndPoint, endLocation, (1.0f / fireDistance) / travelTime);
            if(targetWeapon != null) {
                targetWeapon.transform.position = lerpedEndPoint;
            }
        }
        
        // Whip sequence is done
        if(timeSinceFire > (travelTime * 2) && whipActive) {
            EndWhipSequence();
        }

        // start at second point b/c first point is always at origin
        segmentPositions[0] = firePoint.position;
        whipEffect.SetPosition(0, segmentPositions[0]);

        // Now place the line segments along the way to the end point, with some randomization
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
        const string intersectLayerName = "Terrain";

        bool beganFire = base.BeginFire();

        fireTime = Time.time;
        isExpanding = true;
        whipActive = true;

        // Before activating the effect, reset all points
        for(int i = 0; i < whipSegments; i++) {
            segmentPositions[i] = firePoint.position;
            whipEffect.SetPosition(i, segmentPositions[i]);
        }

        // Check there there is nothing inthe way of the whip (only check for terrain though)
        Vector3 endPoint = owner.GetAimLocation();
        Vector3 toAimPoint = owner.GetAimLocation() - firePoint.position;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, toAimPoint.normalized, toAimPoint.magnitude);
        if(hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer(intersectLayerName)) {
            endPoint = hit.point;
        }

        // Check for weapon at the end point to snap to
        CheckNearbyWeapons(endPoint);

        targetLocation = targetWeapon != null? targetWeapon.transform.position : endPoint;
        lerpedEndPoint = firePoint.position;
        
        whipEffect.enabled = true;
        return beganFire;
    }
    

    public override void EndFire() {
        base.EndFire();
    }


    private void EndWhipSequence() {
        whipEffect.enabled = false;
        whipActive = false;

        if(targetWeapon != null) {
            // Cache the old weapon
            GameObject oldWeapon = owner.MechComponent.leftWeapon != null ? owner.MechComponent.leftWeapon.gameObject : null;

            // and attach it to the owner on the left side
            owner.MechComponent.DoAttachment(MechActor.EAttachSide.Left, targetWeapon.gameObject, Vector3.zero);

            if(destroyOldWeapon && oldWeapon != null) {
                Destroy(oldWeapon);
            }

            // Also, if it was an AI controller, tell it that its weapon was stolen
            if(targetWeapon.owner.GetType() == typeof(AIController)) {
                ((AIController)targetWeapon.owner).WeaponWasStolen();
            }
        }
    }



    private void CheckNearbyWeapons(Vector3 originPoint) {
        //Vector3 ownerAimPoint = owner.GetAimLocation();
        
        RaycastHit2D[] overlaps = Physics2D.CircleCastAll(originPoint, snapRange, Vector2.zero, 0.0f, weaponDetectionLayers);
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
