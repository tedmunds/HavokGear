using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

    private const float zOffset = -10.0f;

    /// <summary>
    /// The player that the camera will follow
    /// </summary>
    [SerializeField]
    public PlayerController targetPlayer;


    private Transform trackTarget;


	private void Start() {
	    if(targetPlayer == null) {
            Debug.LogWarning("Camera Controller: <" + name + "> Does not have a player assigned to track!");
        }
        else {
            trackTarget = targetPlayer.transform;
            targetPlayer.PlayerCamera = this.GetComponent<Camera>(); ;
        }
	}
	


	private void Update() {
        transform.position = new Vector3(trackTarget.position.x, trackTarget.position.y, zOffset);
    }
}
