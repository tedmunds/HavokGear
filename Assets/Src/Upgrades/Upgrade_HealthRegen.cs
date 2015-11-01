using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[XmlRoot("Upgrade_HealthRegen")]
public class Upgrade_HealthRegen : PlayerUpgrade {


    public struct LevelData {
        [XmlElement("healthRegen")]
        public float healthRegen;

        [XmlElement("requiredSlots")]
        public int requiredSlots;

        [XmlElement("requiredPoints")]
        public int requiredPoints;
    }

    [XmlArray("statsPerLevel")]
    [XmlArrayItem("level")]
    public LevelData[] perLevelData;


    public float GetRegenPerSecond(int currentLevel) {
        return perLevelData[currentLevel].healthRegen;
    }



    public override int RequiredSlots(int currentLevel) {
        return perLevelData[currentLevel].requiredSlots;
    }

    public override int PointsToUpgrade(int currentLevel) {
        return perLevelData[currentLevel].requiredPoints;
    }
}
