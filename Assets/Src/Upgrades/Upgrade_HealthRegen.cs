using UnityEngine;
using System.Collections;

public class Upgrade_HealthRegen : PlayerUpgrade {



    private float healthRegenPerSecondPerLevel = 25;

    public Upgrade_HealthRegen(int currentLevel)
        : base(currentLevel) {
        maxUpgradeLevel = 5;
    }





    public float GetRegenPerSecond() {
        return healthRegenPerSecondPerLevel * (currentUpgradeLevel);
    }



    public override int RequiredSlots(int currentLevel) {
        return 1 + (int)(currentLevel * 0.5f);
    }

    public override int PointsToUpgrade(int currentLevel) {
        return 2;
    }
}
