using UnityEngine;
using System.Collections;



public abstract class PlayerUpgrade {

    /// <summary>
    /// Returns the number of slots required for this upgrade to be equipped, at the given input level
    /// </summary>
    public abstract int RequiredSlots(int currentLevel);

    /// <summary>
    /// How many ores does it take to perform this upgrade
    /// </summary>
    public abstract int PointsToUpgrade(int currentLevel);
}
