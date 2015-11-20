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

        gameObject.SetActive(false);
        Destroy(gameObject);

        // update the path area
        Bounds rebuildBounds = new Bounds(transform.position, new Vector3(10.0f, 10.0f, 1.0f));        
        AstarPath.active.UpdateGraphs(GetComponent<Collider2D>().bounds);
    }

	
}
