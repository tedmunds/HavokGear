using UnityEngine;
using System.Collections;

public class Pickup_Health : PickupableItem {

    [SerializeField]
    public float healAmount;

    /// <summary>
    /// health the activator
    /// </summary>
    public override void Activate(MechController activator) {
        if(hasBeenActivated) {
            return;
        }

        if(activator != null & activator.MechComponent != null) {
            Debug.Log("[" + name + "] Heals [" + activator.name + "] for " + healAmount);
            activator.MechComponent.AddHealth(healAmount, gameObject);
        }

        base.Activate(activator);
    }

}
