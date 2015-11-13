using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class UI_PointsCounter : MonoBehaviour {

    private Text textField;
	
	private void Start() {
	    textField = GetComponent<Text>();
	}
	
	
	private void Update() {
        const string prefixText = "";

        textField.text = prefixText + GetOreCount();
	}


    public int GetOreCount() {
        if(WorldManager.instance != null && WorldManager.instance.playerState != null) {
            return WorldManager.instance.playerState.UpgradePoints;
        }

        return 0;
    }
}
