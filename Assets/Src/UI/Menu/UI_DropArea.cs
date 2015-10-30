using UnityEngine;
using System.Collections;


[RequireComponent(typeof(RectTransform))]
public class UI_DropArea : MonoBehaviour {

    [HideInInspector]
    public RectTransform rectTransform;



	private void Start() {
	    rectTransform = GetComponent<RectTransform>();
	}
	





}
