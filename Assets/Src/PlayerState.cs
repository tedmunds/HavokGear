using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerState : MonoBehaviour {

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

	private void Start() {
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
        }

        upgradePoints = saveState.availablePoints;

        upgradeTable.Add("Upgrade_Health", saveState.GetSavedLevelForUpgrade("Upgrade_Health"));
        upgradeTable.Add("Upgrade_HealthRegen", saveState.GetSavedLevelForUpgrade("Upgrade_HealthRegen"));
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
        if(!equippedUpgrades.Contains(upgradeType) && upgradeSlotsInUse < maxUpgradeSlots) {
            equippedUpgrades.Add(upgradeType);
            upgradeSlotsInUse += 1; // TODO: number of slots this upgrade requires     

            Debug.Log(upgradeType + " was equipped on the player state, slots in use [" + upgradeSlotsInUse + " of " + maxUpgradeSlots + "]");
        }

        return false;
    }


    /// <summary>
    /// Uses 1 upgradfe point to improve the target upgrade, returns whether or not it was used 
    /// </summary>
    public bool UseUpgradePoint(string targetUpgrade) {
        int currentLevel = 0;
        if(upgradeTable.TryGetValue(targetUpgrade, out currentLevel) && upgradePoints > 0) {
            // TODO: constrain to the upgrades max level

            upgradeTable.Add(targetUpgrade, currentLevel + 1);
            upgradePoints -= 1;
        }

        return false;
    }

}
