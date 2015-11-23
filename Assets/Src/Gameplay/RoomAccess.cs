using UnityEngine;
using System.Collections;


/// <summary>
/// Base component for anything that clears a segment of Fog of War
/// </summary>
public abstract class RoomAccess : MonoBehaviour {

    [HideInInspector]
    public bool hasBeenActivated = false;

    /// <summary>
    /// The gfog of war that will be cleared when this room access is activated
    /// </summary>
    [SerializeField]
    public FogOfWar targetFog;

    [SerializeField]
    public AudioClip onActivatedSound;

    /// <summary>
    /// Called to clear the target fog
    /// </summary>
    public virtual void Activate(MechController activator) {
        // check that it hasnt already been actiavted
        if(hasBeenActivated) {
            return;
        }

        hasBeenActivated = true;
        if(targetFog != null && !targetFog.HasBeenCleared) {
            targetFog.ClearFog();
        }

        Debug.Log("Room Access object " + gameObject.name + " activated...");
        WorldManager.instance.PlayGlobalSound(onActivatedSound);
    }

}
