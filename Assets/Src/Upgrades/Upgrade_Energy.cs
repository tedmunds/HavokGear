using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[XmlRoot("Upgrade_Energy")]
public class Upgrade_Energy : PlayerUpgrade {



    public struct LevelData {
        [XmlElement("bonusEnergy")]
        public float bonusEnergy;

        [XmlElement("requiredSlots")]
        public int requiredSlots;

        [XmlElement("requiredPoints")]
        public int requiredPoints;
    }

    [XmlArray("statsPerLevel")]
    [XmlArrayItem("level")]
    public LevelData[] perLevelData;


    public float GetBonusEnergy(int currentLevel) {
        return perLevelData[currentLevel].bonusEnergy;
    }

    public override int RequiredSlots(int currentLevel) {
        return perLevelData[currentLevel].requiredSlots;
    }

    public override int PointsToUpgrade(int currentLevel) {
        return perLevelData[currentLevel].requiredPoints;
    }
}
