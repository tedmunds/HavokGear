using UnityEngine;
using System.Collections;

public class Pickup_Ore : PickupableItem {

    /// <summary>
    /// how many unlock points to add to teh palyers tally when this is picked up
    /// </summary>
    [SerializeField]
    public int pointsValue;


    public override void Activate(MechController activator) {
        // Add ythe points ot the player state
        if(WorldManager.instance.playerState != null) {
            WorldManager.instance.playerState.CollectPoints(pointsValue);
        }
        
        // TODO: sound

        base.Activate(activator);
    }
}
