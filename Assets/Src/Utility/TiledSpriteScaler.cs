using UnityEngine;
using System.Collections;

public class TiledSpriteScaler : MonoBehaviour {

    public float TiledScale = 3.0f;

    private void Awake() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        renderer.material.SetFloat("_ScaleX", transform.localScale.x / TiledScale);
        renderer.material.SetFloat("_ScaleY", transform.localScale.y / TiledScale);
    }
 
}
