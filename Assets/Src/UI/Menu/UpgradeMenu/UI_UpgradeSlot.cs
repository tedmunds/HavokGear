using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_UpgradeSlot : MonoBehaviour {


    [SerializeField]
    public Image imageIcon;

	
	
	private void Start() {
        if(imageIcon != null) {
            imageIcon.enabled = false;
        }
	}
	
	
	private void Update() {
	
	}

    
    /// <summary>
    /// Will receive this message when an upgrade tile is dropped onto this slot
    /// </summary>
    public void UI_ReceivedDrop(UI_Draggable draggeble) {
        PlayerState playerState = FindObjectOfType<PlayerState>();
        UI_UpgradeTile upgradeTile = draggeble.GetComponent<UI_UpgradeTile>();

        if(playerState != null && playerState != null) {
            Debug.Log(upgradeTile.playerUpgradeClass + " was dropped into slot [" + name + "]");
            playerState.EquipUpgradeType(upgradeTile.playerUpgradeClass);

            // Set the slots image
            if(upgradeTile.upgradeImage != null && imageIcon != null) {
                imageIcon.sprite = upgradeTile.upgradeImage.sprite;
                imageIcon.enabled = true;
            }
        }
    }
}
