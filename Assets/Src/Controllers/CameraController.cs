using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    
    // Data required to do a camera shake
    public struct CameraShake {
        public float shakeLength;
        public float shakeStrength;
        public float shakeStartTime;
        public float shakeSpeed;
        public float shakeFalloff;
        public bool bIsConstant;

        // Camera that this was applied to 
        public CameraController targetCamera;

        public CameraShake(float shakeLength, float shakeStrength, float shakeSpeed, float shakeFalloff, bool bIsConstant) {
            this.shakeLength = shakeLength;
            this.shakeStrength = shakeStrength;
            this.shakeSpeed = shakeSpeed;
            this.shakeFalloff = shakeFalloff;
            this.bIsConstant = bIsConstant;
            shakeStartTime = Time.time;
            targetCamera = null;
        }
    }

    [SerializeField]
    public PlayerController playerTarget;
    
    /* How closely does the camera follow the ship, how much 'give' there is */
    [SerializeField]
    private float tightness;

    /* How far offset from the ships location can the camera be */
    [SerializeField]
    private float maxOffsetDistance;
   

    private Camera cameraComponent;

    private Vector3 shakeOffset;
    private bool bIsShaking;

    private CameraShake currentShakeData;
    

    void OnEnable() {
        cameraComponent = GetComponent<Camera>();
    }


    void Update() {
        Vector3 newPosition = transform.position;

        // Move the camera location towrads the aim location (mouse) based on how fast the ship is going
        Vector3 targetPosition = playerTarget.transform.position;
        Vector3 aimPosition = playerTarget.GetAimLocation();

        if((targetPosition - aimPosition).magnitude > maxOffsetDistance) {
            aimPosition = targetPosition + (aimPosition - targetPosition).normalized * maxOffsetDistance;
        }

        targetPosition = Vector3.Lerp(targetPosition, aimPosition, 0.5f);

        newPosition = Vector3.Lerp(transform.position, targetPosition, tightness * Time.deltaTime);
        newPosition.z = transform.position.z; // Dont want to change the z position, so camera stays in fixed plane

        transform.position = newPosition;

        // If its doing a camera shake, add offset
        if(bIsShaking) {
            float progress = (Time.time - currentShakeData.shakeStartTime) / currentShakeData.shakeLength;

            // If its a constant shake, it wont end until something has told it to
            if(progress >= 1.0f && !currentShakeData.bIsConstant) {
                bIsShaking = false;
            }

            // Loop if its a constant shake
            if(currentShakeData.bIsConstant && progress >= 1.0f) {
                currentShakeData.shakeStartTime = Time.time;
            }

            Vector3 modifiedOffset = shakeOffset.normalized * shakeOffset.magnitude * ((1.0f - progress) * currentShakeData.shakeFalloff);

            float modifier = Mathf.Sin(progress * 6.48f * currentShakeData.shakeSpeed);

            transform.position += modifiedOffset * modifier;
        }
        
    }

    

    public void StartCameraShake(ref CameraShake shakeData, Vector3 shakeDirection) {
        if(bIsShaking) {
            return;
        }

        shakeDirection.Normalize();

        currentShakeData = shakeData;
        shakeOffset = shakeDirection * currentShakeData.shakeStrength;
        bIsShaking = true;

        // record which camera used this data
        shakeData.targetCamera = this;
    }


    public void ForceEndShake() {
        bIsShaking = false;
        currentShakeData.bIsConstant = false;
    }
}
