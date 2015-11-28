using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_BossHealthBar : MonoBehaviour {

    [SerializeField]
    public Image barOverlay;




    public void UpdateHealthBar(float healthRatio) {
        barOverlay.fillAmount = healthRatio;
    }

}
