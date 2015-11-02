using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_UpgradeSlot : MonoBehaviour {


    [SerializeField]
    public int slotIndex;


    [SerializeField]
    public Image imageIcon;

    [HideInInspector]
    public bool hasEquippedUpgrade = false;

    [HideInInspector]
    public string upgradeClass;

    [HideInInspector]
    public int upgradeLevel;

	
	private void Start() {
        if(imageIcon != null) {
            imageIcon.enabled = false;
        }

        // Try to find what image this slot should ahve (if it has a thing equipped in it on the player state)
        PlayerState playerState = FindObjectOfType<PlayerState>();
        if(playerState != null) {
            
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
            bool wasEquipped = playerState.EquipUpgradeType(upgradeTile.playerUpgradeClass, upgradeTile.playerUpgradeLevel);

            if(wasEquipped) {
                // Set the slots image
                SetIcon(upgradeTile.upgradeImage);

                hasEquippedUpgrade = true;
                upgradeClass = upgradeTile.playerUpgradeClass;
                upgradeLevel = upgradeTile.playerUpgradeLevel;
                UpgradeMenuManager.instance.UpgradePlacedInSlot(upgradeTile.playerUpgradeClass, this);
            }
           
        }
    }


    public void SetIcon(Image newIcon) {
        if(newIcon != null && imageIcon != null) {
            imageIcon.sprite = newIcon.sprite;
            imageIcon.enabled = true;
        }
    }



    public void RemoveUpgrade() {
        if(hasEquippedUpgrade) {
            // Try to remove the upgrade that this slot has in it
            PlayerState playerState = FindObjectOfType<PlayerState>();
            if(playerState != null && playerState.IsUpgradeEquipped(upgradeClass, upgradeLevel)) {
                playerState.UnequipUpgrade(upgradeClass, upgradeLevel);
                
                hasEquippedUpgrade = false;
                upgradeClass = "";

                imageIcon.sprite = null;
                imageIcon.enabled = false;
                UpgradeMenuManager.instance.UpgradeRemovedFromSlot(upgradeClass, this);
            }
        }
    }
}
