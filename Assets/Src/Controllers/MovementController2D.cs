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


    struct CharacterRaycastOrigins {
        public Vector3 topLeft;
        public Vector3 bottomRight;
        public Vector3 bottomLeft;
    }

    public class CharacterCollisionState2D {
        public bool right;
        public bool left;
        public bool above;
        public bool below;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;
        public bool movingDownSlope;
        public float slopeAngle;


        public bool hasCollision() {
            return below || right || left || above;
        }


        public void reset() {
            right = left = above = below = becameGroundedThisFrame = movingDownSlope = false;
            slopeAngle = 0f;
        }


        public override string ToString() {
            return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}",
                                    right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame);
        }
    }
    

    public event Action<RaycastHit2D> onControllerCollidedEvent;
    public event Action<Collider2D> onTriggerEnterEvent;
    public event Action<Collider2D> onTriggerStayEvent;
    public event Action<Collider2D> onTriggerExitEvent;


    /// <summary>
    /// when true, one way platforms will be ignored when moving vertically for a single frame
    /// </summary>
    [HideInInspector]
    public bool ignoreOneWayPlatformsThisFrame;

    [SerializeField]
    [Range(0.001f, 0.3f)]
    float _skinWidth = 0.02f;

    /// <summary>
    /// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
    /// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
    /// </summary>
    public float skinWidth {
        get { return _skinWidth; }
        set {
            _skinWidth = value;
            recalculateDistanceBetweenRays();
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
    
    [Range(2, 20)]
    public int totalHorizontalRays = 8;
    [Range(2, 20)]
    public int totalVerticalRays = 8;

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
    public BoxCollider2D boxCollider;
    [HideInInspector]
    [NonSerialized]
    public Rigidbody2D rigidBody2D;

    [HideInInspector]
    [NonSerialized]
    public CharacterCollisionState2D collisionState = new CharacterCollisionState2D();
    [HideInInspector]
    [NonSerialized]
    public Vector3 velocity;
    public bool isGrounded { get { return collisionState.below; } }

    const float kSkinWidthFloatFudgeFactor = 0.001f;
    
    /// <summary>
    /// holder for our raycast origin corners (TR, TL, BR, BL)
    /// </summary>
    CharacterRaycastOrigins _raycastOrigins;

    /// <summary>
    /// stores our raycast hit during movement
    /// </summary>
    RaycastHit2D _raycastHit;

    /// <summary>
    /// stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
    /// horizontally and vertically so that we can send the events after all collision state is set
    /// </summary>
    List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

    // horizontal/vertical movement data
    float _verticalDistanceBetweenRays;
    float _horizontalDistanceBetweenRays;


    private CircleCollider2D circleCollider;
    
    void Awake() {
        // cache some components
        transform = GetComponent<Transform>();
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody2D = GetComponent<Rigidbody2D>();

        circleCollider = GetComponent<CircleCollider2D>();

        // here, we trigger our properties that have setters with bodies
        skinWidth = _skinWidth;

        // we want to set our CC2D to ignore all collision layers except what is in our triggerMask
        for(var i = 0; i < 32; i++) {
            // see if our triggerMask contains this layer and if not ignore it
            if((triggerMask.value & 1 << i) == 0)
                Physics2D.IgnoreLayerCollision(gameObject.layer, i);
        }
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
        // clear our state
        collisionState.reset();
        _raycastHitsThisFrame.Clear();

        primeRaycastOrigins();

        if(deltaMovement.magnitude != 0.0f) {
            moveRadially(ref deltaMovement);
        }

        // now we check movement in the horizontal dir
        if(deltaMovement.x != 0f)
            moveHorizontally(ref deltaMovement);
        
        // next, check movement in the vertical dir
        if(deltaMovement.y != 0f)
            moveVertically(ref deltaMovement);
        
        // move then update our state
        transform.Translate(deltaMovement, Space.World);

        // only calculate velocity if we have a non-zero deltaTime
        if(Time.deltaTime > 0f)
            velocity = deltaMovement / Time.deltaTime;
        
        // send off the collision events if we have a listener
        if(onControllerCollidedEvent != null) {
            for(var i = 0; i < _raycastHitsThisFrame.Count; i++)
                onControllerCollidedEvent(_raycastHitsThisFrame[i]);
        }

        ignoreOneWayPlatformsThisFrame = false;
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
    /// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance between the rays used for collision detection.
    /// It is also used in the skinWidth setter in case it is changed at runtime.
    /// </summary>
    public void recalculateDistanceBetweenRays() {
        // figure out the distance between our rays in both directions
        // horizontal
        var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
        _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

        // vertical
        var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
        _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
    }
    


    /// <summary>
    /// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
    /// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
    /// </summary>
    /// <param name="futurePosition">Future position.</param>
    /// <param name="deltaMovement">Delta movement.</param>
    void primeRaycastOrigins() {
        // our raycasts need to be fired from the bounds inset by the skinWidth
        var modifiedBounds = boxCollider.bounds;
        modifiedBounds.Expand(-2f * _skinWidth);

        _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        _raycastOrigins.bottomLeft = modifiedBounds.min;
    }



    void moveRadially(ref Vector3 deltaMovement) {
        Vector3 initalRayOrigin = transform.position + (deltaMovement.normalized * circleCollider.radius * 2.0f);
        float rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        
        Vector3 rayOrigin = initalRayOrigin;
        Vector3 rayDirection = deltaMovement.normalized;
        
        _raycastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, platformMask);
        
        if(_raycastHit) {
            deltaMovement = (Vector3)_raycastHit.point - rayOrigin;
            deltaMovement = deltaMovement.normalized * (deltaMovement.magnitude - _skinWidth);
        
            DrawRay(rayOrigin, deltaMovement, Color.red);
        
            _raycastHitsThisFrame.Add(_raycastHit);
        }
    }


    /// <summary>
    /// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
    /// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
    /// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
    /// actually moving the player
    /// </summary>
    void moveHorizontally(ref Vector3 deltaMovement) {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        //var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
        Vector3 initalRayOrigin = isGoingRight ? transform.position + (transform.right * circleCollider.radius * 2.0f) :
                                                 transform.position - (transform.right * circleCollider.radius * 2.0f);


        var ray = new Vector2(initalRayOrigin.x, initalRayOrigin.y);

        DrawRay(ray, rayDirection * rayDistance, Color.red);

        _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
        if(_raycastHit) {
            deltaMovement.x = _raycastHit.point.x - ray.x;
            rayDistance = Mathf.Abs(deltaMovement.x);
            // remember to remove the skinWidth from our deltaMovement
            if(isGoingRight) {
                deltaMovement.x -= _skinWidth;
                collisionState.right = true;
            }
            else {
                deltaMovement.x += _skinWidth;
                collisionState.left = true;
            }

            _raycastHitsThisFrame.Add(_raycastHit);
        }

        //for(var i = 0; i < totalHorizontalRays; i++) {
        //    var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

        //    DrawRay(ray, rayDirection * rayDistance, Color.red);

        //    // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
        //    // walk up sloped oneWayPlatforms
        //    if(i == 0 && collisionState.wasGroundedLastFrame)
        //        _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
        //    else
        //        _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);

        //    if(_raycastHit) {

        //        // set our new deltaMovement and recalculate the rayDistance taking it into account
        //        deltaMovement.x = _raycastHit.point.x - ray.x;
        //        rayDistance = Mathf.Abs(deltaMovement.x);

        //        // remember to remove the skinWidth from our deltaMovement
        //        if(isGoingRight) {
        //            deltaMovement.x -= _skinWidth;
        //            collisionState.right = true;
        //        }
        //        else {
        //            deltaMovement.x += _skinWidth;
        //            collisionState.left = true;
        //        }

        //        _raycastHitsThisFrame.Add(_raycastHit);

        //        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        //        // than the width + fudge bail out because we have a direct impact
        //        if(rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
        //            break;
        //    }
        //}
    }
    
    void moveVertically(ref Vector3 deltaMovement) {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        //var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

        Vector3 initalRayOrigin = isGoingUp ? transform.position + (transform.up * circleCollider.radius * 2.0f) :
                                                 transform.position - (transform.up * circleCollider.radius * 2.0f);

        // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
        initalRayOrigin.x += deltaMovement.x;

        // if we are moving up, we should ignore the layers in oneWayPlatformMask
        var mask = platformMask;

        var ray = new Vector2(initalRayOrigin.x, initalRayOrigin.y);

        DrawRay(ray, rayDirection * rayDistance, Color.red);

        _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
        if(_raycastHit) {
            deltaMovement.y = _raycastHit.point.y - ray.y;
            rayDistance = Mathf.Abs(deltaMovement.y);

            // remember to remove the skinWidth from our deltaMovement
            if(isGoingUp) {
                deltaMovement.y -= _skinWidth;
                collisionState.above = true;
            }
            else {
                deltaMovement.y += _skinWidth;
                collisionState.below = true;
            }

            _raycastHitsThisFrame.Add(_raycastHit);
        }

        //for(var i = 0; i < totalVerticalRays; i++) {
        //    var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

        //    DrawRay(ray, rayDirection * rayDistance, Color.red);
        //    _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);
        //    if(_raycastHit) {
        //        // set our new deltaMovement and recalculate the rayDistance taking it into account
        //        deltaMovement.y = _raycastHit.point.y - ray.y;
        //        rayDistance = Mathf.Abs(deltaMovement.y);

        //        // remember to remove the skinWidth from our deltaMovement
        //        if(isGoingUp) {
        //            deltaMovement.y -= _skinWidth;
        //            collisionState.above = true;
        //        }
        //        else {
        //            deltaMovement.y += _skinWidth;
        //            collisionState.below = true;
        //        }

        //        _raycastHitsThisFrame.Add(_raycastHit);

        //        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        //        // than the width + fudge bail out because we have a direct impact
        //        if(rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
        //            break;
        //    }
        //}
    }


    
}
