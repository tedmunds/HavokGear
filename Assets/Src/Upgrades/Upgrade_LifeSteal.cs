using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[XmlRoot("Upgrade_LifeSteal")]
public class Upgrade_LifeSteal : PlayerUpgrade {


    public struct LevelData {
        [XmlElement("lifeSteal")]
        public float lifeSteal;

        [XmlElement("requiredSlots")]
        public int requiredSlots;

        [XmlElement("requiredPoints")]
        public int requiredPoints;
    }

    [XmlArray("statsPerLevel")]
    [XmlArrayItem("level")]
    public LevelData[] perLevelData;


    public float GetLifeStealAmmount(int currentLevel) {
        return perLevelData[currentLevel].lifeSteal;
    }



    public override int RequiredSlots(int currentLevel) {
        return perLevelData[currentLevel].requiredSlots;
    }

    public override int PointsToUpgrade(int currentLevel) {
        return perLevelData[currentLevel].requiredPoints;
    }
}
