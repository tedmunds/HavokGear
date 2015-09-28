using UnityEngine;
using System.Collections;

public class Whip_PhotonWhip : Weapon {

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

    protected override void Start() {
        base.Start();
        whipEffect = firePoint.GetComponent<LineRenderer>();
        whipEffect.enabled = false;
    }


    protected override void Update() {
        base.Update();

        
        if(isFiring) {
            Vector3 aimPoint = owner.GetAimLocation();
            Vector3 toAimPoint = aimPoint - transform.position;
            Vector3 dirToAimPoint = toAimPoint.normalized;
            float segmentSpacing = toAimPoint.magnitude / whipSegments;

            // start at second point b/c first point is always at origin
            segmentPositions[0] = firePoint.position;
            whipEffect.SetPosition(0, segmentPositions[0]);

            for(int i = 1; i < whipSegments - 1; i++) {
                Vector3 currentPos = segmentPositions[i];

                // Lerp towrads new position faster at earlier segment to ease towards a roughly straight line
                float lerpFactor = (1.0f - i / whipSegments) * Time.deltaTime * whipEasingSpeed;
                Vector3 newPos = Vector3.Lerp(currentPos, firePoint.position + dirToAimPoint * i * segmentSpacing, lerpFactor);

                // add some randomization to the movement
                segmentPositions[i] = newPos +(Vector3)Random.insideUnitCircle * segmentRandomization;
                
                whipEffect.SetPosition(i, segmentPositions[i]);
            }

            Vector3 endPoint = aimPoint;
            whipEffect.SetPosition(whipSegments - 1, endPoint);
        }
    }



    public override bool BeginFire() {
        bool beganFire = base.BeginFire();

        // Before activating the effect, reset all points
        for(int i = 0; i < whipSegments; i++) {
            segmentPositions[i] = firePoint.position;
            whipEffect.SetPosition(i, segmentPositions[i]);
        }

        whipEffect.enabled = true;
        return beganFire;
    }
    

    public override void EndFire() {
        base.EndFire();

        whipEffect.enabled = false;
        // TODO: have a "whip back" effect

        // TODO: check for weapons under some small area at the aim location?, and do a weapon steal however that works
    }




    /// <summary>
    /// Whip fire is very special case weapon, it actually tries to steal opponents weapon
    /// </summary>
    //protected override void Fire() {
    //     base.Fire();
    //}

}
