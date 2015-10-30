using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpgradeMenuManager : MonoBehaviour {

    [SerializeField]
    public Text oreCountField;


    private PlayerState playerState;
	


	private void Start() {
	    playerState = FindObjectOfType<PlayerState>();

        UI_UpgradeSlot[] allSlotIcons = FindObjectsOfType<UI_UpgradeSlot>();
        UI_UpgradeTile[] allUpgrades = FindObjectsOfType<UI_UpgradeTile>();

        int slotIdx = 0;
        for(int i = 0; i < playerState.equippedUpgrades.Count; i++) {

            // check which icon is representing this equipped upgrade
            foreach(UI_UpgradeTile upgradeIcon in allUpgrades) {
                if(upgradeIcon.playerUpgradeClass == playerState.equippedUpgrades[i]) {

                    // Put it in the available slot
                    foreach(UI_UpgradeSlot slotIcon in allSlotIcons) {
                        if(slotIcon.slotIndex == slotIdx) {
                            slotIcon.SetIcon(upgradeIcon.upgradeImage);
                            slotIcon.hasEquippedUpgrade = true;
                            slotIcon.upgradeClass = upgradeIcon.playerUpgradeClass;
                        }
                    }

                    // TODO: add the number of slots this upgrade takes up
                    slotIdx += 1;
                }
            }
        }
	}
	
	
	private void Update() {
        if(oreCountField != null && playerState != null) {
            oreCountField.text = "" + playerState.UpgradePoints;
        }
	}




    public void AddPointToUpgrade(string upgradeName) {
        Debug.Log("Request upgrade for [" + upgradeName + "]");

        bool usedUpgradePoint = playerState.UseUpgradePoint(upgradeName);
    }



    public void FinishUpgradeMenu() {
        // TODO: Goto level selection?

        playerState.SaveState();
        Application.LoadLevel("TestLevel");
    }

}
