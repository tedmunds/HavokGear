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

    [SerializeField]
    public Text numericField;

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
            float watchValue = GetCurrentValue();
            float maxValue = GetMaxValue();
            barOverlay.rectTransform.sizeDelta = new Vector2(baseWidth * (watchValue / maxValue), barOverlay.rectTransform.rect.height);

            if(numericField != null) {
                numericField.text = (int)watchValue + "";
            }   
        }
	}


    /// <summary>
    /// Gets a percentage value for the watched player status
    /// </summary>
    private float GetCurrentValue() {
        if(targetPlayer == null) {
            return 1.0f;
        }

        switch(watchStatus) {
            case ETargetStatus.None:
                return 1.0f;
            case ETargetStatus.Health:
                return targetPlayer.MechComponent.Health;
            case ETargetStatus.Energy:
                return targetPlayer.MechComponent.EnergyLevel;
            case ETargetStatus.Shield:
                return targetPlayer.MechComponent.CurrentShield;
        }

        return 1.0f;
    }


    private float GetMaxValue() {
        if(targetPlayer == null) {
            return 1.0f;
        }

        switch(watchStatus) {
            case ETargetStatus.None:
                return 1.0f;
            case ETargetStatus.Health:
                return targetPlayer.MechComponent.maxhealth + targetPlayer.GetHealthModifier();
            case ETargetStatus.Energy:
                return targetPlayer.MechComponent.maxEnergyLevel + targetPlayer.GetEnergyModifier();
            case ETargetStatus.Shield:
                return targetPlayer.MechComponent.maxShield;
        }

        return 1.0f;
    }



}
