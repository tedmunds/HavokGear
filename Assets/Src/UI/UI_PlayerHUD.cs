using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections;

/// <summary>
/// Manages active elements of the player HUD
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class UI_PlayerHUD : MonoBehaviour {


    [SerializeField]
    public string UI_PhotonWhip_Recharge_Tag;

    [SerializeField]
    public string UI_EquippedWeapon_Tag;

    [SerializeField]
    public string UI_SecondaryWeapon_Tag;

    [SerializeField]
    public string UI_DamageType_Tag;

    [SerializeField]
    public Sprite[] damageTypeIcons;


    /// <summary>
    /// The HUD image element that represents the whip cooldown time
    /// </summary>
    private Image whipCooldownElement;

    [HideInInspector]
    public Image damageTypeElement;
    [HideInInspector]
    public Image equippedWeaponElement;
    [HideInInspector]
    public Image secondarydWeaponElement;

    // The player that this HUD is ownd by
    private PlayerController owner;


    private float whipRechargeStartTime;
    private float whipRechargeLength;
    private bool showingWhipRecharge;


    private void Start() {
        owner = GetComponent<PlayerController>();
        
        // Find the tagged images
        Image[] hud_images = FindObjectsOfType<Image>();
        foreach(Image i in hud_images) {
            if(i.gameObject.tag == UI_PhotonWhip_Recharge_Tag) {
                whipCooldownElement = i;
            }
            if(i.gameObject.tag == UI_EquippedWeapon_Tag) {
                equippedWeaponElement = i;
            }
            if(i.gameObject.tag == UI_DamageType_Tag) {
                damageTypeElement = i;
            }
            if(i.gameObject.tag == UI_SecondaryWeapon_Tag) {
                secondarydWeaponElement = i;
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
        Vector3 screenPos = owner.PlayerCamera.WorldToScreenPoint(owner.transform.position);

        whipCooldownElement.rectTransform.position = screenPos; //Input.mousePosition;
    }


    /// <summary>
    /// Start the whip recharge indicator
    /// </summary>
    public void SetWhipRecharge(float cooldown) {
        whipRechargeLength = cooldown;
        whipRechargeStartTime = Time.time;
        showingWhipRecharge = true;
    }


    public void SetDamageTypeDisplay(Weapon.EDamageType[] damageTypeList) {
        if(damageTypeList != null && damageTypeList.Length > 0) {
            for(int i = 0; i < damageTypeIcons.Length; i++) {
                if(i == (int)damageTypeList[0]) {
                    damageTypeElement.enabled = true;
                    damageTypeElement.sprite = damageTypeIcons[i];
                }
            }
        }
        else {
            damageTypeElement.enabled = false;
        }
    }


}
