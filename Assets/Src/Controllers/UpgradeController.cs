using UnityEngine;
using System.Collections;


public class UpgradeController : MonoBehaviour {


    public Upgrade_Health healthUpgrade;
    public Upgrade_HealthRegen healthRegenUpgrade;


	private void Start() {
        
	}
	
	
	private void Update() {
	
	}

    /// <summary>
    /// Loads the current players upgrade levels and assigns them to the upgrades
    /// </summary>
    public void GetState() {
        PlayerState state = FindObjectOfType<PlayerState>();

        if(state != null) {
            int upgradeLevel;

            if(CanUseUpgrade<Upgrade_Health>(state, out upgradeLevel)) {
                healthUpgrade = new Upgrade_Health(upgradeLevel);
            }
            else {
                healthUpgrade = new Upgrade_Health(0);
            }

            if(CanUseUpgrade<Upgrade_HealthRegen>(state, out upgradeLevel)) {
                healthRegenUpgrade = new Upgrade_HealthRegen(upgradeLevel);
            }
            else {
                healthRegenUpgrade = new Upgrade_HealthRegen(0);
            }

        }
    }


    /// <summary>
    /// Checks if the palyer can use teh given upgrade, and outputs the level of the upgrade if it can
    /// </summary>
    private bool CanUseUpgrade<T>(PlayerState state, out int upgradeLevel) where T : PlayerUpgrade {
        upgradeLevel = 0;

        if(state == null) {
            return false;
        }

        string upgradeName = typeof(T).Name;

        // Make sure there is a valid level for the upgrade and that it is equipped
        if(state.upgradeTable != null && state.upgradeTable.TryGetValue(upgradeName, out upgradeLevel) &&
           state.equippedUpgrades.Contains(upgradeName)) {
            return true;
        }

        return false;
    }


}
