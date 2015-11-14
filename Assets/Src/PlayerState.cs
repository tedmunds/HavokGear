using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerState : MonoBehaviour {

    public static PlayerState instance;


    /// <summary>
    /// Data for one upgrade, its name and its level
    /// </summary>
    public struct UpgradePair {
        public string name;
        public int level;

        public UpgradePair(string name, int level) {
            this.name = name;
            this.level = level;
        }
    }


    /// <summary>
    /// Available points to spend on unlocking upgrades
    /// </summary>
    private int upgradePoints;
    public int UpgradePoints {
        get { return upgradePoints; }
    }

    private int maxUpgradeSlots = 10;
    public int MaxUpgradeSlots {
        get { return maxUpgradeSlots; }
    }

    private int upgradeSlotsInUse;
    public int SlotsInUse {
        get { return upgradeSlotsInUse; }
    }


    /// <summary>
    /// Table maps the players level for each upgrade type
    /// </summary>
    public Dictionary<string, int> upgradeUnlockTable;

    [HideInInspector]
    public List<UpgradePair> equippedUpgrades;

    private string testSavefile = "data/testSave.xml";
    private PlayerSave saveState;

    private void OnEnable() {
        instance = this;
        //LoadState();
    }




    public void CollectPoints(int pointsCollected) {
        upgradePoints += pointsCollected;
    }


    public void LoadState() {
        upgradeUnlockTable = new Dictionary<string, int>();
        equippedUpgrades = new List<UpgradePair>();

        saveState = XMLObjectLoader.LoadXMLObject<PlayerSave>(testSavefile);
        if(saveState == null) {
            Debug.Log("Player State could not load a save file from data/testSave.xml");
            saveState = new PlayerSave();
        }

        upgradePoints = saveState.availablePoints;

        upgradeUnlockTable.Add("Upgrade_Health", saveState.GetSavedLevelForUpgrade("Upgrade_Health"));
        upgradeUnlockTable.Add("Upgrade_HealthRegen", saveState.GetSavedLevelForUpgrade("Upgrade_HealthRegen"));

        if(saveState.equippedUpgrades != null) {
            //equippedUpgrades.AddRange(saveState.equippedUpgrades);
            // update the slots in use
            for(int i = 0; i < saveState.equippedUpgrades.Length; i++) {
                EquipUpgradeType(saveState.equippedUpgrades[i].name, saveState.equippedUpgrades[i].level);
            }
        }
        else {
            saveState.equippedUpgrades = new PlayerSave.UpgradePair[0];
        }
    }


    public void SaveState() {
        if(saveState != null) {
            saveState.UpdateFromState(this);
        }
        else {
            saveState = new PlayerSave();
        }

        XMLObjectLoader.SaveXMLObject<PlayerSave>(testSavefile, saveState);
    }


    /// <summary>
    /// Tries to equip the upgrade type of the given level: returns true if successful
    /// </summary>
    public bool EquipUpgradeType(string upgradeType, int atLevel) {
        if(!IsUpgradeEquipped(upgradeType, atLevel)) {
            // Double check that the upgrade is actually unlocked, otherwise it can be equipped...
            int currentUnlockedLevel = 0;
            upgradeUnlockTable.TryGetValue(upgradeType, out currentUnlockedLevel);

            if(atLevel >= currentUnlockedLevel) {
                return false;
            }

            int requiredSlots = UpgradeManager.instance.GetSlotsForUpgrade(upgradeType, atLevel);

            Debug.Log(upgradeType + " needs " + requiredSlots + " to be equipped.");

            // Check if there is space
            if(upgradeSlotsInUse + requiredSlots <= maxUpgradeSlots) {
                upgradeSlotsInUse += requiredSlots;

                equippedUpgrades.Add(new UpgradePair(upgradeType, atLevel));

                Debug.Log(upgradeType + " was equipped on the player state, slots in use [" + upgradeSlotsInUse + " of " + maxUpgradeSlots + "]");
                return true;
            }
        }

        return false;
    }



    public bool UnequipUpgrade(string upgradeType, int atLevel) {
        if(IsUpgradeEquipped(upgradeType, atLevel)) {

            // Get the number of slots it was using
            //int currentUnlockLevel = 0;
            //upgradeUnlockTable.TryGetValue(upgradeType, out currentUnlockLevel);
            int requiredSlots = UpgradeManager.instance.GetSlotsForUpgrade(upgradeType, atLevel);


            equippedUpgrades.RemoveAt(GetEquipIndex(upgradeType, atLevel));
            upgradeSlotsInUse -= requiredSlots;    

            Debug.Log(upgradeType + " was unequipped from the player state, slots in use [" + upgradeSlotsInUse + " of " + maxUpgradeSlots + "]");
            return true;
        }

        return false;
    }

    public bool IsUpgradeEquipped(string upgradeName, int atLevel) {
        foreach(UpgradePair equipped in equippedUpgrades) {
            if(equipped.name == upgradeName &&
               equipped.level == atLevel) {
                   return true;
            }
        }

        return false;
    }


    public int GetEquipIndex(string upgradeName, int atLevel) {
        for(int i = 0; i < equippedUpgrades.Count; i++) {
            if(equippedUpgrades[i].name == upgradeName &&
               equippedUpgrades[i].level == atLevel) {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Unlock the next level of the target upgrade, returns whether or not it was used 
    /// </summary>
    public bool UnlockUpgradeLevel(string targetUpgrade, out int pointsUsed) {
        int currentUnlockLevel = 0;
        pointsUsed = 0;
        upgradeUnlockTable.TryGetValue(targetUpgrade, out currentUnlockLevel);
        int upgradeMaxUnlockLevel = UpgradeManager.instance.GetMaxLevel(targetUpgrade);

        if(currentUnlockLevel >= upgradeMaxUnlockLevel) {
            return false;
        }

        int requiredPoints = UpgradeManager.instance.GetPointsToUpgrade(targetUpgrade, currentUnlockLevel);

        if(upgradePoints >= requiredPoints) {
            // Perform the upgrade and subtract the points
            upgradeUnlockTable[targetUpgrade] = currentUnlockLevel + 1;
            upgradePoints -= requiredPoints;
            pointsUsed = requiredPoints;

            Debug.Log("Upgrade point used on [" + targetUpgrade + "] :: current level=" + upgradeUnlockTable[targetUpgrade]);
            return true;
        }

        
        return false;
    }


    /// <summary>
    /// Returns all of the equipped levels of the given upgrade
    /// </summary>
    public int[] GetEquippedLevelsFor(string upgradeName) {
        if(equippedUpgrades == null) {
            return new int[0];
        }
        
        List<int> levels = new List<int>(5);
        
        for(int i = 0; i < equippedUpgrades.Count; i++) {
            if(equippedUpgrades[i].name == upgradeName) {
                levels.Add(equippedUpgrades[i].level);
            }
        }

        return levels.ToArray();
    }

}
