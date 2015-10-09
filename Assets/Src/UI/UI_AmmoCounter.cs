using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_AmmoCounter : MonoBehaviour {

    [SerializeField]
    public Text counterField;


    private PlayerController targetPlayer;

    private void Start() {
	
	}
	
	
	private void Update() {
        if(targetPlayer == null) {
            targetPlayer = FindObjectOfType<PlayerController>();
        }
        else {
            counterField.text = "" + targetPlayer.GetCurrentAmmo();
        }
    }
}
