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
        targetFog.ClearFog();

        // TODO: Broken wall sprite or something
        Destroy(gameObject);
    }

	
}
