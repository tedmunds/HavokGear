﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UI_EnemyHealthBar : MonoBehaviour {

    [SerializeField]
    private Image barOverlay;

    [SerializeField]
    private Vector2 screenOffset;

    private Camera mainCamera;
    private Canvas uiCanvas;
    private RectTransform rectTransform;

    [HideInInspector]
    public Actor targetActor;

	private void OnEnable() {
	    uiCanvas = FindObjectOfType<Canvas>();
        if(uiCanvas != null) {
            transform.SetParent(uiCanvas.transform, false);
        }

        rectTransform = GetComponent<RectTransform>();
	}
	
	
	private void Update() {
        if(targetActor != null) {
            if(mainCamera == null) {
                mainCamera = FindObjectOfType<Camera>();
            }

            if(mainCamera != null) {
                // Get screen location of the target actor and add some offset
                Vector2 viewportLoc = mainCamera.WorldToViewportPoint(targetActor.transform.position);

                Vector2 screenLoc = new Vector2(uiCanvas.pixelRect.size.x * viewportLoc.x, 
                                                uiCanvas.pixelRect.size.y * viewportLoc.y);
                
                screenLoc += screenOffset;
                rectTransform.position = screenLoc;

                //set the fill amount from actor health
                float fillPct = targetActor.Health / targetActor.maxhealth;
                barOverlay.fillAmount = fillPct;

                if(fillPct <= 0.0f) {
                    gameObject.SetActive(false);
                }
            }
        }
        else {
            gameObject.SetActive(false);
        }
	}

    /// <summary>
    /// Sets this health bar to respond to this actor
    /// </summary>
    public void AssignToTarget(Actor newTarget) {
        targetActor = newTarget;
    }
    
}