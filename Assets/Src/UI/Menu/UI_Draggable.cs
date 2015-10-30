using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class UI_Draggable : MonoBehaviour {

    public bool dragInstance = false;


    [HideInInspector]
    public RectTransform rectTransform;

    private bool isMaster = true;

	
	private void Start() {
        rectTransform = GetComponent<RectTransform>();
	}



    public void Released() {
        if(!isMaster) {
            Debug.Log(name + " draggable object was released");
            Destroy(gameObject);
        }
    }



    /// <summary>
    /// returns the object that will be dragged when this is selected
    /// </summary>
    public UI_Draggable GetDraggable() {
        if(!dragInstance || !isMaster) {
            return this;
        }

        UI_Draggable instance = (UI_Draggable)Instantiate(this, transform.position, Quaternion.identity);
        instance.transform.SetParent(FindObjectOfType<Canvas>().transform);
        instance.isMaster = false;

        return instance;
    }



    public void WasDroppedIntoArea(UI_DropArea droppedInto) {
        SendMessage("UI_DroppedInto", droppedInto);
        droppedInto.SendMessage("UI_ReceivedDrop", this);

        if(!isMaster) {
            gameObject.SetActive(false);
        }
    }
}
