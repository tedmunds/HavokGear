using UnityEngine;
using System.Collections;



[RequireComponent(typeof(Collider2D))]
public abstract class PickupableItem : MonoBehaviour {

    [SerializeField]
    public bool pickupOnTrigger;

    protected bool hasBeenActivated = false;

    public void OnTriggerEnter2D(Collider2D other) {
        if(!pickupOnTrigger || hasBeenActivated) {
            return;
        }

        // If the player enters trigger area, then activate 
        PlayerController playerController = other.GetComponent<PlayerController>();
        if(playerController != null) {
            Activate(playerController);
        }
    }


    /// <summary>
    /// Player has picked up this item
    /// </summary>
    public virtual void Activate(MechController activator) {
        hasBeenActivated = true;
        gameObject.SetActive(false);
    }


}
