using UnityEngine;
using System.Collections;

public class UpgradeManager : MonoBehaviour {

    public static UpgradeManager instance;


    [HideInInspector]
    public Upgrade_Health upgrade_Health;

    [HideInInspector]
    public Upgrade_HealthRegen upgrade_HealthRegen;


    // data path for upgrade folder
    private const string upgradeDataFolder = "UpgradeData/";


    private void OnEnable() {
        instance = this;

        string pathToResources = Application.dataPath + "/" + upgradeDataFolder;

        // Load the upgrade instances
        //upgrade_Health = XMLObjectLoader.LoadXMLObject<Upgrade_Health>(pathToResources + typeof(Upgrade_Health).Name + ".xml");
        //upgrade_HealthRegen = XMLObjectLoader.LoadXMLObject<Upgrade_HealthRegen>(pathToResources + typeof(Upgrade_HealthRegen).Name + ".xml");

        upgrade_Health = XMLObjectLoader.LoadXMLObject<Upgrade_Health>(upgradeDataFolder + typeof(Upgrade_Health).Name);
        upgrade_HealthRegen = XMLObjectLoader.LoadXMLObject<Upgrade_HealthRegen>(upgradeDataFolder + typeof(Upgrade_HealthRegen).Name);


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
        }

        return 1;
    }






}
