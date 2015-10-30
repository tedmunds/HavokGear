using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_UpgradeTile : MonoBehaviour {

    [SerializeField]
    public string playerUpgradeClass;

    [SerializeField]
    public Image upgradeImage;

	
	private void Start() {
	
	}
	
	
	private void Update() {
	
	}


    /// <summary>
    /// Message recieved when this tile is dropped into an upgrade slot
    /// </summary>
    public void UI_DroppedInto(UI_DropArea droppedInto) {
        
    }
}
