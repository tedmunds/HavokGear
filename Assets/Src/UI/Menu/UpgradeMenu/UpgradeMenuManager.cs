using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpgradeMenuManager : MonoBehaviour {

    // Instances of the upgrades in order to get per instance info about them
    public static Upgrade_Health upgrade_Health = new Upgrade_Health(0);
    public static Upgrade_HealthRegen upgrade_HealthRegen = new Upgrade_HealthRegen(0);


    [SerializeField]
    public Text oreCountField;

    private PlayerState playerState;

    
	private void Start() {
	    playerState = FindObjectOfType<PlayerState>();

        // Update all of the ui elements
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

        int pointsUsed = 0;
        bool usedUpgradePoint = playerState.UseUpgradePoint(upgradeName, out pointsUsed);

        UI_UpgradeTile[] allUpgrades = FindObjectsOfType<UI_UpgradeTile>();
        foreach(UI_UpgradeTile tile in allUpgrades) {
            tile.PointAdded(upgradeName);
        }
    }



    public void FinishUpgradeMenu() {
        // TODO: Goto level selection?

        playerState.SaveState();
        Application.LoadLevel("TestLevel");
    }


    /// <summary>
    /// Gets the slots required to equip the given level of the given upgrade
    /// </summary>
    public static int GetSlotsFromUpgrade(string upgradeName, int currentLevel) {
        // Its not ideal, but a simple switch case that needs to be updated to include all upgrades
        switch(upgradeName) {
            case "Upgrade_Health":
                return upgrade_Health.RequiredSlots(currentLevel);
            case "Upgrade_HealthRegen":
                return upgrade_HealthRegen.RequiredSlots(currentLevel);
        }

        return 0;
    }


    /// <summary>
    /// Gets the number of ore points required to improve the upgrade
    /// </summary>
    public static int GetPointsToUpgrade(string upgradeName, int currentLevel) {
        // Its not ideal, but a simple switch case that needs to be updated to include all upgrades
        switch(upgradeName) {
            case "Upgrade_Health":
                return upgrade_Health.PointsToUpgrade(currentLevel);
            case "Upgrade_HealthRegen":
                return upgrade_HealthRegen.PointsToUpgrade(currentLevel);
        }

        return 1;
    }

    /// <summary>
    /// Gets the max upgrade level of the input upgrade
    /// </summary>
    public static int GetMaxLevel(string upgradeName) {
        // Its not ideal, but a simple switch case that needs to be updated to include all upgrades
        switch(upgradeName) {
            case "Upgrade_Health":
                return upgrade_Health.maxUpgradeLevel;
            case "Upgrade_HealthRegen":
                return upgrade_HealthRegen.maxUpgradeLevel;
        }

        return 1;
    }

}
