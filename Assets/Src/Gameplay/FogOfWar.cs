using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWar : MonoBehaviour {

    [SerializeField]
    public float fadeOutTime = 0.5f;


    private SpriteRenderer spriteRenderer;

    private float clearedTime;
    private bool isCleared;
    public bool HasBeenCleared {
        get { return isCleared; }
    }

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isCleared = false;
    }
	
	
	private void Update() {
	    if(isCleared) {
            float elapsed = Time.time - clearedTime;

            

            if(elapsed < fadeOutTime) {
                float fadePct = 1.0f - elapsed / fadeOutTime;

                Color fadedCol = spriteRenderer.color;
                fadedCol.a = fadePct;
                spriteRenderer.color = fadedCol;
            }
            else {
                spriteRenderer.enabled = false;
                gameObject.SetActive(false);
            }
        }
	}



    /// <summary>
    /// Clears the fog revealing whatever its blocking
    /// </summary>
    public void ClearFog() {
        clearedTime = Time.time;
        isCleared = true;
    }
}
