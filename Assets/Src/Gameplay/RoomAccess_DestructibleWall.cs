using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Actor))]
public class RoomAccess_DestructibleWall : RoomAccess {


    private Actor actorComponent;


    private void Start() {
        actorComponent = GetComponent<Actor>();
        actorComponent.RegisterDeathListener(WasDestroyed);
    }



    public void WasDestroyed(Actor destroyed) {
        if(targetFog != null) {
            targetFog.ClearFog();
        }

        WorldManager.instance.PlayGlobalSound(onActivatedSound);
        
        // TODO: Broken wall sprite or something
        Destroy(gameObject);
    }

	
}
