using UnityEngine;
using System.Collections;



[RequireComponent(typeof(Collider2D))]
public abstract class PickupableItem : MonoBehaviour {

    [SerializeField]
    public bool pickupOnTrigger;

    [SerializeField]
    public bool pickupWithWhip;

    [SerializeField]
    private AudioClip pickupSound;

    protected bool hasBeenActivated = false;


    public void OnTriggerEnter2D(Collider2D other) {
        if(!pickupOnTrigger || hasBeenActivated) {
            return;
        }

        Debug.Log("Trigger enter pickup");

        // If the player enters trigger area, then activate 
        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController == null) {
            playerController = other.GetComponentInChildren<PlayerController>();
            if(playerController == null) {
                playerController = other.GetComponentInParent<PlayerController>();
            }
        }


        if(playerController != null) {
            Activate(playerController);
        }
    }


    /// <summary>
    /// Player has picked up this item
    /// </summary>
    public virtual void Activate(MechController activator) {
        hasBeenActivated = true;
        
        if(pickupSound != null) {
            WorldManager.instance.PlayGlobalSound(pickupSound);
        }

        gameObject.SetActive(false);
    }


}
