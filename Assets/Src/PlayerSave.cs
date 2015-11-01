using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Linq;


/// <summary>
/// XMl interface object that stores players state
/// </summary>
[XmlRoot("PlayerSave")]
public class PlayerSave {


    public struct UpgradePair {
        [XmlElement("name")]
        public string name;

        [XmlElement("level")]
        public int level;
    }


    [XmlElement("name")]
    public string playerName;

    [XmlElement("points")]
    public int availablePoints;

    [XmlArray("upgradeLevels")]
    [XmlArrayItem("upgrade")]
    public UpgradePair[] unlockedLevels;

    [XmlArray("equippedUpgrades")]
    [XmlArrayItem("equipped")]
    public UpgradePair[] equippedUpgrades;



    public void UpdateFromState(PlayerState state) {
        // first cache all of the upgrades unlocked levels
        //for(int i = 0; i < unlockedLevels.Length; i++) {
        //    string upgradeName = unlockedLevels[i].name;
        //
        //    int unlockedLevel;
        //    if(state.upgradeUnlockTable.TryGetValue(upgradeName, out unlockedLevel)) {
        //        unlockedLevels[i].level = unlockedLevel;
        //    }
        //}

        string[] keys = state.upgradeUnlockTable.Keys.ToArray();
        unlockedLevels = new UpgradePair[keys.Length];
        for(int i = 0; i < keys.Length; i++) {
            unlockedLevels[i].name = keys[i];

            int level = 0;
            state.upgradeUnlockTable.TryGetValue(keys[i], out level);
            unlockedLevels[i].level = level;
        }

        availablePoints = state.UpgradePoints;

        //then cache the equipped upgrades and their levels
        equippedUpgrades = new UpgradePair[state.equippedUpgrades.Count];

        for(int i = 0; i < state.equippedUpgrades.Count; i++) {
            equippedUpgrades[i].level = state.equippedUpgrades[i].level;
            equippedUpgrades[i].name = state.equippedUpgrades[i].name;
        }

    }


    public int GetSavedLevelForUpgrade(string upgrade) {
        if(unlockedLevels == null) {
            unlockedLevels = new UpgradePair[0];
        }

        for(int i = 0; i < unlockedLevels.Length; i++) {
            if(unlockedLevels[i].name == upgrade) {
                return unlockedLevels[i].level;
            }
        }

        return 0;
    }
}
