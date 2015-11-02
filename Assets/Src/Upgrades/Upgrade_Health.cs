using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

/// <summary>
/// Modifies the players health
/// </summary>
[XmlRoot("Upgrade_Health")]
public class Upgrade_Health : PlayerUpgrade {

    public struct LevelData {
        [XmlElement("bonusHealth")]
        public float bonusHealth;

        [XmlElement("requiredSlots")]
        public int requiredSlots;

        [XmlElement("requiredPoints")]
        public int requiredPoints;
    }

    [XmlArray("statsPerLevel")]
    [XmlArrayItem("level")]
    public LevelData[] perLevelData;



    public float GetBonusHealth(int currentLevel) {
        return perLevelData[currentLevel].bonusHealth;
    }

    public override int RequiredSlots(int currentLevel) {
        return perLevelData[currentLevel].requiredSlots;
    }

    public override int PointsToUpgrade(int currentLevel) {
        return perLevelData[currentLevel].requiredPoints;
    }
}
