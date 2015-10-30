using UnityEngine;
using System.Collections;
using System.Xml.Serialization;


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
    public UpgradePair[] upgradePairs;

    [XmlArray("equippedUpgrades")]
    [XmlArrayItem("equipped")]
    public string[] equippedUpgrades;



    public void UpdateFromState(PlayerState state) {
        // first cache all of the upgrade values
        for(int i = 0; i < upgradePairs.Length; i++) {
            string upgradeName = upgradePairs[i].name;

            int newVal;
            if(state.upgradeTable.TryGetValue(upgradeName, out newVal)) {
                upgradePairs[i].level = newVal;
            }
        }

        //then cache the poitsn
        availablePoints = state.UpgradePoints;
        equippedUpgrades = state.equippedUpgrades.ToArray();
    }


    public int GetSavedLevelForUpgrade(string upgrade) {
        for(int i = 0; i < upgradePairs.Length; i++) {
            if(upgradePairs[i].name == upgrade) {
                return upgradePairs[i].level;
            }
        }

        return 0;
    }
}
