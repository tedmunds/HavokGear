using UnityEngine;
using System.Collections;

/// <summary>
/// Modifies the players health
/// </summary>
public class Upgrade_Health : PlayerUpgrade {


    private float healthPerLevel = 500;


    public Upgrade_Health(int currentLevel)
        : base(currentLevel) {
        maxUpgradeLevel = 5;
    }



    public float GetBonusHealth() {
        return healthPerLevel * (currentUpgradeLevel);
    }

    public override int RequiredSlots(int currentLevel) {
        return 2;
    }

    public override int PointsToUpgrade(int currentLevel) {
        return 1;
    }
}
