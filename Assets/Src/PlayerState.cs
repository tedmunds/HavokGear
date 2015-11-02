using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerState : MonoBehaviour {

    public static PlayerState instance;


    /// <summary>
    /// Available points to spend on unlocking upgrades
    /// </summary>
    private int upgradePoints;
    public int UpgradePoints {
        get { return upgradePoints; }
    }

    private int maxUpgradeSlots = 6;
    public int MaxUpgradeSlots {
        get { return maxUpgradeSlots; }
    }

    private int upgradeSlotsInUse;


    /// <summary>
    /// Table maps the players level for each upgrade type
    /// </summary>
    public Dictionary<string, int> upgradeTable;

    [HideInInspector]
    public List<string> equippedUpgrades;

    private string testSavefile = "data/testSave.xml";
    private PlayerSave saveState;

    private void OnEnable() {
        instance = this;
        LoadState();
    }




    public void CollectPoints(int pointsCollected) {
        upgradePoints += pointsCollected;
    }


    public void LoadState() {
        upgradeTable = new Dictionary<string, int>();
        equippedUpgrades = new List<string>();

        saveState = XMLObjectLoader.LoadXMLObject<PlayerSave>(testSavefile);
        if(saveState == null) {
            Debug.Log("Player State could not load a save file from data/testSave.xml");
            saveState = new PlayerSave();
        }

        upgradePoints = saveState.availablePoints;

        upgradeTable.Add("Upgrade_Health", saveState.GetSavedLevelForUpgrade("Upgrade_Health"));
        upgradeTable.Add("Upgrade_HealthRegen", saveState.GetSavedLevelForUpgrade("Upgrade_HealthRegen"));

        if(saveState.equippedUpgrades != null) {
            equippedUpgrades.AddRange(saveState.equippedUpgrades);
        }
        else {
            saveState.equippedUpgrades = new string[0];
        }
    }


    public void SaveState() {
        // On destroy, save the current player state
        if(saveState != null) {
            saveState.UpdateFromState(this);
        }
        else {
            saveState = new PlayerSave();
        }

        XMLObjectLoader.SaveXMLObject<PlayerSave>(testSavefile, saveState);
    }


    /// <summary>
    /// Tries to equip the upgrade type: returns true if successful
    /// </summary>
    public bool EquipUpgradeType(string upgradeType) {
        if(!equippedUpgrades.Contains(upgradeType)) {
            // Figure out how many slots are needed
            int currentLevel = 0;
            upgradeTable.TryGetValue(upgradeType, out currentLevel);
            int requiredSlots = UpgradeMenuManager.GetSlotsFromUpgrade(upgradeType, currentLevel);

            // Check if there is space
            if(upgradeSlotsInUse + requiredSlots <= maxUpgradeSlots) {
                upgradeSlotsInUse += requiredSlots;

                equippedUpgrades.Add(upgradeType);

                Debug.Log(upgradeType + " was equipped on the player state, slots in use [" + upgradeSlotsInUse + " of " + maxUpgradeSlots + "]");
                return true;
            }
        }

        return false;
    }



    public bool UnequipUpgrade(string upgradeType) {
        if(equippedUpgrades.Contains(upgradeType)) {

            // Get the number of slots it was using
            int currentLevel = 0;
            upgradeTable.TryGetValue(upgradeType, out currentLevel);
            int requiredSlots = UpgradeMenuManager.GetSlotsFromUpgrade(upgradeType, currentLevel);

            equippedUpgrades.Remove(upgradeType);
            upgradeSlotsInUse -= requiredSlots;    

            Debug.Log(upgradeType + " was unequipped from the player state, slots in use [" + upgradeSlotsInUse + " of " + maxUpgradeSlots + "]");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Uses 1 upgradfe point to improve the target upgrade, returns whether or not it was used 
    /// </summary>
    public bool UseUpgradePoint(string targetUpgrade, out int pointsUsed) {
        int currentLevel = 0;
        upgradeTable.TryGetValue(targetUpgrade, out currentLevel);
        int requiredPoints = UpgradeMenuManager.GetPointsToUpgrade(targetUpgrade, currentLevel);
        int upgradeMaxPointLevel = UpgradeMenuManager.GetMaxLevel(targetUpgrade);

        if(upgradePoints >= requiredPoints && currentLevel + 1 < upgradeMaxPointLevel) {
            // Perform the upgrade and subtract the points
            upgradeTable[targetUpgrade] = currentLevel + 1;
            upgradePoints -= requiredPoints;
            pointsUsed = requiredPoints;

            Debug.Log("Upgrade point used on [" + targetUpgrade + "] :: current level=" + upgradeTable[targetUpgrade]);
            return true;
        }

        pointsUsed = 0;
        return false;
    }

}
