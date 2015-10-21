using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages active elements of the player HUD
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class UI_PlayerHUD : MonoBehaviour {

    public string UI_PhotonWhip_Recharge_Tag;


    /// <summary>
    /// The HUD image element that represents the whip cooldown time
    /// </summary>
    private Image whipCooldownElement;

    // The player that this HUD is ownd by
    private PlayerController owner;


    private float whipRechargeStartTime;
    private float whipRechargeLength;
    private bool showingWhipRecharge;


    private void Start() {
        owner = GetComponent<PlayerController>();

        // Find the recharge tagged image
        Image[] hud_images = FindObjectsOfType<Image>();
        foreach(Image i in hud_images) {
            if(i.gameObject.tag == UI_PhotonWhip_Recharge_Tag) {
                whipCooldownElement = i;
                break;
            }
        }

        if(whipCooldownElement == null) {
            Debug.LogWarning("UI_PlayerHUD could not find UI Image with tag [" + UI_PhotonWhip_Recharge_Tag + "]");
        }
        else {
            whipCooldownElement.fillAmount = 0.0f;
        }
    }


    private void Update() {
        if(Time.time - whipRechargeStartTime > whipRechargeLength && showingWhipRecharge) {
            showingWhipRecharge = false;
            whipCooldownElement.fillAmount = 0.0f;
        }
        else if(showingWhipRecharge) {
            whipCooldownElement.fillAmount = (Time.time - whipRechargeStartTime) / whipRechargeLength;
        }
    }


    private void LateUpdate() {
        whipCooldownElement.rectTransform.position = Input.mousePosition;
    }


    /// <summary>
    /// Start the whip recharge indicator
    /// </summary>
    public void SetWhipRecharge(float cooldown) {
        whipRechargeLength = cooldown;
        whipRechargeStartTime = Time.time;
        showingWhipRecharge = true;
    }

}
