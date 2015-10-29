using UnityEngine;
using System.Collections;

public class PlayerState : MonoBehaviour {

    /// <summary>
    /// Available points to spend on unlocking upgrades
    /// </summary>
    private int upgradePoints;
    public int UpgradePoints {
        get { return upgradePoints; }
    }

    
	private void Start() {
	    
	}
	
	
	private void Update() {
	    
	}




    public void CollectPoints(int pointsCollected) {
        upgradePoints += pointsCollected;
    }

}
