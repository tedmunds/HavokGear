using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_UpgradeTile : MonoBehaviour {

    [SerializeField]
    public string playerUpgradeClass;

    [SerializeField]
    public int playerUpgradeLevel;

    [SerializeField]
    public Image upgradeImage;

    [SerializeField]
    public Sprite lockedImage;

    [SerializeField]
    public Text requiredSlotsField;

    [SerializeField]
    public Text requiredPointsField;

    [HideInInspector]
    public bool isLocked;

    private Sprite defaultImage;

    private const string slotsPrefixString = "";
	

	private void Start() {
	    PlayerState playerState = FindObjectOfType<PlayerState>();
        defaultImage = upgradeImage.sprite;

        if(playerState != null) {
            int unlockedLevel = 0;
            playerState.upgradeUnlockTable.TryGetValue(playerUpgradeClass, out unlockedLevel);

            // If this itile is for a currently locked upgrade level, use teh locked sprite
            if(unlockedLevel <= playerUpgradeLevel) {
                upgradeImage.sprite = lockedImage;
                isLocked = true;
                requiredSlotsField.text = "";
                requiredPointsField.text = "" + UpgradeManager.instance.GetPointsToUpgrade(playerUpgradeClass, playerUpgradeLevel);
            }
            else {
                isLocked = false;
                requiredSlotsField.text = slotsPrefixString + UpgradeManager.instance.GetSlotsForUpgrade(playerUpgradeClass, playerUpgradeLevel);
                requiredPointsField.text = "";
            }

            
        }
	}
	
	
	private void Update() {
	
	}


    /// <summary>
    /// Message recieved when this tile is dropped into an upgrade slot
    /// </summary>
    public void UI_DroppedInto(UI_DropArea droppedInto) {
        
    }


    public void PointAdded(string targetUpgrade, int unlockedLevel) {
        if(targetUpgrade == playerUpgradeClass) {
            int level = 0;
            PlayerState.instance.upgradeUnlockTable.TryGetValue(targetUpgrade, out level);

            if(unlockedLevel > playerUpgradeLevel) {
                upgradeImage.sprite = defaultImage;
                isLocked = false;
                requiredSlotsField.text = slotsPrefixString + UpgradeManager.instance.GetSlotsForUpgrade(playerUpgradeClass, playerUpgradeLevel);
                requiredPointsField.text = "";
            }
        }
    }
}
