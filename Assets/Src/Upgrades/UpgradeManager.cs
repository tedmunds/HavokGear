using UnityEngine;
using System.Collections;

public class UpgradeManager : MonoBehaviour {

    public static UpgradeManager instance;


    [HideInInspector]
    public Upgrade_Health upgrade_Health;

    [HideInInspector]
    public Upgrade_HealthRegen upgrade_HealthRegen;

    [HideInInspector]
    public Upgrade_Energy upgrade_Energy;

    [HideInInspector]
    public Upgrade_LifeSteal upgrade_LifeSteal;


    // data path for upgrade folder
    private const string upgradeDataFolder = "UpgradeData/";


    private void OnEnable() {
        instance = this;

        string pathToResources = Application.dataPath + "/" + upgradeDataFolder;

        // Load the upgrade instances
        upgrade_Health = XMLObjectLoader.LoadXMLObjectInternal<Upgrade_Health>(upgradeDataFolder + typeof(Upgrade_Health).Name);
        upgrade_HealthRegen = XMLObjectLoader.LoadXMLObjectInternal<Upgrade_HealthRegen>(upgradeDataFolder + typeof(Upgrade_HealthRegen).Name);
        upgrade_Energy = XMLObjectLoader.LoadXMLObjectInternal<Upgrade_Energy>(upgradeDataFolder + typeof(Upgrade_Energy).Name);
        upgrade_LifeSteal = XMLObjectLoader.LoadXMLObjectInternal<Upgrade_LifeSteal>(upgradeDataFolder + typeof(Upgrade_LifeSteal).Name);

        PlayerState playerState = FindObjectOfType<PlayerState>();
        if(playerState != null) {
            playerState.LoadState();
        }
    }




    public int GetSlotsForUpgrade(string upgradeName, int currentLevel) {
        currentLevel = currentLevel > 0? currentLevel - 1 : currentLevel;
        switch(upgradeName) {
            case "Upgrade_Health":
                return upgrade_Health.RequiredSlots(currentLevel);
            case "Upgrade_HealthRegen":
                return upgrade_HealthRegen.RequiredSlots(currentLevel);
            case "Upgrade_Energy":
                return upgrade_Energy.RequiredSlots(currentLevel);
            case "Upgrade_LifeSteal":
                return upgrade_LifeSteal.RequiredSlots(currentLevel);
        }

        return 0;
    }



    /// <summary>
    /// Gets the number of ore points required to improve the upgrade
    /// </summary>
    public int GetPointsToUpgrade(string upgradeName, int currentLevel) {
        currentLevel = currentLevel > 0 ? currentLevel - 1 : currentLevel;

        // Its not ideal, but a simple switch case that needs to be updated to include all upgrades
        switch(upgradeName) {
            case "Upgrade_Health":
                return upgrade_Health.PointsToUpgrade(currentLevel);
            case "Upgrade_HealthRegen":
                return upgrade_HealthRegen.PointsToUpgrade(currentLevel);
            case "Upgrade_Energy":
                return upgrade_Energy.PointsToUpgrade(currentLevel);
            case "Upgrade_LifeSteal":
                return upgrade_LifeSteal.PointsToUpgrade(currentLevel);
        }

        return 1;
    }

    public int GetMaxLevel(string upgradeName) {
        // Its not ideal, but a simple switch case that needs to be updated to include all upgrades
        switch(upgradeName) {
            case "Upgrade_Health":
                return upgrade_Health.perLevelData.Length;
            case "Upgrade_HealthRegen":
                return upgrade_HealthRegen.perLevelData.Length;
            case "Upgrade_Energy":
                return upgrade_Energy.perLevelData.Length;
            case "Upgrade_LifeSteal":
                return upgrade_LifeSteal.perLevelData.Length;
        }

        return 1;
    }






}
