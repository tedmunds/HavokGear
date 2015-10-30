using UnityEngine;
using System.Collections;



public abstract class PlayerUpgrade {

    /// <summary>
    /// The upgrades current numeric level
    /// </summary>
    public int currentUpgradeLevel;

    /// <summary>
    /// The max level this upgrade can reach
    /// </summary>
    public int maxUpgradeLevel;



    public PlayerUpgrade(int currentLevel) {
        currentUpgradeLevel = currentLevel;
    }


    /// <summary>
    /// Returns the number of slots required for this upgrade to be equipped, at the given input level
    /// </summary>
    public abstract int RequiredSlots(int currentLevel);


}
