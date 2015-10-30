using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpgradeMenuManager : MonoBehaviour {

    [SerializeField]
    public Text oreCountField;


    private PlayerState playerState;
	


	private void Start() {
	    playerState = FindObjectOfType<PlayerState>();
	}
	
	
	private void Update() {
        if(oreCountField != null && playerState != null) {
            oreCountField.text = "" + playerState.UpgradePoints;
        }
	}
}
