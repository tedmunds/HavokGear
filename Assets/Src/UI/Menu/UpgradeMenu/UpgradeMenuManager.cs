using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class UpgradeMenuManager : MonoBehaviour {

    // Instances of the upgrades in order to get per instance info about them
    //public static Upgrade_Health upgrade_Health = new Upgrade_Health();
    //public static Upgrade_HealthRegen upgrade_HealthRegen = new Upgrade_HealthRegen();

    public static UpgradeMenuManager instance;

    [SerializeField]
    public Text oreCountField;

    [SerializeField]
    public Text slotsInUseField;

    private PlayerState playerState;

    private void OnEnable() {
        instance = this;
    }


	private void Start() {
	    playerState = FindObjectOfType<PlayerState>();

        // Update all of the ui elements
        UI_UpgradeSlot[] allSlotIcons = FindObjectsOfType<UI_UpgradeSlot>();
        UI_UpgradeTile[] allUpgrades = FindObjectsOfType<UI_UpgradeTile>();

        int slotIdx = 0;
        for(int i = 0; i < playerState.equippedUpgrades.Count; i++) {

            // check which icon is representing this equipped upgrade
            foreach(UI_UpgradeTile upgradeIcon in allUpgrades) {
                if(upgradeIcon.playerUpgradeClass == playerState.equippedUpgrades[i].name &&
                   upgradeIcon.playerUpgradeLevel == playerState.equippedUpgrades[i].level) {

                    // Put it in the next available slot
                    foreach(UI_UpgradeSlot slotIcon in allSlotIcons) {
                        if(slotIcon.slotIndex == slotIdx) {
                            slotIcon.SetIcon(upgradeIcon.upgradeImage);
                            slotIcon.hasEquippedUpgrade = true;
                            slotIcon.upgradeClass = upgradeIcon.playerUpgradeClass;
                            slotIcon.upgradeLevel = upgradeIcon.playerUpgradeLevel;
                        }
                    }

                    // TODO: add the number of slots this upgrade takes up
                    slotIdx += 1;
                }
            }
        }

        // Set initial slots in use
        if(slotsInUseField != null) {
            slotsInUseField.text = "" + playerState.SlotsInUse;
        }
	}
	
	
	private void Update() {
        if(oreCountField != null && playerState != null) {
            oreCountField.text = "" + playerState.UpgradePoints;
        }
	}




    public void UnlockNewUpgradeLevel(string upgradeName) {
        Debug.Log("Request unlock for [" + upgradeName + "]");

        int pointsUsed = 0;
        bool wasUnlocked = playerState.UnlockUpgradeLevel(upgradeName, out pointsUsed);

        int newUnlockLevel = 0;
        playerState.upgradeUnlockTable.TryGetValue(upgradeName, out newUnlockLevel);

        UI_UpgradeTile[] allUpgrades = FindObjectsOfType<UI_UpgradeTile>();
        foreach(UI_UpgradeTile tile in allUpgrades) {
            if(tile.playerUpgradeClass == upgradeName) {
                tile.PointAdded(upgradeName, newUnlockLevel);
            }
        }
    }



    public void FinishUpgradeMenu() {
        // TODO: Goto level selection?

        playerState.SaveState();
        Application.LoadLevel("TestLevel");
    }


    public void UpgradePlacedInSlot(string upgradeName, UI_UpgradeSlot slot) {
        if(slotsInUseField != null) {
            slotsInUseField.text = "" + playerState.SlotsInUse;
        }
    }


    public void UpgradeRemovedFromSlot(string upgradeName, UI_UpgradeSlot slot) {
        if(slotsInUseField != null) {
            slotsInUseField.text = "" + playerState.SlotsInUse;
        }
    }

}
