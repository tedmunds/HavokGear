/**
 * Original code by prime31 on Github:
 * https://github.com/prime31/CharacterController2D/blob/master/Assets/CharacterController2D/CharacterController2D.cs
 *
 * Modified by Thomas Edmunds on 9/24/2015 for use in HavokGear
 * thomase@sfu.ca
 * ID: 301178089
 *
 * Changes:
 * - altered functionality to suit top down movement over side scroller
 * - simplified code by removing slope handling and groud detection (not needed for top down)
 * 
 * Note: 
 * The reason this script was selected is because of its collision resolution handling in 2D, which is not natively supported in unity
 * It acts as the equivalent to the CharacterController in Unity3D
 */


#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;


[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class MovementController2D : MonoBehaviour {



    public event Action<RaycastHit2D> onControllerCollidedEvent;
    public event Action<Collider2D> onTriggerEnterEvent;
    public event Action<Collider2D> onTriggerStayEvent;
    public event Action<Collider2D> onTriggerExitEvent;



    [SerializeField]
    [Range(0.001f, 0.3f)]
    float _skinWidth = 0.02f;

    private float radius;

    /// <summary>
    /// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
    /// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
    /// </summary>
    public float skinWidth {
        get { return _skinWidth; }
        set {
            _skinWidth = value;
        }
    }


    /// <summary>
    /// mask with all layers that the player should interact with
    /// </summary>
    public LayerMask platformMask = 0;

    /// <summary>
    /// mask with all layers that trigger events should fire when intersected
    /// </summary>
    public LayerMask triggerMask = 0;

    /// <summary>
    /// Simple physics simulation properties
    /// </summary>
    [SerializeField]
    private float physicsMass = 1.0f;

    [SerializeField]
    private float physicsFriction = 0.3f;
    public float PhysicsDecel {
        get { return physicsFriction;  }
    }

    private Vector3 physicsVel;
    public float PhysicsSpeed {
        get { return physicsVel.magnitude; }
    }


    [HideInInspector]
    [NonSerialized]
    public new Transform transform;

    [HideInInspector]
    [NonSerialized]
    public Rigidbody2D rigidBody2D;

    [HideInInspector]
    [NonSerialized]
    public Vector3 velocity;

    private const float kSkinWidthFloatFudgeFactor = 0.001f;
    
    RaycastHit2D[] hits2D = new RaycastHit2D[2];

    /// <summary>
    /// Main collider for this mover
    /// </summary>
    private CircleCollider2D circleCollider;

    public bool collidedThisFrame;



    void Awake() {
        // cache some components
        transform = GetComponent<Transform>();
        rigidBody2D = GetComponent<Rigidbody2D>();

        circleCollider = GetComponent<CircleCollider2D>();

        // here, we trigger our properties that have setters with bodies
        skinWidth = _skinWidth;

        radius = circleCollider.radius + skinWidth;
    }


    public void OnTriggerEnter2D(Collider2D col) {
        if(onTriggerEnterEvent != null)
            onTriggerEnterEvent(col);
    }


    public void OnTriggerStay2D(Collider2D col) {
        if(onTriggerStayEvent != null)
            onTriggerStayEvent(col);
    }


    public void OnTriggerExit2D(Collider2D col) {
        if(onTriggerExitEvent != null)
            onTriggerExitEvent(col);
    }
    

    [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
    void DrawRay(Vector3 start, Vector3 dir, Color color) {
        Debug.DrawRay(start, dir, color);
    }

    
    /// <summary>
    /// attempts to move the character to position + deltaMovement. Any colliders in the way will cause the movement to
    /// stop when run into.
    /// </summary>
    /// <param name="deltaMovement">Delta movement.</param>
    public void Move(Vector3 deltaMovement) {
        collidedThisFrame = false;

        Vector3 moveTo = moveLinearResolve(deltaMovement, 10.0f, 20.0f, 60.0f, 80.0f);
        moveTo.z = 0.0f;

        transform.position = moveTo;

        // only calculate velocity if we have a non-zero deltaTime
        if(Time.deltaTime > 0f) {
            velocity = deltaMovement / Time.deltaTime;
        }
    }



    /// <summary>
    /// Applies a linear force to the mover
    /// </summary>
    public void ApplyForce(Vector3 force) {
        if(physicsMass > 0.0f) {
            force = force / physicsMass;
            physicsVel += force;
        }
    }

    private void Update() {
        if(physicsVel.magnitude > 0.0f) {
            Move(physicsVel * Time.deltaTime);

            physicsVel = physicsVel * physicsFriction;
        }
    }



    /// <summary>
    /// Resolves the movement of the input displacement size. 
    /// test angles is a set of angles to raycast relative to displacement
    /// </summary>
    private Vector2 moveLinearResolve(Vector2 displacement, params float[] testAngles) {
        Vector2 movementDirection = displacement.normalized * radius;
        Vector2 targetPosition = (Vector2)transform.position + displacement;

#if UNITY_EDITOR
        //Debug.DrawLine(targetPosition, targetPosition + movementDirection, Color.cyan);
#endif

        // Test the desired direction for a collision and update targetPosition if any is found
        targetPosition += GetValidDirectionAdjustment(targetPosition, movementDirection);

        // Test movementDirection + and - each testAngle for a collision and update targetPosition if any is found
        for(int i = 0; i < testAngles.Length; i++) {
            targetPosition += GetValidDirectionAdjustment(targetPosition, Quaternion.Euler(0, 0, testAngles[i]) * movementDirection);
            targetPosition += GetValidDirectionAdjustment(targetPosition, Quaternion.Euler(0, 0, -testAngles[i]) * movementDirection);

#if UNITY_EDITOR
            Debug.DrawLine(targetPosition, targetPosition + (Vector2)(Quaternion.Euler(0, 0, testAngles[i]) * movementDirection), Color.cyan);
            Debug.DrawLine(targetPosition, targetPosition + (Vector2)(Quaternion.Euler(0, 0, -testAngles[i]) * movementDirection), Color.cyan);
#endif
        }

        return targetPosition;
    }


    /// <summary>Tests <paramref name="direction"/> from <paramref name="targetPosition"/> + <paramref name="radius"/> for a collision. If one is found, returns a Vector2 adjustment to 
    /// the closest valid position. Otherwise returns Vector2.zero.</summary>
    /// <returns>The closest valid position to <paramref name="targetPosition"/>.</returns>
    /// <param name="targetPosition">The desired position to move to.</param>
    /// <param name="direction">The direction to test for a collision.</param>
    private Vector2 GetValidDirectionAdjustment(Vector2 targetPosition, Vector2 direction) {
        Vector2 validPositionAdjustment = Vector2.zero;

        int amountOfHits = Physics2D.RaycastNonAlloc(targetPosition, direction, hits2D, radius, platformMask);
        RaycastHit2D hit2D;

        /// Because the character has a collider, to ensure we can collide with other characters if desired
        /// we need to allow for up to two hit detections. One for the character's collider and the other 
        /// for our actual collision.
        //if(amountOfHits == 0 || (amountOfHits == 1 && hits2D[0].fraction < 0.5f)) {
        if(amountOfHits == 0 || hits2D[0].collider.gameObject == gameObject) {
            // We hit nothing, or we only hit ourselves
            return validPositionAdjustment;
        }
        else if(amountOfHits == 1 || (amountOfHits > 1 && hits2D[0].fraction > 0.5f)) {
            // We hit one of more colliders, but none of them was ours
            hit2D = hits2D[0];
            collidedThisFrame = true;
        }
        else {
            // We hit ourselves, but we also hit something else.
            hit2D = hits2D[1];
            collidedThisFrame = true;
        }

        validPositionAdjustment = hit2D.normal.normalized * ((1.0f - hit2D.fraction) * radius);

#if UNITY_EDITOR
        //Debug.DrawLine(hit2D.point, hit2D.point += validPositionAdjustment, Color.magenta);
#endif

        return validPositionAdjustment;
    }



    
}
