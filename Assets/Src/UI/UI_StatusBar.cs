using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UI_StatusBar : MonoBehaviour {

    /// <summary>
    /// Indicates which player status to watch with this bar
    /// </summary>
    public enum ETargetStatus {
        None,
        Health,
        Energy,
        Shield,
    }

    [SerializeField]
    private ETargetStatus watchStatus;

    [SerializeField]
    public Image barOverlay;

    private PlayerController targetPlayer;

    private float baseWidth;

    private void Start() {
        baseWidth = barOverlay.rectTransform.rect.width;
    }

	private void Update() {
	    if(targetPlayer == null) {
            targetPlayer = FindObjectOfType<PlayerController>();
        }
        else {
            // Update the width
            float watchValue = GetWatchedValue();
            barOverlay.rectTransform.sizeDelta = new Vector2(baseWidth * watchValue, barOverlay.rectTransform.rect.height);
        }
	}


    /// <summary>
    /// Gets a percentage value for the watched player status
    /// </summary>
    private float GetWatchedValue() {
        if(targetPlayer == null) {
            return 1.0f;
        }

        switch(watchStatus) {
            case ETargetStatus.None:
                return 1.0f;
            case ETargetStatus.Health:
                return targetPlayer.MechComponent.Health / targetPlayer.MechComponent.maxhealth;
            case ETargetStatus.Energy:
                return targetPlayer.MechComponent.EnergyLevel / targetPlayer.MechComponent.maxEnergyLevel;
            case ETargetStatus.Shield:
                return targetPlayer.MechComponent.CurrentShield / targetPlayer.MechComponent.maxShield;
        }

        return 1.0f;
    }
}
