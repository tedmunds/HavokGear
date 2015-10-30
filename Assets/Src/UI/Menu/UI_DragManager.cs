using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_DragManager : MonoBehaviour {

    /// <summary>
    /// element that is being dragged right now
    /// </summary>
    [HideInInspector]
    public UI_Draggable currentDragged;


    private UI_DropArea[] dropAreas;


	private void Start() {
        dropAreas = FindObjectsOfType<UI_DropArea>();
	}



    private void Update() {
        if(currentDragged != null) {
            currentDragged.rectTransform.position = Input.mousePosition;
        }
    }



    public void DraggableHandler(UI_Draggable draggable) {
        if(currentDragged == null) {
            currentDragged = draggable.GetDraggable();
        }
        else if(currentDragged == draggable) {
            OnDraggableDrop(draggable);
            currentDragged = null;
        }
    }


    private void OnDraggableDrop(UI_Draggable draggable) {        
        draggable.Released();
        Vector2 mousePos = Input.mousePosition;

        // check if it was released in a drop area
        for(int i = 0; i < dropAreas.Length; i++) {
            Rect screenRect = dropAreas[i].rectTransform.rect;
            screenRect.position = dropAreas[i].rectTransform.position;

            if(screenRect.Contains(mousePos)) {
                draggable.WasDroppedIntoArea(dropAreas[i]);
                break;
            }
        }
    }

}
