using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_UpgradeTile : MonoBehaviour {

    [SerializeField]
    public string playerUpgradeClass;

    [SerializeField]
    public Image upgradeImage;

    [SerializeField]
    public Text upgradeLevelField;

    [SerializeField]
    public Text requiredSlotsField;

    private const string levelPrefixString = "Level: ";
    private const string slotsPrefixString = "Slots: ";
	
	private void Start() {
	    PlayerState playerState = FindObjectOfType<PlayerState>();
        if(playerState != null) {
            int level = 0;
            playerState.upgradeTable.TryGetValue(playerUpgradeClass, out level);
        
            upgradeLevelField.text = levelPrefixString + level;
            requiredSlotsField.text = slotsPrefixString + UpgradeMenuManager.GetSlotsFromUpgrade(playerUpgradeClass, level);
        }
	}
	
	
	private void Update() {
	
	}


    /// <summary>
    /// Message recieved when this tile is dropped into an upgrade slot
    /// </summary>
    public void UI_DroppedInto(UI_DropArea droppedInto) {
        
    }


    public void PointAdded(string targetUpgrade) {
        if(targetUpgrade == playerUpgradeClass) {
            int level = 0;
            PlayerState.instance.upgradeTable.TryGetValue(targetUpgrade, out level);

            upgradeLevelField.text = levelPrefixString + level;
            requiredSlotsField.text = slotsPrefixString + UpgradeMenuManager.GetSlotsFromUpgrade(playerUpgradeClass, level);
        }
    }
}
